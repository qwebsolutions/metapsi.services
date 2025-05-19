using System.Web;

namespace Metapsi.WhatsApp;

public static partial class WhatsAppService
{
    public static string GetChallengeQuery(this HttpRequest request)
    {
        var challenge = request.QueryString["hub.challenge"];
        return challenge;
    }

    public static void SetContentTypeHeader(this HttpResponse response, string contentType)
    {
        response.ContentType = contentType;
    }
}
