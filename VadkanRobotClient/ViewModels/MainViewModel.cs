using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VadkanRobotClient.Services;
using VadkanRobotClient.Models;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Text.Json;


namespace VadkanRobotClient.ViewModels
{
    public class MainViewModel : ViewModelBase, IDrawable
    {
        private double CollisionDistance => State?.CollisionDistance ?? 1;
        private double SensorDistance => State?.SensorDistance ?? 5;
        private double MoveDistance = 1;
        private double RotationStep = 1;

        public MainViewModel()
        {
            LoadImage();
            RefreshCommand = new Command(async () => await LoadState());
            ForwardCommand = new Command(async () => await Move());
            BackwardCommand = new Command(async () => await MoveBackward());
            RotateLeftCommand = new Command(async () => await RotateLeft());
            RotateRightCommand = new Command(async () => await RotateRight());
            OpenSettingsCommand = new Command(async () => { await Shell.Current.GoToAsync(nameof(RobotSettingsPage));});
            ChargeCommand = new Command(async () => await ChargeBattery());
            SelfTestCommand = new Command(async () => await RunSelfTest());
            ClearObstaclesCommand = new Command(async () => await ClearAllObstacles());

            var settingsService = SettingsService.Instance;

            ApplySettings(settingsService.Current);

            settingsService.SettingsChanged += () => { ApplySettings(settingsService.Current);};

            _ = InitializeAsync();
      
        }
        private async Task InitializeAsync()
        {
            await LoadState();
        }
        private void LoadImage()
        {
            RobotImageSource = ImageSource.FromFile("robot.png");
            AcronImageSource = ImageSource.FromFile("acron.png");
            
        }

        private ImageSource _acronImageSource;
        public ImageSource AcronImageSource
        {
            get => _acronImageSource;
            set { _acronImageSource = value; OnPropertyChanged(); }
        }

        private ImageSource _robotImageSource;
        public ImageSource RobotImageSource
        {
            get => _robotImageSource;
            set { _robotImageSource = value; OnPropertyChanged(); }
        }

        private RobotApiService _api = new RobotApiService();

        private const float Scale = 20f;      

        // get Canvas size from GraphicsView SizeChanged
        private float _canvasWidth;
        public float CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                if (_canvasWidth != value)
                {
                    _canvasWidth = value;
                    OnPropertyChanged(nameof(RobotX));
                }
            }
        }

        private float _canvasHeight;
        public float CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                if (_canvasHeight != value)
                {
                    _canvasHeight = value;
                    OnPropertyChanged(nameof(RobotY));
                }
            }
        }

        // set the Robot picture coordinates
        public float RobotX => State != null ? (float)State.X * Scale  - CanvasWidth / 2 : 0;
        public float RobotY => State != null ? (float)State.Y * Scale  - CanvasHeight / 2 : 0;
        public float RobotAngle => State != null ? (float)State.Angle : 0;


        private RobotState _state;
        public RobotState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RobotX));
                OnPropertyChanged(nameof(RobotY));
                OnPropertyChanged(nameof(RobotAngle));
            }
        }

        private int _collisionCount;
        public int CollisionCount
        {
            get => _collisionCount;
            set
            {
                _collisionCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RobotX));
                OnPropertyChanged(nameof(RobotY));
                OnPropertyChanged(nameof(RobotAngle));
            }
        }

        public ICommand OpenSettingsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand RotateCommand { get; }
        public ICommand BackwardCommand { get; }
        public ICommand RotateLeftCommand { get; }
        public ICommand RotateRightCommand { get; }
        public ICommand ChargeCommand { get; }
        public ICommand ClearObstaclesCommand { get; }
        public ICommand SelfTestCommand { get; }
        public async Task LoadState()
        {
            State = await _api.GetStateAsync();
            var oldObstacles = Obstacles;
            Obstacles = await _api.GetObstacles();

            if (State.Battery <= 0)
            {
                await HandleBatteryEmpty();
                return;
            }

            if (State.DistanceSensor < CollisionDistance)
            {
                CollisionCount++;

                // Remove from server
                await _api.RemoveCollidedObstacle(State, CollisionDistance);
                Obstacles = await _api.GetObstacles();
                ObstacleChanged?.Invoke();
            }
            RefreshCanvas?.Invoke();
        }

        private async Task ClearAllObstacles()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Clear obstacles",
                "Are you sure you want to remove all obstacles?",
                "Yes",
                "No");

            if (!confirm)
                return;

            try
            {
                await _api.ClearObstacles();

                // UI frissítés
                Obstacles.Clear();
                Obstacles = await _api.GetObstacles();
                OnPropertyChanged(nameof(Obstacles));
                await Task.Delay(100);
                ObstacleChanged?.Invoke();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to clear obstacles:\n" + ex.Message,
                    "OK");
            }
        }

        private async Task ChargeBattery()
        {
            await _api.Charge();

            await Task.Delay(300); // kis várakozás (API miatt)

            await LoadState();
        }

        private async Task HandleBatteryEmpty()
        {
            bool charge = await Application.Current.MainPage.DisplayAlert(
                "Battery empty ⚠️",
                "Battery is low! Charge up?",
                "Charge",
                "Exit");

            if (charge)
            {
                await _api.Charge();
                await Task.Delay(500);
                await LoadState();
            }
            else
            {
                // do not leave until it is charged!
                //await HandleBatteryEmpty();
                return;
            }
        }

        private bool CanMove()
        {
            return State != null && State.Battery > 0;
        }

        private List<Obstacle> _obstacles;
        public List<Obstacle> Obstacles
        {
            get => _obstacles ??= new List<Obstacle>();
            set
            {
                _obstacles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RobotX));
                OnPropertyChanged(nameof(RobotY));
                OnPropertyChanged(nameof(RobotAngle));
            }
        }

        // Action jelzés ütközésnél
        public Action ObstacleChanged { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // background
            canvas.FillColor = Colors.Green;
            canvas.FillRectangle(dirtyRect);

            // robot (if it is a dot not picture)
            /*if (State != null)
            {
                canvas.FillColor = Colors.Blue;

                canvas.FillCircle(
                    (float)State.X * scale,
                    (float)State.Y * scale,
                    5
                );
            }*/

            // obstackles
            /*if (Obstacles != null)
            {
                canvas.FillColor = Colors.Brown;

                foreach (var o in Obstacles)
                {
                    canvas.FillCircle(
                        (float)o.X * Scale,
                        (float)o.Y * Scale,
                        4
                    );
                }
            }*/


            // sensor
            if (State != null)
            {

                float centerX = (float)State.X * Scale;
                float centerY = (float)State.Y * Scale;

                double angle = State.Angle;

                double viewDistance = SensorDistance; // world unit
                double halfAngle = 30;   // degree

                // degree in radian
                double startAngle = (angle - halfAngle) * Math.PI / 180.0;
                double endAngle = (angle + halfAngle) * Math.PI / 180.0;

                
                int steps = 20;

                var path = new PathF();

                // kezdőpont = robot közepe
                path.MoveTo(centerX, centerY);

                for (int i = 0; i <= steps; i++)
                {
                    double t = startAngle + (endAngle - startAngle) * i / steps;

                    float x = centerX + (float)(Math.Cos(t) * viewDistance * Scale);
                    float y = centerY + (float)(Math.Sin(t) * viewDistance * Scale);

                    path.LineTo(x, y);
                }

                // vissza a középpontba
                path.Close();

                // áttetsző zöld
                canvas.FillColor = Colors.Yellow.WithAlpha(0.3f);

                canvas.FillPath(path);
            }
        }

        private List<Image> _obstacleImages = new List<Image>();

        public async void RefreshObstacleImages(Grid canvasGrid)
        {
            // Előző akadályok eltávolítása
            foreach (var img in _obstacleImages)
            {
                canvasGrid.Children.Remove(img);
            }
            _obstacleImages.Clear();

            // Új akadályok hozzáadása
            if (Obstacles != null && AcronImageSource != null)
            {
                foreach (var o in Obstacles)
                {
                    var img = new Image
                    {
                        Source = AcronImageSource,
                        WidthRequest = 20,
                        HeightRequest = 20
                    };

                    img.TranslationX = (float)o.X * Scale - CanvasWidth/2; // középre igazítás
                    img.TranslationY = (float)o.Y * Scale - CanvasHeight/2;

                    canvasGrid.Children.Add(img);
                    _obstacleImages.Add(img);
                }
            }      
        }

        public Action RefreshCanvas { get; set; }

        public async void AddObstacleFromScreen(double x, double y)
        {

            double worldX = x / Scale;
            double worldY = y / Scale;

            var obstacle = new Obstacle
            {
                X = worldX,
                Y = worldY
            };

            // elküldjük a szervernek
            await _api.AddObstacle(obstacle);

            // frissítés szerverről
            await LoadState();
        }
        private async Task MoveBackward()
        {
            if (!CanMove())
            {
                await HandleBatteryEmpty();
                return;
            }

            double angleRad = _state.Angle * Math.PI / 180.0;
            if ((float)State.X - Math.Cos(angleRad) * MoveDistance / 20 < 0 ||
                (float)State.Y - Math.Sin(angleRad) * MoveDistance / 20 < 0 ||
                (float)State.X - Math.Cos(angleRad) * MoveDistance / 20 > CanvasWidth / 20 ||
                (float)State.Y - Math.Sin(angleRad) * MoveDistance / 20 > CanvasHeight / 20)
            {
                return;
            }

            await _api.MoveForward(-MoveDistance);
            await LoadState();
        }

        private async Task RotateLeft()
        {
            if (!CanMove())
            {
                await HandleBatteryEmpty();
                return;
            }

            await _api.Rotate(-RotationStep);
            await LoadState();
        }

        private async Task RotateRight()
        {
            if (!CanMove())
            {
                await HandleBatteryEmpty();
                return;
            }

            await _api.Rotate(RotationStep);
            await LoadState();
        }

        private async Task Move()
        {
            if (!CanMove())
            {
                await HandleBatteryEmpty();
                return;
            }

            double angleRad = _state.Angle * Math.PI / 180.0;

            if ((float)State.X + Math.Cos(angleRad) * MoveDistance / 20 < 0 ||
                (float)State.Y + Math.Sin(angleRad) * MoveDistance / 20 < 0 ||
                (float)State.X + Math.Cos(angleRad) * MoveDistance / 20 > CanvasWidth / 20 ||
                (float)State.Y + Math.Sin(angleRad) * MoveDistance / 20 > CanvasHeight /20)
            {
                return;
            }

            await _api.MoveForward(MoveDistance);
            await LoadState();
        }

        public RobotSettings Settings { get; set; }

        private void ApplySettings(RobotSettings s)
        {
            MoveDistance = s.Speed;
            RotationStep = s.RotationSpeed;
        }

        private async Task RunSelfTest()
        {
            try
            {
                var result = await _api.SelfTest();

                await Application.Current.MainPage.DisplayAlert(
                    "Self Test",
                    result,
                    "OK");

                await LoadState();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Self test failed:\n" + ex.Message,
                    "OK");
            }
        }

    }
}
