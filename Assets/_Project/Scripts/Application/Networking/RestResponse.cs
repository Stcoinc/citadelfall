namespace ClubGamerZone.TowerDefense.Application.Networking
{
    public sealed class RestResponse
    {
        public RestResponse(long statusCode, string body, string error)
        {
            StatusCode = statusCode;
            Body = body ?? string.Empty;
            Error = error ?? string.Empty;
        }

        public long StatusCode { get; }

        public string Body { get; }

        public string Error { get; }

        public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299 && string.IsNullOrWhiteSpace(Error);
    }
}
