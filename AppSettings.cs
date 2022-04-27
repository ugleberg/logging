using Microsoft.Extensions.Configuration;
using static System.Boolean;


namespace Logging;

public static class AppSettings
{
    private static readonly IConfiguration Configuration;
    
    static AppSettings()
    {
        Configuration = new ConfigurationBuilder().
            AddJsonFile( "appsettings.json", false, false ).
            Build();
    }
    
    
    public static string GetString( string key )
    {
        return Configuration[ key ] ?? throw new Exception( $"appsettings.json doesn't contain key: { key }" );
    }
    
    
    public static bool GetBool( string key )
    {
        return Parse( Configuration[ key ] ?? string.Empty );
    }
}