# KYC Jibit SDK

This library provides a .NET SDK for interacting with the Jibit API using RestClient package.

- **Developers:**
    - [Mostafa Hasanpour](https://gitlab.com/MostafaHasanpour)
    - [Ehsan Heidarzadeh](https://gitlab.com/ehsanheidarzadeh959)

- **Maintainer**
    - [Mostafa Hasanpour](https://gitlab.com/MostafaHasanpour)
    - [Ehsan Heidarzadeh](https://gitlab.com/ehsanheidarzadeh959)

- **Package Version:** 1.0.0

## Key Units

* **JibitRestClient:** The main entry point for interacting with the Jibit API.
* **JibitAlphaRestClient:** The main entry point for interacting with the Jibit API.
* **JibitEndPoints:** Class containing constants for Jibit API endpoints.
* **JibitAlphaEndPoints:** Class containing constants for Jibit Alpha API endpoints.
* **JibitResponseWrapper** wrapper class that parses jibit errors and data to required objects

## References

* NT.SDK.RestClient

## Project Structure

NT.SDK.Sumsub

- Models
    - Requests (Contains request models)
    - Responses (Contains response models)
- BootsTrapper
- RestClients (Contains client classes for interacting with Jibit API endpoints)
- Utils (Contains utility classes like Endpoints and configuration)

## Attention
1) The Jibit Alpha works with Permanent access token, and Jibit itself works with api key and secret key.
2) Please provide proper logger to the package jibit APIs response objects changes rapidly without any notice.
3) The photos taken from user needs to be in proper format and structure which explained in summary docs.
4) In general the camera needs to be close to the object whatever the object is person face or documents.
5) for jibit you should send birthdate like YYYYMMDD but in jibit alpha is YYYY/MM/DD 

## Usage

### First Add to service collection

1. **Default Configuration:**
   this method will register the service life cycle as Transient and the debug mode option is false
    ```csharp
    builder.Services.AddJibitRestClient(
        new JibitRestClientConfiguration("YOUR_APIKEY", "YOUR_SECRET_KEY", JibitEndPoints.BaseUrl, 10000, logger));
   
    builder.Services.AddJibitAlphaRestClient(
        new JibitAlphaRestClientConfiguration("YOUR_PERMENANT_TOKEN", JibitAlphaEndPoints.BaseUrl, 20000, logger));
    ```

2. **With debug mode option:**

    ```csharp
    builder.Services.AddJibitRestClient(
        new JibitRestClientConfiguration("YOUR_APIKEY", "YOUR_SECRET_KEY", JibitEndPoints.BaseUrl, 10000, logger), true);
   
    builder.Services.AddJibitAlphaRestClient(
        new JibitAlphaRestClientConfiguration("YOUR_PERMENANT_TOKEN", JibitAlphaEndPoints.BaseUrl, 20000, logger), true);
    ```

3. **With Custom service life cycle:**

    ```csharp
    builder.Services.AddJibitRestClient(
        new JibitRestClientConfiguration("YOUR_APIKEY", "YOUR_SECRET_KEY", JibitEndPoints.BaseUrl, 10000, logger), ServiceLifetime.Transient);
   
    builder.Services.AddJibitAlphaRestClient(
        new JibitAlphaRestClientConfiguration("YOUR_PERMENANT_TOKEN", JibitAlphaEndPoints.BaseUrl, 20000, logger), ServiceLifetime.Transient);
    ```
4. **With Custom service life cycle and debug mode option:**

    ```csharp
    builder.Services.AddJibitRestClient(
       new JibitRestClientConfiguration("YOUR_APIKEY", "YOUR_SECRET_KEY", JibitEndPoints.BaseUrl, 10000, logger),
         false,
         ServiceLifetime.Transient);
   
    builder.Services.AddJibitAlphaRestClient(
        new JibitAlphaRestClientConfiguration("YOUR_PERMENANT_TOKEN", JibitAlphaEndPoints.BaseUrl, 20000, logger),
        false, 
        ServiceLifetime.Transient);
    ```

### Adding service with HashiCorpVault

```csharp
await builder.AddHashiCorpVaultAsync(
    option =>
    {
        option.EnvironmentFileName = ".env";
        option.JsonFileName = "vault.credentials.json";
        option.VaultServiceTimeout = TimeSpan.FromSeconds(60);
    },
    async (context, cancellationToken) =>
    {
        await HashiCorpVaultImplementationsAsync(
            context,
            cancellationToken
        );
    },
    isDebugMode: false,
    cancellationToken: default
);

async Task HashiCorpVaultImplementationsAsync(
    IHashiCorpVaultContext context,
    CancellationToken cancellationToken = default
)
{
    await context.HealthCheckAsync(cancellationToken: cancellationToken);

    await context.AddJibitClientAsync(opt =>
    {
        opt.JibitRestClientLifeCycle = ServiceLifetime.Scoped;
        opt.JibitAlphaRestClientLifeCycle = ServiceLifetime.Scoped;
        opt.Logger = logger;
        opt.JibitBasePath = JibitEndPoints.BaseUrl;
        opt.JibitAlphaBasePath = JibitAlphaEndPoints.BaseUrl;
    });
}
```

### Making API Calls

for making API calls to Jibit API endpoints you only need to inject the interfaces from constructor and use them
for example

```csharp
public class JibitService(
    IJibitRestClient jibitRestClient,
    IJibitAlphaRestClient jibitAlphaRestClient,
    JibitCredentialsManagement jibitCredentialsManagement)
    : IJibitService
{
    private async Task<GetAccessTokenResponse> GetAccessToken()
    {
        var response = await jibitRestClient.JibitAccessTokenApi.GetAccessToken();
        
        if (!response.Successed)
            throw new Exception(response.Result.Errors != null
                ? response.Result.Errors.First().Message
                : "Unknown Error happened");
        
        ArgumentNullException.ThrowIfNull(response.Result.Data);
        return response.Result.Data;
    }
}
```
## Endpoints interfaces
```csharp
public interface IJibitAccessTokenApi
{
    /// <summary>
    ///     Use this api to get access token for using Jibit APIs.
    /// </summary>
    public Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> GetAccessToken();

    /// <summary>
    ///     the refresh token itself is valid for 48 hours. use this endpoint to refresh the access token.
    ///     the access token itself is valid 24 hours.
    /// </summary>
    /// <param name="request">RefreshAccessTokenRequest</param>
    public Task<ApiResponse<JibitResponseWrapper<GetAccessTokenResponse>>> RefreshAccessToken(
        RefreshAccessTokenRequest request);
}


public interface IBiometricKycApi
{
    /// <summary>
    /// This API designed to check whether the user's speech matched with the content (Text) you intended or not.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitAlphaResponseWrapper<OneStepKycResponse>>> GetOneStepKycResult(
        OneStepKycRequest request,
        string bearerToken);

    public Task<ApiResponse<JibitAlphaResponseWrapper<JibitAlphaHealthCheckResponse>>> HealthCheck(string bearerToken);
}

public interface IDocumentKycApi
{
    /// <summary>
    /// This API is designed to check whether the user’s uploaded document is the same one as 
    /// expected or not. The default state is currently configured to recognize national cards.(Kart Melli).
    /// the distance between camera and document needs to be close in order to jibit accepts it.
    /// </summary>
    /// <returns>ApiResponse[JibitResponseWrapper[DocumentVerificationResponse]]</returns>
    public Task<ApiResponse<JibitAlphaResponseWrapper<DocumentVerificationResponse>>> GetDocumentVerificationResult(
        DocumentVerificationRequest request, string bearerToken);

    /// <summary>
    /// This API is designed to do OCR on selected documents such as national cards. The system 
    /// extracts the texts in the document and them. The following data is for national card (Kart Melli)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns>ApiResponse[JibitResponseWrapper[OpticalCharacterRecognitionResponse]]</returns>
    public Task<ApiResponse<JibitAlphaResponseWrapper<OpticalCharacterRecognitionResponse>>>
        GetOpticalCharacterRecognitionResult(OpticalCharacterRecognitionRequest request, string bearerToken);
}

public interface IIdentityDetailApi
{
    /// <summary>
    ///     use this Api to get user identity details from Sabt Ahval
    /// </summary>
    /// <param name="request">GetIdentityDetailsRequest</param>
    /// <param name="bearerToken">string</param>
    /// <returns>ApiResponse[JibitResponseWrapper[GetIdentityDetailsResponse]]</returns>
    public Task<ApiResponse<JibitResponseWrapper<GetIdentityDetailsResponse>>> GetIdentityDetails(
        GetIdentityDetailsRequest request, string bearerToken);
}

public interface IMatchingApi
{
    /// <summary>
    /// this method matches the client mobile number with the national code provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchMobileNumberWithNationalCode(MatchMobileNumberWithNationalCode request, string bearerToken);
    
    /// <summary>
    /// this method matches the client card number with the national code and birthdate provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchCardNumberWithIdentityDetails(MatchCardNumberWithIdentityDetails request, string bearerToken);
    
    /// <summary>
    /// this method matches the client Iban number with the national code and birthdate provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchIbanWithIdentityDetails(MatchIbanNumberWithIdentityDetails request, string bearerToken);
    
    /// <summary>
    /// this method matches the client Account number with the national code and birthdate provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>s
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<JibitMatchResult>>> MatchAccountNumberWithIdentityDetails(MatchAccountNumberWithIdentityDetails request, string bearerToken);
}

public interface ICardsApi
{
    /// <summary>
    /// This Endpoint return the Iban number of the bank account owner.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public Task<ApiResponse<JibitResponseWrapper<CardNumberToIbanResponse>>> CardNumberToIban(
        CardNumberToIbanRequest request, string bearerToken);
}
```

## Jibit Alpha access token
for requesting to jibit alpha endpoints you need to authorize the request with permanent token which jibit gives you.

## Handling Jibit Access Token

Jibit Access token are valid for 24 Hour and there is no need to request new token for every API call.
so for having efficient. it is crucial for storing token and reuse it for 24 Hour.
a basic implementation of this logic added to the sample for demonstration purposes.
you can handle token in any pattern you see fit.

```csharp
public class JibitCredentialsManagement(IMemoryCache memoryCache)
{
    private const string CacheKey = "JibitCredentialsCacheDto";
    
    public JibitCredentialsCacheDto? GetCredentialsFromCache()
    {
        var result = memoryCache.TryGetValue(CacheKey, out JibitCredentialsCacheDto? cacheDto);
        if (!result || cacheDto == null) return null;
        
        return cacheDto;
    }
    
    public void StoreNewCredentialsInCache(JibitCredentialsCacheDto result)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(2));

        memoryCache.Set(CacheKey, result, cacheEntryOptions);
    }
}
```

```csharp
public class JibitService(
    IJibitRestClient jibitRestClient,
    IJibitAlphaRestClient jibitAlphaRestClient,
    JibitCredentialsManagement jibitCredentialsManagement)
    : IJibitService
{
    private JibitCredentialsCacheDto? credentials = jibitCredentialsManagement.GetCredentialsFromCache();

    public async Task<GetIdentityDetailsResponse> GetIdentityDetails(GetIdentityDetailsRequest request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitRestClient.IdentityDetailApi.GetIdentityDetails(request, credentials!.BearerToken));
    }

    public async Task<AutomaticSpeechRecognitionResponse> GetAutomaticSpeechRecognitionData(AutomaticSpeechRecognitionRequest request)
    {
        return await HandleRequestWithCredentials(() =>
            jibitAlphaRestClient.BiometricKycApi.GetAutomaticSpeechRecognitionData(request, credentials!.BearerToken));
    }

    private async Task<TResponse> HandleRequestWithCredentials<TResponse>(
        Func<Task<ApiResponse<JibitResponseWrapper<TResponse>>>> apiCall)
        where TResponse : IResponseBody
    {
        if (credentials == null)
        {
            await GetNewJibitCredentials();
        }
    
        ArgumentNullException.ThrowIfNull(credentials);
    
        var response = await apiCall();
        if (!response.Successed && response.Result?.Errors?.First().Message == "forbidden")
        {
            await RefreshJibitCredentials();
            response = await apiCall();
        }
    
        ArgumentNullException.ThrowIfNull(response.Result);
        ArgumentNullException.ThrowIfNull(response.Result.Data);
    
        return response.Result.Data;    
    }
    
    private async Task GetNewJibitCredentials()
    {
        var result = await GetAccessToken();

        var data = new JibitCredentialsCacheDto()
        {
            BearerToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };

        jibitCredentialsManagement.StoreNewCredentialsInCache(data);

        credentials = data;
    }

    private async Task RefreshJibitCredentials()
    {
        var request = PrepareRefreshAccessTokenRequest();

        var result = await RefreshAccessToken(request);

        var data = new JibitCredentialsCacheDto()
        {
            BearerToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };

        jibitCredentialsManagement.StoreNewCredentialsInCache(data);

        credentials = data;
    }

    private RefreshAccessTokenRequest PrepareRefreshAccessTokenRequest()
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(credentials.BearerToken);
        ArgumentNullException.ThrowIfNull(credentials.RefreshToken);

        var request = new RefreshAccessTokenRequest()
        {
            AccessToken = credentials.BearerToken,
            RefreshToken = credentials.RefreshToken
        };
        return request;
    }

    private async Task<GetAccessTokenResponse> GetAccessToken()
    {
        var response = await jibitRestClient.JibitAccessTokenApi.GetAccessToken();

        if (!response.Successed)
            throw new Exception(response.Result.Errors != null
                ? response.Result.Errors.First().Message
                : "Unknown Error happened");

        ArgumentNullException.ThrowIfNull(response.Result.Data);
        return response.Result.Data;
    }

    private async Task<GetAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest request)
    {
        var response = await jibitRestClient.JibitAccessTokenApi.RefreshAccessToken(request);

        if (!response.Successed)
            throw new Exception(response.Result.Errors != null
                ? response.Result.Errors.First().Message
                : "Unknown Error happened");

        ArgumentNullException.ThrowIfNull(response.Result.Data);
        return response.Result.Data;
    }
}
```
