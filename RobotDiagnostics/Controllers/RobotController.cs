using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotDiagnostics.Models;
using RobotDiagnostics.Services;

namespace RobotDiagnostics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RobotController : ControllerBase
    {
        private readonly RobotService _service;

        public RobotController(RobotService service)
        {
            _service = service;
        }
        public class RobotSettings
        {
            public double Speed { get; set; }
            public double RotationSpeed { get; set; }
            public double SensorDistance { get; set; }
            public double CollisionDistance { get; set; }
        }

        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            return Ok(new RobotSettings
            {
                Speed = _service.GetState().Speed,
                RotationSpeed = _service.GetState().RotationSpeed,
                SensorDistance = _service.GetState().SensorDistance,
                CollisionDistance = _service.GetState().CollisionDistance
            });
        }

        [HttpPost("settings")]
        public IActionResult UpdateSettings([FromBody] RobotSettings settings)
        {
            _service.UpdateSettings(
                settings.Speed,
                settings.RotationSpeed,
                settings.SensorDistance,
                settings.CollisionDistance);

            return Ok();
        }

        [HttpGet("state")]
        public IActionResult GetState()
        {
            return Ok(_service.GetState());
        }

        [HttpPost("move")]
        public IActionResult Move([FromQuery] double distance)
        {
            _service.Move(distance);
            return Ok();
        }

        [HttpPost("selftest")]
        public IActionResult SelfTest()
        {
            return Ok(_service.SelfTest());
        }

        [HttpDelete("logs")]
        public IActionResult ClearLogs()
        {
            return Ok();
        }

        [HttpPost("rotate")]
        public IActionResult Rotate([FromBody] RotateCommand cmd)
        {
            _service.Rotate(cmd.Angle);
            return Ok();
        }

        [HttpGet("obstacles")]
        public IActionResult GetObstacles()
        {
            return Ok(_service.GetObstacles());
        }

        [HttpPost("obstacles")]
        public IActionResult AddObstacle([FromBody] Obstacle obstacle)
        {
            _service.AddObstacle(obstacle);

            return Ok();
        }

        [HttpDelete("obstacles")]
        public IActionResult ClearObstacles()
        {
            _service.ClearObstacles();
            return Ok();
        }

        [HttpDelete("obstacles/collided")]
        public IActionResult RemoveCollidedObstacle([FromBody] RobotState state, [FromQuery] double collisionDistance = 0.5)
        {
            _service.RemoveCollidedObstacle(state, collisionDistance);
            return Ok();
        }

        [HttpPost("charge")]
        public IActionResult Charge()
        {
            _service.Charge();
            return Ok();
        }
    }
}
