using ServiceStack;
using GatewayTest.ServiceModel;

namespace GatewayTest.ServiceInterface;

public class MyServices : Service
{
    public HelloResponse Any(Hello request)
    {
        if (request.Mode > 0)
        {
            var grsp = base.Gateway.Send<HelloGatewayResponse>(
                new HelloGateway
                {
                    Name = request.Name,
                    Mode = request.Mode
                });

            var rsp = new HelloResponse { Result = grsp?.Result ?? "error occurred in GatewayRequestFilter"};
            return rsp;
        }

        return new HelloResponse { Result = $"Hello, {request.Name}!" };
    }

    public HelloGatewayResponse Any(HelloGateway request)
    {
        return new HelloGatewayResponse { Result = $"Hello, {request.Name}, with a Gateway detour" };
    }
}