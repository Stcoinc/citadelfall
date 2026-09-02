using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClubGamerZone.TowerDefense.Application.Networking;
using UnityEngine.Networking;

namespace ClubGamerZone.TowerDefense.Infrastructure.Http
{
    public sealed class UnityRestClient : IRestClient
    {
        public async Task<RestResponse> SendAsync(RestRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var webRequest = CreateWebRequest(request))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                return new RestResponse(webRequest.responseCode, webRequest.downloadHandler.text, webRequest.error);
            }
        }

        private static UnityWebRequest CreateWebRequest(RestRequest request)
        {
            var webRequest = new UnityWebRequest(request.Url, ToUnityMethod(request.Method))
            {
                downloadHandler = new DownloadHandlerBuffer()
            };

            if (!string.IsNullOrWhiteSpace(request.Body))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.Body));
                webRequest.SetRequestHeader("Content-Type", "application/json");
            }

            foreach (var header in request.Headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }

            return webRequest;
        }

        private static string ToUnityMethod(RestHttpMethod method)
        {
            switch (method)
            {
                case RestHttpMethod.Get:
                    return UnityWebRequest.kHttpVerbGET;
                case RestHttpMethod.Post:
                    return UnityWebRequest.kHttpVerbPOST;
                case RestHttpMethod.Put:
                    return UnityWebRequest.kHttpVerbPUT;
                case RestHttpMethod.Patch:
                    return "PATCH";
                case RestHttpMethod.Delete:
                    return UnityWebRequest.kHttpVerbDELETE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }
}
