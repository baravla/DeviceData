using DeviceDataModels;

namespace DeviceDataBackend.Services {
    public interface IDeviceDataStore {
        void AddDevicePacket(DevicePacket packet);
        IEnumerable<DevicePacket> GetDevicePackets(Func<DevicePacket, bool> filter);
    }
}
