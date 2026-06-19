using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VadkanRobotClient.Models
{
    public class RobotState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Battery { get; set; }
        public double Temperature { get; set; }
        public string Status { get; set; }
        public double DistanceSensor { get; set; }
        public double Angle { get; set; }
        public double SensorDistance { get; set; }
        public double CollisionDistance { get; set; }
    }
}
