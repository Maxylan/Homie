namespace HomieTests.EndToEnd;

[Collection(nameof(HomieTestCollection))]
public class EndToEndTests : TestSuite
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndToEndTests"/> class with a reference to the broker.
    /// </summary>
    /// <param name="broker"></param>
    public EndToEndTests(HomieBackofficeBroker broker) : base(broker) { }

    [Fact]
    public void Test1()
    {

    }
}