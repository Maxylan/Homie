// (c) 2024 @Maxylan
namespace HomieTests.EndToEnd;

using HomieTests.Mocks.v1;

[Collection(nameof(HomieTestCollection))]
public abstract class TestSuite<T> where T : class, IBaseMock
{
    // Reference to the test broker.
    protected HomieBackofficeBroker? Broker { get; }
    
    // Reference to test mockup.
    protected T Mockup { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSuite{T}"/> class with a reference to the broker.
    /// </summary>
    /// <param name="broker"></param>
    /// <param name="mockup"></param>
    public TestSuite(T mockup, HomieBackofficeBroker broker) { 
        Mockup = mockup;
        Broker = broker;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSuite{T}"/> class.
    /// </summary>
    /// <param name="broker"></param>
    /// <param name="mockup"></param>
    public TestSuite(T mockup) { 
        Mockup = mockup;
    }
}