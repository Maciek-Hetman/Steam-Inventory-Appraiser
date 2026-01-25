using System.Net;

namespace MyApi.Http;

public class SteamRateLimitHandler : DelegatingHandler
{
    private readonly ILogger<SteamRateLimitHandler> _logger;
    private static readonly object _lockObject = new();
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private const int DelayMilliseconds = 1500;
    private const int MaxRetries = 3;
    private const int RetryDelayMilliseconds = 3000;

    public SteamRateLimitHandler(ILogger<SteamRateLimitHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await ApplyRateLimitAsync();

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning(
                    "Steam API returned 429 (Too Many Requests) for {Method} {Uri}. " +
                    "Attempt {Attempt}/{MaxRetries}",
                    request.Method,
                    request.RequestUri,
                    attempt + 1,
                    MaxRetries);

                if (attempt < MaxRetries - 1)
                {
                    await Task.Delay(RetryDelayMilliseconds, cancellationToken);
                    
                    response.Dispose();
                    continue;
                }
                else
                {
                    _logger.LogError(
                        "Steam API returned 429 after {MaxRetries} retries for {Method} {Uri}",
                        MaxRetries,
                        request.Method,
                        request.RequestUri);
                    return response;
                }
            }

            return response;
        }

        throw new InvalidOperationException("Unexpected state in retry logic");
    }

    private static async Task ApplyRateLimitAsync()
    {
        lock (_lockObject)
        {
            var elapsed = DateTime.UtcNow - _lastRequestTime;
            var remainingDelay = DelayMilliseconds - (int)elapsed.TotalMilliseconds;

            if (remainingDelay > 0)
            {

                Task.Delay(remainingDelay).Wait();
            }

            _lastRequestTime = DateTime.UtcNow;
        }
    }
}
