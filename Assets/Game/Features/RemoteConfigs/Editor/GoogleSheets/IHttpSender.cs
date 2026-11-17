using System.Threading.Tasks;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public interface IHttpSender
    {
        Task<HttpResponse> Send(string method, string url, string body, string contentType, string accessToken);
    }
}
