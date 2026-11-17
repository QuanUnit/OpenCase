using System;

namespace Features.RemoteConfigs.Editor.GoogleSheets
{
    [Serializable]
    public class GoogleServiceAccountKey
    {
        public string client_email;
        public string private_key;
        public string token_uri;
    }
}
