// (c) 2024 @Maxylan

namespace HomieTests.Mocks.v1;

using v1 = Homie.Api.v1;

public interface IBaseMock
{
    public const string EndpointVersion = "v1";
}

public abstract class BaseMock: IBaseMock
{
    // Mock endpoints for v1/...
    public const string EndpointVersion = v1.Version.Name /* + "/..." */;

    private Lazy<HttpClient> _client = new Lazy<HttpClient>(
        () => HomieTests.HomieBackofficeBroker.webApplicationFactory.CreateClient()
    );
    protected HttpClient httpClient => _client.Value;
}

public readonly struct Endpoint<T> : IEquatable<(T, HttpResponseMessage?, Exception?)>
{
    public T Result { get; }
    public HttpResponseMessage? ResponseMessage { get; }
    public Exception? Exception { get; }

    // "Implicit" and "Explicit" conversions to/from the touple
    public static implicit operator Endpoint<T>((T, HttpResponseMessage?, Exception?) touple) => 
        new Endpoint<T>(touple) {};

    public Endpoint((T, HttpResponseMessage?, Exception?) result) => 
        (Result, ResponseMessage, Exception) = result;
    public Endpoint(T result, HttpResponseMessage? responseMessage, Exception? exception) {
        Result = result;
        ResponseMessage = responseMessage;
        Exception = exception;
    }

    public bool Equals((T, HttpResponseMessage?, Exception?) other) => 
        other.Equals((Result, ResponseMessage, Exception));

    /// <summary>
    /// Deconstructs the Endpoint Result into its components.
    /// </summary>
    /// <param name="platforms"></param>
    /// <param name="responseMessage"></param>
    /// <param name="exception"></param>
    internal void Deconstruct(out T platforms, out HttpResponseMessage? responseMessage, out Exception? exception)
    {
        platforms = Result;
        responseMessage = ResponseMessage;
        exception = Exception;
    }

}

/* // Couldn't get this to work.
public readonly struct EndpointResult<T> : IEquatable<ValueTask<(T, HttpResponseMessage?, Exception?)>>
{
    public T Result { get; }
    public HttpResponseMessage? ResponseMessage { get; }
    public Exception? Exception { get; }

    public EndpointResult((T, HttpResponseMessage?, Exception?) result) => 
        (Result, ResponseMessage, Exception) = result;

    // "Implicit" and "Explicit" conversions to/from `ValueTask`
    public static implicit operator ValueTask<(T, HttpResponseMessage?, Exception?)>(EndpointResult<T> task) => 
        new ValueTask<(T, HttpResponseMessage?, Exception?)>();
    public static explicit operator EndpointResult<T>(ValueTask<(T, HttpResponseMessage?, Exception?)> task) => 
        new EndpointResult<T>();

    public bool Equals(EndpointResult<T> other) => 
        (Result?.Equals(other.Result) ?? false) && ResponseMessage == other.ResponseMessage && Exception == other.Exception;

    public bool Equals(ValueTask<(T, HttpResponseMessage?, Exception?)> other) => 
        (Result, ResponseMessage, Exception).Equals(other.Result);
}
*/