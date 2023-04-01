using Microsoft.Extensions.Configuration;
using static System.Boolean;


namespace Logging;


public static class Config
{
    private const string Filename = "logging.json";
    private static readonly IConfiguration? Configuration;


    public static bool Exists()
    {
        return File.Exists(Filename);
    }
    
    static Config()
    {
        Configuration = new ConfigurationBuilder().
            AddJsonFile( Filename, true, false ).
            Build();
    }
    
    
    public static string GetString( string key )
    {
        return Configuration?[ key ] ?? string.Empty;
    }
    
    
    public static bool GetBool( string key )
    {
        return Parse( Configuration?[ key ] ?? string.Empty );
    }
}