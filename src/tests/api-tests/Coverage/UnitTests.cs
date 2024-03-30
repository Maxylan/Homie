namespace HomieTests.Units;

[Collection(nameof(HomieTestCollection))]
public class UnitTests
{
    // Reference to our test broker.
    protected HomieBackofficeBroker Broker { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitTests"/> class with a reference to the broker.
    /// </summary>
    /// <param name="broker"></param>
    public UnitTests(HomieBackofficeBroker broker) => Broker = broker;

    [Fact]
    public void Test1()
    {

    }
}