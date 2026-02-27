using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using TLCCompanion.Messages;
using TLCCompanion.Models;

namespace TLCCompanion.ViewModels.Pages
{
    public partial class MapViewModel : ObservableObject
    {
        private Map? _map;
        private static List<POIWorldmap> _poiWorldmapList = [];

        // Start of Constructors region

        #region Constructors

        public MapViewModel()
        {
            InitPOIWorldmapData();
            InitMap();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public Map? Map
        { 
            get => _map;
            set
            {
                SetProperty(ref _map, value);
            }
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private static void MapTapped(object? sender, MapEventArgs e)
        {
            var mapInfo = e.GetMapInfo(e.Map.Layers.Where(l => l.Name == "POI"));
            var calloutStyle = mapInfo.Feature?.Styles.OfType<CalloutStyle>().FirstOrDefault();
            if (calloutStyle is not null)
            {
                calloutStyle.Enabled = !calloutStyle.Enabled;
                mapInfo.Layer?.DataHasChanged(); // To trigger a refresh of graphics.
                e.Handled = true;
            }
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void InitMap()
        {
            //_map.Layers.FindLayer("POI").First().Enabled = true;

            _map = new Map();
            _map.BackColor = Mapsui.Styles.Color.FromArgb(255, 20, 80, 25);

            // Layers
            _map.Layers.Add(CreatePOIWorldmap());
            _map.Layers.Add(CreatePlayerPosition());

            // Widgets
            _map.Widgets.Add(new MouseCoordinatesWidget());
            var mapInfoWidget = new MapInfoWidget(_map, l => l.Name == "POI")
            {
                FeatureToText = feature =>
                {
                    if (feature is PointFeature pointFeature)
                    {
                        string featureInfo = $"Name: {pointFeature["Name"]} | X: {pointFeature.Point.X} Y: {pointFeature.Point.Y}";
                        return featureInfo;
                    }
                    return string.Empty;
                }
            };
            _map.Widgets.Add(mapInfoWidget);
            var mapInfoWidgetPlayer = new MapInfoWidget(_map, l => l.Name == "PlayerPosition")
            {
                FeatureToText = feature =>
                {
                    if (feature is PointFeature pointFeature)
                    {
                        string featureInfo = $"X: {pointFeature.Point.X} Y: {pointFeature.Point.Y}";
                        return featureInfo;
                    }
                    return string.Empty;
                }
            };
            _map.Widgets.Add(mapInfoWidgetPlayer);

            // Events
            _map.Tapped += MapTapped;
        }

        private void InitPOIWorldmapData()
        {
            _poiWorldmapList.Clear();
            string resourcePath = @$".\Data\POIWorldmap.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    _poiWorldmapList = JsonSerializer.Deserialize<List<POIWorldmap>>(stream) ?? new List<POIWorldmap>();
                }
            }
        }

        private static ILayer CreatePlayerPosition()
        {
            return new AnimatedPointLayer(new PlayerPointProvider())
            {
                Name = "PlayerPosition",
                Style = new ImageStyle
                {
                    Image = new Image
                    {
                        Source = "embedded://TLCCompanion.Images.Indicator_Player.svg",
                        SvgFillColor = Mapsui.Styles.Color.FromArgb(255, 255, 0, 0)
                    },
                    SymbolScale = 0.1
                }
            };
        }

        private static MemoryLayer CreatePOIWorldmap()
        {
            var features = new List<PointFeature>();

            foreach (var poi in _poiWorldmapList)
            {
                var feature = new PointFeature(poi.PositionX, poi.PositionY * -1);
                feature["Name"] = poi.Localized;
                feature.Styles.Add(new ImageStyle
                {
                    Image = new Image
                    {
                        Source = $"embedded://TLCCompanion.Images.{poi.IconName}.png",
                    },
                    SymbolScale = 0.08
                });
                feature.Styles.Add(new CalloutStyle
                {
                    Title = $"{poi.Localized}",
                    TitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
                    TitleFontColor = Color.Gray,
                    MaxWidth = 250,
                    Enabled = false,
                    Offset = new Offset(0, SymbolStyle.DefaultHeight * 1f),
                    BalloonDefinition = new CalloutBalloonDefinition
                    {
                        RectRadius = 10,
                        ShadowWidth = 4,
                    }
                });
                features.Add(feature);
            }

            return new MemoryLayer
            {
                Name = "POI",
                Features = features,
                Style = new SymbolStyle
                {
                    SymbolScale = 0.5,
                    Fill = null
                }
            };
        }

        #endregion

    }

    public class PlayerPointProvider : MemoryProvider, IDynamic, IDisposable
    {
        private (double PositionX, double PositionY) _position = (0, 0);
        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

        public event EventHandler? DataChanged;

        public PlayerPointProvider()
        {
            // Init messages
            WeakReferenceMessenger.Default.Register<PlayerCoordinatesUpdatedMessage>(this, HandlePlayerCoordinatesUpdatedMessage);

            Catch.TaskRun(RunTimerAsync);
        }

        private void HandlePlayerCoordinatesUpdatedMessage(object recipient, PlayerCoordinatesUpdatedMessage message)
        {
            var playerCoordinatesUpdatedMessageParams = message.Value;

            _position.PositionX = playerCoordinatesUpdatedMessageParams.PlayerCoordinates.X;
            _position.PositionY = playerCoordinatesUpdatedMessageParams.PlayerCoordinates.Y * -1;
        }

        private async Task RunTimerAsync()
        {
            while (true)
            {
                await _timer.WaitForNextTickAsync();
                OnDataChanged();
            }
        }

        void IDynamic.DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new EventArgs());
        }

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var playerFeature = new PointFeature(_position.PositionX, _position.PositionY);
            playerFeature["ID"] = "player";
            return Task.FromResult((IEnumerable<IFeature>)[playerFeature]);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
