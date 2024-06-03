using GatewayTest.ServiceModel;
using ServiceStack.Text;
using System.Net;
using ServiceStack.Web;

[assembly: HostingStartup(typeof(GatewayTest.AppHost))]

namespace GatewayTest;

public class AppHost() : AppHostBase("GatewayTest"), IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services => {
            // Configure ASP.NET Core IOC Dependencies
            services.AddTransient<IServiceGatewayFactory>(provider => new MyInProcessGatewayFactory());
        });

    public override void Configure()
    {
        // Configure ServiceStack, Run custom logic after ASP.NET Core Startup
        SetConfig(new HostConfig { });
        
        

        this.GatewayRequestFiltersAsync.Add(async (req, dto) =>
        {
            if (dto is HelloGateway hello && hello.Mode == 2)
            {
                var res = req.Response;
                res.StatusCode = (int)HttpStatusCode.OK;
                res.ContentType = MimeTypes.Json;

                var responseDto = new HelloGatewayResponse 
                { 
                    Result = $"Hello, {hello.Name}, short-circuited in the Request Filter" 
                };

                await res.WriteToResponse(req, responseDto).ConfigAwait();
                await res.EndRequestAsync();
            }
        });  
    }
}

public class MyInProcessGatewayFactory : IServiceGatewayFactory
{
    public IServiceGateway GetServiceGateway(IRequest request)
    {
        return new MyInProcessGateway(request);
    }
}

public class MyInProcessGateway : InProcessServiceGateway
{
    public MyInProcessGateway(IRequest req) : base(req) { }

    protected override async Task<TResponse> ExecAsync<TResponse>(object request)
    {
        if (Request is IConvertRequest convertRequest)
            request = convertRequest.Convert(request);

        var appHost = HostContext.AppHost;
        var filterResult = await appHost.ApplyGatewayRequestFiltersAsync(Request, request);
        if (!filterResult && !Request.Response.IsClosed) 
            return default;
        
        // If req.Response is closed, assume populating the response is handled by the filter
        if (Request.Response.IsClosed)
            return await ConvertToResponseAsync<TResponse>(Request.Response).ConfigAwait();

        if (request is object[] requestDtos)
        {
            foreach (var requestDto in requestDtos)
            {
                await ExecValidatorsAsync(requestDto);
            }
        }
        else
        {
            await ExecValidatorsAsync(request);
        }

        var response = await HostContext.ServiceController.GatewayExecuteAsync(request, Request, applyFilters: false);

        var responseDto = await ConvertToResponseAsync<TResponse>(response).ConfigAwait();

        if (!await appHost.ApplyGatewayResponseFiltersAsync(Request, responseDto))
            return default;

        return responseDto;
    }
}