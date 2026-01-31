using TLCCompanion.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace TLCCompanion.Views.Pages
{
    /// <summary>
    /// Interaction logic for MapPage.xaml
    /// </summary>
    public partial class MapPage : INavigableView<MapViewModel>
    {
        public MapViewModel ViewModel { get; }

        public MapPage(MapViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
