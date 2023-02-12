using Bunkum.CustomHttpListener;

BunkumHttpListener listener = new(new Uri("http://127.0.0.1:10061"));

listener.StartListening();

while (true)
{
    await listener.WaitForConnectionAsync();
}