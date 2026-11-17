using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Tools.Web
{
    public static class DownloadTools
    {
        public static Task Download(string name, string id, string pathDownload)
        {
            return TaskDownload(name, id, pathDownload);
        }

        private static async Task TaskDownload(string name, string id, string pathDownload)
        {
            Debug.Log($"Start download package {name}");
            var uwr = new UnityWebRequest($"https://drive.google.com/uc?export=download&id={id}",
                UnityWebRequest.kHttpVerbGET);
            var path = Path.Combine(pathDownload, $"{name}.unitypackage");
            uwr.downloadHandler = new DownloadHandlerFile(path);
            var asyncOperation = uwr.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"<color=red>{uwr.error}</color>");
            }
            else
            {
                Debug.Log("File successfully downloaded and saved to " + path);
            }
            
            Debug.Log($"Completed download package {name}");
        }

        public static async Task<string> DownloadGoogleSheetAsCsvToPath(string sheetId, string pageId,
            string localFileName)
        {
            var localPath = Path.Combine(Application.dataPath, localFileName);

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            var url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={pageId}";
            var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log($"Loading google sheet: {url}");

            var asyncRequest = uwr.SendWebRequest();

            while (!asyncRequest.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Loading failed: {uwr.error}");
                return null;
            }

            var folderPath = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            await File.WriteAllTextAsync(localPath, uwr.downloadHandler.text);
            Debug.Log($"File saved by path: {localPath}");

            return localPath;
        }

        public static async Task<string> DownloadText(string url, int timeoutSeconds, Action<float> progressChanged)
        {
            using var request = UnityWebRequest.Get(url);
            request.timeout = timeoutSeconds;
            var operation = request.SendWebRequest();

            while (operation.isDone == false)
            {
                progressChanged(request.downloadProgress);
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Download failed: {url}: {request.error}");
            }

            progressChanged(1f);
            return request.downloadHandler.text;
        }

        public static async Task<string> DownloadGoogleSheetAsCsv(string sheetId, string pageId)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={pageId}";
            var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log($"Loading google sheet: {url}");

            var asyncRequest = uwr.SendWebRequest();

            while (!asyncRequest.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Loading failed: {uwr.error}");
                return null;
            }

            return uwr.downloadHandler.text;
        }
    }
}