using CommunityToolkit.Mvvm.Messaging;
using Memory;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using TLCCompanion.Messages;

namespace TLCCompanion.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private string _appStatus = "Not connected";
        private double _delayMonitoring = 100;
        private string _gameStatus = "Not found";
        private bool _isGameFound = false;
        private bool _isToggleSwitchChecked = false;
        private Mem _memLib = new();
        private string _playerPositionDescription = "X: 0.0, Y: 0.0, Z: 0.0";

        // Start of Constructors region

        #region Constructors

        public DashboardViewModel()
        {
            // Init tasks
            _ = StartMonitoringTask();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public string AppStatus 
        { 
            get => _appStatus;
            set => SetProperty(ref _appStatus, value);
        }

        public string GameStatus
        { 
            get => _gameStatus;
            set => SetProperty(ref _gameStatus, value);
        }

        public bool IsToggleSwitchChecked
        {
            get => _isToggleSwitchChecked;
            set => SetProperty(ref _isToggleSwitchChecked, value);
        }

        public string PlayerPositionDescription 
        { 
            get => _playerPositionDescription;
            set => SetProperty(ref _playerPositionDescription, value);
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        private void ReadPlayerPosition()
        {
            //0.6.1.606677
            float playerX = _memLib.ReadFloat("base+0x0A7CA2A8,188,60,4D0");
            float playerY = _memLib.ReadFloat("base+0x0A7CA2A8,188,60,4D4");
            float playerZ = _memLib.ReadFloat("base+0x0A7CA2A8,188,60,4D8");
            PlayerPositionDescription = $"X: {playerX}, Y: {playerY}, Z: {playerZ}";

            WeakReferenceMessenger.Default.Send(new PlayerCoordinatesUpdatedMessage(new PlayerCoordinatesUpdatedMessageParams
            {
                PositionX = playerX,
                PositionY = playerY,
                PositionZ = playerZ
            }));
        }

        private async Task StartMonitoringTask()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        _isGameFound = _memLib.OpenProcess("VoyageSteam-Win64-Shipping");
                        if (_isGameFound)
                        {
                            GameStatus = "Found";
                            if (IsToggleSwitchChecked)
                            {
                                AppStatus = "Connected";
                                ReadPlayerPosition();
                            }
                            else
                            {
                                AppStatus = "Not connected";
                            }
                        }
                        else
                        {
                            IsToggleSwitchChecked = false;
                            GameStatus = "Not found";
                            AppStatus = "Not connected";
                        }
                        _delayMonitoring = 100;
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                        _delayMonitoring = 1000;

                        IsToggleSwitchChecked = false;
                        GameStatus = "Not found";
                        AppStatus = "Not connected";
                    }
                });
                await Task.Delay(TimeSpan.FromMilliseconds(_delayMonitoring));
            }
        }

        #endregion




















    }
}
