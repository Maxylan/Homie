// (c) 2024 @Maxylan
namespace HomieTests.Mocks.v1;

using HomieTests;
using Microsoft.AspNetCore.Mvc.Testing;
using Homie.Api.v1.TransferModels;
using v1 = Homie.Api.v1;
using System.Net.Http.Json;

public partial class PlatformsMock : BaseMock
{
    /// <summary>
    /// Mock endpoints for v1/platforms
    /// </summary>
    public const string PlatformsEndpoint = v1.Version.Name + "/platforms";

    /// <summary>
    /// (Development) Gets all platforms.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<Endpoint<PlatformDTO[]?>> GetAll()
    {
        HttpResponseMessage? responseMessage = null;
        PlatformDTO[]? platforms = null;
        Exception? exception = null;

        try {
            responseMessage = await httpClient.GetAsync(PlatformsEndpoint);
            if (responseMessage.IsSuccessStatusCode)
            {
                platforms = await responseMessage.Content.ReadFromJsonAsync<PlatformDTO[]>();
            }
            else {
                // Handle error case
            }
        }
        catch (Exception e) {
            exception = e;
        }

        return (platforms, responseMessage, exception);
    }
}