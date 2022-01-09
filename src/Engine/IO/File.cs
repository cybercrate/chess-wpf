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
                BlackTaken.Add(new PieceId(ToStatus(lineText[charNumber]), false));
            }
        }

        lineText = sr.ReadLine();

        if (lineText is not null)
        {
            for (var charNumber = 0; charNumber < lineText.Length; charNumber += 2)
            {
                WhiteTaken.Add(new PieceId(ToStatus(lineText[charNumber])));
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
                    tw.Write(condition.Chessboard[row, column].White ? Token.White : Token.Black);
                }
            }
            
            tw.WriteLine();
        }

        // Writing other condition parameters into 10th line.
        var result = new char[7]
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
        char c;

        // Loading pieces.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                c = (char)tr.Read();
                
                // If there is no other char to read, sr.Read() returns invalid symbol.
                if (c is Token.Invalid)
                {
                    return null;
                }
                
                condition.Chessboard[row, column] = new PieceId(ToStatus(c));
                
                if (c is not Token.Empty)
                {
                    c = (char)tr.Read();
                    condition.Chessboard[row, column].White = c is Token.White;
                }
                else
                {
                    // Reads the empty space behind the empty square.
                    tr.Read();
                }
            }
            
            // Reads the return character.
            tr.Read();
            
            // Reads the new line character.
            tr.Read();
        }

        // Loading other parameters.
        c = (char)tr.Read();
        condition.WhiteOnTurn = c is Token.True;
        
        c = (char)tr.Read();
        condition.WhiteKingMoved = c is Token.True;
        
        c = (char)tr.Read();
        condition.WhiteSmallRookMoved = c is Token.True;
        
        c = (char)tr.Read();
        condition.WhiteLargeRookMoved = c is Token.True;
        
        c = (char)tr.Read();
        condition.BlackKingMoved = c is Token.True;
        
        c = (char)tr.Read();
        condition.BlackSmallRookMoved = c is Token.True;
        
        c = (char)tr.Read();
        condition.BlackLargeRookMoved = c is Token.True;
        
        c = (char)tr.Read();
        var c2 = (char)tr.Read();
        
        if (c2 is not Token.Return)
        {
            var c3 = (char)tr.Read();
            
            if (c3 is not Token.Return)
            {
                condition.Draw50 = int.Parse($"{c}{c2}{c3}");
                
                // Reads the return character.
                tr.Read();
            }
            else
            {
                condition.Draw50 = int.Parse($"{c}{c2}");
            }
        }
        else
        {
            condition.Draw50 = int.Parse(c.ToString());
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
