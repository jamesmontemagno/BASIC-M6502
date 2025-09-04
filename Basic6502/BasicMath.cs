using System;

namespace Basic6502;

/// <summary>
/// Mathematical functions and operations equivalent to the original 6502 BASIC math package.
/// Provides floating point operations, mathematical functions, and expression evaluation.
/// </summary>
public static class BasicMath
{
    private static readonly Random _random = new();
    
    /// <summary>
    /// Evaluates a mathematical expression using the same precedence rules as the original BASIC.
    /// </summary>
    public static double EvaluateExpression(string expression, Dictionary<string, BasicVariable> variables)
    {
        // Remove spaces and convert to uppercase
        expression = expression.Replace(" ", "").ToUpperInvariant();
        
        if (string.IsNullOrEmpty(expression))
            return 0;
            
        // Handle parentheses first
        while (expression.Contains('('))
        {
            int lastOpen = expression.LastIndexOf('(');
            int firstClose = expression.IndexOf(')', lastOpen);
            
            if (firstClose == -1)
                throw new BasicException("SYNTAX");
                
            string subExpr = expression.Substring(lastOpen + 1, firstClose - lastOpen - 1);
            double subResult = EvaluateExpression(subExpr, variables);
            
            expression = expression.Substring(0, lastOpen) + subResult.ToString("G17") + expression.Substring(firstClose + 1);
        }
        
        return EvaluateSimpleExpression(expression, variables);
    }
    
    private static double EvaluateSimpleExpression(string expression, Dictionary<string, BasicVariable> variables)
    {
        // Handle unary minus
        if (expression.StartsWith("-"))
        {
            return -EvaluateSimpleExpression(expression.Substring(1), variables);
        }
        
        if (expression.StartsWith("+"))
        {
            return EvaluateSimpleExpression(expression.Substring(1), variables);
        }
        
        // Check for functions
        if (expression.StartsWith("ABS("))
            return HandleFunction("ABS", expression, variables);
        if (expression.StartsWith("INT("))
            return HandleFunction("INT", expression, variables);
        if (expression.StartsWith("SGN("))
            return HandleFunction("SGN", expression, variables);
        if (expression.StartsWith("SQR("))
            return HandleFunction("SQR", expression, variables);
        if (expression.StartsWith("RND("))
            return HandleFunction("RND", expression, variables);
        if (expression.StartsWith("SIN("))
            return HandleFunction("SIN", expression, variables);
        if (expression.StartsWith("COS("))
            return HandleFunction("COS", expression, variables);
        if (expression.StartsWith("TAN("))
            return HandleFunction("TAN", expression, variables);
        if (expression.StartsWith("LOG("))
            return HandleFunction("LOG", expression, variables);
        if (expression.StartsWith("EXP("))
            return HandleFunction("EXP", expression, variables);
        if (expression.StartsWith("ATN("))
            return HandleFunction("ATN", expression, variables);
        
        // Find the lowest precedence operator (rightmost)
        for (int precedence = 1; precedence <= 5; precedence++)
        {
            for (int i = expression.Length - 1; i >= 0; i--)
            {
                char op = expression[i];
                int opPrecedence = GetOperatorPrecedence(op);
                
                if (opPrecedence == precedence)
                {
                    string left = expression.Substring(0, i);
                    string right = expression.Substring(i + 1);
                    
                    if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
                        continue;
                        
                    double leftVal = EvaluateSimpleExpression(left, variables);
                    double rightVal = EvaluateSimpleExpression(right, variables);
                    
                    return op switch
                    {
                        '+' => leftVal + rightVal,
                        '-' => leftVal - rightVal,
                        '*' => leftVal * rightVal,
                        '/' => rightVal != 0 ? leftVal / rightVal : throw new BasicException("DIVISION BY ZERO"),
                        '^' => Math.Pow(leftVal, rightVal),
                        _ => throw new BasicException("SYNTAX")
                    };
                }
            }
        }
        
        // No operators found, must be a number or variable
        if (double.TryParse(expression, out double value))
            return value;
            
        if (variables.ContainsKey(expression))
            return variables[expression].Value;
            
        // Try to parse as a variable name (might be undefined)
        if (IsValidVariableName(expression))
        {
            // Uninitialized variable defaults to 0
            return 0;
        }
        
        throw new BasicException("SYNTAX");
    }
    
    private static int GetOperatorPrecedence(char op)
    {
        return op switch
        {
            '+' or '-' => 1,
            '*' or '/' => 2,
            '^' => 3,
            _ => 0
        };
    }
    
    private static double HandleFunction(string functionName, string expression, Dictionary<string, BasicVariable> variables)
    {
        int openParen = expression.IndexOf('(');
        int closeParen = expression.LastIndexOf(')');
        
        if (openParen == -1 || closeParen == -1 || closeParen <= openParen)
            throw new BasicException("SYNTAX");
            
        string argument = expression.Substring(openParen + 1, closeParen - openParen - 1);
        double argValue = EvaluateExpression(argument, variables);
        
        return functionName switch
        {
            "ABS" => Math.Abs(argValue),
            "INT" => Math.Floor(argValue),
            "SGN" => Math.Sign(argValue),
            "SQR" => argValue >= 0 ? Math.Sqrt(argValue) : throw new BasicException("ILLEGAL QUANTITY"),
            "RND" => argValue <= 0 ? _random.NextDouble() : _random.NextDouble(),
            "SIN" => Math.Sin(argValue),
            "COS" => Math.Cos(argValue),
            "TAN" => Math.Tan(argValue),
            "LOG" => argValue > 0 ? Math.Log(argValue) : throw new BasicException("ILLEGAL QUANTITY"),
            "EXP" => Math.Exp(argValue),
            "ATN" => Math.Atan(argValue),
            _ => throw new BasicException("SYNTAX")
        };
    }
    
    private static bool IsValidVariableName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
            
        if (!char.IsLetter(name[0]))
            return false;
            
        for (int i = 1; i < name.Length; i++)
        {
            if (!char.IsLetterOrDigit(name[i]))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Formats a number for output similar to the original BASIC FOUT routine.
    /// </summary>
    public static string FormatNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return "0";
            
        // Handle zero
        if (Math.Abs(value) < 1e-15)
            return " 0";
            
        // Add leading space for positive numbers (BASIC convention)
        string prefix = value >= 0 ? " " : "";
        
        // Use scientific notation for very large or very small numbers
        if (Math.Abs(value) >= 1e9 || (Math.Abs(value) < 0.01 && Math.Abs(value) != 0))
        {
            return prefix + value.ToString("E");
        }
        
        // Regular formatting
        if (Math.Abs(value) >= 1)
        {
            // Integer part exists
            if (value == Math.Floor(value))
                return prefix + value.ToString("0");
            else
                return prefix + value.ToString("0.######");
        }
        else
        {
            // Fractional number
            return prefix + value.ToString("0.######");
        }
    }
}