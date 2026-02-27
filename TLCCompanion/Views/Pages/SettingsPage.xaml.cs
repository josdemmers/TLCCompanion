using Microsoft.Win32;
using System.IO;
using TLCCompanion.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace TLCCompanion.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void ButtonDllPath_Click(object sender, RoutedEventArgs e)
        {
            // https://devblogs.microsoft.com/dotnet/wpf-file-dialog-improvements-in-dotnet-8/
            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(DllPathTextBox.Text),
                Filter = "Dll files (*.dll)|*.dll"
            };

            if (fileDialog.ShowDialog() == true)
            {
                DllPathTextBox.Text = fileDialog.FileName;
            }
        }
    }
}
