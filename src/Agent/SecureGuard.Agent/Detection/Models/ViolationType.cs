namespace SecureGuard.Agent.Detection.Models;

public enum ViolationType
{
    ImageUpload,
    CredentialFile,
    UnknownIp,
    CredentialPattern,
    SensitiveFileAccess,
    SuspiciousProcess
}
