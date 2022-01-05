using Engine.Conditions;
using Engine.Pieces.Base;
using System.IO;
using System.Text;

namespace Engine.IO;

/// <summary>
/// Serves for saving and loading chess condition in logical form (string form).
/// </summary>
internal class File
{
    public Condition Condition { get; private set; }
        
    public List<PieceChar> BlackTaken { get; }
        
    public List<PieceChar> WhiteTaken { get; }
        
    public Stack<Condition> History { get; }
        
    public File()
    {
        Condition = new Condition();
        BlackTaken = new List<PieceChar>();
        WhiteTaken = new List<PieceChar>();
        History = new Stack<Condition>();
    }

    public File(Condition condition, List<PieceChar> blackTaken, List<PieceChar> whiteTaken, Stack<Condition> history)
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

        // Creating file
        var fs = System.IO.File.Create(fileName);

        // Writing data into the file
        using StreamWriter sw = new(fs);
        
        WriteBasicData(sw, Condition);
        
        // Writing taken pieces into 11th and 12th line.
        foreach (PieceChar pc in BlackTaken)
        {
            sw.Write(pc.Status);
            sw.Write('c');
        }
        
        sw.WriteLine();
        
        foreach (PieceChar pc in WhiteTaken)
        {
            sw.Write(pc.Status);
            sw.Write('b');
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
                BlackTaken.Add(new PieceChar(lineText[charNumber], false));
            }
        }

        lineText = sr.ReadLine();

        if (lineText is not null)
        {
            for (var charNumber = 0; charNumber < lineText.Length; charNumber += 2)
            {
                WhiteTaken.Add(new PieceChar(lineText[charNumber], true));
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
    private static void WriteBasicData(TextWriter sw, Condition condition)
    {
        // Writing pieces positions into 2nd through 9th line.
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 8; column++)
            {
                if (condition.Chessboard[row, column].Status is 'n')
                {
                    sw.Write(condition.Chessboard[row, column].Status + " ");
                }
                else
                {
                    sw.Write(condition.Chessboard[row, column].Status);
                    sw.Write(condition.Chessboard[row, column].White ? 'b' : 'c');
                }
            }
            sw.WriteLine();
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

        sw.WriteLine(sb.ToString());
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
                
                // If there is no other char to read, sr.Read() returns '\uffff'
                if (c is '\uffff')
                {
                    return null;
                }
                
                condition.Chessboard[row, column] = new PieceChar(c);
                
                if (c is not 'n')
                {
                    c = (char)sr.Read();
                    condition.Chessboard[row, column].White = c is 'b';
                }
                else
                {
                    // Reads the empty space behind the empty square ('n').
                    sr.Read();
                }
            }
            
            // Reads the return character.
            sr.Read();
            
            // Reads the new line character.
            sr.Read();
        }

        // Loading other parameters.
        // White on turn.
        c = (char)sr.Read();
        condition.WhiteOnTurn = c is 'T';
        
        // Other booleans.
        c = (char)sr.Read();
        condition.WhiteKingMoved = c is 'T';
        
        c = (char)sr.Read();
        condition.WhiteSmallRookMoved = c is 'T';
        
        c = (char)sr.Read();
        condition.WhiteLargeRookMoved = c is 'T';
        
        c = (char)sr.Read();
        condition.BlackKingMoved = c is 'T';
        
        c = (char)sr.Read();
        condition.BlackSmallRookMoved = c is 'T';
        
        c = (char)sr.Read();
        condition.BlackLargeRookMoved = c is 'T';
        
        c = (char)sr.Read();
        var c2 = (char)sr.Read();
        
        if (c2 != '\r')
        {
            var c3 = (char)sr.Read();
            
            if (c3 is not '\r')
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
}
