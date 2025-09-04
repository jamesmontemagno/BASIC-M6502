using Basic6502;

namespace Basic6502;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("MICROSOFT BASIC for 6502 - C# Edition");
        Console.WriteLine("Copyright 1976-1978 Microsoft Corporation");
        Console.WriteLine("Converted to C# and .NET");
        Console.WriteLine();

        var interpreter = new BasicInterpreter();
        interpreter.Run();
    }
}
