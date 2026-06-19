using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VadkanRobotClient.Models
{
    public class RobotSettings
    {
        public double Speed { get; set; } = 1.0;         // Move distance
        public double RotationSpeed { get; set; } = 5.0; // Rotate degrees
        public double SensorDistance { get; set; } = 5.0;
        public double CollisionDistance { get; set; } = 1.0;
    }
}
