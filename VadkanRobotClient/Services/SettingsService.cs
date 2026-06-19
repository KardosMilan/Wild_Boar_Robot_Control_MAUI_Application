using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VadkanRobotClient.Models;

namespace VadkanRobotClient.Services
{
    public class SettingsService
    {
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        public RobotSettings Current { get; private set; }

        public event Action SettingsChanged;

        private string path = Path.Combine(FileSystem.AppDataDirectory, "settings.json");

        public SettingsService()
        {
            Load();
        }

        public void Load()
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                Current = JsonSerializer.Deserialize<RobotSettings>(json);
            }

            Current ??= new RobotSettings
            {
                Speed = 1,
                RotationSpeed = 5,
            };
        }

        public void Save(RobotSettings settings)
        {
            Current = settings;

            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(path, json);

            SettingsChanged?.Invoke();
        }
    }
}
