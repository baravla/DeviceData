using Xunit;
using DeviceDataBackend.Services;
using DeviceDataModels;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DeviceDataTests
{
    public class DeviceDataStoreTests
    {
        private DeviceDataStore _store;

        public DeviceDataStoreTests()
        {
            _store = new DeviceDataStore();
        }

        [Fact]
        public async System.Threading.Tasks.Task AddDevicePacket_ShouldStorePacket()
        {
            var packet = new DevicePacket
            {
                PatientId = "P1",
                Source = "DeviceA",
                Timestamp = DateTime.UtcNow,
                Parameters = new List<DeviceParameter> { new DeviceParameter { Name = "HR", Value = 80 } }
            };

            await _store.AddDevicePacketAsync(packet);

            var all = (await _store.GetDevicePacketsAsync()).ToList();
            Assert.Contains(packet, all);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetDevicePackets_ShouldFilterByPatientId()
        {
            var now = DateTime.UtcNow;
            var packet1 = new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = now };
            var packet2 = new DevicePacket { PatientId = "P2", Source = "D1", Timestamp = now };

            await _store.AddDevicePacketAsync(packet1);
            await _store.AddDevicePacketAsync(packet2);

            var filtered = (await _store.GetDevicePacketsAsync(patientId: "P1")).ToList();
            Assert.Single(filtered);
            Assert.Equal("P1", filtered.First().PatientId);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetDevicePackets_ShouldFilterByTimeRange()
        {
            var t1 = DateTime.UtcNow.AddMinutes(-10);
            var t2 = DateTime.UtcNow.AddMinutes(-5);
            var t3 = DateTime.UtcNow;

            await _store.AddDevicePacketAsync(new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = t1 });
            await _store.AddDevicePacketAsync(new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = t2 });
            await _store.AddDevicePacketAsync(new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = t3 });

            var filtered = (await _store.GetDevicePacketsAsync(filterFrom: t1.AddMinutes(1), filterTo: t3.AddMinutes(-1))).ToList();
            Assert.Single(filtered);
            Assert.Equal(t2, filtered.First().Timestamp);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetDevicePackets_ShouldFilterByParameters()
        {
            var now = DateTime.UtcNow;
            var packet1 = new DevicePacket
            {
                PatientId = "P1",
                Source = "D1",
                Timestamp = now,
                Parameters = new List<DeviceParameter> { new DeviceParameter { Name = "HR", Value = 80 } }
            };

            var packet2 = new DevicePacket
            {
                PatientId = "P1",
                Source = "D1",
                Timestamp = now,
                Parameters = new List<DeviceParameter> { new DeviceParameter { Name = "HR", Value = 100 } }
            };

            await _store.AddDevicePacketAsync(packet1);
            await _store.AddDevicePacketAsync(packet2);

            var filtered = (await _store.GetDevicePacketsAsync(
                parameterName: "HR", minValue: 90, maxValue: 110
            )).ToList();

            Assert.Single(filtered);
            Assert.Equal(100d, filtered.First().Parameters.First().Value);
        }
    }
}
