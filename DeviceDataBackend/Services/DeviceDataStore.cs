using DeviceDataModels;
using Microsoft.Extensions.Caching.Memory;

namespace DeviceDataBackend.Services {
    public class DeviceDataStore : IDeviceDataStore {
        MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public void AddDevicePacket(DevicePacket packet) {
            var key = $"{packet.Source}_{packet.Timestamp:O}";
            _cache.Set(key, packet);
        }
        
        public IEnumerable<DevicePacket> GetDevicePackets(Func<DevicePacket, bool> filter) {
            var results = new List<DevicePacket>();
            foreach (var key in _cache.Keys) {
                if (_cache.TryGetValue(key, out var value) &&
                    value is DevicePacket packet &&
                    filter(packet)) 
                {
                    results.Add(packet);
                }
            }

            return results;
        }
    }
}
