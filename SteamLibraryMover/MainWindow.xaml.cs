using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SteamLibraryMover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            gamelist.ItemsSource = Contents;
        }

        private async void start_move_Click(object sender, RoutedEventArgs e)
        {
            start_move.IsEnabled = false;
            var target_dir = targetdir.SelectedPathInfo;
            foreach (var i in from i in Contents.ToArray()
                              where !i.RequireReqmove
                              select i)
                Contents.Remove(i);
            foreach (SteamLibraryContent i in Contents)
                i.RequireReqmove = true;
            try
            {
                foreach (var i in Contents.ToArray())
                    if (i.RequireReqmove)
                    {
                        try
                        {
                            await i.RunMove(this, target_dir);
                            Contents.Remove(i);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), ex.Message);
                        }
                    }
            }
            finally
            {
                start_move.IsEnabled = true;
            }
        }

        private void targetdir_OnFolderChanged(object sender, EventArgs e)
        {
            start_move.IsEnabled = targetdir.SelectedPathInfo != null && sourcedir.SelectedPathInfo != null;
        }

        private void sourcedir_OnFolderChanged(object sender, EventArgs e)
        {
            targetdir_OnFolderChanged(sender, e);
            Contents.Clear();
            if (sourcedir.SelectedPathInfo != null)
            {
                foreach (var i in from i in sourcedir.SelectedPathInfo.GetFiles()
                                  where i.Name.StartsWith("appmanifest") && i.Name.EndsWith(".acf")
                                  let obj = new SteamLibraryContent(i)
                                  group obj by obj.Name into j
                                  let cobj = j.First()
                                  orderby cobj.TotalSizeMb descending
                                  select cobj)
                {
                    Contents.Add(i);
                }
            }
        }

        private readonly ObservableCollection<SteamLibraryContent> Contents = new();

        private void required_Checked(object sender, RoutedEventArgs e)
        {
            if (start_move.IsEnabled == true)
                if ((sender as CheckBox).DataContext is SteamLibraryContent content)
                {
                    content.RequireReqmove = true;
                }
        }

        private void required_Unchecked(object sender, RoutedEventArgs e)
        {
            if (start_move.IsEnabled == true)
                if ((sender as CheckBox).DataContext is SteamLibraryContent content)
                {
                    content.RequireReqmove = false;
                }
        }
    }
}