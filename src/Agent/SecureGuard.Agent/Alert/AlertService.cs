using Microsoft.Extensions.Options;
using SecureGuard.Agent.Detection.Models;

namespace SecureGuard.Agent.Alert;

public class AlertService
{
    private readonly ILogger<AlertService> _logger;
    private readonly AgentOptions _options;

    public AlertService(ILogger<AlertService> logger, IOptions<AgentOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task ShowAlertAsync(DetectionResult result)
    {
        _logger.LogWarning("SECURITY ALERT: {ViolationType} - {Message}", result.ViolationType, result.Message);

        // Show WinForms popup on UI thread
        try
        {
            if (OperatingSystem.IsWindows())
            {
                await Task.Run(() => ShowWindowsAlert(result));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show alert popup");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ShowWindowsAlert(DetectionResult result)
    {
        try
        {
            var message = $"{result.Message}\n\nAction telah diblok.";
            var title = "Security Alert - SecureGuard DLP";

            // Use MessageBox for alert display (compatible with Windows service)
            System.Windows.Forms.MessageBox.Show(
                message,
                title,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning,
                System.Windows.Forms.MessageBoxDefaultButton.Button1,
                System.Windows.Forms.MessageBoxOptions.ServiceNotification
            );
        }
        catch
        {
            // MessageBox may not be available in service context without UI interaction
        }
    }
}
