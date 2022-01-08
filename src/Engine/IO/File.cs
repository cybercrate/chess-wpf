using Engine.Conditions;
using System.IO;
using System.Text;
using Engine.Pieces.Types;

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
        StringBuilder sb = new();
        sb.Append(condition.WhiteOnTurn.ToString()[0]);
        sb.Append(condition.WhiteKingMoved.ToString()[0]);
        sb.Append(condition.WhiteSmallRookMoved.ToString()[0]);
        sb.Append(condition.WhiteLargeRookMoved.ToString()[0]);
        sb.Append(condition.BlackKingMoved.ToString()[0]);
        sb.Append(condition.BlackSmallRookMoved.ToString()[0]);
        sb.Append(condition.BlackLargeRookMoved.ToString()[0]);
        sb.Append(condition.Draw50);

        tw.WriteLine(sb.ToString());
    }
    
    /// <summary>
    /// Loading basic data (pieces positions and other).
    /// </summary>
    private static Condition? LoadBasicData(TextReader sr)
    {
        Condition condition = new();
        char c;

        // Loading pieces.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                c = (char)sr.Read();
                
                // If there is no other char to read, sr.Read() returns '\uffff'.
                if (c is Token.Invalid)
                {
                    return null;
                }
                
                condition.Chessboard[row, column] = new PieceId(ToStatus(c));
                
                if (c is not Token.Empty)
                {
                    c = (char)sr.Read();
                    condition.Chessboard[row, column].White = c is Token.White;
                }
                else
                {
                    // Reads the empty space behind the empty square.
                    sr.Read();
                }
            }
            
            // Reads the return character.
            sr.Read();
            
            // Reads the new line character.
            sr.Read();
        }

        // Loading other parameters.
        c = (char)sr.Read();
        condition.WhiteOnTurn = c is Token.Result;
        
        c = (char)sr.Read();
        condition.WhiteKingMoved = c is Token.Result;
        
        c = (char)sr.Read();
        condition.WhiteSmallRookMoved = c is Token.Result;
        
        c = (char)sr.Read();
        condition.WhiteLargeRookMoved = c is Token.Result;
        
        c = (char)sr.Read();
        condition.BlackKingMoved = c is Token.Result;
        
        c = (char)sr.Read();
        condition.BlackSmallRookMoved = c is Token.Result;
        
        c = (char)sr.Read();
        condition.BlackLargeRookMoved = c is Token.Result;
        
        c = (char)sr.Read();
        var c2 = (char)sr.Read();
        
        if (c2 is not Token.Return)
        {
            var c3 = (char)sr.Read();
            
            if (c3 is not Token.Return)
            {
                condition.Draw50 = int.Parse($"{c.ToString()}{c2.ToString()}{c3.ToString()}");
                
                // Reads the return character.
                sr.Read();
            }
            else
            {
                condition.Draw50 = int.Parse($"{c.ToString()}{c2.ToString()}");
            }
        }
        else
        {
            condition.Draw50 = int.Parse(c.ToString());
        }

        // Read the new line character.
        sr.Read();

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
