using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Basic6502;

/// <summary>
/// Main BASIC interpreter class - converts the assembly language BASIC interpreter
/// to C# while maintaining the same functionality and behavior.
/// </summary>
public class BasicInterpreter
{
    private readonly Dictionary<int, string> _program = new();
    private readonly Dictionary<string, BasicVariable> _variables = new();
    private readonly Dictionary<string, BasicArray> _arrays = new();
    private readonly Stack<LoopContext> _loopStack = new();
    private readonly Stack<GosubContext> _gosubStack = new();
    private readonly Random _random = new();
    
    private int _currentLine = 0;
    private bool _isRunning = false;
    private bool _isImmediate = false;
    private string _inputBuffer = string.Empty;
    private int _inputPosition = 0;
    
    // Terminal properties (from original assembly)
    private int _terminalWidth = 72;
    private int _printPosition = 0;
    
    public void Run()
    {
        PrintWelcomeMessage();
        CommandLoop();
    }
    
    private void PrintWelcomeMessage()
    {
        // Calculate available memory (simulated)
        var freeBytes = 32768 - _program.Count * 64; // Rough approximation
        Console.WriteLine($"{freeBytes} BYTES FREE");
        Console.WriteLine();
        Console.Write("READY");
        Console.WriteLine();
    }
    
    private void CommandLoop()
    {
        while (true)
        {
            try
            {
                Console.Write("");
                var input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                    continue;
                    
                ProcessLine(input);
            }
            catch (BasicException ex)
            {
                Console.WriteLine($"?{ex.Message} ERROR");
                if (!_isImmediate && _currentLine > 0)
                    Console.WriteLine($" IN {_currentLine}");
                StopProgram();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?SYSTEM ERROR: {ex.Message}");
                StopProgram();
            }
        }
    }
    
    private void ProcessLine(string input)
    {
        input = input.Trim().ToUpperInvariant();
        
        if (string.IsNullOrEmpty(input))
            return;
            
        // Check if line starts with a line number
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0 && int.TryParse(parts[0], out int lineNumber))
        {
            // Program line
            if (parts.Length > 1)
            {
                _program[lineNumber] = parts[1];
            }
            else
            {
                // Delete line
                _program.Remove(lineNumber);
            }
        }
        else
        {
            // Immediate mode command
            _isImmediate = true;
            _currentLine = 0;
            ExecuteStatement(input);
            _isImmediate = false;
            
            if (!_isRunning)
                Console.WriteLine("READY");
        }
    }
    
    private void ExecuteStatement(string statement)
    {
        if (string.IsNullOrEmpty(statement))
            return;
            
        var tokens = TokenizeStatement(statement);
        if (tokens.Count == 0)
            return;
            
        var command = tokens[0];
        
        switch (command)
        {
            case "RUN":
                ExecuteRun();
                break;
            case "LIST":
                ExecuteList(tokens);
                break;
            case "NEW":
                ExecuteNew();
                break;
            case "PRINT":
                ExecutePrint(tokens);
                break;
            case "LET":
                ExecuteLet(tokens);
                break;
            case "IF":
                ExecuteIf(tokens);
                break;
            case "GOTO":
                ExecuteGoto(tokens);
                break;
            case "FOR":
                ExecuteFor(tokens);
                break;
            case "NEXT":
                ExecuteNext(tokens);
                break;
            case "END":
                ExecuteEnd();
                break;
            case "STOP":
                ExecuteStop();
                break;
            case "INPUT":
                ExecuteInput(tokens);
                break;
            case "GOSUB":
                ExecuteGosub(tokens);
                break;
            case "RETURN":
                ExecuteReturn();
                break;
            case "DIM":
                ExecuteDim(tokens);
                break;
            case "DATA":
                // Data statements are processed during READ
                break;
            case "READ":
                ExecuteRead(tokens);
                break;
            case "RESTORE":
                ExecuteRestore();
                break;
            case "REM":
                // Do nothing for remarks
                break;
            default:
                // Check if it's an assignment without LET
                if (tokens.Count >= 3 && tokens[1] == "=")
                {
                    ExecuteAssignment(tokens);
                }
                else
                {
                    throw new BasicException("SYNTAX");
                }
                break;
        }
    }
    
    private List<string> TokenizeStatement(string statement)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        
        for (int i = 0; i < statement.Length; i++)
        {
            char c = statement[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (!inQuotes && (c == ' ' || c == ',' || c == ';' || c == ':'))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                
                if (c != ' ')
                    tokens.Add(c.ToString());
            }
            else if (!inQuotes && (c == '=' || c == '<' || c == '>'))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                
                // Handle two-character operators
                if (i + 1 < statement.Length)
                {
                    var next = statement[i + 1];
                    if ((c == '<' && (next == '>' || next == '=')) ||
                        (c == '>' && next == '='))
                    {
                        tokens.Add(c.ToString() + next);
                        i++; // Skip the next character
                        continue;
                    }
                }
                
                tokens.Add(c.ToString());
            }
            else
            {
                current.Append(c);
            }
        }
        
        if (current.Length > 0)
            tokens.Add(current.ToString());
            
        return tokens;
    }
    
    private void ExecuteRun()
    {
        _isRunning = true;
        _currentLine = 0;
        _loopStack.Clear();
        _gosubStack.Clear();
        
        // Start from the first line
        var sortedLines = _program.Keys.OrderBy(x => x).ToList();
        if (sortedLines.Count == 0)
            return;
            
        int lineIndex = 0;
        
        while (lineIndex < sortedLines.Count && _isRunning)
        {
            _currentLine = sortedLines[lineIndex];
            var originalLine = _currentLine;
            
            ExecuteStatement(_program[_currentLine]);
            
            // Check if execution jumped to a different line
            if (_currentLine == originalLine)
            {
                // Normal progression to next line
                lineIndex++;
            }
            else
            {
                // Jump occurred, find the new line index
                lineIndex = sortedLines.IndexOf(_currentLine);
                if (lineIndex == -1)
                {
                    // Line not found, stop execution
                    break;
                }
                lineIndex++; // Move to next line after the jumped-to line
            }
        }
        
        if (_isRunning)
        {
            Console.WriteLine("READY");
            _isRunning = false;
        }
    }
    
    private void ExecuteList(List<string> tokens)
    {
        var sortedLines = _program.Keys.OrderBy(x => x).ToList();
        
        foreach (var lineNumber in sortedLines)
        {
            Console.WriteLine($"{lineNumber} {_program[lineNumber]}");
        }
    }
    
    private void ExecuteNew()
    {
        _program.Clear();
        _variables.Clear();
        _arrays.Clear();
        _loopStack.Clear();
        _gosubStack.Clear();
        _isRunning = false;
    }
    
    private void ExecutePrint(List<string> tokens)
    {
        if (tokens.Count == 1)
        {
            // Just PRINT with no arguments
            Console.WriteLine();
            _printPosition = 0;
            return;
        }
        
        bool suppressNewline = false;
        int i = 1;
        
        while (i < tokens.Count)
        {
            var token = tokens[i];
            
            if (token == ";")
            {
                suppressNewline = true;
                i++;
                continue;
            }
            
            if (token == ",")
            {
                // Tab to next print zone (every 14 characters in original BASIC)
                var nextZone = ((_printPosition / 14) + 1) * 14;
                while (_printPosition < nextZone)
                {
                    Console.Write(" ");
                    _printPosition++;
                }
                suppressNewline = true;
                i++;
                continue;
            }
                
            if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                // String literal
                var text = token.Substring(1, token.Length - 2);
                Console.Write(text);
                _printPosition += text.Length;
                suppressNewline = false;
                i++;
            }
            else
            {
                // Expression - collect tokens until we hit a separator
                var exprTokens = new List<string>();
                while (i < tokens.Count && tokens[i] != ";" && tokens[i] != ",")
                {
                    exprTokens.Add(tokens[i]);
                    i++;
                }
                
                if (exprTokens.Count > 0)
                {
                    var exprStr = string.Join("", exprTokens);
                    
                    // Check if it's a string expression
                    if (BasicString.IsStringExpression(exprStr, _variables) || 
                        exprStr.Contains("CHR$(") || exprStr.Contains("LEFT$(") || 
                        exprStr.Contains("RIGHT$(") || exprStr.Contains("MID$(") || 
                        exprStr.Contains("STR$("))
                    {
                        var strValue = BasicString.EvaluateStringExpression(exprStr, _variables);
                        Console.Write(strValue);
                        _printPosition += strValue.Length;
                    }
                    else
                    {
                        var value = EvaluateExpression(exprTokens);
                        var formatted = BasicMath.FormatNumber(value);
                        Console.Write(formatted);
                        _printPosition += formatted.Length;
                    }
                    suppressNewline = false;
                }
            }
        }
        
        // Check if line ends with semicolon or comma
        if (tokens.Count > 1 && (tokens.Last() == ";" || tokens.Last() == ","))
        {
            suppressNewline = true;
        }
        
        if (!suppressNewline)
        {
            Console.WriteLine();
            _printPosition = 0;
        }
    }
    
    private void ExecuteLet(List<string> tokens)
    {
        if (tokens.Count < 4 || tokens[2] != "=")
            throw new BasicException("SYNTAX");
            
        var varName = tokens[1];
        var exprTokens = tokens.Skip(3).ToList();
        var exprStr = string.Join("", exprTokens);
        
        if (varName.EndsWith("$") || BasicString.IsStringExpression(exprStr, _variables) ||
            exprStr.Contains("CHR$(") || exprStr.Contains("LEFT$(") || 
            exprStr.Contains("RIGHT$(") || exprStr.Contains("MID$(") || 
            exprStr.Contains("STR$("))
        {
            // String assignment
            var stringValue = BasicString.EvaluateStringExpression(exprStr, _variables);
            _variables[varName] = new BasicVariable { StringValue = stringValue, IsString = true };
        }
        else
        {
            // Numeric assignment
            var value = EvaluateExpression(exprTokens);
            _variables[varName] = new BasicVariable { Value = value };
        }
    }
    
    private void ExecuteAssignment(List<string> tokens)
    {
        if (tokens.Count < 3 || tokens[1] != "=")
            throw new BasicException("SYNTAX");
            
        var varName = tokens[0];
        var exprTokens = tokens.Skip(2).ToList();
        var exprStr = string.Join("", exprTokens);
        
        if (varName.EndsWith("$") || BasicString.IsStringExpression(exprStr, _variables) ||
            exprStr.Contains("CHR$(") || exprStr.Contains("LEFT$(") || 
            exprStr.Contains("RIGHT$(") || exprStr.Contains("MID$(") || 
            exprStr.Contains("STR$("))
        {
            // String assignment
            var stringValue = BasicString.EvaluateStringExpression(exprStr, _variables);
            _variables[varName] = new BasicVariable { StringValue = stringValue, IsString = true };
        }
        else
        {
            // Numeric assignment
            var value = EvaluateExpression(exprTokens);
            _variables[varName] = new BasicVariable { Value = value };
        }
    }
    
    private void ExecuteIf(List<string> tokens)
    {
        // Find THEN keyword
        var thenIndex = tokens.FindIndex(t => t == "THEN");
        if (thenIndex == -1)
            throw new BasicException("SYNTAX");
            
        var conditionTokens = tokens.Skip(1).Take(thenIndex - 1).ToList();
        var thenTokens = tokens.Skip(thenIndex + 1).ToList();
        
        var condition = EvaluateCondition(conditionTokens);
        
        if (condition)
        {
            ExecuteStatement(string.Join(" ", thenTokens));
        }
    }
    
    private void ExecuteGoto(List<string> tokens)
    {
        if (tokens.Count != 2 || !int.TryParse(tokens[1], out int lineNumber))
            throw new BasicException("SYNTAX");
            
        if (!_program.ContainsKey(lineNumber))
            throw new BasicException("UNDEF'D STATEMENT");
            
        _currentLine = lineNumber;
        // Don't execute here - let the main loop handle it
    }
    
    private void ExecuteFor(List<string> tokens)
    {
        // FOR I = 1 TO 10 STEP 1
        if (tokens.Count < 6)
            throw new BasicException("SYNTAX");
            
        var varName = tokens[1];
        if (tokens[2] != "=")
            throw new BasicException("SYNTAX");
            
        var toIndex = tokens.FindIndex(t => t == "TO");
        if (toIndex == -1)
            throw new BasicException("SYNTAX");
            
        var stepIndex = tokens.FindIndex(t => t == "STEP");
        var startValue = EvaluateExpression(tokens.Skip(3).Take(toIndex - 3).ToList());
        var endValue = EvaluateExpression(tokens.Skip(toIndex + 1).Take(stepIndex == -1 ? tokens.Count - toIndex - 1 : stepIndex - toIndex - 1).ToList());
        var stepValue = stepIndex == -1 ? 1.0 : EvaluateExpression(tokens.Skip(stepIndex + 1).ToList());
        
        _variables[varName] = new BasicVariable { Value = startValue };
        
        var loopContext = new LoopContext
        {
            VariableName = varName,
            EndValue = endValue,
            StepValue = stepValue,
            LineNumber = _currentLine
        };
        
        _loopStack.Push(loopContext);
    }
    
    private void ExecuteNext(List<string> tokens)
    {
        if (_loopStack.Count == 0)
            throw new BasicException("NEXT WITHOUT FOR");
            
        var loop = _loopStack.Peek();
        var currentValue = _variables[loop.VariableName].Value;
        currentValue += loop.StepValue;
        _variables[loop.VariableName].Value = currentValue;
        
        bool continueLoop = loop.StepValue > 0 ? currentValue <= loop.EndValue : currentValue >= loop.EndValue;
        
        if (continueLoop)
        {
            // Continue loop - go back to line after FOR
            _currentLine = loop.LineNumber;
        }
        else
        {
            // Exit loop
            _loopStack.Pop();
        }
    }
    
    private void ExecuteEnd()
    {
        Console.WriteLine("READY");
        StopProgram();
    }
    
    private void ExecuteStop()
    {
        Console.WriteLine($"BREAK IN {_currentLine}");
        StopProgram();
    }
    
    private void ExecuteInput(List<string> tokens)
    {
        if (tokens.Count < 2)
            throw new BasicException("SYNTAX");
            
        Console.Write("? ");
        var input = Console.ReadLine() ?? "";
        
        if (double.TryParse(input, out double value))
        {
            _variables[tokens[1]] = new BasicVariable { Value = value };
        }
        else
        {
            _variables[tokens[1]] = new BasicVariable { StringValue = input, IsString = true };
        }
    }
    
    private void ExecuteGosub(List<string> tokens)
    {
        if (tokens.Count != 2 || !int.TryParse(tokens[1], out int lineNumber))
            throw new BasicException("SYNTAX");
            
        if (!_program.ContainsKey(lineNumber))
            throw new BasicException("UNDEF'D STATEMENT");
            
        // Push current line onto GOSUB stack
        _gosubStack.Push(new GosubContext { ReturnLine = _currentLine });
        
        // Jump to subroutine
        _currentLine = lineNumber;
        // Don't execute here - let the main loop handle it
    }
    
    private void ExecuteReturn()
    {
        if (_gosubStack.Count == 0)
            throw new BasicException("RETURN WITHOUT GOSUB");
            
        var context = _gosubStack.Pop();
        _currentLine = context.ReturnLine;
        // The main loop will continue from the next line after the GOSUB
    }
    
    private void ExecuteDim(List<string> tokens)
    {
        // DIM A(10), B(5,5)
        for (int i = 1; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token == ",") continue;
            
            var parenIndex = token.IndexOf('(');
            if (parenIndex == -1)
            {
                // Simple variable
                var varName = token;
                _variables[varName] = new BasicVariable { Value = 0 };
            }
            else
            {
                // Array variable
                var arrayName = token.Substring(0, parenIndex);
                var dimensionsStr = token.Substring(parenIndex + 1, token.LastIndexOf(')') - parenIndex - 1);
                var dimStrings = dimensionsStr.Split(',');
                var dimensions = dimStrings.Select(d => int.Parse(d.Trim())).ToArray();
                
                _arrays[arrayName] = new BasicArray { Dimensions = dimensions };
                
                // Initialize all elements to 0
                InitializeArray(arrayName, dimensions, new int[dimensions.Length], 0);
            }
        }
    }
    
    private void InitializeArray(string arrayName, int[] dimensions, int[] currentIndices, int dimensionIndex)
    {
        if (dimensionIndex >= dimensions.Length)
        {
            // Set the value at this position
            var key = string.Join(",", currentIndices);
            _arrays[arrayName].Values[key] = 0;
            return;
        }
        
        for (int i = 0; i <= dimensions[dimensionIndex]; i++)
        {
            currentIndices[dimensionIndex] = i;
            InitializeArray(arrayName, dimensions, currentIndices, dimensionIndex + 1);
        }
    }
    
    private void ExecuteRead(List<string> tokens)
    {
        // READ A, B, C
        var dataValues = GetDataValues();
        int dataIndex = 0;
        
        for (int i = 1; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token == ",") continue;
            
            if (dataIndex >= dataValues.Count)
                throw new BasicException("OUT OF DATA");
                
            if (double.TryParse(dataValues[dataIndex], out double value))
            {
                _variables[token] = new BasicVariable { Value = value };
            }
            else
            {
                _variables[token] = new BasicVariable { StringValue = dataValues[dataIndex], IsString = true };
            }
            
            dataIndex++;
        }
    }
    
    private List<string> GetDataValues()
    {
        var dataValues = new List<string>();
        
        foreach (var line in _program.Values)
        {
            if (line.Trim().StartsWith("DATA "))
            {
                var dataLine = line.Substring(5); // Remove "DATA "
                var values = dataLine.Split(',').Select(v => v.Trim()).ToList();
                dataValues.AddRange(values);
            }
        }
        
        return dataValues;
    }
    
    private void ExecuteRestore()
    {
        // Reset data pointer (in this simple implementation, DATA is re-read each time)
        // The original 6502 version had a data pointer that RESTORE would reset
    }
    
    private double EvaluateExpression(List<string> tokens)
    {
        if (tokens.Count == 0)
            return 0;
            
        // Rebuild the expression more carefully, preserving function calls
        var expression = ReconstructExpression(tokens);
        return BasicMath.EvaluateExpression(expression, _variables);
    }
    
    private string ReconstructExpression(List<string> tokens)
    {
        var result = new StringBuilder();
        
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            
            // Don't add spaces around parentheses or operators
            if (token == "(" || token == ")" || token == "+" || token == "-" || 
                token == "*" || token == "/" || token == "^" || token == "=" ||
                token == "<" || token == ">" || token == "<=" || token == ">=" || token == "<>")
            {
                result.Append(token);
            }
            else if (i > 0 && (tokens[i-1] == "(" || result.Length == 0))
            {
                // Don't add space after open parenthesis or at start
                result.Append(token);
            }
            else if (i < tokens.Count - 1 && tokens[i+1] == "(")
            {
                // Don't add space before open parenthesis (function calls)
                result.Append(token);
            }
            else
            {
                if (result.Length > 0)
                    result.Append(" ");
                result.Append(token);
            }
        }
        
        return result.ToString();
    }
    
    private bool EvaluateCondition(List<string> tokens)
    {
        var expression = string.Join("", tokens);
        
        // Look for comparison operators
        var operators = new[] { "<=", ">=", "<>", "=", "<", ">" };
        
        foreach (var op in operators)
        {
            var index = expression.IndexOf(op);
            if (index > 0 && index < expression.Length - op.Length)
            {
                var left = expression.Substring(0, index);
                var right = expression.Substring(index + op.Length);
                
                var leftVal = BasicMath.EvaluateExpression(left, _variables);
                var rightVal = BasicMath.EvaluateExpression(right, _variables);
                
                return op switch
                {
                    "=" => Math.Abs(leftVal - rightVal) < 0.000001,
                    "<" => leftVal < rightVal,
                    ">" => leftVal > rightVal,
                    "<=" => leftVal <= rightVal,
                    ">=" => leftVal >= rightVal,
                    "<>" => Math.Abs(leftVal - rightVal) >= 0.000001,
                    _ => false
                };
            }
        }
        
        // If no comparison operator, evaluate as true/false based on value
        var value = BasicMath.EvaluateExpression(expression, _variables);
        return Math.Abs(value) >= 0.000001;
    }
    
    private void StopProgram()
    {
        _isRunning = false;
        _currentLine = 0;
        _loopStack.Clear();
        _gosubStack.Clear();
    }
}

public class BasicVariable
{
    public double Value { get; set; }
    public string StringValue { get; set; } = "";
    public bool IsString { get; set; }
}

public class BasicArray
{
    public Dictionary<string, double> Values { get; set; } = new();
    public int[] Dimensions { get; set; } = Array.Empty<int>();
}

public class LoopContext
{
    public string VariableName { get; set; } = "";
    public double EndValue { get; set; }
    public double StepValue { get; set; }
    public int LineNumber { get; set; }
}

public class GosubContext
{
    public int ReturnLine { get; set; }
}

public class BasicException : Exception
{
    public BasicException(string message) : base(message) { }
}