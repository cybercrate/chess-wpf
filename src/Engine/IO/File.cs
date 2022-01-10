using Engine.Conditions;
using Engine.Pieces.Types;
using System.IO;

namespace Engine.IO;

/// <summary>
/// Serves for saving and loading chess condition in logical form (string form).
/// </summary>
internal class File
{
    public Condition Condition { get; private set; }
        
    public List<PieceId> BlackTaken { get; }
        
    public List<PieceId> WhiteTaken { get; }
        
    public Stack<Condition> History { get; }
        
    public File()
    {
        Condition = new Condition();
        BlackTaken = new List<PieceId>();
        WhiteTaken = new List<PieceId>();
        History = new Stack<Condition>();
    }

    public File(Condition condition, List<PieceId> blackTaken, List<PieceId> whiteTaken, Stack<Condition> history)
    {
        Condition = condition;
        BlackTaken = blackTaken;
        WhiteTaken = whiteTaken;
        History = history;
    }

    /// <summary>
    /// Saves the condition into file with name of current date and time.
    /// </summary>
    public void Save(string fileName)
    {
        if (Condition is null || BlackTaken is null || WhiteTaken is null || History is null)
        {
            throw new Exception("Missing data.");
        }

        // Writing data into the file.
        using StreamWriter sw = new(System.IO.File.Create(fileName));
        
        WriteBasicData(sw, Condition);
        
        // Writing taken pieces into 11th and 12th line.
        foreach (PieceId pieceId in BlackTaken)
        {
            sw.Write(ToChar(pieceId.Status));
            sw.Write(Token.Black);
        }
        
        sw.WriteLine();
        
        foreach (PieceId pieceId in WhiteTaken)
        {
            sw.Write(ToChar(pieceId.Status));
            sw.Write(Token.White);
        }
        
        sw.WriteLine();

        // Writing moves history into 13th to n-th line.
        foreach (Condition condition in History)
        {
            WriteBasicData(sw, condition);
        }
    }
    
    /// <summary>
    /// Loads condition and stores data in properties.
    /// </summary>
    /// <returns>Loaded successfully?</returns>
    public void Load(Stream stream)
    {
        Condition = new Condition();
        History.Clear();
        BlackTaken.Clear();
        WhiteTaken.Clear();

        using StreamReader sr = new(stream);
        
        Condition = LoadBasicData(sr)!;

        // Loading taken pieces in logical form.
        var lineText = sr.ReadLine();
        
        if (lineText is not null)
        {
            for (var charNumber = 0; charNumber < lineText.Length; charNumber += 2)
            {
                BlackTaken.Add(new PieceId(ToStatus(lineText[charNumber]), PieceColor.Black));
            }
        }

        lineText = sr.ReadLine();

        if (lineText is not null)
        {
            for (var charNumber = 0; charNumber < lineText.Length; charNumber += 2)
            {
                WhiteTaken.Add(new PieceId(ToStatus(lineText[charNumber]), PieceColor.White));
            }
        }

        // Loading executed moves history.
        List<Condition> reversedHistory = new();

        while (true)
        {
            var condition = LoadBasicData(sr);
            
            if (condition is null)
            {
                break;
            }

            reversedHistory.Add(condition);
        }

        for (var i = reversedHistory.Count - 1; i >= 0; i--)
        {
            History.Push(reversedHistory[i]);
        }
    }
    
    /// <summary>
    /// Writing basic data (pieces positions and other parameters) into 9 lines.
    /// </summary>
    private static void WriteBasicData(TextWriter tw, Condition condition)
    {
        // Writing pieces positions into 2nd through 9th line.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                if (condition.Chessboard[row, column].Status is Status.Empty)
                {
                    tw.Write($"{Token.Empty}]");
                }
                else
                {
                    tw.Write(ToChar(condition.Chessboard[row, column].Status));
                    tw.Write(condition.Chessboard[row, column].PieceColor is PieceColor.White
                        ? Token.White
                        : Token.Black);
                }
            }
            
            tw.WriteLine();
        }

        // Writing other condition parameters into 10th line.
        var result = new[]
        {
            condition.WhiteOnTurn ? Token.True : Token.False,
            condition.WhiteKingMoved ? Token.True : Token.False,
            condition.WhiteSmallRookMoved ? Token.True : Token.False,
            condition.WhiteLargeRookMoved ? Token.True : Token.False,
            condition.BlackKingMoved ? Token.True : Token.False,
            condition.BlackSmallRookMoved ? Token.True : Token.False,
            condition.BlackLargeRookMoved ? Token.True : Token.False
        };

        tw.WriteLine($"{new string(result)}{condition.Draw50}");
    }
    
    /// <summary>
    /// Loading basic data (pieces positions and other).
    /// </summary>
    private static Condition? LoadBasicData(TextReader tr)
    {
        Condition condition = new();
        char firstChar;

        // Loading pieces.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                firstChar = (char)tr.Read();
                
                // If an invalid character is read, subsequent reading will be interrupted immediately.
                if (firstChar is Token.Invalid)
                {
                    return null;
                }
                
                condition.Chessboard[row, column] = new PieceId(ToStatus(firstChar), PieceColor.White);
                
                if (firstChar is not Token.Empty)
                {
                    firstChar = (char)tr.Read();
                    
                    condition.Chessboard[row, column].PieceColor =
                        firstChar is Token.White ? PieceColor.White : PieceColor.Black;
                }
                else
                {
                    // Reads the empty space behind the empty square.
                    tr.Read();
                }
            }
            
            // Reads the return and new line characters.
            tr.Read();
            tr.Read();
        }

        // Loading additional configuration parameters.
        firstChar = (char)tr.Read();
        condition.WhiteOnTurn = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        condition.WhiteKingMoved = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        condition.WhiteSmallRookMoved = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        condition.WhiteLargeRookMoved = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        condition.BlackKingMoved = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        condition.BlackSmallRookMoved = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        condition.BlackLargeRookMoved = firstChar is Token.True;
        
        firstChar = (char)tr.Read();
        var secondChar = (char)tr.Read();
        
        if (secondChar is not Token.Return)
        {
            var thirdChar = (char)tr.Read();
            
            if (thirdChar is not Token.Return)
            {
                condition.Draw50 = int.Parse($"{firstChar}{secondChar}{thirdChar}");
                
                // Read the return character.
                tr.Read();
            }
            else
            {
                condition.Draw50 = int.Parse($"{firstChar}{secondChar}");
            }
        }
        else
        {
            condition.Draw50 = int.Parse(firstChar.ToString());
        }

        // Read the new line character.
        tr.Read();

        return condition;
    }

    private static char ToChar(Status status) => status switch
    {
        Status.Empty => Token.Empty,
        Status.EnPassant => Token.EnPassant,
        Status.King => Token.King,
        Status.Queen => Token.Queen,
        Status.Rook => Token.Rook,
        Status.Bishop => Token.Bishop,
        Status.Knight => Token.Knight,
        Status.Pawn => Token.Pawn,
        _ => throw new Exception("Unknown status.")
    };

    private static Status ToStatus(char symbol) => symbol switch
    {
        Token.Empty => Status.Empty,
        Token.EnPassant => Status.EnPassant,
        Token.King => Status.King,
        Token.Queen => Status.Queen,
        Token.Rook => Status.Rook,
        Token.Bishop => Status.Bishop,
        Token.Knight => Status.Knight,
        Token.Pawn => Status.Pawn,
        _ => throw new Exception("Missing data.")
    };
}
