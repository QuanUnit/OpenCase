namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    public class HttpResponse
    {
        public readonly bool IsSuccess;
        public readonly long Code;
        public readonly string Text;
        public readonly string Error;

        public HttpResponse(bool isSuccess, long code, string text, string error)
        {
            IsSuccess = isSuccess;
            Code = code;
            Text = text;
            Error = error;
        }
    }
}
