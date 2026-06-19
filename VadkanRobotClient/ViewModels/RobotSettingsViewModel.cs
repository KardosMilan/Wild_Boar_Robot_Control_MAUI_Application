using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Input;
using VadkanRobotClient.Models;
using VadkanRobotClient.Services;
using System.ComponentModel;

namespace VadkanRobotClient.ViewModels 
{
    public class RobotSettingsViewModel : INotifyPropertyChanged
    {
        public RobotSettingsViewModel()
        {

            SaveCommand = new Command(async () => await Save());
            BackCommand = new Command(async () =>
                await Shell.Current.GoToAsync(".."));
        }
        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        private RobotApiService _api = new RobotApiService();

        // Set propertycanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // sensor distance
        private string _sensorDistance;
        public string SensorDistance
        {
            get => _sensorDistance;
            set
            {
                _sensorDistance = value;
                OnPropertyChanged();
            }
        }

        // collision distance
        private string _collisionDistance;
        public string CollisionDistance
        {
            get => _collisionDistance;
            set
            {
                _collisionDistance = value;
                OnPropertyChanged();
            }
        }

        // Speed
        private string _speed;
        public string Speed
        {
            get => _speed;
            set
            {
              _speed = value;
              OnPropertyChanged();
            }
        }

        // Rotation speed
        private string _rotationSpeed;
        public string RotationSpeed
        {
            get => _rotationSpeed;
            set
            {
              _rotationSpeed = value;
              OnPropertyChanged();
            }
        }

        // server URL
        private string _serverUrl;
        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                if (_serverUrl != value)
                {
                    _serverUrl = value;
                    OnPropertyChanged();
                }
            }
        }
        public async Task LoadSettings()
        {
            await Task.Delay(50);
            var settings =  await _api.GetSettings();
            Speed = settings.Speed.ToString();
            RotationSpeed = settings.RotationSpeed.ToString();
            SensorDistance = settings.SensorDistance.ToString();
            CollisionDistance = settings.CollisionDistance.ToString();
            // Preferences és SettingsService Current biztonságosan UI thread-en
            ServerUrl = Preferences.Get("server_url", "https://localhost:7116");
        }
        private async Task Save()
        {
            // Validate
            var (isValid, error) = await Validate();

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Error", error, "OK");
                return;
            }

            var settings = new RobotSettings
            {
                Speed = double.TryParse(Speed, out var s) ? s : 1,
                RotationSpeed = double.TryParse(RotationSpeed, out var r) ? r : 5,
                SensorDistance = double.Parse(SensorDistance),
                CollisionDistance = double.Parse(CollisionDistance)
            };

            // Save
            try
            {
                await _api.UpdateSettings(settings);

                SettingsService.Instance.Save(settings);
                Preferences.Set("server_url", ServerUrl);

                await Application.Current.MainPage.DisplayAlert("Success","Settings saved!","OK");

                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error","Failed to send settings:\n" + ex.Message,"OK");
            }
        }

        private async Task<(bool IsValid, string Error)> Validate()
        {
            if (!double.TryParse(Speed, out var speed) || speed < 1 || speed > 10)
                return (false, "Speed must be between 1 and 10");

            if (!double.TryParse(RotationSpeed, out var rot) || rot < 1 || rot > 45)
                return (false, "Rotation speed must be between 1 and 45");

            if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return (false, "Invalid server URL");

            if (!double.TryParse(SensorDistance, out var sd) || sd <= 0 || sd > 20)
                return (false, "Sensor distance must be between 1 and 20");


            if (!double.TryParse(CollisionDistance, out var cd) || cd <= 0 || cd > 5)
                return (false, "Collision distance must be between 0 and 5");

            if (sd < cd)
                return (false, "Collision distance must be less then the sensor distance");

            bool isAlive = await _api.CheckServerAvailable(ServerUrl);

            if (!isAlive)
                return (false, "Server is not reachable or not running");

            return (true, "");
        }

    }
}
