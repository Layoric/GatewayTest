using ServiceStack;

namespace GatewayTest.ServiceModel;

public class Hello : IGet, IReturn<HelloResponse>
{
    public required string Name { get; set; }
    public int Mode { get; set; } = 0;
}

public class HelloResponse
{
    public required string Result { get; set; }
}


public class HelloGateway : IGet, IReturn<HelloGatewayResponse>
{
    public required string Name { get; set; }
    public int Mode { get; set; } = 0;
}

public class HelloGatewayResponse
{
    public required string Result { get; set; }
}
