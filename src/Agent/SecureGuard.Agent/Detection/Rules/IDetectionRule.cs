using SecureGuard.Agent.Detection.Models;
using Titanium.Web.Proxy.EventArguments;

namespace SecureGuard.Agent.Detection.Rules;

public interface IDetectionRule
{
    string RuleName { get; }
    Task<DetectionResult> EvaluateAsync(SessionEventArgs session);
}
