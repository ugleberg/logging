using Elasticsearch.Net;


namespace Logging;


// Predefined log levels, Information is used as the default.
public enum Level
{
    Information,
    Warning,
    Error,
    Fatal
};


// Defines a log object that can be written to the console and Elasticsearch.
public class Log
{
    // Elasticsearch writer.
    private static readonly ElasticLowLevelClient? LowLevelClient;

    // Registers the configured environment name, and stamps each metrics with it.
    private static readonly string Environment;

    // Registers the computer's machine name and stamps each metric with it.
    private static readonly string Machine;

    // Holds the index prefix loaded from the config file.
    private static readonly string? Prefix;

    // Indicates whether a console as available.
    private static readonly bool Console;


    // Initializes the logging mechanism.
    static Log()
    {
        // Read the environment name from the system environment variable.
        Environment = System.Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" ) ?? "local";
        
        // Read the machine name.
        Machine = System.Environment.MachineName;

        // Skip the rest if there's no config file.
        if ( !Config.Exists() ) return;

        // Read the configuration from the config file.
        var serverUri = Config.GetString( "Logs:ServerUri" );
        var fingerprint = Config.GetString( "Logs:Fingerprint" );
        var username = Config.GetString( "Logs:Username" );
        var password = Config.GetString( "Logs:Password" );
        Prefix = Config.GetString( "Logs:Prefix" );
        Console = Config.GetBool( "Metrics:Console" );

        // Configure the Elasticsearch writer.
        var connectionConfiguration =
            new ConnectionConfiguration( new Uri( serverUri ) )
                .BasicAuthentication( username, password )
                .CertificateFingerprint( fingerprint );

        // Create the Elasticsearch writer.
        LowLevelClient = new ElasticLowLevelClient( connectionConfiguration );
    }


    // Set message and default values of the log object.
    public Log( string message )
    {
        _time = DateTime.Now;
        Id = Guid.NewGuid().ToString();
        Message = message;
        _level = Logging.Level.Information;
    }


    // Set exception as message and default values of the log object.
    public Log( Exception e )
    {
        _time = DateTime.Now;
        Id = Guid.NewGuid().ToString();
        Message = e.ToString();
        _level = Logging.Level.Error;
    }


    // Automatically assigned unique object id.
    #region string Id

    private string Id { get; }

    #endregion


    // Message set in the constructor.
    #region string Message

    private string Message { get; }

    #endregion


    // Timestamp set in the constructor.
    #region DateTime Time

    private readonly DateTime _time;

    #endregion


    // Set the log level, defaults to Level.Information.
    #region LogLevel

    private Level _level;

    public Log Level( Level value )
    {
        _level = value;
        return this;
    }

    public Level Level()
    {
        return _level;
    }

    #endregion


    // Set the Scope, for example to the Id of the root metric surrounding the body of code doing the logging.
    #region Scope

    private string? _scope;

    public Log Scope( string value )
    {
        _scope = value;
        return this;
    }

    public string? Scope()
    {
        return _scope;
    }

    #endregion


    // Set a data object to include structured data into the log object.
    #region object Data

    private object? _data;

    public Log Data( object value )
    {
        _data = value;
        return this;
    }

    #endregion

    
    // Write the log object.
    public void Write()
    {
        // Construct the serializable log object.
        var doc = new
        {
            Time = _time,
            Message,
            Level = _level.ToString(),
            Event = _scope,
            Data = _data,
            Environment,
            Machine,
            User = System.Environment.UserName
        };

        // Write the log on the console if ordered in the config file or if there's no config file at all.
        if ( Console || LowLevelClient == null )
            Logging.Console.Write( doc );

        // Skip the rest if not configured to write to Elasticsearch.
        if ( LowLevelClient == null ) return;

        // Write the log object to Elasticsearch.
        var response = LowLevelClient.Index<BytesResponse>( $"{Prefix}-{_time:yyyy-MM}", Id, PostData.Serializable( doc ) );

        // Write an error message containing the log object to the console if writing to Elasticsearch failed.
        if ( !response.Success )
            Logging.Console.Write( $"Error: unable to write log: {doc}" );
    }
}