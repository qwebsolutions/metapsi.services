using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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


public class MediaType
{
    public string Extension { get; set; }
    public string MimeType { get; set; }

    public static class Audio
    {
        public static MediaType AAC = new() { Extension = ".aac", MimeType = "audio/aac" };
        public static MediaType AMR = new() { Extension = ".amr", MimeType = "audio/amr" };
        public static MediaType MP3 = new() { Extension = ".mp3", MimeType = "audio/mpeg" };
        public static MediaType MP4 = new() { Extension = ".m4a", MimeType = "audio/mp4" };
        public static MediaType OGG = new() { Extension = ".ogg", MimeType = "audio/ogg" };
    }

    public static class Document
    {
        public static MediaType Text = new() { Extension = ".txt", MimeType = "text/plain" };
        public static MediaType Excel = new() { Extension = ".xls", MimeType = "application/vnd.ms-excel" };
        public static MediaType ExcelX = new() { Extension = ".xlsx", MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
        public static MediaType Word = new() { Extension = ".doc", MimeType = "application/msword" };
        public static MediaType WordX = new() { Extension = ".docx", MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        public static MediaType PowerPoint = new() { Extension = ".ppt", MimeType = "application/vnd.ms-powerpoint" };
        public static MediaType PowerPointX = new() { Extension = ".pptx", MimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation" };
        public static MediaType PDF = new() { Extension = ".pdf", MimeType = "application/pdf" };
    }

    public static class Image
    {
        public static MediaType JPEG = new() { Extension = ".jpeg", MimeType = "image/jpeg" };
        public static MediaType PNG = new() { Extension = ".png", MimeType = "image/png" };
    }

    public static class Sticker
    {
        public static MediaType WEBP = new() { Extension = ".webp", MimeType = "image/webp" };
    }

    public static class Video
    {
        public static MediaType _3GPP = new() { Extension = ".3gp", MimeType = "video/3gp" };
        public static MediaType MP4 = new() { Extension = ".mp4", MimeType = "video/mp4" };
    }

    public static string GetContentType(string fileExtension)
    {
        return SupportedTypes.FirstOrDefault(x => x.Extension == fileExtension)?.MimeType;
    }

    public static List<MediaType> SupportedAudioTypes =
        new List<MediaType>()
        {
            Audio.AAC,
            Audio.AMR,
            Audio.MP3,
            Audio.MP4,
            Audio.OGG
        };

    public static List<MediaType> SupportedDocumentTypes =
        new List<MediaType>()
        {
            Document.Text,
            Document.Excel,
            Document.ExcelX,
            Document.Word,
            Document.WordX,
            Document.PowerPoint,
            Document.PowerPointX,
            Document.PDF
        };

    public static List<MediaType> SupportedImageTypes =
        new List<MediaType>()
        {
            Image.JPEG,
            Image.PNG,
        };

    public static List<MediaType> SupportedVideoTypes =
        new List<MediaType>()
        {
            Video._3GPP,
            Video.MP4
        };

    public static List<MediaType> SupportedStickerTypes =
        new List<MediaType>()
        {
            Sticker.WEBP
        };

    public static List<MediaType> SupportedTypes =
        new List<MediaType>()
        {
            Audio.AAC,
            Audio.AMR,
            Audio.MP3,
            Audio.MP4,
            Audio.OGG,
            Document.Text,
            Document.Excel,
            Document.ExcelX,
            Document.Word,
            Document.WordX,
            Document.PowerPoint,
            Document.PowerPointX,
            Document.PDF,
            Image.JPEG,
            Image.PNG,
            Sticker.WEBP,
            Video._3GPP,
            Video.MP4
        };
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

        string jsonString = Metapsi.Serialize.ToJson(message);

        // Create StringContent with UTF8 encoding and application/json media type
        request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
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

    public static async Task<UploadMediaResponse> UploadMedia(
        this WhatsAppCloudApiClient apiClient,
        byte[] media,
        string contentType,
        string filePath,
        string businessNumberId = null)
    {
        if (string.IsNullOrEmpty(businessNumberId))
            businessNumberId = apiClient.DefaultBusinessNumberId;

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.facebook.com/{apiClient.ApiVersion}/{businessNumberId}/media");
        request.Headers.Add("Authorization", "Bearer " + apiClient.BearerToken);

        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent(media);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent(contentType), "type");
            content.Add(new StringContent("whatsapp"), "messaging_product");

            request.Content = content;

            var postResult = await apiClient.HttpClient.SendAsync(request);

            var resultBody = await postResult.Content.ReadAsStringAsync();
            Console.WriteLine(resultBody);

            var result = Metapsi.Serialize.FromJson<UploadMediaResponse>(resultBody);
            return result;
        }
    }

    public static void EnsureSuccessfulResponse(this ICloudApiResponse response)
    {
        if (HasError(response))
        {
            throw new Exception(Metapsi.Serialize.ToJson(response.error));
        }
    }

    public static bool HasError(this ICloudApiResponse response)
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