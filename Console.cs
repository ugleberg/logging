using static System.Int32;


namespace Logging;


// Console writes to the console, and reads simple keyboard input.
public static class Console
{
    // Defines the timestamp format leading all output to the console.
    private const string TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
    
    // Determine if the session is running from the console (and not as a Windows service).
    private static readonly bool NoConsole = System.Console.LargestWindowWidth == 0;


    // Write text to the console.
    public static void Write( string message )
    {
        // Don't write if there's no console.
        if ( NoConsole ) return;

        // Create timestamp and write it along with the message.
        var timestamp = DateTime.Now.ToString( TimestampFormat );
        System.Console.WriteLine( $"{timestamp}{message}" );
    }


    // Write an object to the console.
    public static void Write( object data )
    {
        // Don't write if there's no console.
        if ( NoConsole ) return;

        // Create timestamp and write it along with the deserialized object.
        var timestamp = DateTime.Now.ToString( TimestampFormat );
        System.Console.WriteLine( $"{timestamp}{data}" );
    }


    // Read a text from the console.
    public static void ReadString( string message, out string? value )
    {
        // Don't read if there's no console.
        if ( NoConsole )
        {
            value = null;
            return;
        }

        // Prompt for console keyboard input. 
        System.Console.Write( message );
        value = System.Console.ReadLine() ?? "";
    }


    // Read a masked password from the console.
    public static void ReadPassword( string message, out string? value )
    {
        // Don't read if there's no console.
        if ( NoConsole )
        {
            value = null;
            return;
        }

        // Ensure non-null result.
        value = "";
        
        // Write the prompt for a password.
        System.Console.Write( message );
        
        // Keep reading/appending characters until user hits <Enter>.
        var keyEvent = System.Console.ReadKey( true );
        while ( keyEvent.Key != ConsoleKey.Enter )
        {
            // Check the entered key.
            if ( keyEvent.Key != ConsoleKey.Backspace )
            {
                // Write an asterisk to mask the entered character.
                System.Console.Write( "*" );
                
                // Append the entered character to the input.
                value += keyEvent.KeyChar;
            }
            else if ( keyEvent.Key == ConsoleKey.Backspace )
            {
                // Handle backspace by removing the last input character and the last written asterisk.
                if ( !string.IsNullOrEmpty( value ) )
                {
                    value = value[..^1];
                    var pos = System.Console.CursorLeft;
                    System.Console.SetCursorPosition( pos - 1, System.Console.CursorTop );
                    System.Console.Write( " " );
                    System.Console.SetCursorPosition( pos - 1, System.Console.CursorTop );
                }
            }

            keyEvent = System.Console.ReadKey( true );
        }

        // Write a new-line character to the console to move the cursor to the beginning of the next line.
        System.Console.WriteLine();
    }


    // Read an integer from the console.
    public static void ReadInt( string message, out int value )
    {
        // Ensure at least retuning zero.
        value = 0;
        
        // Don't read if there's no console.
        if ( NoConsole )
            return;

        // Prompt for console keyboard input. 
        System.Console.Write( message );
        var input = System.Console.ReadLine();
        
        // Convert text input to an integer.
        TryParse( input, out value );
    }
}