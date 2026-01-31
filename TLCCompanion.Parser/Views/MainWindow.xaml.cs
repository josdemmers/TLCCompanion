using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace TLCCompanion.Parser.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            if (!string.IsNullOrWhiteSpace(GameDataLocationPathTextBox.Text))
            {
                folderDialog.InitialDirectory = Path.GetDirectoryName(GameDataLocationPathTextBox.Text);
            }

            if (folderDialog.ShowDialog() == true)
            {
                string? selectedFolderPath = folderDialog.FolderName;
                if (selectedFolderPath != null)
                {
                    GameDataLocationPathTextBox.Text = selectedFolderPath;
                }
            }
        }
    }
}