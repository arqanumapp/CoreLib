namespace CoreLib.Models.Dtos.Device.Add
{
    internal class NewDevicePayload
    {
        public string AccountId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string SPK { get; set; }
        public string SPrK { get; set; }
        public string SPKSignature { get; set; }
        public List<NewDevicePreKey> PreKeys { get; set; }
    }
}
