using System.Net;

namespace MyApi.Http;

/// <summary>
/// Delegating handler that implements rate limiting (1.5s between requests)
/// and retry logic for 429 (Too Many Requests) responses from Steam API.
/// </summary>
public class SteamRateLimitHandler : DelegatingHandler
{
    private readonly ILogger<SteamRateLimitHandler> _logger;
    private static readonly object _lockObject = new();
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private const int DelayMilliseconds = 1500; // 1.5 seconds
    private const int MaxRetries = 3;
    private const int RetryDelayMilliseconds = 3000; // 3 seconds before retry

    public SteamRateLimitHandler(ILogger<SteamRateLimitHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Apply rate limiting
        await ApplyRateLimitAsync();

        // Retry logic for 429 responses
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
                    // Wait before retrying
                    await Task.Delay(RetryDelayMilliseconds, cancellationToken);
                    
                    // Create a new request for retry (dispose the failed response)
                    response.Dispose();
                    continue;
                }
                else
                {
                    // Out of retries, return the 429 response
                    _logger.LogError(
                        "Steam API returned 429 after {MaxRetries} retries for {Method} {Uri}",
                        MaxRetries,
                        request.Method,
                        request.RequestUri);
                    return response;
                }
            }

            // Success or non-429 response
            return response;
        }

        // Should not reach here, but return a failed response if it does
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
                // We need to wait before making the next request
                // Schedule the wait
                Task.Delay(remainingDelay).Wait();
            }

            _lastRequestTime = DateTime.UtcNow;
        }
    }
}
