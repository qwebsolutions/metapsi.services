using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Metapsi.WhatsApp;

public class WhatsAppServiceClient
{
    public WhatsAppServiceClient() { }
    public WhatsAppServiceClient(string baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; set; }
    public HttpClient HttpClient { get; set; } = new HttpClient();
}

public class GetMediaResponse
{
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
}

public class UploadMediaRequest
{
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
    public string FilePath { get; set; }
}

public static class WhatsAppClientExtensions
{
    public static async Task<PostMessageResponse> PostMessage(this WhatsAppServiceClient client, MessageObject message)
    {
        var postTextUrl = client.BaseUrl.TrimEnd('/') + $"/{WhatsAppServiceExtensions.PostMessagePath}";
        var response = await client.HttpClient.PostAsJsonAsync(postTextUrl, message);
        response.EnsureSuccessStatusCode();
        var stringContent = await response.Content.ReadAsStringAsync();
        var result = Metapsi.Serialize.FromJson<PostMessageResponse>(stringContent);
        return result;
    }

    public static async Task<PostMessageResponse> PostMessage(this WhatsAppServiceClient client, string toPhoneNumber, string text)
    {
        return await client.PostMessage(WhatsAppMessage.Text(toPhoneNumber, text));
    }

    public static async Task<UploadMediaResponse> UploadMedia(this WhatsAppServiceClient client, byte[] content, string contentType, string filePath)
    {
        var postTextUrl = client.BaseUrl.TrimEnd('/') + $"/{WhatsAppServiceExtensions.UploadMediaPath}";
        var response = await client.HttpClient.PostAsJsonAsync(postTextUrl, new UploadMediaRequest()
        {
            Content = content,
            ContentType = contentType,
            FilePath = filePath
        });

        response.EnsureSuccessStatusCode();
        var stringContent = await response.Content.ReadAsStringAsync();
        var result = Metapsi.Serialize.FromJson<UploadMediaResponse>(stringContent);
        return result;
    }

    public static async Task<GetMediaResponse> GetMedia(this WhatsAppServiceClient client, string mediaId)
    {
        var getMediaUrl = client.BaseUrl.TrimEnd('/') + $"/{WhatsAppServiceExtensions.GetMediaPath}/" + mediaId;
        var response = await client.HttpClient.GetAsync(getMediaUrl);
        response.EnsureSuccessStatusCode();
        using MemoryStream memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);
        await memoryStream.FlushAsync();
        var content = memoryStream.ToArray();
        return new GetMediaResponse()
        {
            Content = content,
            ContentType = response.Content.Headers.ContentType.MediaType
        };
    }
}
