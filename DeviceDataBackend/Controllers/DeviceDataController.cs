using DeviceDataBackend.Services;
using DeviceDataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeviceDataBackend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceDataController : ControllerBase {

        private readonly IDeviceDataStore _cache;

        public DeviceDataController(IDeviceDataStore cache) {
            _cache = cache;
        }

        [HttpPost]
        public IActionResult Add([FromBody] DevicePacket packet) {
            _cache.AddDevicePacket(packet);
            return Ok();
        }

        [HttpGet]
        public IActionResult Get(
                [FromQuery] string? patientId,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? to,
                [FromQuery] string[]? parameterName,
                [FromQuery] double[]? minValue,
                [FromQuery] double[]? maxValue) 
        {
            Func<DevicePacket, bool> packetFilter = p =>
                (string.IsNullOrEmpty(patientId) || p.PatientId == patientId) &&
                (!from.HasValue || p.Timestamp >= from.Value) &&
                (!to.HasValue || p.Timestamp <= to.Value);

            List<Func<DeviceParameter, bool>>? parameterFilters = null;

            if (parameterName != null && parameterName.Length > 0) {
                parameterFilters = new List<Func<DeviceParameter, bool>>();

                for (int i = 0; i < parameterName.Length; i++) {
                    string name = parameterName[i];
                    double? min = (minValue != null && i < minValue.Length) ? minValue[i] : (double?)null;
                    double? max = (maxValue != null && i < maxValue.Length) ? maxValue[i] : (double?)null;

                    parameterFilters.Add(param =>
                        param.Name == name &&
                        (!min.HasValue || param.Value >= min.Value) &&
                        (!max.HasValue || param.Value <= max.Value)
                    );
                }
            }

            var results = _cache.GetDevicePackets(packetFilter, parameterFilters).ToList();

            return Ok(results);
        }
    }
}
