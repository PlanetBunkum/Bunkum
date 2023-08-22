using Newtonsoft.Json;

namespace BunkumSampleApplication.Models;

[JsonObject]
public struct WeatherStatistic
{
    public DateTimeOffset Time;
    public int Temperature;
    public string Description;
}