using System;
using System.Text;

namespace Basic6502;

/// <summary>
/// String functions and operations equivalent to the original 6502 BASIC string handling.
/// Provides string manipulation functions like LEN, MID$, LEFT$, RIGHT$, CHR$, ASC, etc.
/// </summary>
public static class BasicString
{
    /// <summary>
    /// Evaluates a string expression and returns the result.
    /// </summary>
    public static string EvaluateStringExpression(string expression, Dictionary<string, BasicVariable> variables)
    {
        expression = expression.Trim();
        
        if (string.IsNullOrEmpty(expression))
            return "";
            
        // Handle string literals
        if (expression.StartsWith("\"") && expression.EndsWith("\""))
        {
            return expression.Substring(1, expression.Length - 2);
        }
        
        // Handle string functions
        if (expression.StartsWith("CHR$("))
            return HandleStringFunction("CHR$", expression, variables);
        if (expression.StartsWith("LEFT$("))
            return HandleStringFunction("LEFT$", expression, variables);
        if (expression.StartsWith("RIGHT$("))
            return HandleStringFunction("RIGHT$", expression, variables);
        if (expression.StartsWith("MID$("))
            return HandleStringFunction("MID$", expression, variables);
        if (expression.StartsWith("STR$("))
            return HandleStringFunction("STR$", expression, variables);
        
        // Handle string variables
        if (variables.ContainsKey(expression) && variables[expression].IsString)
        {
            return variables[expression].StringValue;
        }
        
        // Handle simple variable names that might be string variables
        if (expression.EndsWith("$") && variables.ContainsKey(expression))
        {
            var variable = variables[expression];
            return variable.IsString ? variable.StringValue : "";
        }
        
        // Handle string concatenation with +
        var plusIndex = expression.IndexOf('+');
        if (plusIndex > 0)
        {
            var left = expression.Substring(0, plusIndex).Trim();
            var right = expression.Substring(plusIndex + 1).Trim();
            
            var leftStr = EvaluateStringExpression(left, variables);
            var rightStr = EvaluateStringExpression(right, variables);
            
            return leftStr + rightStr;
        }
        
        return "";
    }
    
    private static string HandleStringFunction(string functionName, string expression, Dictionary<string, BasicVariable> variables)
    {
        var openParen = expression.IndexOf('(');
        var closeParen = expression.LastIndexOf(')');
        
        if (openParen == -1 || closeParen == -1 || closeParen <= openParen)
            throw new BasicException("SYNTAX");
            
        var arguments = expression.Substring(openParen + 1, closeParen - openParen - 1);
        
        return functionName switch
        {
            "CHR$" => HandleChr(arguments, variables),
            "LEFT$" => HandleLeft(arguments, variables),
            "RIGHT$" => HandleRight(arguments, variables),
            "MID$" => HandleMid(arguments, variables),
            "STR$" => HandleStr(arguments, variables),
            _ => throw new BasicException("SYNTAX")
        };
    }
    
    private static string HandleChr(string arguments, Dictionary<string, BasicVariable> variables)
    {
        var value = BasicMath.EvaluateExpression(arguments, variables);
        var charCode = (int)Math.Round(value);
        
        if (charCode < 0 || charCode > 255)
            throw new BasicException("ILLEGAL QUANTITY");
            
        return ((char)charCode).ToString();
    }
    
    private static string HandleLeft(string arguments, Dictionary<string, BasicVariable> variables)
    {
        var parts = SplitArguments(arguments);
        if (parts.Length != 2)
            throw new BasicException("SYNTAX");
            
        var str = EvaluateStringExpression(parts[0], variables);
        var length = (int)BasicMath.EvaluateExpression(parts[1], variables);
        
        if (length < 0)
            return "";
        if (length >= str.Length)
            return str;
            
        return str.Substring(0, length);
    }
    
    private static string HandleRight(string arguments, Dictionary<string, BasicVariable> variables)
    {
        var parts = SplitArguments(arguments);
        if (parts.Length != 2)
            throw new BasicException("SYNTAX");
            
        var str = EvaluateStringExpression(parts[0], variables);
        var length = (int)BasicMath.EvaluateExpression(parts[1], variables);
        
        if (length < 0)
            return "";
        if (length >= str.Length)
            return str;
            
        return str.Substring(str.Length - length);
    }
    
    private static string HandleMid(string arguments, Dictionary<string, BasicVariable> variables)
    {
        var parts = SplitArguments(arguments);
        if (parts.Length < 2 || parts.Length > 3)
            throw new BasicException("SYNTAX");
            
        var str = EvaluateStringExpression(parts[0], variables);
        var start = (int)BasicMath.EvaluateExpression(parts[1], variables) - 1; // BASIC uses 1-based indexing
        
        if (start < 0 || start >= str.Length)
            return "";
            
        if (parts.Length == 2)
        {
            return str.Substring(start);
        }
        else
        {
            var length = (int)BasicMath.EvaluateExpression(parts[2], variables);
            if (length <= 0)
                return "";
            if (start + length > str.Length)
                length = str.Length - start;
                
            return str.Substring(start, length);
        }
    }
    
    private static string HandleStr(string arguments, Dictionary<string, BasicVariable> variables)
    {
        var value = BasicMath.EvaluateExpression(arguments, variables);
        return BasicMath.FormatNumber(value).Trim(); // Remove leading space for STR$
    }
    
    private static string[] SplitArguments(string arguments)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        int parenDepth = 0;
        bool inQuotes = false;
        
        for (int i = 0; i < arguments.Length; i++)
        {
            char c = arguments[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (!inQuotes && c == '(')
            {
                parenDepth++;
                current.Append(c);
            }
            else if (!inQuotes && c == ')')
            {
                parenDepth--;
                current.Append(c);
            }
            else if (!inQuotes && c == ',' && parenDepth == 0)
            {
                parts.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        
        if (current.Length > 0)
            parts.Add(current.ToString().Trim());
            
        return parts.ToArray();
    }
    
    /// <summary>
    /// Gets the length of a string (LEN function).
    /// </summary>
    public static int GetLength(string str)
    {
        return str?.Length ?? 0;
    }
    
    /// <summary>
    /// Gets the ASCII value of the first character in a string (ASC function).
    /// </summary>
    public static int GetAscii(string str)
    {
        if (string.IsNullOrEmpty(str))
            throw new BasicException("ILLEGAL QUANTITY");
            
        return (int)str[0];
    }
    
    /// <summary>
    /// Converts a string to uppercase.
    /// </summary>
    public static string ToUpper(string str)
    {
        return str?.ToUpperInvariant() ?? "";
    }
    
    /// <summary>
    /// Evaluates whether an expression should be treated as a string.
    /// </summary>
    public static bool IsStringExpression(string expression, Dictionary<string, BasicVariable>? variables = null)
    {
        expression = expression.Trim();
        
        // String literals
        if (expression.StartsWith("\"") && expression.EndsWith("\""))
            return true;
            
        // String functions
        if (expression.StartsWith("CHR$(") || 
            expression.StartsWith("LEFT$(") || 
            expression.StartsWith("RIGHT$(") || 
            expression.StartsWith("MID$(") || 
            expression.StartsWith("STR$("))
            return true;
            
        // String variables (ending with $)
        if (expression.EndsWith("$"))
            return true;
            
        // Check if it's a known string variable
        if (variables != null && variables.ContainsKey(expression) && variables[expression].IsString)
            return true;
            
        return false;
    }
}