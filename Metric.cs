using Elasticsearch.Net;


namespace Logging;


public sealed class Metric
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

    
    static Metric()
    {
        var serverUri = AppSettings.GetString( "Metrics:ServerUri" );
        var fingerprint = AppSettings.GetString( "Metrics:Fingerprint" );
        var username = AppSettings.GetString( "Metrics:Username" );
        var password = AppSettings.GetString( "Metrics:Password" );
        Prefix = AppSettings.GetString( "Metrics:Prefix" );
        Console = AppSettings.GetBool( "Metrics:Console" );

        var connectionConfiguration =
            new ConnectionConfiguration( new Uri( serverUri ) )
                .BasicAuthentication( username, password )
                .CertificateFingerprint( fingerprint );   
        
        LowLevelClient = new ElasticLowLevelClient( connectionConfiguration );
        Environment = System.Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" ) ?? "local";
        Machine = System.Environment.MachineName;
    }
    
    
    public Metric( string name )
    {
        _start = DateTime.Now;
        Id = Guid.NewGuid().ToString();
        Name = name;
        _result = "OK";
        _scope = Id;
        _event = Id;
    }

    
    #region string Id

    private string Id { get; }

    #endregion

    
    #region string Name

    private string Name { get; }

    #endregion

 
    #region DateTime Start

    private readonly DateTime _start;

    #endregion

 
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


    #region string Elapsed

    public int Elapsed { get; private set; }

    #endregion

    
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


    public void Write()
    {
        var stop = DateTime.Now;
        Elapsed = ( stop - _start ).Milliseconds;
        var doc = new
        {
            Name,
            Start = _start,
            Stop = stop,
            Elapsed,
            Count = _count,
            Result = _result,
            Initiator = _initiator,
            Scope = _scope,
            Event = _event,
            Entity = _entity,
            Environment,
            Machine,
            User = System.Environment.UserName
        };
  
        var response = LowLevelClient.Index< BytesResponse >( $"{Prefix}-{_start:yyyy-MM}", Id, PostData.Serializable( doc ) );

        if( response.Success )
        {
            if( Console )
                Logging.Console.Write( doc );
            
            return;
        }
        
        Logging.Console.Write( $"Error: unable to write metric: {doc}" );
    }
}