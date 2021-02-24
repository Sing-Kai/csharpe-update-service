using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace UpdateService
{
    public class FileDropWatcher
    {
        private readonly string _Path;
        private readonly string _Filter;
        private readonly ObservableFileSystemWatcher _Watcher;
        private readonly Subject<FileDropped> _PollResults = new Subject<FileDropped>();
        public IObservable<FileDropped> Dropped { get; private set; }

        public FileDropWatcher(string path, string filter)
        {
            this._Path = path;
            this._Filter = filter;
            this._Watcher = new ObservableFileSystemWatcher(delegate (FileSystemWatcher w) {
                w.Path = path;
                w.Filter = filter;
                w.NotifyFilter = NotifyFilters.LastWrite;
            });
            this._Watcher.Created.Select<FileSystemEventArgs, FileDropped>(c => new FileDropped(c));
            IObservable<FileDropped> observable = this._Watcher.Changed.Select<FileSystemEventArgs, FileDropped>(c => new FileDropped(c));
            this.Dropped = observable;
        }

        public void Dispose()
        {
            this._Watcher.Dispose();
        }

        public void PollExisting()
        {
            foreach (string str in Directory.GetFiles(this._Path, this._Filter))
            {
                this._PollResults.OnNext(new FileDropped(str));
            }
        }

        public void Start()
        {
            this._Watcher.Start();
        }

        public void Stop()
        {
            this._Watcher.Stop();
        }
    }
}



