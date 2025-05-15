namespace CoreLib.Models.Dtos.Device.Add
{
    internal class NewDeviceRequest
    {
        public string TrustedDeviceId { get; set; }
        public string Payload { get; set; }
        public string TrustedSignature { get; set; } // AES GCM encrypted signature of the payload
        public string PayloadHash { get; set; }
        public string TempId { get; set; } //Shake256 hash of aes key
        public long Timestamp { get; set; } 
    }
}
