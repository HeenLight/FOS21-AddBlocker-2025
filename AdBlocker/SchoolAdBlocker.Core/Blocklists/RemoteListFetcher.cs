using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolAdBlocker.Core.Blocklists
{
    public sealed class RemoteListFetcher
    {
        private readonly HttpClient _http;

        public RemoteListFetcher(HttpClient http)
        {
            _http = http;
        }

        public async Task<FetchResult> FetchAsync(BlocklistSource source, CancellationToken ct)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, source.Url))
            {
                if (!string.IsNullOrWhiteSpace(source.ETag))
                    request.Headers.TryAddWithoutValidation("If-None-Match", source.ETag);
                if (source.LastModified.HasValue)
                    request.Headers.IfModifiedSince = source.LastModified;

                using (var response = await _http.SendAsync(request, ct).ConfigureAwait(false))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                    {
                        return FetchResult.NotModified();
                    }

                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return FetchResult.Updated(
                        content,
                        response.Headers.ETag != null ? response.Headers.ETag.Tag : null,
                        response.Content.Headers.LastModified
                    );
                }
            }
        }
    }

    public sealed class FetchResult
    {
        public bool IsModified { get; }
        public string Content { get; }
        public string ETag { get; }
        public DateTimeOffset? LastModified { get; }

        private FetchResult(bool isModified, string content, string etag, DateTimeOffset? lastModified)
        {
            IsModified = isModified;
            Content = content;
            ETag = etag;
            LastModified = lastModified;
        }

        public static FetchResult NotModified() => new FetchResult(false, null, null, null);
        public static FetchResult Updated(string content, string etag, DateTimeOffset? lastModified)
            => new FetchResult(true, content, etag, lastModified);
    }
}