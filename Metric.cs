using Elastic.Clients.Elasticsearch;
using Elastic.Transport;


namespace Logging;


// Defines a metric object that can be written to the console and Elasticsearch. 
public sealed class Metric
{
    // Elasticsearch writer.
    private static readonly ElasticsearchClient? ElasticsearchClient;

    // Registers the configured environment name, and stamps each metrics with it.
    private static readonly string Environment;

    // Registers the computer's machine name and stamps each metric with it.
    private static readonly string Machine;

    // Holds the index prefix loaded from the config file.
    private static readonly string? Prefix;

    // Indicates whether a console as available.
    private static readonly bool Console;


    // Initializes the metrics mechanism.
    static Metric()
    {
        // Read the environment name from the system environment variable.
        Environment = System.Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" ) ?? "local";
        
        // Read the machine name.
        Machine = System.Environment.MachineName;

        // Skip the rest if there's no config file.
        if ( !Config.Exists() ) return;

        // Read the configuration from the config file.
        var serverUri = Config.GetString( "Metrics:ServerUri" );
        var fingerprint = Config.GetString( "Metrics:Fingerprint" );
        var username = Config.GetString( "Metrics:Username" );
        var password = Config.GetString( "Metrics:Password" );
        Prefix = Config.GetString( "Metrics:Prefix" );
        Console = Config.GetBool( "Metrics:Console" );

        // Configure the Elasticsearch writer.
        var settings = new ElasticsearchClientSettings( new Uri( serverUri ) )
            .CertificateFingerprint( fingerprint )
            .Authentication( new BasicAuthentication( username, password ) );

        // Create the Elasticsearch writer.
        ElasticsearchClient = new ElasticsearchClient( settings );
    }


    // Set name and default values of the metric object.
    public Metric( string name )
    {
        _start = DateTime.Now;
        Id = Guid.NewGuid().ToString();
        Name = name;
        _result = "OK";
        _scope = Id;
    }


    // Automatically assigned unique id of the metric object.
    #region string Id

    public string Id { get; }

    #endregion


    // Metric's name.
    #region string Name

    private string Name { get; }

    #endregion


    // Holds the metric's start time.
    #region DateTime Start

    private readonly DateTime _start;

    #endregion


    // Get/set the retry value.
    #region Retry

    private int _retry;

    public Metric Retry( int value )
    {
        _retry = value;
        return this;
    }

    public int Retry()
    {
        return _retry;
    }

    #endregion


    // Get/set the Count value.
    #region Count

    private int _count;

    public Metric Count( int value )
    {
        _count = value;
        return this;
    }

    public int Count()
    {
        return _count;
    }

    #endregion


    // Get the calculated elapsed time after the metric is written.
    #region string Elapsed

    public int Elapsed { get; private set; }

    #endregion


    // Get/set the Result value, defaults to "OK".
    #region string Result

    private string? _result;

    public Metric Result( string? value )
    {
        _result = value;
        return this;
    }

    public string? Result()
    {
        return _result;
    }

    #endregion


    // Get/set the Reason value, usually only when the Result isn't "OK".
    #region string Reason

    private string? _reason;

    public Metric Reason( string? value )
    {
        _reason = value;
        return this;
    }

    public string? Reason()
    {
        return _reason;
    }

    #endregion


    // Get/set the Scope value, defaults to the metric's Id.
    #region string Scope

    private string _scope;

    public Metric Scope( string value )
    {
        _scope = value;
        return this;
    }

    public string Scope()
    {
        return _scope;
    }

    #endregion


    // Get/set the Initiator value.
    #region Initiator

    private string? _initiator;

    public Metric Initiator( string? value )
    {
        _initiator = value;
        return this;
    }

    public string? Initiator()
    {
        return _initiator;
    }

    #endregion


    // Get/set the Emitter value.
    #region Emitter

    private string? _emitter;

    public Metric Emitter( string? value )
    {
        _emitter = value;
        return this;
    }

    public string? Emitter()
    {
        return _emitter;
    }

    #endregion


    // Get/set the Event value.
    #region Event

    private string? _event;

    public Metric Event( string? value )
    {
        _event = value;
        return this;
    }

    public string? Event()
    {
        return _event;
    }

    #endregion


    // Get/set the Entity value.
    #region Entity

    private string? _entity;

    public Metric Entity( string? value )
    {
        _entity = value;
        return this;
    }

    public string? Entity()
    {
        return _entity;
    }

    #endregion


    // Write the metric object.
    public  void Write()
    {
        // Register the stop time.
        var stop = DateTime.Now;
        
        // Calculate the elapsed time in milliseconds.
        Elapsed = ( stop - _start ).Milliseconds;
        
        // Construct the serializable metric object.
        var doc = new
        {
            Id,
            Name,
            Start = _start,
            Stop = stop,
            Elapsed,
            Retry = _retry,
            Count = _count,
            Result = _result,
            Reason = _reason,
            Initiator = _initiator,
            Emitter = _emitter,
            Scope = _scope,
            Event = _event,
            Entity = _entity,
            Environment,
            Machine,
            User = System.Environment.UserName
        };

        // Write the metric on the console if ordered in the config file or if there's no config file at all.
        if ( Console || ElasticsearchClient == null )
            Logging.Console.Write( doc );

        // Skip the rest if not configured to write to Elasticsearch.
        if ( ElasticsearchClient == null ) return;

        // Write the metric to Elasticsearch.
        var response = ElasticsearchClient.Index( doc, $"{Prefix}-{_start:yyyy-MM}" );

        // Write an error message containing the metric object to the console if writing to Elasticsearch failed.
        if ( !response.IsValidResponse )
            Logging.Console.Write( $"Error: unable to write metric: {doc}" );
    }
}