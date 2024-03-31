// (c) 2024 @Maxylan
namespace HomieTests.EndToEnd;

using Homie.Api.v1.TransferModels;
using HomieTests.Mocks.v1;
using Xunit.Sdk;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Arrange-Act-Assert (AAA) Structure:
/// 
///     Organize each test method following the AAA pattern:
///     Arrange: Set up the necessary preconditions and inputs.
///     Act: Perform the action being tested (e.g., make an HTTP request to an API endpoint).
///     Assert: Verify the expected outcome or behavior.
/// </remarks>
[Collection(nameof(HomieTestCollection))]
public class PlatformTests : TestSuite<PlatformsMock>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformTests"/> class.<br/>
    /// Part of <see cref="HomieTestCollection"/>'s EndToEnd test suite.
    /// </summary>
    /// <param name="broker"></param>
    public PlatformTests(PlatformsMock platformsMock, HomieBackofficeBroker broker) : base(platformsMock, broker) { }

    [Fact]
    [Trait("Category", "EndToEnd")]
    public async void GetAll()
    {
        // Arrange
        // ..

        // Act
        var (
            platforms, 
            responseMessage, 
            exception
        ) = await Mockup.GetAll();

        // Assert
        Assert.Null(exception);
        Assert.NotNull(responseMessage);
        Assert.True(responseMessage!.IsSuccessStatusCode);
        Assert.NotNull(platforms);
        Assert.NotEmpty(platforms);
    }
}