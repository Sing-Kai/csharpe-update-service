using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Reactive;

namespace UpdateService
{
    public class DllUpdateProcess
    {
        private FileDropWatcher _watcher;
        private List<string> _dllConsumptionLog;
        private List<string> _updateFailureLog;
        private string _sourceDllPath { get; set; }

        private string _localDllPath { get; set; }

        private string _dllConsumedLogPath { get; set; }

        private string _updateFailureLogPath { get; set; }

        private string LocalExe { get; set; }

        private bool _autoShutDown { get; set; }

        public List<string> _dll { get; set; }

        public DllUpdateProcess()
        {

        }

        public DllUpdateProcess(bool autoShutDown)
        {
            this._autoShutDown = autoShutDown;
            this._sourceDllPath = ConfigurationManager.AppSettings["SourceDllPath"];
            this._localDllPath = ConfigurationManager.AppSettings["LocalDllPath"];
            this._dllConsumedLogPath = ConfigurationManager.AppSettings["DllConsumedLogPath"];
            this._updateFailureLogPath = ConfigurationManager.AppSettings["UpdateFailureLogPath"];
            this.LocalExe = ConfigurationManager.AppSettings["LocalExe"];
            this._dll = new List<string>();
            this._watcher = new FileDropWatcher(this._sourceDllPath, "*.dll");
            this._dllConsumptionLog = new List<string>();
        }

        private List<string> GetDllConsumptionLog() =>  new List<string>(File.ReadAllLines(this._dllConsumedLogPath));

        private List<string> GetUpdateFailureLog() =>  new List<string>(File.ReadAllLines(this._updateFailureLogPath));

        private bool IsDllConsumed(string dllName) =>  this._dllConsumptionLog.Contains(dllName);

        private bool IsFileLocked(string localDllPath)
        {
            FileInfo info = new FileInfo(localDllPath);
            FileStream stream = null;
            try
            {
                stream = info.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return false;
        }

        private bool IsModified(string sourceDllPath, string localDllPath) =>
            (DateTime.Compare(File.GetLastWriteTime(sourceDllPath), File.GetLastWriteTime(localDllPath)) != 0);

        private void LogUpdateFailure(string dllName)
        {
            if (!this._updateFailureLog.Contains(dllName))
            {
                File.AppendAllText(this._updateFailureLogPath, dllName + Environment.NewLine);
            }
        }

        private void PrintList()
        {
            using (List<string>.Enumerator enumerator = this._dll.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Console.WriteLine(enumerator.Current);
                }
            }
        }

        private void PrintList(List<string> logs)
        {
            using (List<string>.Enumerator enumerator = logs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Console.WriteLine(enumerator.Current);
                }
            }
        }

        private void ProcessDlls()
        {
            if (!this.ValidDirectoryPath())
            {
                Console.WriteLine("One of the Directory Paths Don't Exist!");
            }
            else
            {
                foreach (string str in this._dll)
                {
                    this.ProcessSingleDll(str);
                }
            }
        }

        private void ProcessSingleDll(string sourceDllPath)
        {
            string fileName = Path.GetFileName(sourceDllPath);
            string path = Path.Combine(this._localDllPath, fileName);
            if (!File.Exists(path))
            {
                File.Copy(sourceDllPath, path, true);
                Console.WriteLine("add in new dll");
            }
            else
            {
                bool flag = this.IsFileLocked(path);
                if (this.IsModified(sourceDllPath, path) && !flag)
                {
                    Console.WriteLine("over write file");
                    File.Copy(sourceDllPath, path, true);
                }
                else
                {
                    Console.WriteLine("Update failed");
                    this.LogUpdateFailure(fileName);
                }
            }
        }

        public void Start()
        {
            this._dllConsumptionLog = new List<string>();
            this.WatchingDllDrops(this._sourceDllPath);
            this.UpdateChecking();
        }

        private void StartProcess()
        {
            if (this._dll.Count == 0)
            {
                if ((this.TotalRunning(this.LocalExe) == 0) && this._autoShutDown)
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                this._updateFailureLog = this.GetUpdateFailureLog();
                this.PrintList();
                this.ProcessDlls();
                this._dll.Clear();
                this._dllConsumptionLog.Clear();
                this._updateFailureLog.Clear();
            }
        }

        private int TotalRunning(string FullPath)
        {
            string directoryName = Path.GetDirectoryName(FullPath);
            int num = 0;
            Process[] processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(FullPath).ToLower());
            for (int i = 0; i < processesByName.Length; i++)
            {
                if (processesByName[i].MainModule.FileName.StartsWith(directoryName, StringComparison.InvariantCultureIgnoreCase))
                {
                    num++;
                }
            }
            return num;
        }

        private void UpdateChecking()
        {
            Observable.Interval(TimeSpan.FromSeconds(5.0)).TimeInterval<long>().Subscribe<TimeInterval<long>>(x => this.StartProcess());
        }

        private void UpdateFileTracker(FileDropped file)
        {
            if (!this._dll.Contains(file.FullPath))
            {
                this._dll.Add(file.FullPath);
            }
        }

        private bool ValidDirectoryPath() =>
            (Directory.Exists(this._sourceDllPath) && Directory.Exists(this._localDllPath));

        private void WatchingDllDrops(string path)
        {
            try
            {
                this._watcher.Start();
                this._watcher.Dropped.ForEachAsync<FileDropped>(h => this.UpdateFileTracker(h));
            }
            catch (Exception exception)
            {
                Console.WriteLine("{0}", exception.Message);
            }
        }

    }
}
