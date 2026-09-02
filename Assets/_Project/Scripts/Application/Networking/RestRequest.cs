using System;
using System.Collections.Generic;

namespace ClubGamerZone.TowerDefense.Application.Networking
{
    public sealed class RestRequest
    {
        public RestRequest(RestHttpMethod method, string url, string body, IReadOnlyDictionary<string, string> headers)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL is required.", nameof(url));
            }

            Method = method;
            Url = url;
            Body = body ?? string.Empty;
            Headers = headers ?? new Dictionary<string, string>();
        }

        public RestHttpMethod Method { get; }

        public string Url { get; }

        public string Body { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }
    }
}
