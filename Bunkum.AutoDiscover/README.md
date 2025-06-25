# Bunkum.AutoDiscover
An implementation of AutoDiscover for Bunkum.

See documentation here on this API here: https://docs.littlebigrefresh.com/autodiscover-api

## Usage

```csharp
using Bunkum.AutoDiscover.Extensions;

server.AddAutoDiscover(serverBrand: "Refresh", baseEndpoint: "/lbp");
```