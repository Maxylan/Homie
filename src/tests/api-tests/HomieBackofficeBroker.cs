using Homie;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomieTests;

/// <summary>
/// The entrypoint of the testing project.<br/>
/// Contains all testing definitions.
/// </summary>
public partial class HomieBackofficeBroker
{
    internal WebApplicationFactory<Backoffice> webApplicationFactory { get; }
    internal HttpClient httpClient { get; }

    // Construct the broker that will be performing our tests.
    public HomieBackofficeBroker()
    {
        webApplicationFactory = new WebApplicationFactory<Backoffice>();

        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
    }
}

[CollectionDefinition(nameof(HomieTestCollection))]
public class HomieTestCollection : ICollectionFixture<HomieBackofficeBroker> { }