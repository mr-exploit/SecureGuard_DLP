using Microsoft.Extensions.Options;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;
using System.Net;

namespace SecureGuard.Agent.Detection.Rules;

public class UnknownIpRule : IDetectionRule
{
    public string RuleName => "UnknownIp";

    private readonly HashSet<string> _whitelistedIps;

    public UnknownIpRule(IOptions<AgentOptions> options)
    {
        _whitelistedIps = new HashSet<string>(options.Value.WhitelistedIps ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<DetectionResult> EvaluateAsync(DetectionContext context)
    {
        if (string.IsNullOrEmpty(context.DestinationIp))
            return new DetectionResult { IsBlocked = false };

        var ip = context.DestinationIp;

        if (IPAddress.TryParse(ip, out var parsedIp))
        {
            if (IPAddress.IsLoopback(parsedIp) || _whitelistedIps.Contains(ip))
                return new DetectionResult { IsBlocked = false };
        }
        else
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(ip);
                if (addresses.Any(a => IPAddress.IsLoopback(a) || _whitelistedIps.Contains(a.ToString())))
                    return new DetectionResult { IsBlocked = false };
            }
            catch
            {
                // DNS resolution failed — still flag unknown hosts
            }
        }

        if (_whitelistedIps.Contains(ip))
            return new DetectionResult { IsBlocked = false };

        return new DetectionResult
        {
            IsBlocked = false,
            IsFlagged = true,
            ViolationType = ViolationTypes.UnknownIp,
            Severity = SeverityLevels.Medium,
            Message = $"Connection to unknown IP/host: {ip}",
            ProcessName = context.ProcessName ?? "unknown",
            DestinationIp = ip
        };
    }
}
