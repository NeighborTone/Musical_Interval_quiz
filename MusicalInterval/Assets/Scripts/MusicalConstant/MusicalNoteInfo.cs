
//とりあえず全部#で表す

public enum NoteNames
{
    E1 = 0,
    F1,
    G1,
    A1,
    B1,
    C2,
    D2,
    E2,
    F2,
    G2,
    A2,
    B2,
    C3,
    D3,
    E3,
    F3,
    G3,
    A3,
    B3,
    C4,
    D4,
    E4,
    F4,
    G4,
    A4,
    Invalid,
}

public enum Accidental //臨時記号
{
    None,
    Sharp,
    Flatto
}

[System.Serializable]
public struct MusicalNoteInfo
{
    public Key currentKey;
    public string musicalAlphabet;
    public NoteNames noteNameNotAccid;
    public Accidental accidental;

    static public void AccidentalToString(Accidental accidental, out string accidentalStr)
    {
        switch(accidental)
        {
            case Accidental.Sharp: accidentalStr = "#"; break;
            case Accidental.Flatto: accidentalStr = "♭"; break;
            default: accidentalStr = ""; break;
        }
    } 
}