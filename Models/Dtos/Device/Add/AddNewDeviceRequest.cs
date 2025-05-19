namespace CoreLib.Models.Dtos.Device.Add
{
    internal class AddNewDeviceRequest
    {
        public byte[] DevicePayload { get; set; }

        public byte[] DeviceTrustedSignature { get; set; }

        public List<AddNewDevicePreKeysRequest> PreKeysPayload { get; set; }
    }
}
