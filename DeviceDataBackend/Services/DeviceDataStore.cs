using DeviceDataModels;
using System.Collections.Concurrent;

namespace DeviceDataBackend.Services {
    public class DeviceDataStore : IDeviceDataStore {
        private readonly ConcurrentDictionary<string, DevicePacket> _store = new();

        public void AddDevicePacket(DevicePacket packet) {
            if (packet is null)
                throw new ArgumentNullException(nameof(packet));

            if (string.IsNullOrWhiteSpace(packet.PatientId))
                throw new ArgumentException("PatientId must be provided", nameof(packet.PatientId));

            if (string.IsNullOrWhiteSpace(packet.Source))
                throw new ArgumentException("Source must be provided", nameof(packet.Source));

            // Ensure Parameters collection is initialized to avoid null refs elsewhere
            packet.Parameters ??= new List<DeviceParameter>();

            // Use an immutable snapshot key using timestamp ISO format
            var key = $"{packet.Source}_{packet.Timestamp:O}";
            _store[key] = packet;
        }

        public IEnumerable<DevicePacket> GetDevicePackets(
            Func<DevicePacket, bool> packetFilter,
            IEnumerable<Func<DeviceParameter, bool>>? parameterFilters = null) {
            if (packetFilter is null)
                throw new ArgumentNullException(nameof(packetFilter));

            var results = new List<DevicePacket>();

            // Take a snapshot of values to avoid issues if collection is modified concurrently
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

            return results;
        }

        // Convenience overloads used by tests and simple callers
        public IEnumerable<DevicePacket> GetDevicePackets()
            => GetDevicePackets(p => true, null);

        public IEnumerable<DevicePacket> GetDevicePackets(
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

            return GetDevicePackets(packetFilter, parameterFilters);
        }
    }
}
