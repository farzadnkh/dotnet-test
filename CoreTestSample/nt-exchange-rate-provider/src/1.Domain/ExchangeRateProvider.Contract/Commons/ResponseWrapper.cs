namespace ExchangeRateProvider.Contract.Commons;

public class ResponseWrapper<T>
{
    public bool IsSuccess { get; set; }
    public T Response { get; set; }
    public List<string> Errors { get; set; } = [];

    public Dictionary<string, object> Data { get; set; }

    public ResponseWrapper()
    {
        Errors = [];
        Data = [];
    }

    public ResponseWrapper(T response)
    {
        IsSuccess = true;
        Response = response;
        Errors = [];
        Data = [];
    }

    public ResponseWrapper(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors ?? [];
        Data = [];
    }

    public void AddError(string errors)
    {
        Errors.Add(errors);
        IsSuccess = false;
    }
}
