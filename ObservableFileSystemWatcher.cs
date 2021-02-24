using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace UpdateService
{
    public class ObservableFileSystemWatcher
    {
        public readonly FileSystemWatcher Watcher;

        public ObservableFileSystemWatcher(Action<FileSystemWatcher> configure) : this(new FileSystemWatcher())
        {
            configure(this.Watcher);
        }

        public ObservableFileSystemWatcher(FileSystemWatcher watcher)
        {
            this.Watcher = watcher;
            this.Changed = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(delegate (FileSystemEventHandler h)
            {
                this.Watcher.Changed += h;
            }, delegate (FileSystemEventHandler h)
            {
                this.Watcher.Changed -= h;
            }).Select<EventPattern<FileSystemEventArgs>, FileSystemEventArgs>(x => x.EventArgs);
            this.Renamed = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(delegate (RenamedEventHandler h)
            {
                this.Watcher.Renamed += h;
            }, delegate (RenamedEventHandler h)
            {
                this.Watcher.Renamed -= h;
            }).Select<EventPattern<RenamedEventArgs>, RenamedEventArgs>(x => x.EventArgs);
            this.Deleted = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(delegate (FileSystemEventHandler h)
            {
                this.Watcher.Deleted += h;
            }, delegate (FileSystemEventHandler h)
            {
                this.Watcher.Deleted -= h;
            }).Select<EventPattern<FileSystemEventArgs>, FileSystemEventArgs>(x => x.EventArgs);
            this.Errors = Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(delegate (ErrorEventHandler h)
            {
                this.Watcher.Error += h;
            }, delegate (ErrorEventHandler h)
            {
                this.Watcher.Error -= h;
            }).Select<EventPattern<ErrorEventArgs>, ErrorEventArgs>(x => x.EventArgs);
            this.Created = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(delegate (FileSystemEventHandler h)
            {
                this.Watcher.Created += h;
            }, delegate (FileSystemEventHandler h)
            {
                this.Watcher.Created -= h;
            }).Select<EventPattern<FileSystemEventArgs>, FileSystemEventArgs>(x => x.EventArgs);
        }

        public void Dispose()
        {
            this.Watcher.Dispose();
        }

        public void Start()
        {
            this.Watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            this.Watcher.EnableRaisingEvents = false;
        }

        public IObservable<FileSystemEventArgs> Changed { get; private set; }

        public IObservable<RenamedEventArgs> Renamed { get; private set; }

        public IObservable<FileSystemEventArgs> Deleted { get; private set; }

        public IObservable<ErrorEventArgs> Errors { get; private set; }

        public IObservable<FileSystemEventArgs> Created { get; private set; }


    }
}

