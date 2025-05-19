using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        var postTextUrl = client.BaseUrl.TrimEnd('/') + $"/{WhatsAppService.PostMessagePath}";
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
        var postTextUrl = client.BaseUrl.TrimEnd('/') + $"/{WhatsAppService.UploadMediaPath}";
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
        var getMediaUrl = client.BaseUrl.TrimEnd('/') + $"/{WhatsAppService.GetMediaPath}/" + mediaId;
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

    private static Task<HttpResponseMessage> PostAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        // Serialize the object to JSON
        string json = System.Text.Json.JsonSerializer.Serialize(value);

        // Create StringContent with JSON and correct media type
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Send POST request with the JSON content
        return client.PostAsync(requestUri, content, cancellationToken);
    }
}
