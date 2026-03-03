using DeviceDataModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceDataBackend.Services {
    public class DeviceDataStore : IDeviceDataStore {
        private readonly ConcurrentDictionary<string, DevicePacket> _store = new();
        private static long _globalSequence;
        public Task AddDevicePacketAsync(DevicePacket packet) {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));

            if (string.IsNullOrWhiteSpace(packet.PatientId))
                throw new ArgumentException("PatientId must be provided", nameof(packet.PatientId));

            if (string.IsNullOrWhiteSpace(packet.Source))
                throw new ArgumentException("Source must be provided", nameof(packet.Source));

            // let's make it not null by default 
            packet.Parameters ??= new List<DeviceParameter>();

            // generate new unique key for the packet 
            var seq = Interlocked.Increment(ref _globalSequence);
            var key = $"{packet.Source}_{packet.PatientId}_{packet.Timestamp:O}_{seq}";
            _store[key] = packet;

            return Task.CompletedTask;
        }

        public Task<IEnumerable<DevicePacket>> GetDevicePacketsAsync(
            Func<DevicePacket, bool> packetFilter,
            IEnumerable<Func<DeviceParameter, bool>>? parameterFilters = null) {
            if (packetFilter is null)
                throw new ArgumentNullException(nameof(packetFilter));

            var results = new List<DevicePacket>();

            // snaphot to avoid concurrent modification issues
            var snapshot = _store.Values.ToArray();

            foreach (var packet in snapshot) {
                if (!packetFilter(packet))
                    continue;

                bool match = true;

                if (parameterFilters != null) {
                    foreach (var pf in parameterFilters) {
                        if (!packet.Parameters.Any(pf)) {
                            match = false;
                            break;
                        }
                    }
                }

                if (match)
                    results.Add(packet);
            }

            return Task.FromResult<IEnumerable<DevicePacket>>(results);
        }

        // For tests 
        public Task<IEnumerable<DevicePacket>> GetDevicePacketsAsync()
            => GetDevicePacketsAsync(p => true, null);

        public Task<IEnumerable<DevicePacket>> GetDevicePacketsAsync(
            string? patientId = null,
            string? source = null,
            DateTime? filterFrom = null,
            DateTime? filterTo = null,
            string? parameterName = null,
            double? minValue = null,
            double? maxValue = null)
        {
            Func<DevicePacket, bool> packetFilter = p =>
                (string.IsNullOrEmpty(patientId) || p.PatientId == patientId) &&
                (string.IsNullOrEmpty(source) || p.Source == source) &&
                (!filterFrom.HasValue || p.Timestamp >= filterFrom.Value) &&
                (!filterTo.HasValue || p.Timestamp <= filterTo.Value);

            List<Func<DeviceParameter, bool>>? parameterFilters = null;

            if (!string.IsNullOrEmpty(parameterName)) {
                parameterFilters = new List<Func<DeviceParameter, bool>> {
                    param =>
                        param.Name == parameterName &&
                        (!minValue.HasValue || param.Value >= minValue.Value) &&
                        (!maxValue.HasValue || param.Value <= maxValue.Value)
                };
            }

            return GetDevicePacketsAsync(packetFilter, parameterFilters);
        }
}

}
