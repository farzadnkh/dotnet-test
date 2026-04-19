using Microsoft.AspNetCore.Mvc;
using NT.KYC.Jibit.Models.Requests;
using NT.KYC.Jibit.Models.Responses;
using sample.NT.KYC.Jibit.Services;

namespace sample.NT.KYC.Jibit.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class KycProviderController(IJibitService jibitService, ILogger<KycProviderController> logger) : ControllerBase
{
    [HttpGet(Name = "GetIdentityDetails")]
    public async Task<ActionResult<GetIdentityDetailsResponse>> GetIdentityDetails(
        [FromQuery] GetIdentityDetailsRequest request)
    {
        try
        {
            var result = await jibitService.GetIdentityDetails(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPost(Name = "GetAutomaticSpeechRecognitionData")]
    public async Task<ActionResult<OneStepKycResponse>> GetOneStepKycData(
        [FromForm] OneStepKycRequest request)
    {
        try
        {
            var result = await jibitService.GetOneStepKycData(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost(Name = "GetDocumentVerificationResult")]
    public async Task<ActionResult<DocumentVerificationResponse>> GetDocumentVerificationResult(
        [FromForm] DocumentVerificationRequest request)
    {
        try
        {
            var result = await jibitService.GetDocumentVerificationResult(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost(Name = "GetOpticalCharacterRecognitionResult")]
    public async Task<ActionResult<OpticalCharacterRecognitionResponse>> GetOpticalCharacterRecognitionResult(
        [FromForm] OpticalCharacterRecognitionRequest request)
    {
        try
        {
            var result = await jibitService.GetOpticalCharacterRecognitionResult(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost(Name = "MatchCardNumber")]
    public async Task<ActionResult<OpticalCharacterRecognitionResponse>> MatchCardNumber(
        [FromForm] MatchCardNumberWithIdentityDetails request)
    {
        try
        {
            var result = await jibitService.MatchCardNumberWithIdentityDetails(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost(Name = "MatchPhoneNumber")]
    public async Task<ActionResult<OpticalCharacterRecognitionResponse>> MatchPhoneNumber(
        [FromForm] MatchMobileNumberWithNationalCode request)
    {
        try
        {
            var result = await jibitService.MatchMobileNumberWithNationalCode(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost(Name = "MatchAccountNumberWithIdentityDetails")]
    public async Task<ActionResult<OpticalCharacterRecognitionResponse>> MatchAccountNumberWithIdentityDetails(
        [FromForm] MatchAccountNumberWithIdentityDetails request)
    {
        try
        {
            var result = await jibitService.MatchAccountNumberWithIdentityDetails(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    
    [HttpPost(Name = "MatchIbanWithIdentityDetails")]
    public async Task<ActionResult<OpticalCharacterRecognitionResponse>> MatchIbanWithIdentityDetails(
        [FromForm] MatchIbanNumberWithIdentityDetails request)
    {
        try
        {
            var result = await jibitService.MatchIbanWithIdentityDetails(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    
    [HttpPost(Name = "CardNumberToIban")]
    public async Task<ActionResult<CardNumberToIbanResponse>> CardNumberToIban(
        [FromForm] CardNumberToIbanRequest request)
    {
        try
        {
            var result = await jibitService.CardNumberToIban(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    
    [HttpGet(Name = "HealthCheck")]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            var result = await jibitService.HealthCheck();
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet(Name = "GetToken")]
    public async Task<ActionResult> GetToken()
    {
        try
        {
            var result = await jibitService.GetToken();
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost(Name = "RefreshToken")]
    public async Task<ActionResult> RefreshToken([FromForm] RefreshAccessTokenRequest request)
    {
        try
        {
            var result = await jibitService.RefreshToken(request);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest(e.Message);
        }
    }
}