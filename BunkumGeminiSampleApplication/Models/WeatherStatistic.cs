using Newtonsoft.Json;

namespace BunkumGeminiSampleApplication.Models;

[JsonObject]
public struct WeatherStatistic
{
    public DateTimeOffset Time;
    public int Temperature;
    public string Description;
}