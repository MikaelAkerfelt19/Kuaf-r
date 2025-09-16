namespace Kuafor.Web.Services.Interfaces
{
    public interface IWhatsAppMediaService
    {
        Task<string> UploadMediaAsync(Stream mediaStream, string fileName, string contentType);
        Task<bool> SendImageAsync(string phoneNumber, string mediaUrl, string caption = "");
        Task<bool> SendDocumentAsync(string phoneNumber, string mediaUrl, string fileName, string caption = "");
        Task<bool> SendVideoAsync(string phoneNumber, string mediaUrl, string caption = "");
        Task<bool> SendAudioAsync(string phoneNumber, string mediaUrl);
        Task<bool> SendLocationAsync(string phoneNumber, double latitude, double longitude, string name = "", string address = "");
        Task<bool> SendContactAsync(string phoneNumber, string contactName, string contactPhone);
        Task<bool> SendInteractiveMessageAsync(string phoneNumber, string headerText, string bodyText, string footerText, List<WhatsAppButton> buttons);
    }

    public class WhatsAppButton
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "reply"; // reply, url, phone_number
        public string? Value { get; set; }
    }
}