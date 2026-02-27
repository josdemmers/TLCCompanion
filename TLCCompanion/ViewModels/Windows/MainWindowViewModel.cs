using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TLCCompanion.Interfaces;
using TLCCompanion.Messages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace TLCCompanion.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ISettingsManager _settingsManager;

        private ObservableCollection<object> _menuItems = new();
        private ObservableCollection<object> _footerMenuItems = new();

        private string _applicationTitle = "The Last Caretaker Companion";

        // Start of Constructors region

        #region Constructors

        public MainWindowViewModel(ISettingsManager settingsManager)
        {
            // Init services
            _settingsManager = settingsManager;

            // Init View commands
            ApplicationClosingCommand = new RelayCommand(ApplicationClosingExecute);
            ApplicationLoadedCommand = new RelayCommand(ApplicationLoadedExecute);

            // Init menu items
            InitMenuItems();
            InitFooterMenuItems();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public ICommand ApplicationClosingCommand { get; }
        public ICommand ApplicationLoadedCommand { get; }

        public ObservableCollection<object> MenuItems { get => _menuItems; set => _menuItems = value; }
        public ObservableCollection<object> FooterMenuItems { get => _footerMenuItems; set => _footerMenuItems = value; }

        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private void ApplicationClosingExecute()
        {
            WeakReferenceMessenger.Default.Send(new ApplicationClosingMessage(new ApplicationClosingMessageParams()));
        }

        private void ApplicationLoadedExecute()
        {
            ChangeTheme(_settingsManager.Settings.Theme);

            WeakReferenceMessenger.Default.Send(new ApplicationLoadedMessage(new ApplicationLoadedMessageParams()));
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void ChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    break;

                default:
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);

                    break;
            }
        }

        private void InitMenuItems()
        {
            MenuItems.Add(new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            });

            MenuItems.Add(new NavigationViewItem()
            {
                Content = "Map",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Map24 },
                TargetPageType = typeof(Views.Pages.MapPage)
            });
        }

        private void InitFooterMenuItems()
        {
            FooterMenuItems.Add(new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            });
        }

        #endregion
    }
}
