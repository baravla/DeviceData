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
        public void AddDevicePacket_ShouldStorePacket()
        {
            var packet = new DevicePacket
            {
                PatientId = "P1",
                Source = "DeviceA",
                Timestamp = DateTime.UtcNow,
                Parameters = new List<DeviceParameter> { new DeviceParameter { Name = "HR", Value = 80 } }
            };

            _store.AddDevicePacket(packet);

            var all = _store.GetDevicePackets().ToList();
            Assert.Contains(packet, all);
        }

        [Fact]
        public void GetDevicePackets_ShouldFilterByPatientId()
        {
            var now = DateTime.UtcNow;
            var packet1 = new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = now };
            var packet2 = new DevicePacket { PatientId = "P2", Source = "D1", Timestamp = now };

            _store.AddDevicePacket(packet1);
            _store.AddDevicePacket(packet2);

            var filtered = _store.GetDevicePackets(patientId: "P1").ToList();
            Assert.Single(filtered);
            Assert.Equal("P1", filtered.First().PatientId);
        }

        [Fact]
        public void GetDevicePackets_ShouldFilterByTimeRange()
        {
            var t1 = DateTime.UtcNow.AddMinutes(-10);
            var t2 = DateTime.UtcNow.AddMinutes(-5);
            var t3 = DateTime.UtcNow;

            _store.AddDevicePacket(new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = t1 });
            _store.AddDevicePacket(new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = t2 });
            _store.AddDevicePacket(new DevicePacket { PatientId = "P1", Source = "D1", Timestamp = t3 });

            var filtered = _store.GetDevicePackets(filterFrom: t1.AddMinutes(1), filterTo: t3.AddMinutes(-1)).ToList();
            Assert.Single(filtered);
            Assert.Equal(t2, filtered.First().Timestamp);
        }

        [Fact]
        public void GetDevicePackets_ShouldFilterByParameters()
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

            _store.AddDevicePacket(packet1);
            _store.AddDevicePacket(packet2);

            var filtered = _store.GetDevicePackets(
                parameterName: "HR", minValue: 90, maxValue: 110
            ).ToList();

            Assert.Single(filtered);
            Assert.Equal(100d, filtered.First().Parameters.First().Value);
        }
    }
}
