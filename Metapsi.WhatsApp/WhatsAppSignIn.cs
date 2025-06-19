using System.Threading.Tasks;

namespace Metapsi.WhatsApp;

public static partial class WhatsAppClientExtensions
{
    public static async Task SendSignInCode(
        this WhatsAppServiceClient whatsAppServiceClient,
        string phoneNumber,
        string accessCode,
        string languageCode)
    {
        await whatsAppServiceClient.PostMessage(
            WhatsAppMessage.Template(
                phoneNumber,
                "otp_code",
                languageCode,
                new Metapsi.WhatsApp.Messages.ComponentObject(
                    "body",
                    new Metapsi.WhatsApp.Messages.ParameterObject()
                    {
                        type = "text",
                        text = accessCode
                    }),
                new Metapsi.WhatsApp.Messages.ComponentObject(
                    "button",
                    new Metapsi.WhatsApp.Messages.ParameterObject()
                    {
                        type = "text",
                        text = accessCode
                    })
                {
                    sub_type = "url",
                    index = "0",
                }));
    }
}