# Bunkum

![Nuget downloads](https://img.shields.io/nuget/dt/Bunkum?color=blue&label=nuget%20downloads&logo=nuget)
![Version](https://img.shields.io/nuget/v/Bunkum?label=version)

A free and open-source protocol-agnostic request server built with flexibility yet ease of use in mind.

Bunkum gives you the freedom to do whatever you need it to do, even if that sort of means breaking the specification of the protocol you're working with.
But that's because not every client is compliant, either! ;)

## Supported protocols

We support a few protocols out of the box:

- HTTP(s)
- Gopher
- Gemini

You also have the option of writing your own protocol. Bunkum is flexible as long as you stick to the concept of request and response.

Keep in mind that Bunkum primarily uses HTTP as reference for its design. Concepts that work in HTTP might not play nicely with other protocols, and vice versa.
We do try to keep alternative protocol support working as best we can though, so if there's a major issue with how you expect a protocol to work then feel free to make an issue

## Notable projects using Bunkum

> **Note**
> If you'd like to show off your project here - don't hesitate to open a pull request!

[LittleBigRefresh/Refresh](https://github.com/LittleBigRefresh/Refresh) - A second-generation custom server for LittleBigPlanet that focuses on code quality and reliability.

[LittleBigRefresh/Refresh.GopherFrontend](https://github.com/LittleBigRefresh/Refresh.GopherFrontend) - A proxy for Refresh ApiV3 that allows Gopher (and soon Gemini) clients to explore Refresh instances. Great example of Bunkum's protocol-agnostic support!

[turecross321/SoundShapesServer](https://github.com/turecross321/SoundShapesServer) - A custom server for Sound Shapes

[turecross321/K.O.R-Server](https://github.com/turecross321/K.O.R-Server) - Official server for [K.O.R](https://t-u-r-e.itch.io/kor)
