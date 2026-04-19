namespace ExchangeRateProvider.Contract.Commons.Helpers;

public static class ClientIdHelper
{
    public static string CreateAndEncryptClinetId(string username, string projectName, int userId, int consumerId, string encryptionKey)
        => EncryptClientId(CreateRowClientId(username, projectName, userId, consumerId), encryptionKey);

    public static string CreateRowClientId(string username, string projectName, int userId, int consumerId)
        => $"{projectName.Replace(" ", "-")}_{userId}_{username}_{consumerId}";

    public static string EncryptClientId(string rowClientId, string encryptionKey)
        => rowClientId.EncryptString(encryptionKey);

    public static string GetClientId(string clientId, string encryptionKey)
        => clientId.DecryptString(encryptionKey);
}
