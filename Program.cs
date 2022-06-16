using Logging;
using Console = Logging.Console;


Console.Write( "Starting logging demo" );

var obj = new { Var1 = "Var1", Var2 = "Var2" };
Console.Write( obj  );

new Log( "Just a simple info log entry" ).
    Level( Level.Information ).
    Event( Guid.NewGuid().ToString() ).
    Data( obj ).
    Write();

var metric = new Metric( "Main" ).
    Initiator( "Console" );

Thread.Sleep( 555 );

try
{
    Console.Input( "Enter a divisor value: ", out int divisor );
    var unused = 7 / divisor;
}
catch( Exception e )
{
    new Log( e ).
        Write();

    metric.Result( "Error" );
}

metric.Write();

Console.Write( $"Logging demo finished, elapsed {metric.Elapsed} msecs" );