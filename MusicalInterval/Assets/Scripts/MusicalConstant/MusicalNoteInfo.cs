
//とりあえず全部#で表す

public enum NoteNamesNotAccidental
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
public enum NoteNames
{
    E1 = 0,
    F1,
    F_s_1,
    G1,
    G_s_1,
    A1,
    A_s_1,
    B1,
    C2,
    C_s_2,
    D2,
    D_s_2,
    E2,
    F2,
    F_s_2,
    G2,
    G_s_2,
    A2,
    A_s_2,
    B2,
    C3,
    D3,
    D_s_3,
    E3,
    F3,
    F_s_3,
    G3,
    G_s_3,
    A3,
    A_s_3,
    B3,
    C4,
    D4,
    D_s_4,
    E4,
    F4,
    F_s_4,
    G4,
    G_s_4,
    A4,
    A_s_4,
    Invalid,
}

[System.Serializable]
public struct MusicalNoteInfo
{
    public Key currentKey;
    public string musicalAlphabet;
    public NoteNamesNotAccidental noteNameNotAccid;
    public NoteNames trueNoteName;
}