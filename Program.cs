using Logging;
using Console = Logging.Console;


// Start measuring, use the metric's id to identify the job run.
var metric = new Metric( "Main" ).Emitter( "Console app" );
Console.Write( $"Starting logging demo, job run = {metric.Id}" );

// Log the job run.
new Log( "Running logging demo" ).Scope( metric.Id ).Level( Level.Information ).Data( metric ).Write();

// Stop measuring, and write the metric.
metric.Write();
Console.Write( $"Logging demo finished, elapsed {metric.Elapsed} msecs" );