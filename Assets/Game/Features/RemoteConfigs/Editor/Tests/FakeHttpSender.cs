using System.Collections.Generic;
using System.Threading.Tasks;
using Features.RemoteConfigs.Editor.GoogleSheets;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class FakeHttpSender : IHttpSender
    {
        public readonly List<string> Requests = new List<string>();
        public readonly List<string> Bodies = new List<string>();

        private readonly Queue<HttpResponse> _responses = new Queue<HttpResponse>();

        public void Enqueue(bool isSuccess, long code, string text)
        {
            _responses.Enqueue(new HttpResponse(isSuccess, code, text, isSuccess == true ? string.Empty : "HTTP/1.1 error"));
        }

        public Task<HttpResponse> Send(string method, string url, string body, string contentType, string accessToken)
        {
            Requests.Add($"{method} {url} {accessToken}");
            Bodies.Add(body);

            return Task.FromResult(_responses.Dequeue());
        }
    }
}
