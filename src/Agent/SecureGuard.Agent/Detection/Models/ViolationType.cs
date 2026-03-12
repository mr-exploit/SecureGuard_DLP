namespace SecureGuard.Agent.Detection.Models;

public enum ViolationType
{
    ImageUpload,
    CredentialFile,
    CredentialPattern,
    UnknownIp,
    SensitiveFileAccess,
    UnauthorizedProcess
}
