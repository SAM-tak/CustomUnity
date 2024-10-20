using UnityEngine.Networking;

namespace CustomUnity
{
    /// <summary>
    /// Utilities for UnityWebRequest
    /// </summary>
    public static class WebRequest
    {
        public static UnityWebRequest PostJson(string uri, string json)
        {
            var ret = UnityWebRequest.PostWwwForm(uri, json);
            ret.uploadHandler.contentType = "application/json";
            return ret;
        }

        public static UnityWebRequest PutJson(string uri, string json)
        {
            var ret = UnityWebRequest.Put(uri, json);
            ret.uploadHandler.contentType = "application/json";
            return ret;
        }

        public static UnityWebRequest Post(string uri, byte[] payload, string contentType)
        {
            return new UnityWebRequest(
                uri,
                UnityWebRequest.kHttpVerbPOST,
                new DownloadHandlerBuffer(),
                new UploadHandlerRaw(payload) { contentType = contentType }
            );
        }

        public static UnityWebRequest Put(string uri, byte[] payload, string contentType)
        {
            return new UnityWebRequest(
                uri,
                UnityWebRequest.kHttpVerbPOST,
                new DownloadHandlerBuffer(),
                new UploadHandlerRaw(payload) { contentType = contentType }
            );
        }
    }
}
