using RobotDiagnostics.Models;

namespace RobotDiagnostics.Services
{
    public class RobotService
    {
        private double _sensorDistance = 5;
        private double _collisionDistance = 1;

        private RobotState _state = new RobotState
        {
            X = 0,
            Y = 0,
            Battery = 100,
        };

        public RobotState GetState()
        {
            _state.DistanceSensor = CalculateDistance(_sensorDistance);

            _state.SensorDistance = _sensorDistance;
            _state.CollisionDistance = _collisionDistance;

            return _state;
        }
        public void Rotate(double angle)
        {
            if (_state.Angle + angle >= 360)
                _state.Angle = _state.Angle + angle - 360;

            else if (_state.Angle + angle <= -360)
                _state.Angle = _state.Angle + angle + 360;

            else
                _state.Angle = _state.Angle + angle;

            _state.RotationSpeed = Math.Abs(angle);
        }
        public void Move(double distance)
        {
            double angleRad = _state.Angle * Math.PI / 180.0;

            _state.X += Math.Cos(angleRad) * distance * _state.Speed / 20;
            _state.Y += Math.Sin(angleRad) * distance * _state.Speed / 20;

            _state.Battery--;
            _state.Status = "Moving";
        }
        public string SelfTest()
        {
            _state.Status = "Testing...";
            Thread.Sleep(1000);
            _state.Status = "OK";
            return "Self test OK";
        }

        private double CalculateDistance(double viewDistance = 5)
        {
            double minDistance = double.MaxValue;

            double angleRad = _state.Angle * Math.PI / 180.0;
            double dirX = Math.Cos(angleRad);
            double dirY = Math.Sin(angleRad);

            double maxAngle = Math.Cos(30 * Math.PI / 180.0); // látószög 30 fok fél oldal

            foreach (var obs in _obstacles)
            {
                var dx = obs.X - _state.X;
                var dy = obs.Y - _state.Y;
                var dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist == 0) continue;

                var dot = (dx * dirX + dy * dirY) / dist;
                if (dot < maxAngle) continue;

                // csak akkor vegyük figyelembe, ha a látótávolságon belül van
                if (dist <= viewDistance && dist < minDistance)
                {
                    minDistance = dist;
                    if (dist <= _collisionDistance)
                    {
                        _state.Status = "Eat";
                    }
                }
            }

            return minDistance == double.MaxValue ? viewDistance : minDistance;
        }

        private List<Obstacle> _obstacles = new()
        {
            /*new Obstacle { X = 5, Y = 5 },
            new Obstacle { X = 10, Y = 3 },
            new Obstacle { X = 2, Y = 8 }*/
        };
        public void AddObstacle(Obstacle o)
        {
            _obstacles.Add(o);
        }
        public List<Obstacle> GetObstacles()
        {
            return _obstacles;
        }
        public void ClearObstacles()
        {
            _obstacles.Clear();
        }

        public void RemoveCollidedObstacle(RobotState state, double collisionDistance = 0.5)
        {
            /*_obstacles = _obstacles
                .Where(o =>
                {
                    var dx = o.X - state.X;
                    var dy = o.Y - state.Y;
                    var dist = Math.Sqrt(dx * dx + dy * dy);
                    return dist > collisionDistance;
                })
                .ToList();*/
            Obstacle closest = null;
            double minDist = double.MaxValue;

            foreach (var o in _obstacles)
            {
                var dx = o.X - state.X;
                var dy = o.Y - state.Y;
                var dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < minDist)
                {
                    minDist = dist;
                    closest = o;
                }
            }

            // csak akkor törlünk, ha tényleg ütközés van
            if (closest != null && minDist <= collisionDistance)
            {
                _obstacles.Remove(closest);
            }
        }

        public void UpdateSettings(double speed, double rotationSpeed, double sensorDistance, double collisionDistance)
        {
            _state.Speed = speed;
            _state.RotationSpeed = rotationSpeed;
            _sensorDistance = sensorDistance;
            _collisionDistance = collisionDistance;
        }

        public void Charge()
        {
            _state.Battery = 100;
            _state.Status = "Charging complete";
        }


    }
}
