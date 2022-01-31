using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SteamLibraryMover
{
    /// <summary>
    /// SteamLibraryFolder.xaml 的交互逻辑
    /// </summary>
    public partial class SteamLibraryFolder : UserControl
    {
        public SteamLibraryFolder()
        {
            InitializeComponent();
        }

        private void browser_steam_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "select steam.dll",
                FileName = "steam.dll",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Steam 库文件夹标记(steam.dll)|steam.dll"
            };

            if (dialog.ShowDialog() == true)
            {
                string steamDllPath = Path.GetFullPath(dialog.FileName);
                string steamDllDirPath = Path.GetDirectoryName(steamDllPath);
                string steamAppsPath = Path.Combine(steamDllDirPath, "steamapps");
                if (Directory.Exists(steamAppsPath))
                {
                    SelectedPathInfo = new DirectoryInfo(steamAppsPath);
                    OnFolderChanged?.Invoke(this, new EventArgs());
                    dir.Text = SelectedPathInfo.FullName;
                }
                else
                {
                    MessageBox.Show($"steamapps目录不存在{Environment.NewLine}{steamAppsPath}");
                }
            }
        }
        
    public event EventHandler<EventArgs> OnFolderChanged;

        public DirectoryInfo SelectedPathInfo { get; private set; }
    }
}