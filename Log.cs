using Elasticsearch.Net;


namespace Logging;


public enum Level
{
    Information,
    Warning,
    Error,
    Fatal
};


public class Log
{
    // Low-level client used to write to Elastic.
    private static readonly ElasticLowLevelClient LowLevelClient;
    
    // Registers the configured environment name, and stamps each metrics with it.
    private static readonly string Environment;

    // Registers the computer's machine name and stamps each metric with it.
    private static readonly string Machine;
    
    // TODO: Add comment...
    private static readonly string Prefix;

    private static readonly bool Console;

    
    static Log()
    {
        var serverUri = AppSettings.GetString( "Logs:ServerUri" );
        var fingerprint = AppSettings.GetString( "Logs:Fingerprint" );
        var username = AppSettings.GetString( "Logs:Username" );
        var password = AppSettings.GetString( "Logs:Password" );
        Prefix = AppSettings.GetString( "Logs:Prefix" );
        Console = AppSettings.GetBool( "Metrics:Console" );

        var connectionConfiguration =
            new ConnectionConfiguration( new Uri( serverUri ) )
                .BasicAuthentication( username, password )
                .CertificateFingerprint( fingerprint );   
        
        LowLevelClient = new ElasticLowLevelClient( connectionConfiguration );
        Environment = System.Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" ) ?? "local";
        Machine = System.Environment.MachineName;
    }
    
    
    public Log( string message )
    {
        _time = DateTime.Now;
        Id = Guid.NewGuid().ToString();
        Message = message;
        _level = Logging.Level.Information;
    }

    
    public Log( Exception e )
    {
        _time = DateTime.Now;
        Id = Guid.NewGuid().ToString();
        Message = e.ToString();
        _level = Logging.Level.Error;
    }

    
    #region string Id

    private string Id { get; }

    #endregion

    
    #region string Message

    private string Message { get; }

    #endregion

 
    #region DateTime Time

    private readonly DateTime _time;

    #endregion
    

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
    

    #region Event

    private string? _event;

    public Log Event( string value )
    {
        _event = value;
        return this;
    }

    public string? Event()
    {
        return _event;
    }

    #endregion

    
    #region object Data

    private object? _data;
    
    public Log Data( object value )
    {
        _data = value;
        return this;
    }

    #endregion

    public void Write()
    {
        var year = _time.Year;
        var month = _time.Month;
        var index = $"{Prefix}-{year}-{month}";

        var doc = new
        {
            Time = _time,
            Message,
            Level = _level.ToString(),
            Event = _event,
            Data = _data,
            Environment,
            Machine,
            User = System.Environment.UserName
        };
        
        var response = LowLevelClient.Index< BytesResponse >( index, Id, PostData.Serializable( doc ) );

        if( response.Success )
        {
            if( Console )
                Logging.Console.Write( doc );

            return;
        }
        
        Logging.Console.Write( $"Error: unable to write log: { doc }" );
    }
}