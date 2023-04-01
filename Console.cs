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
        System.Console.WriteLine( $"{ timestamp }{ message }" );
    }


    public static void Write( object data )
    {
        if( NoConsole ) return;

        var timestamp = DateTime.Now.ToString( TimestampFormat );
        System.Console.WriteLine( $"{ timestamp }{ data }" );
    }


    public static void ReadString( string message, out string value )
    {
        System.Console.Write( message );
        value = System.Console.ReadLine() ?? "";
    }


    public static void ReadPassword( string message, out string value )
    {
        value = "";
        System.Console.Write(message);
        var info = System.Console.ReadKey(true);
        while (info.Key != ConsoleKey.Enter)
        {
            if (info.Key != ConsoleKey.Backspace)
            {
                System.Console.Write("*");
                value += info.KeyChar;
            }
            else if (info.Key == ConsoleKey.Backspace)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = value[..^1];
                    var pos = System.Console.CursorLeft;
                    System.Console.SetCursorPosition(pos - 1, System.Console.CursorTop);
                    System.Console.Write(" ");
                    System.Console.SetCursorPosition(pos - 1, System.Console.CursorTop);
                }
            }

            info = System.Console.ReadKey(true);
        }

        // add a new line because user pressed enter at the end of their password
        System.Console.WriteLine();
    }


    public static void ReadInt( string message, out int value )
    {
        System.Console.Write( message );
        var input = System.Console.ReadLine();
        if( TryParse( input, result: out value ) ) return;
        value = 0;
    }
}