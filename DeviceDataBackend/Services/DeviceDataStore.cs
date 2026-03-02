using DeviceDataModels;
using System.Collections.Concurrent;

namespace DeviceDataBackend.Services {
    public class DeviceDataStore : IDeviceDataStore {
        private readonly ConcurrentDictionary<string, DevicePacket> _store = new();

        public void AddDevicePacket(DevicePacket packet) {
            var key = $"{packet.Source}_{packet.Timestamp:O}";
            _store[key] = packet;
        }

        public IEnumerable<DevicePacket> GetDevicePackets(
            Func<DevicePacket, bool> packetFilter,
            IEnumerable<Func<DeviceParameter, bool>>? parameterFilters = null) {
            var results = new List<DevicePacket>();

            foreach (var packet in _store.Values) {
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
    }
}
