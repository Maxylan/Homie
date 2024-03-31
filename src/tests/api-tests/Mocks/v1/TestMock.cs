namespace HomieTests.Mocks.v1;

using Homie;
using v1 = Homie.Api.v1;

public partial class HomieBackofficeBroker
{
    // Mock endpoints for v1/...
    public const string Endpoint = v1.Version.Name + "/...";

    /// <summary>
    /// Gets all ...
    /// </summary>
    /// <returns></returns>
    public async ValueTask<List<object>> GetAll()
    {
        // await httpClient;
        return new List<object>();
    }
}