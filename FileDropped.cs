using System.IO;

namespace UpdateService
{
    public class FileDropped
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public FileDropped()
        {
        }

        public FileDropped(FileSystemEventArgs fileEvent)
        {
            this.Name = fileEvent.Name;
            this.FullPath = fileEvent.FullPath;
        }

        public FileDropped(string filePath)
        {
            this.Name = Path.GetFileName(filePath);
            this.FullPath = filePath;
        }


    }
}
