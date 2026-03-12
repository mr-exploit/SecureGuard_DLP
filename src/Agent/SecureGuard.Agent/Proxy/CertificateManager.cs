using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Titanium.Web.Proxy;

namespace SecureGuard.Agent.Proxy;

public class CertificateManager
{
    private readonly ProxyServer _proxyServer;
    private readonly ILogger<CertificateManager> _logger;
    private const string CertSubject = "SecureGuardCA";
    private const string CertPath = "SecureGuardCA.crt";

    public CertificateManager(ProxyServer proxyServer, ILogger<CertificateManager> logger)
    {
        _proxyServer = proxyServer;
        _logger = logger;
    }

    public void EnsureRootCa()
    {
        try
        {
            _proxyServer.CertificateManager.EnsureRootCertificate();

            if (!IsRootCaTrusted())
            {
                InstallRootCa();
            }

            _logger.LogInformation("Root CA certificate is ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure Root CA certificate");
        }
    }

    private bool IsRootCaTrusted()
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(
                X509FindType.FindBySubjectName, CertSubject, false);
            return certs.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private void InstallRootCa()
    {
        try
        {
            var rootCert = _proxyServer.CertificateManager.RootCertificate;
            if (rootCert == null)
            {
                _logger.LogWarning("Root CA certificate not available for installation");
                return;
            }

            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(rootCert);
            store.Close();

            _logger.LogInformation("Root CA certificate installed to Trusted Root store");

            // Also export the certificate file
            ExportCertificate(rootCert);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Cannot install Root CA - insufficient privileges. Run as Administrator.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install Root CA certificate");
        }
    }

    private void ExportCertificate(X509Certificate2 cert)
    {
        try
        {
            var certBytes = cert.Export(X509ContentType.Cert);
            File.WriteAllBytes(CertPath, certBytes);
            _logger.LogInformation("Root CA certificate exported to {Path}", CertPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to export certificate to file");
        }
    }

    public void UninstallRootCa()
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            var certs = store.Certificates.Find(
                X509FindType.FindBySubjectName, CertSubject, false);
            foreach (var cert in certs)
            {
                store.Remove(cert);
            }
            store.Close();
            _logger.LogInformation("Root CA certificate removed from Trusted Root store");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to uninstall Root CA certificate");
        }
    }
}
