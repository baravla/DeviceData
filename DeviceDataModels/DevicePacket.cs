namespace DeviceDataModels {
    public class DevicePacket {
        public string Source { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string PatientId { get; set; } = null!;
        public List<DeviceParameter> Parameters { get; set; } = new();
    }
}
