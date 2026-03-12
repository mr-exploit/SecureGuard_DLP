using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;
using Titanium.Web.Proxy.EventArguments;

namespace SecureGuard.Agent.Detection.Rules;

public class UnknownIpRule : IDetectionRule
{
    public string RuleName => "UnknownIpRule";

    private readonly List<string> _whitelistedIps;

    public UnknownIpRule(List<string> whitelistedIps)
    {
        _whitelistedIps = whitelistedIps;
    }

    public Task<DetectionResult> EvaluateAsync(SessionEventArgs session)
    {
        try
        {
            var destinationHost = session.HttpClient.Request.RequestUri.Host;

            // Skip if destination is localhost or private IP
            if (IsLocalOrPrivate(destinationHost))
                return Task.FromResult(DetectionResult.NoViolation());

            // Check if the IP/host is in whitelist
            if (IsWhitelisted(destinationHost))
                return Task.FromResult(DetectionResult.NoViolation());

            var result = DetectionResult.Violation(
                ViolationTypes.UnknownIp,
                SeverityLevels.Medium,
                "Flag",
                "Koneksi ke IP tidak dikenal terdeteksi.",
                $"Connection to unknown destination: {destinationHost}"
            );
            result.DestinationIp = destinationHost;
            return Task.FromResult(result);
        }
        catch
        {
            return Task.FromResult(DetectionResult.NoViolation());
        }
    }

    private bool IsWhitelisted(string host)
    {
        foreach (var ip in _whitelistedIps)
        {
            if (ip.Equals(host, StringComparison.OrdinalIgnoreCase))
                return true;
            // Simple prefix match for CIDR-like entries
            if (ip.Contains('/'))
            {
                var prefix = ip.Split('/')[0];
                var prefixParts = prefix.Split('.');
                var hostParts = host.Split('.');
                if (prefixParts.Length <= hostParts.Length)
                {
                    bool matches = true;
                    for (int i = 0; i < prefixParts.Length - 1; i++)
                    {
                        if (prefixParts[i] != hostParts[i]) { matches = false; break; }
                    }
                    if (matches) return true;
                }
            }
        }
        return false;
    }

    private static bool IsLocalOrPrivate(string host)
    {
        if (host == "localhost" || host == "127.0.0.1" || host == "::1")
            return true;
        if (host.StartsWith("10.") || host.StartsWith("192.168."))
            return true;
        if (host.StartsWith("172."))
        {
            var parts = host.Split('.');
            if (parts.Length >= 2 && int.TryParse(parts[1], out var second))
                return second >= 16 && second <= 31;
        }
        return false;
    }
}
