using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using VadkanRobotClient.Models;

namespace VadkanRobotClient.Services
{
    public class RobotApiService
    {
        private HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5116")
        };

        public async Task<RobotState> GetStateAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<RobotState>("/api/Robot/state");
            }
            catch (TaskCanceledException)
            {
                // timeout
                await ShowError("Timeout! Robot is not answering!");
                Preferences.Clear();
            }
            catch (HttpRequestException)
            {
                // nincs kapcsolat / rossz URL
                await ShowError("Bot server is not accessible!");
            }
            catch (Exception ex)
            {
                await ShowError("Unknown error: " + ex.Message);
            }

            return null;
        }

        private async Task ShowError(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            });
        }
        public async Task MoveForward(double distance)
        {
            await _http.PostAsync($"/api/Robot/move?distance={distance}", null);
        }

        public async Task Rotate(double angle)
        {
            await _http.PostAsJsonAsync("/api/Robot/rotate", new { angle });
        }

        public async Task get(double angle)
        {
            await _http.PostAsJsonAsync("/api/Robot/rotate", new { angle });
        }

        private List<Obstacle> _obstacles = new();
        public async Task<List<Obstacle>> GetObstacles()
        {
            return await _http.GetFromJsonAsync<List<Obstacle>>("api/Robot/obstacles");
        }
        public async Task AddObstacle(Obstacle obstacle)
        {
            await _http.PostAsJsonAsync("api/Robot/obstacles", obstacle);
        }

        // Clear all obstacles
        public async Task ClearObstacles()
        {
            await _http.DeleteAsync("api/Robot/obstacles");
        }

        // Remove obstacles near robot after collision
        public async Task RemoveCollidedObstacle(RobotState state, double collisionDistance = 0.5)
        {
            // 1. Létrehozunk egy HttpRequestMessage-et DELETE metódussal
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Robot/obstacles/collided?collisionDistance={collisionDistance}")
            {
                Content = JsonContent.Create(state) // 2. A robot állapotát JSON-ként küldjük
            };

            // 3. Küldjük a kérés a szervernek
            var response = await _http.SendAsync(request);

            // 4. Ellenőrizzük, hogy sikeres volt-e a törlés
            response.EnsureSuccessStatusCode();

            // 5. Frissítjük a lokális akadálylistát a szerver állapotával
            _obstacles = await _http.GetFromJsonAsync<List<Obstacle>>("api/Robot/obstacles");
        }

        public async Task LoadObstacles()
        {
            _obstacles = await _http.GetFromJsonAsync<List<Obstacle>>("api/Robot/obstacles");
        }

        public async Task<RobotSettings> GetSettings()
        {
            return await _http.GetFromJsonAsync<RobotSettings>("api/Robot/settings");
        }
        public async Task UpdateSettings(RobotSettings dto)
        {
            await _http.PostAsJsonAsync("api/Robot/settings", dto);
        }

        public async Task<bool> CheckServerAvailable(string url)
        {
            try
            {
                using var http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(2) // ne fagyjon
                };

                var response = await http.GetAsync(url + "/api/Robot/state");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task Charge()
        {
            await _http.PostAsync("/api/Robot/charge", null);
        }

        public RobotApiService()
        {
            
            var url = Preferences.Get("server_url", "http://localhost:5116");

            _http = new HttpClient
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(2)
            };
        }

        public async Task<string> SelfTest()
        {
            var response = await _http.PostAsync("api/robot/selftest", null);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

    }
}
