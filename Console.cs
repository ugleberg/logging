using static System.Int32;

namespace Logging;

public static class Console
{
    private const string TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
    private static readonly bool NoConsole = System.Console.LargestWindowWidth == 0;


    public static void Write( string message )
    {
        if( NoConsole ) return;

        var timestamp = DateTime.Now.ToString( TimestampFormat );
        System.Console.WriteLine( $"{timestamp}{message}" );
    }


    public static void Write( object message )
    {
        if( NoConsole ) return;

        var timestamp = DateTime.Now.ToString( TimestampFormat );
        System.Console.WriteLine( $"{timestamp}{message}" );
    }


    public static void Input( string message, out string value )
    {
        System.Console.Write( message );
        value = System.Console.ReadLine() ?? "";
    }


    public static void Input( string message, out int value )
    {
        System.Console.Write( message );
        var input = System.Console.ReadLine();
        if( TryParse( input, result: out value ) ) return;
        value = 0;
    }
}