
//12平均律
//ドイツ語
public enum EqualTemperament
{
    E1 = 0,
    F1,
    Fis1,
    G1,
    Gis1,
    A1,
    Ais1,
    H1,
    C2,
    Cis2,
    D2,
    Dis2,
    E2,
    F2,
    Fis2,
    G2,
    Gis2,
    A2,
    Ais2,
    H2,
    C3,
    Cis3,
    D3,
    Dis3,
    E3,
    F3,
    Fis3,
    G3,
    Gis3,
    A3,
    Ais3,
    H3,
    C4,
    Cis4,
    D4,
    Dis4,
    E4,
    F4,
    Fis4,
    G4,
    Gis4,
    A4,
    Ais4,
    Invalid,
}


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
    Natural = 0,
    Sharp = 1,
    Flatto = -1,
}

[System.Serializable]
public struct MusicalNoteInfo
{
    public Key currentKey;
    public string noteName;
    public NoteNames noteNameNotAccid;
    public Accidental accidental;
    public EqualTemperament equalTemperament;

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