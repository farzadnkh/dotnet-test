using NT.SDK.RestClient.Models;
using System.Net;

namespace NT.SDK.ExchangeRateProvider.Models.Options;


/// <summary>
/// Represents a generic wrapper for API responses related to Exrp operations.
/// </summary>
public class ExrpApiResponseWrapper : IResponseWrapper
{
    #region CTOR
    /// <summary>
    /// Initializes a new instance of the <see cref="ExrpApiResponseWrapper"/> class.
    /// </summary>
    public ExrpApiResponseWrapper()
    {

    }
    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the Exrp operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the result code of the Exrp operation.
    /// </summary>
    public HttpStatusCode? Code { get; set; }

    /// <summary>
    /// Gets or sets a message associated with the Exrp operation.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets a collection of errors associated with the Exrp operation.
    /// </summary>
    public ICollection<ApisErrorResponseData> Errors { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Adds an error to the collection of errors.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="details">Additional details about the error.</param>

    public void AddError(string code, string message, params ApisErrorResponseDetails[] details)
    {
        if (Errors is null)
            Errors = new List<ApisErrorResponseData>();
        Errors.Add(new ApisErrorResponseData { Code = code, Message = message, Details = details.ToList() });
    }


    /// <summary>
    /// Sets the collection of errors associated with the Exrp operation.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    public void SetErrors(List<ApisErrorResponseData> errors)
    {
        Errors = errors;
    }
    #endregion
}

/// <summary>
/// Represents a generic wrapper for API responses related to Exrp operations with additional data of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the additional data.</typeparam>
public class ExrpApiResponseWrapper<TResult> : ExrpApiResponseWrapper
    where TResult : IResponseBody
{
    /// <summary>
    /// Gets or sets the additional data associated with the Exrp operation.
    /// </summary>
    public TResult Data { get; set; }

    /// <summary>
    /// Sets the additional data associated with the Exrp operation.
    /// </summary>
    /// <param name="data">The additional data.</param>
    public void SetData(TResult data)
    {
        if (data != null)
        {
            Data = data;
        }
    }
}

