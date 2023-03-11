using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

//編集不可のパラメータをInspectorに表示する
//https://kazupon.org/unity-no-edit-param-view-inspector/
public class ReadOnlyAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(_position, _property, _label);
        EditorGUI.EndDisabledGroup();
    }
}
#endif
//

//列挙型の乱数を取得する
//https://baba-s.hatenablog.com/entry/2014/02/20/000000
/// <summary>
/// 列挙型に関する汎用クラス
/// </summary>
public static class EnumCommon
{
    private static readonly System.Random mRandom = new System.Random();  // 乱数

    /// <summary>
    /// 指定された列挙型の値をランダムに返します
    /// </summary>
    public static T Random<T>(int min, int max)
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .OrderBy(c => mRandom.Next(min, max))
            .FirstOrDefault();
    }

    /// <summary>
    /// 指定された列挙型の値の数返します
    /// </summary>
    public static int GetLength<T>()
    {
        return Enum.GetValues(typeof(T)).Length;
    }
}
//
public class GameManager : MonoBehaviour
{
    [SerializeField] List<MusicalClef> m_clefList = new(3);
    [SerializeField] Key m_currentKey = Key.C;
    
    /// <summary>
    /// 現在の音部記号
    /// </summary>
    Clef m_currentClef = Clef.G;

    [SerializeField] GameObject m_settingCanvas;
    [SerializeField] TextMeshProUGUI m_intervalText;
    [SerializeField] TextMeshProUGUI m_semitoneText;
    [SerializeField] Toggle m_accidToggle;
    [SerializeField] TMP_Dropdown m_clefDropdown;

    [SerializeField] List<Note> m_leftNote = new(14);
    [SerializeField] List<Note> m_rightNote = new(14);

    /// <summary>
    /// 音程
    /// </summary>
    [SerializeField] MusicalInterval m_musicalInterval;
    
    /// <summary>
    /// 音程(異名同音用)
    /// </summary>
    [SerializeField] MusicalInterval m_musicalIntervalEnharmonic;

    [SerializeField, ReadOnly] int m_octaveInterval = 0;
    [SerializeField, ReadOnly] string m_intervalStr = ""; 

    public bool IsAccidental { get {return m_accidToggle.isOn;} private set{} }


    void NoteAllHide()
    {
        foreach (var note in m_leftNote)
        {
            note.gameObject.SetActive(false);
        }

        foreach (var note in m_rightNote)
        {
            note.gameObject.SetActive(false);
        }

    }

    void SetClef()
    {
        switch(m_clefDropdown.value)
        {
            case 0: m_currentClef = Clef.G; break;
            case 1: m_currentClef = Clef.C; break;
            case 2: m_currentClef = Clef.F; break;
        }
        foreach (var clef in m_clefList)
        {
            clef.gameObject.SetActive(false);
            if (m_currentClef == clef.clef)
            {
                clef.gameObject.SetActive(true);
            }
        }
    }

    public void OpenSettingCanvas()
    {
        m_settingCanvas.SetActive(true);
    }

    public void CloseSettingCanvas()
    {
        InitQuiz();
        m_settingCanvas.SetActive(false);
    }

    public void InitQuiz()
    {
        SetClef();
        NoteAllHide();
        GenerateQuiz();
    }

    void Awake()
    {
        InitQuiz();
    }

    //そもそもindexでいいのかわからんがとりあえずこのような形に
    //indexはト音記号の場合13 = A4。ヘ音記号の場合は13 = C3。
    //この関数で臨時記号は調整しない
    NoteNames GetIndexToNoteName(int index)
    {
        int[] values = (int[])Enum.GetValues(typeof(NoteNames));
        NoteNames result = NoteNames.Invalid;
        foreach (int value in values)
        {
            if (index == value)
            {
                result = (NoteNames)value;
            }
        }

        switch (m_currentClef)
        {
            case Clef.G:
                result += 5 + 7; //ヘ音記号からト音記号に変換するには、五度上げるか四度下げる.+7はオクターブ
                break;
            case Clef.C:
                result += 6; //ヘ音記号からハ音記号に変換するには、6度上げる
                break;
            default:
                break;
        }

        return result;
    }

    /// <summary>
    /// 臨時記号を消した音名を得る。
    /// </summary>
    EqualTemperament GetNoteNameRemoveAccidental(Accidental accidental, EqualTemperament equalTemperament)
    {
        int[] values = (int[])Enum.GetValues(typeof(EqualTemperament));
        int res = 0;
        //ドイツ読みに変換
        foreach (int value in values)
        {
            var source_type = (EqualTemperament)value;
            if (equalTemperament == source_type)
            {
                switch (accidental)
                {
                    case Accidental.None:
                        res = value + (int)Accidental.None;
                        break;
                    case Accidental.Sharp:
                        res = value - (int)Accidental.Sharp;
                        break;
                    case Accidental.Flatto:
                        res = value + ((int)Accidental.Flatto * -1);
                        break;
                }
            }
        }
        return (EqualTemperament)res;
    }

    /// <summary>
    /// 絶対的な音名を得る。#系で表す
    /// </summary>
    EqualTemperament GetNoteName(Accidental accidental, string aplphabet, string noteNum)
    {
        int[] values = (int[])Enum.GetValues(typeof(EqualTemperament));
        int res = 0;
        //ドイツ読みに変換
        if(aplphabet == "B")
        {
            aplphabet = "H";
        }
        aplphabet += noteNum;
        foreach (int value in values)
        {
            var source_type = (EqualTemperament)value;
            if (aplphabet == source_type.ToString())
            {
                switch (accidental)
                {
                    case Accidental.None:
                        res = value + (int)Accidental.None;
                        break;
                    case Accidental.Sharp:
                        res = value + (int)Accidental.Sharp;
                        break;
                    case Accidental.Flatto:
                        res = value + (int)Accidental.Flatto;
                        break;
                }
            }
        }
        return (EqualTemperament)res;
    }

    void ClacMusicalInterval(Note first, Note second)
    {
        var max_degree = (EqualTemperament)Math.Max((int)first.noteInfo.equalTemperament, (int)second.noteInfo.equalTemperament);
        var min_degree = (EqualTemperament)Math.Min((int)first.noteInfo.equalTemperament, (int)second.noteInfo.equalTemperament);

        Note hi_note_info;
        Note low_note_info;

        if(max_degree == first.noteInfo.equalTemperament)
        {
            hi_note_info = first;
            low_note_info = second;
        }
        else
        {
            hi_note_info = second;
            low_note_info = first;
        }

        var semitone_num = ((int)max_degree - (int)min_degree); //半音の数から算出
        m_octaveInterval = 0;
        //オクターブ内に納める
        if (semitone_num > 12)
        {
            m_octaveInterval = 1;
            semitone_num -= 12;
        }
        else if (semitone_num > 24)
        {
            m_octaveInterval = 2;
            semitone_num -= 24;
        }

        switch(semitone_num)
        {
            case 0:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 1 };     break;
            case 1:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 2 };     break;
            case 2:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 2 };     break;
            case 3:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 3 };     break;
            case 4:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 3 };     break;
            case 5:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 4 };     break;
            case 6:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Tritone, interval = 4 };     break;
            case 7:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 5 };     break;
            case 8:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 6 };     break;
            case 9:  m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 6 };     break;
            case 10: m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 7 };     break;
            case 11: m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 7 };     break;
            case 12: m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 8 };     break;
            default: m_musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = -999 };  break;
        }

        CalcEnharmonic(semitone_num, hi_note_info, low_note_info);

        m_intervalStr = m_musicalInterval.intervalName;
        m_intervalText.text = m_musicalInterval.intervalName;
        m_semitoneText.SetText("半音{0}個", semitone_num);

    }

    void CalcEnharmonic(int semitoneNum, Note hi, Note low)
    {
        //どちらの音になんの記号がついているか取得し、その音程の異名同音を設定する
        var hi_note_name_accid = hi.noteInfo.accidental;
        var low_note_name_accid = low.noteInfo.accidental;


        //高い音符の五線譜上の音(位置)を取得
        var hi_note_name_not_accid = GetNoteNameRemoveAccidental(hi_note_name_accid, hi.noteInfo.equalTemperament);
        
        //低い音符の五線譜上の音(位置)を取得
        var low_note_name_not_accid = GetNoteNameRemoveAccidental(low_note_name_accid, low.noteInfo.equalTemperament);
        


        switch (semitoneNum)
        {
            case 0:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 1 }; break;
            case 1:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Minor, interval = 2 }; break;
            case 2:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Major, interval = 2 }; break;
            case 3:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Minor, interval = 3 }; break;
            case 4:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Major, interval = 3 }; break;
            case 5:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 4 }; break;
            case 6:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Tritone, interval = 4 }; break;
            case 7:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 5 }; break;
            case 8:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Minor, interval = 6 }; break;
            case 9:  m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Major, interval = 6 }; break;
            case 10: m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Minor, interval = 7 }; break;
            case 11: m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Major, interval = 7 }; break;
            case 12: m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 8 }; break;
            default: m_musicalIntervalEnharmonic = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = -999 }; break;
        }
    }

    void GenerateQuiz()
    {
        var left_index = UnityEngine.Random.Range(0, m_leftNote.Count);
        var right_index = UnityEngine.Random.Range(0, m_rightNote.Count);
        var left_note_name = GetIndexToNoteName(left_index);
        var right_note_name = GetIndexToNoteName(right_index);

        Accidental left_accid;
        Accidental right_accid;

        if (IsAccidental)
        {
            left_accid = EnumCommon.Random<Accidental>((int)Accidental.Flatto, (int)Accidental.Sharp + 1);
            right_accid = EnumCommon.Random<Accidental>((int)Accidental.Flatto, (int)Accidental.Sharp + 1);
        }
        else
        {
            left_accid = Accidental.None;
            right_accid = Accidental.None;
        }


        EqualTemperament left_temperament;
        EqualTemperament right_temperament;

        string left_accid_str = "";
        string right_accid_str = "";

        var left_alphabet_str = left_note_name.ToString()[0].ToString();
        var left_note_num_str = left_note_name.ToString()[1].ToString();
        var right_alphabet_str = right_note_name.ToString()[0].ToString();
        var right_note_num_str = right_note_name.ToString()[1].ToString();

        MusicalNoteInfo.AccidentalToString(left_accid, out left_accid_str);
        MusicalNoteInfo.AccidentalToString(right_accid, out right_accid_str);

        var left_note = m_leftNote[left_index];
        var right_note = m_rightNote[right_index];

        left_note.gameObject.SetActive(true);
        right_note.gameObject.SetActive(true);

        left_temperament = GetNoteName(left_accid, left_alphabet_str, left_note_num_str);
        right_temperament = GetNoteName(right_accid, right_alphabet_str, right_note_num_str);


        // デバッグ用（見た目は変わらず内部の音程だけ変わるので注意）
        // left_temperament = GetNoteName(Accidental.None, "G", "4");
        // right_temperament = GetNoteName(Accidental.None, "F", "4");

        left_note.InitMusicalInfo(new()
        {
            noteNameNotAccid = GetIndexToNoteName(left_index),
            currentKey = m_currentKey,
            accidental = left_accid,
            noteName = left_alphabet_str + left_accid_str + left_note_num_str,
            equalTemperament = left_temperament

        });


        right_note.InitMusicalInfo(new()
        {
            noteNameNotAccid = GetIndexToNoteName(right_index),
            currentKey = m_currentKey,
            accidental = right_accid,
            noteName = right_alphabet_str + right_accid_str + right_note_num_str ,
            equalTemperament = right_temperament
        });

        ClacMusicalInterval(left_note, right_note);
    }
}
