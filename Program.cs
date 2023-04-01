using Logging;
using Console = Logging.Console;


var metric = new Metric( "Main" ).Initiator( "Console" );
Console.Write( $"Starting logging demo, execution id = {metric.Id}" );

new Log( "Running logging demo" ).Scope( metric.Id ).Data( metric ).Write();

metric.Write();
Console.Write( $"Logging demo finished, elapsed {metric.Elapsed} msecs" );