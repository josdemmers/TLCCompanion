using CommunityToolkit.Mvvm.Messaging;
using Memory;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Timers;
using TLCCompanion.Entities;
using TLCCompanion.Interfaces;
using TLCCompanion.Messages;

namespace TLCCompanion.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ISettingsManager _settingsManager;

        private string _appStatus = "Not connected";
        private double _delayMonitoring = 100;
        private string _gameStatus = "Not found";
        private bool _isApplicationClosing = false;
        private bool _isDllInjected = false;
        private bool _isGameFound = false;
        private bool _isConsoleEnabled = false;
        private bool _isMasterSwitchOn = false;
        private readonly SemaphoreSlim _lockPipe = new SemaphoreSlim(1, 1);
        private Mem _memLib = new();
        private NamedPipeServerStream? _namedPipeServerStream;
        private string _playerPositionDescription = "X: 0.0, Y: 0.0, Z: 0.0";
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;
        private System.Timers.Timer _timerUpdates = new();

        // Start of Constructors region

        #region Constructors

        public DashboardViewModel(ISettingsManager settingsManager)
        {
            // Init services
            _settingsManager = settingsManager;

            // Init messages
            WeakReferenceMessenger.Default.Register<ApplicationClosingMessage>(this, HandleApplicationClosingMessage);

            // Init tasks
            _ = StartMonitoringTask();

            // Init timers
            _timerUpdates.Interval = 250;
            _timerUpdates.Elapsed += TimerUpdatesElapsedHandler;
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

        public bool IsDllInjected
        {
            get => _isDllInjected;
            set
            {
                SetProperty(ref _isDllInjected, value);
                if (_isDllInjected)
                {
                    if (_isGameFound)
                    {
                        Task.Run(() =>
                        {
                            string dllPath = _settingsManager.Settings.Dll.Equals("Default")
                                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TLCCompanion-dll.dll")
                                : _settingsManager.Settings.DllPath;
                            if (File.Exists(dllPath) && _memLib.InjectDll(dllPath))
                            {
                                InitPipe();
                            }
                            else
                            {
                                IsDllInjected = false;
                            }
                        });
                    }
                    else
                    {
                        IsDllInjected = false;
                    }
                }
                else
                {
                    IsMasterSwitchOn = false;

                    Task.Run(async () =>
                    {
                        await SendCommandAsyncNoResponse("CMD_DISCONNECT");
                        if(_namedPipeServerStream?.IsConnected ?? false)
                        {
                            _namedPipeServerStream?.Disconnect();
                        }
                    });
                }
            }
        }
        

        public bool IsMasterSwitchOn
        {
            get => _isMasterSwitchOn;
            set
            {
                SetProperty(ref _isMasterSwitchOn, value);
                if (_isMasterSwitchOn)
                {
                    _timerUpdates.Start();
                }
                else
                {
                    _timerUpdates.Stop();
                }
            }
        }

        public bool IsConsoleEnabled
        {
            get => _isConsoleEnabled;
            set
            {
                SetProperty(ref _isConsoleEnabled, value);
                if (_isConsoleEnabled)
                {
                    _ = SendCommandAsyncNoResponse("CMD_CONSOLE_ON");
                }
                else
                {
                    _ = SendCommandAsyncNoResponse("CMD_CONSOLE_OFF");
                }
            }
        }

        public string PlayerPositionDescription 
        { 
            get => _playerPositionDescription;
            set => SetProperty(ref _playerPositionDescription, value);
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private async void HandleApplicationClosingMessage(object recipient, ApplicationClosingMessage message)
        {
            await SendCommandAsyncNoResponse("CMD_DISCONNECT");
            _isApplicationClosing = true;
        }

        private async void TimerUpdatesElapsedHandler(object? sender, ElapsedEventArgs e)
        {
            _timerUpdates.Stop();

            string? response = await SendCommandAsync("CMD_GET_LOCATION");
            if (response != null)
            {
                WeakReferenceMessenger.Default.Send(new PlayerCoordinatesUpdatedMessage(new PlayerCoordinatesUpdatedMessageParams
                {
                    PlayerCoordinates = PlayerCoordinates.FromJsonString(response)
                }));
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                PlayerPositionDescription = response ?? string.Empty;
            });

            _timerUpdates.Start();
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void InitPipe()
        {
            Thread ClientThread = new Thread(() => StartNamedPipeThread());
            ClientThread.Start();
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
                        }
                        else
                        {
                            GameStatus = "Not found";
                        }
                        _delayMonitoring = 100;
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Exception: {exception}");
                        _delayMonitoring = 1000;
                        GameStatus = "Not found";
                    }
                });
                await Task.Delay(TimeSpan.FromMilliseconds(_delayMonitoring));
            }
        }

        private async Task<string?> SendCommandAsync(string command)
        {
            if (_namedPipeServerStream == null || _streamReader == null || _streamWriter == null) return null;

            await _lockPipe.WaitAsync();
            try
            {
                await _streamWriter.WriteAsync(command);
                return await _streamReader.ReadLineAsync();
            }
            finally
            {
                _lockPipe.Release();
            }
        }

        private async Task SendCommandAsyncNoResponse(string command)
        {
            if (_namedPipeServerStream == null || _streamReader == null || _streamWriter == null) return;

            await _lockPipe.WaitAsync();
            try
            {
                await _streamWriter.WriteAsync(command);
            }
            finally
            {
                _lockPipe.Release();
            }

        }

        private void StartNamedPipeThread()
        {
            AppStatus = "Connecting";

            _namedPipeServerStream = new NamedPipeServerStream("tlcpipe");
            _namedPipeServerStream.WaitForConnectionAsync().ContinueWith(async _ =>
            {
                AppStatus = "Connected";

                _streamReader = new StreamReader(_namedPipeServerStream);
                _streamWriter = new StreamWriter(_namedPipeServerStream) { AutoFlush = true };

                while (_namedPipeServerStream.IsConnected && !_isApplicationClosing)
                {         
                    await Task.Delay(100);
                }

                AppStatus = "Disconnected";

                _namedPipeServerStream?.Dispose();
                _namedPipeServerStream = null;
            });
        }

        #endregion




















    }
}
