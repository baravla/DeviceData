using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Add([FromBody] DevicePacket packet) {
            try {
                if (packet == null)
                    return BadRequest("Packet payload is required.");

                if (string.IsNullOrWhiteSpace(packet.PatientId))
                    return BadRequest("PatientId is required.");

                if (string.IsNullOrWhiteSpace(packet.Source))
                    return BadRequest("Source is required.");

                if (packet.Timestamp == default)
                    packet.Timestamp = DateTime.UtcNow;

                await _cache.AddDevicePacketAsync(packet);
                return Ok();
            }
            catch (ArgumentException ex) {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) {
                // Log exception here when a logger is available
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(
                [FromQuery] string? patientId,
                [FromQuery] string? source,
                [FromQuery] DateTime? from,
                [FromQuery] DateTime? to,
                [FromQuery] string[]? parameterName,
                [FromQuery] double[]? minValue,
                [FromQuery] double[]? maxValue) 
        {
            try {
                if (from.HasValue && to.HasValue && from > to)
                    return BadRequest("'from' must be less than or equal to 'to'.");

                Func<DevicePacket, bool> packetFilter = p =>
                    (string.IsNullOrEmpty(patientId) || p.PatientId == patientId) &&
                    (string.IsNullOrEmpty(source) || p.Source == source) &&
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

                var results = (await _cache.GetDevicePacketsAsync(packetFilter, parameterFilters)).ToList();
                return Ok(results);
            }
            catch (Exception ex) {
                // Log exception when a logger is available
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing the request.");
            }
        }
    }
}
