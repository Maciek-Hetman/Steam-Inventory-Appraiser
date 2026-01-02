using System.Text.RegularExpressions;

namespace MyApi.Helpers;

public static class SteamProfileHelper
{
    public static async Task<string?> GetSteamId64Async(
        string profileUrl,
        IHttpClientFactory factory)
    {
        if (profileUrl.Contains("/profiles/"))
            return profileUrl.Split("/profiles/")[1].TrimEnd('/');

        var vanity = profileUrl.TrimEnd('/').Split('/').Last();
        var client = factory.CreateClient("SteamClient");

        var xml = await client.GetStringAsync($"id/{vanity}?xml=1");
        var match = Regex.Match(xml, "<steamID64>(\\d+)</steamID64>");

        return match.Success ? match.Groups[1].Value : null;
    }
}