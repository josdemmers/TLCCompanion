using CommunityToolkit.Mvvm.Messaging;
using System.Reflection.Metadata;
using System.Windows.Input;
using TLCCompanion.Interfaces;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace TLCCompanion.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly ISettingsManager _settingsManager;

        private string _appVersion = string.Empty;
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;
        private bool _isInitialized = false;

        // Start of Constructors region

        #region Constructors

        public SettingsViewModel(ISettingsManager settingsManager)
        {
            // Init services
            _settingsManager = settingsManager;

            // Init View commands
            ChangeThemeCommand = new RelayCommand<string>(ChangeThemeExecute);
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public ICommand ChangeThemeCommand { get; }

        public string AppVersion
        {
            get => _appVersion;
            set => SetProperty(ref _appVersion, value);
        }

        public ApplicationTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                SetProperty(ref _currentTheme, value);
                _settingsManager.Settings.Theme = CurrentTheme == ApplicationTheme.Light ? "theme_light" : "theme_dark";
                _settingsManager.SaveSettings();
            }
        }

        public bool IsInitialized
        {
            get => _isInitialized;
            set => SetProperty(ref _isInitialized, value);
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private void ChangeThemeExecute(string? parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }

            return Task.CompletedTask;
        }

        #endregion

        // Start of Methods region

        #region Methods

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"The Last Caretaker Companion - {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        #endregion
    }
}
