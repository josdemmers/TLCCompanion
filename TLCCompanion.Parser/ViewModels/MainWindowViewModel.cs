using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using TLCCompanion.Models;
using TLCCompanion.Parser.Models;

namespace TLCCompanion.Parser.ViewModels
{
    public class MainWindowViewModel
    {
        private string _gameDataLocationPath = @"C:\Tools\FModel\Output\Exports\Voyage";
        private readonly object _lockPOIWorldMapCommand = new object();

        private List<Indicator> _indicatorListList = [];
        private List<POIWorldmap> _poiWorldmapList = [];
        private List<WorldmapObject> _worldmapObjectList = [];

        // Start of Constructors region

        #region Constructors

        public MainWindowViewModel()
        {
            // Init View commands
            ParsePOIWorldmapCommand = new RelayCommand(ParsePOIWorldmapExecute);
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public ICommand ParsePOIWorldmapCommand { get; }

        public string GameDataLocationPath
        {
            get => _gameDataLocationPath;
            set
            {
                _gameDataLocationPath = value;
            }
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private void ParsePOIWorldmapExecute()
        {
            lock(_lockPOIWorldMapCommand)
            {
                _indicatorListList.Clear();
                _poiWorldmapList.Clear();
                _worldmapObjectList.Clear();

                // Parse VoyageWorld2.json
                string voyageWorld2Path = $"{_gameDataLocationPath}\\Content\\Maps\\VoyageWorld2.json";
                var jsonAsText = File.ReadAllText(voyageWorld2Path);
                _worldmapObjectList = System.Text.Json.JsonSerializer.Deserialize<List<WorldmapObject>>(jsonAsText) ?? [];
                _worldmapObjectList.RemoveAll(p => p.Type != "VoyageLocatorComponent");

                // Parse all Indicators
                //string indicatorPath = $"{_gameDataLocationPath}\\Content\\Textures\\UI\\Indicator\\";
                string indicatorPath = $"{_gameDataLocationPath}\\Content\\Data\\Indicator\\";
                if (Directory.Exists(indicatorPath))
                {
                    var fileEntries = Directory.EnumerateFiles(indicatorPath).Where(file => file.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
                    foreach (string fileName in fileEntries)
                    {
                        using (FileStream? stream = File.OpenRead(fileName))
                        {
                            if (stream != null)
                            {
                                var indicator = (JsonSerializer.Deserialize<List<Indicator>>(stream) ?? []).FirstOrDefault() ?? new Indicator();
                                _indicatorListList.Add(indicator);
                            }
                        }
                    }
                }

                // Create POIWorldmap list
                foreach (var worldmapObject in _worldmapObjectList)
                {
                    var poiWorldmap = new POIWorldmap();
                    poiWorldmap.Name = worldmapObject.Properties.properties.Name;
                    poiWorldmap.Localized = string.IsNullOrWhiteSpace(worldmapObject.Properties.properties.DisplayName.LocalizedString) ?
                        worldmapObject.Properties.properties.DisplayName.CultureInvariantString :
                        worldmapObject.Properties.properties.DisplayName.LocalizedString;
                    poiWorldmap.PositionX = worldmapObject.Properties.RelativeLocation.X;
                    poiWorldmap.PositionY = worldmapObject.Properties.RelativeLocation.Y;
                    poiWorldmap.PositionZ = worldmapObject.Properties.RelativeLocation.Z;

                    // Find Indicator icon
                    string indicatorName = worldmapObject.Properties.OverrideIndicator.ObjectName;
                    int start = indicatorName.IndexOf('\'') + 1;
                    int end = indicatorName.LastIndexOf('\'');
                    indicatorName = indicatorName.Substring(start, end - start);
                    var indicator = _indicatorListList.FirstOrDefault(i => i.Name == indicatorName) ?? new Indicator();
                    string iconName = indicator.Properties.Icons[0].Value.ObjectName;
                    start = iconName.IndexOf('\'') + 1;
                    end = iconName.LastIndexOf('\'');
                    iconName = iconName.Substring(start, end - start);
                    poiWorldmap.IconName = iconName;

                    _poiWorldmapList.Add(poiWorldmap);
                }

                // Save POIWorldmap list to file
                _poiWorldmapList.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                SavePOIWorldmapListToFile();
            }
        }

        #endregion

        // Start of Methods region

        #region Methods

        private void SavePOIWorldmapListToFile()
        {
            string fileName = $"Data/POIWorldmap.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _poiWorldmapList, options);
        }

        #endregion
    }
}
