using NT.DDD.Domain.Exceptions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Contract.Settings.Dtos;

public class SettingModel
{
    [Range(0, int.MaxValue, ErrorMessage = "Snapshot Clock Time cannot be negative.")]
    public int SnapshotClockTime { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Percentage Difference cannot be negative.")]
    public int PercentageDifference { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Time Difference (milliseconds) cannot be negative.")]
    [Display(Name = "Time Difference (ms)")]
    public int TimeDifference { get; set; }

    [Range(1000, int.MaxValue, ErrorMessage = "CryptoJobs Interval (milliseconds) cannot be negative.")]
    [Display(Name = "CryptoJobs Interval (ms)")]
    public int CryptoJobsInterval { get; set; } = 5;

    [Range(1000, int.MaxValue, ErrorMessage = "Fiat JobsInterval (milliseconds) cannot be negative.")]
    [Display(Name = "Fiat JobsInterval (ms)")]
    public int FiatJobsInterval { get; set; } = 1;

    [Range(1000, int.MaxValue, ErrorMessage = "Fiat JobsInterval (milliseconds) cannot be negative.")]
    [Display(Name = "Fiat JobsInterval (ms)")]
    public decimal ManualRateChangePercentage { get; set; }
}