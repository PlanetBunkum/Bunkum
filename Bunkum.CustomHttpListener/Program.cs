using System.Net;
using Bunkum.CustomHttpListener;

BunkumHttpListener listener = new(new Uri("http://127.0.0.1:10061"));

listener.StartListening();

while (true)
{
    await listener.WaitForConnectionAsync(async context =>
    {
        await context.SendResponse(HttpStatusCode.OK);
    });
}