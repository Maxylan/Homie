// (c) 2024 @Maxylan
namespace HomieTests;

using Homie;
using Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// The entrypoint of the testing project.<br/>
/// Contains all testing definitions.
/// </summary>
public partial class HomieBackofficeBroker
{
    public static WebApplicationFactory<Backoffice> webApplicationFactory { get; private set; } = null!;

    // Construct the broker that will be performing our tests.
    public HomieBackofficeBroker() {
        webApplicationFactory = new WebApplicationFactory<Backoffice>();
    }
}

[CollectionDefinition(nameof(HomieTestCollection))]
public class HomieTestCollection : ICollectionFixture<HomieBackofficeBroker> { }