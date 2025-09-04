# BASIC-M6502 C# Edition

A complete conversion of Microsoft BASIC for the 6502 microprocessor from assembly language to C# and .NET 8.0.

## About

This project converts the historic Microsoft BASIC interpreter (Version 1.1, originally written in 6502 assembly language) to modern C# while maintaining full compatibility with the original BASIC language syntax and behavior.

The original 6955-line assembly source code has been carefully analyzed and converted to object-oriented C# code with the following architecture:

- **BasicInterpreter**: Main interpreter engine
- **BasicMath**: Mathematical expression evaluation and functions  
- **BasicString**: String handling and string functions
- **Program**: Entry point and console interface

## Building and Running

```bash
cd Basic6502
dotnet build
dotnet run
```

## Features

### Core Commands
- `RUN` - Execute the program
- `LIST` - Display program lines
- `NEW` - Clear the program and variables
- `END` - End program execution
- `STOP` - Stop program execution

### Program Flow
- `GOTO line` - Jump to line number
- `GOSUB line` / `RETURN` - Subroutine calls
- `FOR variable = start TO end [STEP increment]` / `NEXT variable` - Loops
- `IF condition THEN statement` - Conditionals

### Variables and Assignment
- Numeric variables: `A`, `B`, `X1`, etc.
- String variables: `A$`, `NAME$`, etc.
- Assignment: `LET A = 5` or `A = 5`
- String assignment: `A$ = "HELLO"`

### Input/Output
- `PRINT expression[;expression]...` - Output with formatting
- `INPUT variable` - Input from user
- Print formatting: `;` (no space), `,` (tab to next zone)

### Mathematical Functions
- `ABS(x)` - Absolute value
- `INT(x)` - Integer part
- `SGN(x)` - Sign (-1, 0, or 1)
- `SQR(x)` - Square root
- `RND(x)` - Random number
- `SIN(x)`, `COS(x)`, `TAN(x)` - Trigonometric functions
- `LOG(x)` - Natural logarithm
- `EXP(x)` - e raised to power x
- `ATN(x)` - Arctangent

### String Functions
- `LEN(string)` - Length of string
- `ASC(string)` - ASCII value of first character
- `CHR$(n)` - Character with ASCII value n
- `LEFT$(string, n)` - Leftmost n characters
- `RIGHT$(string, n)` - Rightmost n characters
- `MID$(string, start[, length])` - Substring
- `STR$(number)` - Convert number to string
- `VAL(string)` - Convert string to number

### Operators
- Arithmetic: `+`, `-`, `*`, `/`, `^` (exponentiation)
- Comparison: `=`, `<`, `>`, `<=`, `>=`, `<>` (not equal)
- Proper operator precedence with parentheses support

## Example Programs

### Hello World
```basic
10 PRINT "HELLO, WORLD!"
20 END
```

### Loop Example
```basic
10 FOR I = 1 TO 10
20 PRINT I; " SQUARED IS "; I*I
30 NEXT I
40 END
```

### String Operations
```basic
10 A$ = "HELLO"
20 B$ = "WORLD"
21 PRINT A$; " "; B$
30 PRINT "LENGTH OF "; A$; " IS "; LEN(A$)
40 PRINT "FIRST LETTER: "; CHR$(ASC(A$))
50 END
```

### Mathematical Functions
```basic
10 PRINT "SQUARE ROOT OF 16 IS "; SQR(16)
20 PRINT "SINE OF PI/2 IS "; SIN(3.14159/2)
30 PRINT "RANDOM NUMBER: "; RND(1)
40 END
```

## Error Messages

The interpreter provides the same error messages as the original BASIC:
- `SYNTAX ERROR` - Invalid syntax
- `DIVISION BY ZERO ERROR` - Division by zero
- `ILLEGAL QUANTITY ERROR` - Invalid argument to function
- `NEXT WITHOUT FOR ERROR` - NEXT without matching FOR
- `RETURN WITHOUT GOSUB ERROR` - RETURN without GOSUB
- `UNDEF'D STATEMENT ERROR` - GOTO/GOSUB to non-existent line

## Historical Significance

This code represents a faithful conversion of one of the most historically significant pieces of software from the personal computer revolution. The original Microsoft BASIC for 6502 was:

- The foundation software for early personal computers (Apple II, Commodore PET, etc.)
- Microsoft's first major commercial success
- The interpreter that introduced millions to programming
- A masterpiece of efficient coding for 8-bit systems

This C# version preserves the behavior and feel of the original while bringing it to modern .NET platforms.

## Architecture Notes

The conversion maintains the original's design patterns while adapting to modern object-oriented principles:

- **Expression Evaluation**: Maintains the original's precedence-based parsing
- **Variable Storage**: Uses modern Dictionary collections instead of linked lists
- **Memory Management**: Leverages .NET garbage collection instead of manual memory management
- **String Handling**: Uses .NET strings while preserving BASIC string semantics
- **Math Functions**: Uses .NET Math library while maintaining BASIC function signatures

## License

This implementation is based on the original Microsoft BASIC source code and maintains compatibility with the original language specification.