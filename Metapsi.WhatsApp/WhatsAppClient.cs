using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Metapsi.WhatsApp;

public class WhatsAppClient
{
    public WhatsAppClient() { }
    public WhatsAppClient(string baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; set; }
    public HttpClient HttpClient { get; set; } = new HttpClient();
}

public static class WhatsAppClientExtensions
{
    public static async Task PostMessage<T>(this WhatsAppClient client, T message)
        where T : IWhatsAppOutboundMessage
    {
        var postTextUrl = client.BaseUrl.TrimEnd('/') + "/postmessage/" + typeof(T).Name;
        var response = await client.HttpClient.PostAsJsonAsync(postTextUrl, message);
        response.EnsureSuccessStatusCode();
    }

    public static async Task PostMessage(this WhatsAppClient client, string toPhoneNumber, string text)
    {
        await client.PostMessage(WhatsAppMessage.Text(toPhoneNumber, text));
    }
}
