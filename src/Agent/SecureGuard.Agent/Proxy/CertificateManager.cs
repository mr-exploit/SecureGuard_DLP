using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecureGuard.Agent.Proxy;

public class CertificateManager
{
    private readonly ILogger<CertificateManager> _logger;
    private X509Certificate2? _rootCa;
    private readonly Dictionary<string, X509Certificate2> _certCache = new();
    private readonly object _lock = new();

    public CertificateManager(ILogger<CertificateManager> logger)
    {
        _logger = logger;
    }

    public X509Certificate2 GetOrCreateRootCa()
    {
        lock (_lock)
        {
            if (_rootCa != null) return _rootCa;

            const string subjectName = "CN=SecureGuard Root CA, O=SecureGuard, C=ID";
            _rootCa = GenerateSelfSignedCertificate(subjectName, isCa: true);
            InstallRootCa(_rootCa);
            _logger.LogInformation("SecureGuard Root CA generated and installed.");
            return _rootCa;
        }
    }

    public X509Certificate2 GetOrCreateDomainCertificate(string domain)
    {
        lock (_lock)
        {
            if (_certCache.TryGetValue(domain, out var cached))
                return cached;

            var rootCa = GetOrCreateRootCa();
            var cert = GenerateDomainCertificate(domain, rootCa);
            _certCache[domain] = cert;
            return cert;
        }
    }

    private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, bool isCa)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subjectName, rsa,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(certificateAuthority: isCa, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));

        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        if (isCa)
        {
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        }

        var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var notAfter = DateTimeOffset.UtcNow.AddYears(10);

        var cert = request.CreateSelfSigned(notBefore, notAfter);
        return new X509Certificate2(cert.Export(X509ContentType.Pfx), (string?)null,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }

    private static X509Certificate2 GenerateDomainCertificate(string domain, X509Certificate2 issuer)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN={domain}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName(domain);
        request.CertificateExtensions.Add(sanBuilder.Build());

        var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var notAfter = DateTimeOffset.UtcNow.AddYears(1);

        var serialNumber = new byte[8];
        RandomNumberGenerator.Fill(serialNumber);

        using var issuerWithKey = issuer.HasPrivateKey ? issuer : null;
        var cert = request.Create(issuer, notBefore, notAfter, serialNumber);

        return cert.CopyWithPrivateKey(rsa);
    }

    private void InstallRootCa(X509Certificate2 certificate)
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
            _logger.LogInformation("Root CA installed to LocalMachine\\Root store.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to install Root CA to LocalMachine store. Trying CurrentUser store.");
            try
            {
                using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
                _logger.LogInformation("Root CA installed to CurrentUser\\Root store.");
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to install Root CA to certificate store.");
            }
        }
    }
}
