using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Metapsi.WhatsApp;

public class MediaUrl
{
    public string messaging_product { get; set; }
    public string url { get; set; }
    public string mime_type { get; set; }
    public string sha256 { get; set; }
    public long file_size { get; set; }
    public string id { get; set; }
}

/// <summary>
/// Performs HTTP requests directly to the WhatsApp Cloud API
/// </summary>
public class WhatsAppCloudApiClient
{
    public HttpClient HttpClient { get; set; }
    public string BearerToken { get; set; }
    public string DefaultBusinessNumberId { get; set; }
    public string ApiVersion { get; set; } = "v18.0";

    public WhatsAppCloudApiClient(HttpClient httpClient, string bearerToken, string businessNumberId)
    {
        this.HttpClient = httpClient;
        this.BearerToken = bearerToken;
        this.DefaultBusinessNumberId = businessNumberId;
    }

    public WhatsAppCloudApiClient(string bearerToken, string businessNumberId)
    {
        this.HttpClient = new HttpClient();
        this.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
        this.BearerToken = bearerToken;
        this.DefaultBusinessNumberId = businessNumberId;
    }
}

public static class WhatsAppCloudApiExtensions
{
    public static async Task<PostMessageResponse> PostMessage(this WhatsAppCloudApiClient apiClient, MessageObject message, string businessNumberId = null)
    {
        if (string.IsNullOrEmpty(businessNumberId))
            businessNumberId = apiClient.DefaultBusinessNumberId;

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.facebook.com/{apiClient.ApiVersion}/{businessNumberId}/messages");
        request.Headers.Add("Authorization", "Bearer " + apiClient.BearerToken);
        request.Content = JsonContent.Create(message);

        var postResult = await apiClient.HttpClient.SendAsync(request);

        var resultBody = await postResult.Content.ReadAsStringAsync();
        Console.WriteLine(resultBody);

        var result = Metapsi.Serialize.FromJson<PostMessageResponse>(resultBody);
        return result;
    }

    public static async Task<MediaUrl> GetMediaUrl(this WhatsAppCloudApiClient apiClient, string mediaId)
    {
        var getMediaUrlRequest = new HttpRequestMessage(HttpMethod.Get, $"https://graph.facebook.com/{apiClient.ApiVersion}/{mediaId}");
        getMediaUrlRequest.Headers.Add("Authorization", "Bearer " + apiClient.BearerToken);

        var getMediaUrlResult = await apiClient.HttpClient.SendAsync(getMediaUrlRequest);
        getMediaUrlResult.EnsureSuccessStatusCode();

        var resultBody = await getMediaUrlResult.Content.ReadAsStringAsync();
        Console.WriteLine(resultBody);

        var mediaUrl = Metapsi.Serialize.FromJson<MediaUrl>(resultBody);
        return mediaUrl;
    }

    public static async Task<HttpResponseMessage> GetMedia(this WhatsAppCloudApiClient apiClient, string mediaId)
    {
        var mediaUrl = await apiClient.GetMediaUrl(mediaId);

        var getMediaContentRequest = new HttpRequestMessage(HttpMethod.Get, mediaUrl.url);
        getMediaContentRequest.Headers.Add("Authorization", "Bearer " + apiClient.BearerToken);

        var getMediaContentResult = await apiClient.HttpClient.SendAsync(getMediaContentRequest);
        getMediaContentResult.EnsureSuccessStatusCode();

        return getMediaContentResult;
    }

    public static void EnsureSuccessfulResponse(this PostMessageResponse response)
    {
        if (HasError(response))
        {
            throw new Exception(Metapsi.Serialize.ToJson(response.error));
        }
    }

    public static bool HasError(this PostMessageResponse response)
    {
        if (response.error != null)
        {
            if (!string.IsNullOrEmpty(response.error.message))
            {
                return true;
            }
        }

        return false;
    }
}