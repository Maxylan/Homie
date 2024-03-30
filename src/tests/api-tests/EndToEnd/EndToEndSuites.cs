namespace HomieTests.EndToEnd;

[Collection(nameof(HomieTestCollection))]
public abstract class TestSuite
{
    // Reference to the test broker.
    protected HomieBackofficeBroker Broker { get; }

    // One-liner constructor.
    public TestSuite(HomieBackofficeBroker broker) => Broker = broker;
}