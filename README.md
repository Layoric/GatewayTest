# web

.NET 8.0 Empty Web Template

I have slightly altered the template to demonstrate what I think is a problem with GatewayRequestFilters.

The Hello class now has an integer Mode property.

------------------------------------------------------------------------------------------------------------
https://localhost:5002/api/Hello?Name=matt&Mode=0

Setting the mode to 0 (or not setting it) causes the Hello service to return the HelloResponse, with the same Name that was passed in (this mirrors the original functionality). It should work fine.

------------------------------------------------------------------------------------------------------------
https://localhost:5002/api/Hello?Name=matt&Mode=1

Setting the mode to 1 causes the HelloService to call a second service via the Gateway, which appends ", with a Gateway detour" to the passed in name. This also should work.

------------------------------------------------------------------------------------------------------------
https://localhost:5002/api/Hello?Name=matt&Mode=2

Setting the mode to 2 causes the GatewayRequestFiltersAsync to kick in, which attempts to create a response object that reads $"Hello, {hello.Name}, short-circuited in the Request Filter". This response object is then written to the pipeline. 

This mode is not working. The return from Gateway.Send is null. I attempted to load symbols and debug. I can see the response.Dto being set, and I also saw the response.OutputStream getting populated, but somewhere along the line the OutputSteam gets disposed or cleared out.


