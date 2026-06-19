using VadkanRobotClient.ViewModels;
using Microsoft.Maui.Controls;
using System.Linq;

namespace VadkanRobotClient
{
    public partial class MainPage : ContentPage
    {
        MainViewModel vm => BindingContext as MainViewModel;
        public MainPage()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            BindingContext = vm;
            vm.RefreshCanvas = () => graphicsView.Invalidate();
            vm.ObstacleChanged = () => vm.RefreshObstacleImages(CanvasGrid);

            graphicsView.SizeChanged += OnGraphicsViewSizeChanged;
            LoadInitialObstacles();
        }

        private void OnCanvasTouched(object sender, TouchEventArgs e)
        {
            // First touch pont
            if (!e.Touches.Any())
                return;

            var touch = e.Touches[0];

            // get ViewModel
            if (BindingContext is MainViewModel vm)
            {
                vm.AddObstacleFromScreen(touch.X, touch.Y);
                LoadInitialObstacles();
            }
        }

        private async void OnGraphicsViewSizeChanged(object sender, EventArgs e)
        {
            // get viewmodel size
            if (BindingContext is MainViewModel vm)
            {
                vm.CanvasWidth = (float)graphicsView.Width;
                vm.CanvasHeight = (float)graphicsView.Height;
  
                await vm.LoadState();
                vm.RefreshObstacleImages(CanvasGrid);
            }
            
        }

        private async void LoadInitialObstacles()
        {
            // Letöltjük az akadályokat a szerverről
            await vm.LoadState();

            // Frissítjük a Grid-et az akadályokkal
            vm.RefreshObstacleImages(CanvasGrid);
        }

    }

}