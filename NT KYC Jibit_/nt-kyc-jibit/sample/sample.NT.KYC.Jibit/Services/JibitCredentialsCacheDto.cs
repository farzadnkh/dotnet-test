namespace sample.NT.KYC.Jibit.Services;

public class JibitCredentialsCacheDto
{
    public string BearerToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
    
    
    public string PermanentToken { get; set; } = string.Empty;
}