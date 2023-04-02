using Microsoft.Extensions.Configuration;
using static System.Boolean;


namespace Logging;


// Reads values from a json config file.
public static class Config
{
    // Defines the name of the optional config file.
    private const string Filename = "logging.json";

    // Config json file reader.
    private static readonly IConfiguration? Configuration;


    // Returns true if there's a config file.
    public static bool Exists()
    {
        return File.Exists( Filename );
    }

    
    // Create the config json file reader. 
    static Config()
    {
        Configuration = new ConfigurationBuilder().AddJsonFile( Filename, true, false ).Build();
    }


    // Get a string value from the config file.
    public static string GetString( string key )
    {
        return Configuration?[key] ?? string.Empty;
    }


    // Get a boolean value from the config file.
    public static bool GetBool( string key )
    {
        return Parse( Configuration?[key] ?? string.Empty );
    }
}