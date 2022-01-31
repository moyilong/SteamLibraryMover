using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibraryMover
{
    internal class SteamLibraryContent : INotifyPropertyChanged
    {
        public bool RequireReqmove { get; set; } = false;
        public string Name { get; set; }
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore
        public double TotalSizeMb { get; set; }

        public double TotalSizeGb => Math.Round(TotalSizeMb / 1024.0f, 4);

        public double CopiedSizeMb { get; set; }
        public FileInfo ACFFullPath { get; set; }

       
        public string CurrentStatus { get; set; } = "";
        public bool Exist { get; set; } = true;

        public SteamLibraryContent()
        {

        }
        public string Source => SourceGameRoot.Parent.FullName;
        public SteamLibraryContent(FileInfo acfFile)
        {
            ACFFullPath = acfFile;

            foreach (var i in File.ReadAllLines(ACFFullPath.FullName))
                if (i.Contains("\"installdir\""))
                {
                    Name = i.Replace("\"installdir\"", "").Trim().Trim('"');
                }
            if (!SourceGameRoot.Exists)
            {
                Exist = false;
            }
            else
            {
                TotalSizeMb = Math.Round(SourceGameFiles.Sum(i => i.Length) / 1024.0f / 1024.0f, 2);
            }
        }

        public DirectoryInfo SourceGameRoot => new(Path.Combine(ACFFullPath.Directory.FullName, "Common", Name));

        public IEnumerable<FileInfo> SourceGameFiles => SourceGameRoot.EnumerateFiles("*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
        });

        static async Task Move(FileInfo i, FileInfo d)
        {
            using (var or = i.OpenRead())
            using (var ow = d.OpenWrite())
                await or.CopyToAsync(ow, 16 * 1024 * 1024);

            d.CreationTimeUtc = i.CreationTimeUtc;
            d.LastAccessTimeUtc = i.LastAccessTimeUtc;
            d.LastWriteTimeUtc = i.LastWriteTimeUtc;
            d.Attributes = i.Attributes;

        }

        async public Task RunMove(MainWindow window, DirectoryInfo targetPath)
        {
            foreach (var i in from i in SourceGameFiles
                              orderby i.Length descending
                              select i)
            {
                string target = Path.Combine(targetPath.FullName, "Common", i.FullName.Substring(SourceGameRoot.Parent.FullName.Length).Trim('\\','/'));
                string targetDir = Path.GetDirectoryName(target);
                window.Dispatcher.Invoke(() =>
                {
                    CurrentStatus = $"{Math.Round(i.Length / 1024.0f / 1024.0f, 2),16}Mb {i.Name} ";
                });
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
                FileInfo d = new FileInfo(target);
                await Move(i, d);
                window.Dispatcher.Invoke(() =>
                {
                    CopiedSizeMb = Math.Min(CopiedSizeMb + Math.Round(i.Length / 1024.0f / 1024.0f, 2), TotalSizeMb);
                });
            }
            window.Dispatcher.Invoke(() =>
            {
                CurrentStatus = $"Copy acf and remove old";
            });
            await Move(ACFFullPath, new FileInfo(Path.Combine(targetPath.FullName, ACFFullPath.Name)));
            SourceGameRoot.Delete(true);
            ACFFullPath.Delete();
        }
    }
}
