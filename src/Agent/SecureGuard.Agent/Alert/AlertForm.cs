using SecureGuard.Agent.Detection.Models;
using System.Windows.Forms;

namespace SecureGuard.Agent.Alert;

public class AlertForm : Form
{
    private readonly DetectionResult _result;

    public AlertForm(string message, DetectionResult result)
    {
        _result = result;
        InitializeComponent(message);
    }

    private void InitializeComponent(string message)
    {
        this.Text = "Security Alert - SecureGuard";
        this.Size = new System.Drawing.Size(420, 220);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.TopMost = true;
        this.BackColor = System.Drawing.Color.White;

        var iconLabel = new Label
        {
            Text = "⚠",
            Font = new System.Drawing.Font("Segoe UI", 32f),
            ForeColor = System.Drawing.Color.OrangeRed,
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(60, 60),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        var titleLabel = new Label
        {
            Text = "Security Alert",
            Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.DarkRed,
            Location = new System.Drawing.Point(90, 20),
            Size = new System.Drawing.Size(300, 30),
        };

        var messageLabel = new Label
        {
            Text = message,
            Font = new System.Drawing.Font("Segoe UI", 10f),
            Location = new System.Drawing.Point(90, 55),
            Size = new System.Drawing.Size(300, 40),
            AutoSize = false
        };

        var actionLabel = new Label
        {
            Text = "Action telah diblok.",
            Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.DarkRed,
            Location = new System.Drawing.Point(90, 100),
            Size = new System.Drawing.Size(300, 25)
        };

        var violationLabel = new Label
        {
            Text = $"Type: {_result.ViolationType} | Severity: {_result.Severity}",
            Font = new System.Drawing.Font("Segoe UI", 8f),
            ForeColor = System.Drawing.Color.Gray,
            Location = new System.Drawing.Point(20, 135),
            Size = new System.Drawing.Size(380, 20)
        };

        var okButton = new Button
        {
            Text = "OK",
            Location = new System.Drawing.Point(160, 160),
            Size = new System.Drawing.Size(100, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.DarkRed,
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 10f)
        };
        okButton.Click += (s, e) => { this.Close(); };

        this.Controls.AddRange(new Control[] { iconLabel, titleLabel, messageLabel, actionLabel, violationLabel, okButton });

        // Auto-close after 10 seconds
        var timer = new System.Windows.Forms.Timer { Interval = 10000 };
        timer.Tick += (s, e) => { timer.Stop(); this.Close(); };
        timer.Start();
    }
}
