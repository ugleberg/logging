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
    private static readonly ElasticLowLevelClient? LowLevelClient;
    
    // Registers the configured environment name, and stamps each metrics with it.
    private static readonly string Environment;

    // Registers the computer's machine name and stamps each metric with it.
    private static readonly string Machine;
    
    // TODO: Add comment...
    private static readonly string? Prefix;

    private static readonly bool Console;

    
    static Log()
    {
        if (Config.Exists())
        {
            var serverUri = Config.GetString("Logs:ServerUri");
            var fingerprint = Config.GetString("Logs:Fingerprint");
            var username = Config.GetString("Logs:Username");
            var password = Config.GetString("Logs:Password");
            Prefix = Config.GetString("Logs:Prefix");
            Console = Config.GetBool("Metrics:Console");

            var connectionConfiguration =
                new ConnectionConfiguration(new Uri(serverUri))
                    .BasicAuthentication(username, password)
                    .CertificateFingerprint(fingerprint);

            LowLevelClient = new ElasticLowLevelClient(connectionConfiguration);
        }

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
        
        if( Console || LowLevelClient == null)
            Logging.Console.Write(doc);

        if (LowLevelClient == null) return;
        
        var response = LowLevelClient.Index<BytesResponse>($"{Prefix}-{_time:yyyy-MM}", Id, PostData.Serializable(doc));

        if (!response.Success)
            Logging.Console.Write($"Error: unable to write log: {doc}");
    }
}