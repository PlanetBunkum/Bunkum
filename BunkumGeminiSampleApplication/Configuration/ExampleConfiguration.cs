using Bunkum.Core.Configuration;

namespace BunkumGeminiSampleApplication.Configuration;

public class ExampleConfiguration : Config
{
    /// <summary>
    /// A number representing the schema version of your configuration.
    /// Start at 1, then change it when you make a change to your config class.
    /// </summary>
    public override int CurrentConfigVersion => 1;
    
    /// <summary>
    /// The configuration's current schema version. Do not touch this - you do not need to.
    /// </summary>
    public override int Version { get; set; }
    
    /// <summary>
    /// Migrate a configuration. Called when the config is loaded and the current version is newer than the one in the configuration.
    /// </summary>
    /// <param name="oldVer">The version of the configuration we are migrating.</param>
    /// <param name="oldConfig">The configuration we are migrating, stored as dynamic.</param>
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }

    public string ExampleVariable { get; set; } = "This is the default for the example variable.";
}