using Microsoft.Extensions.Options;
using SecureGuard.Agent.Detection.Models;
using SecureGuard.Shared.Constants;

namespace SecureGuard.Agent.Alert;

public class AlertService
{
    private readonly ILogger<AlertService> _logger;
    private readonly AgentOptions _options;
    private readonly SemaphoreSlim _alertThrottle = new(1, 1);
    private DateTime _lastAlertTime = DateTime.MinValue;

    public AlertService(ILogger<AlertService> logger, IOptions<AgentOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task ShowAlertAsync(string message, DetectionResult result)
    {
        // Throttle alerts to avoid flooding the user
        if (!await _alertThrottle.WaitAsync(100))
            return;

        try
        {
            if (DateTime.UtcNow - _lastAlertTime < TimeSpan.FromSeconds(5))
                return;

            _lastAlertTime = DateTime.UtcNow;
            _logger.LogWarning("SECURITY ALERT: {Message}", message);

            // Show WinForms popup on a dedicated STA thread
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    using var form = new AlertForm(message, result);
                    System.Windows.Forms.Application.Run(form);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error showing alert form.");
                }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            await Task.Delay(100); // Give thread time to start
        }
        finally
        {
            _alertThrottle.Release();
        }
    }
}
