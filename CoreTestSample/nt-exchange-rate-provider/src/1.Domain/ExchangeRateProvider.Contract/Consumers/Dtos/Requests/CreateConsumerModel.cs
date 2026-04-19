using ExchangeRateProvider.Domain.Commons;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Contract.Consumers.Dtos.Requests;

public class CreateConsumerModel
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }

    [MaxLength(32, ErrorMessage = "Maximum project name length reached.")]
    public string ProjectName { get; set; }
    public bool Published { get; set; }
    public bool IsActive { get; set; }
    public string ApiKey { get; set; }
    public int CreatorUserId { get; set; }
    public bool IsEditMode { get; set; } = true;
    public int TokenTtl { get; set; }
    public int UserId { get; set; }
    
    public List<SelectListItem> Users { get; set; } = new();
    
    public IEnumerable<SelectListItem> Scopes { get; set; } =
    [
         new() { Value = "websocket", Text = "WebSocket" },
         new() { Value = "realtime-api", Text = "RealTime-API" }
    ];
    public string SelectedScope { get; set; }

    public string ClientId { get; set; }
    
    public string ClientSecret { get; set; }
    
    public int ConsumerId { get; set; }
    
    public string WhiteListIps { get; set; }
    public List<string> ProviderIds { get; set; }
    public List<string> MarketIds { get; set; }
    public List<string> PairIds { get; set; }
    public SpreadOptions SpreadOptions { get; set; }
    public IEnumerable<SelectListItem> ProviderOptions { get; set; }
    public IEnumerable<SelectListItem> MarketOptions { get; set; }
    public IEnumerable<SelectListItem> PairOptions { get; set; }
    public IEnumerable<SelectListItem> ConsumerOptions { get; set; }
}