
//12平均律
//ドイツ語
public enum EqualTemperament
{
    Ces = -1, //例外...内部的にしか使わないから妥協、独自の音名使えば解決するが、なんか嫌だった
    C = 0,
    Cis,
    D,
    Dis,
    E,
    F,
    Fis,
    G,
    Gis,
    A,
    Ais,
    H,
    His
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
    None = 0,
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