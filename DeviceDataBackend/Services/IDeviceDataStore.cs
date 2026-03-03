using DeviceDataModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceDataBackend.Services {
    public interface IDeviceDataStore {
        Task AddDevicePacketAsync(DevicePacket packet);
        Task<IEnumerable<DevicePacket>> GetDevicePacketsAsync(
            Func<DevicePacket, bool> packetFilter,
            IEnumerable<Func<DeviceParameter, bool>>? parameterFilters = null);
    }
}
