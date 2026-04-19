namespace NT.KYC.Jibit.HashiCorpVault.Models;

public class JibitClientCredentials
{
    public string ApiKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;
    
    public string PermanentToken { get; set; } = string.Empty;
}