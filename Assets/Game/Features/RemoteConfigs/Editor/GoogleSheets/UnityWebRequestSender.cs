using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class UnityWebRequestSender : IHttpSender
    {
        private const string AuthorizationHeader = "Authorization";
        private const string ContentTypeHeader = "Content-Type";

        private readonly int _timeoutSeconds;

        public UnityWebRequestSender(int timeoutSeconds)
        {
            _timeoutSeconds = timeoutSeconds;
        }

        public async Task<HttpResponse> Send(string method, string url, string body, string contentType, string accessToken)
        {
            using var request = new UnityWebRequest(url, method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _timeoutSeconds;

            if (body != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.SetRequestHeader(ContentTypeHeader, contentType);
            }

            if (accessToken != null)
            {
                request.SetRequestHeader(AuthorizationHeader, $"Bearer {accessToken}");
            }

            var operation = request.SendWebRequest();

            while (operation.isDone == false)
            {
                await Task.Yield();
            }

            var isSuccess = request.result == UnityWebRequest.Result.Success;
            return new HttpResponse(isSuccess, request.responseCode, request.downloadHandler.text ?? string.Empty, request.error ?? string.Empty);
        }
    }
}
