namespace RobotDiagnostics.Models
{
    public class RobotState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Battery { get; set; }
        public double DistanceSensor { get; set; }
        public string Status { get; set; } = "Idle";
        public double Angle { get; set; }
        public double Speed { get; set; } = 1;
        public double RotationSpeed { get; set; } = 5;

        public double SensorDistance { get; set; } = 5;
        public double CollisionDistance { get; set; } = 1;
    }
}
