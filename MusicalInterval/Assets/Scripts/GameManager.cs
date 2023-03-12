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
    [SerializeField] TextMeshProUGUI m_naturalIntervalText;
    [SerializeField] TextMeshProUGUI m_enharmonicIntervalText;
    [SerializeField] TextMeshProUGUI m_semitoneText;
    [SerializeField] TextMeshProUGUI m_octaveText;

    [SerializeField] Toggle m_accidToggle;
    [SerializeField] TMP_Dropdown m_clefDropdown;

    [SerializeField] List<Note> m_leftNote = new(14);
    [SerializeField] List<Note> m_rightNote = new(14);

    /// <summary>
    /// 音程
    /// </summary>
    MusicalInterval m_musicalInterval;
    
    /// <summary>
    /// 音程(異名同音用)
    /// </summary>
    MusicalInterval m_musicalIntervalEnharmonic;

    int m_octaveInterval = 0;


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
        foreach (int value in values)
        {
            var source_type = (EqualTemperament)value;
            if (equalTemperament == source_type)
            {
                switch (accidental)
                {
                    case Accidental.Natural:
                        res = value + (int)Accidental.Natural;
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
                    case Accidental.Natural:
                        res = value + (int)Accidental.Natural;
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

    void CreateMusicalInterval(int semitoneNum, out MusicalInterval musicalInterval)
    {
         switch(semitoneNum)
        {
            case 0:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 1 };     break;
            case 1:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 2 };     break;
            case 2:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 2 };     break;
            case 3:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 3 };     break;
            case 4:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 3 };     break;
            case 5:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 4 };     break;
            case 6:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Tritone, interval = 4 };     break;
            case 7:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 5 };     break;
            case 8:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 6 };     break;
            case 9:  musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 6 };     break;
            case 10: musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Minor,   interval = 7 };     break;
            case 11: musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major,   interval = 7 };     break;
            case 12: musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = 8 };     break;
            default: musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Perfect, interval = -999 };  break;
        }
    }

    void ClacMusicalInterval(Note first, Note second)
    {
        var max_degree = (EqualTemperament)Math.Max((int)first.noteInfo.equalTemperament, (int)second.noteInfo.equalTemperament);
        var min_degree = (EqualTemperament)Math.Min((int)first.noteInfo.equalTemperament, (int)second.noteInfo.equalTemperament);

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

        CreateMusicalInterval(semitone_num, out m_musicalInterval);

        {   // 異名同音を得る処理

            Note hi_note_info;
            Note low_note_info;

            if (max_degree == first.noteInfo.equalTemperament)
            {
                hi_note_info = first;
                low_note_info = second;
            }
            else
            {
                hi_note_info = second;
                low_note_info = first;
            }
            CalcEnharmonic(hi_note_info, low_note_info);
        }

        if(semitone_num == 0)
        {
            m_musicalInterval.quality = MusicalInterval.MusicalQuality.Unison;
        }

        m_octaveText.gameObject.SetActive(m_octaveInterval > 0 ? true : false);
        m_intervalText.SetText(m_musicalInterval.intervalName);
        m_semitoneText.SetText("半音{0}個", semitone_num);

    }
    
    bool IsAugmented(Accidental hiAccid, Accidental lowAccid)
    {
        return ((hiAccid == Accidental.Sharp && lowAccid == Accidental.Natural) || 
                 lowAccid == Accidental.Flatto && hiAccid == Accidental.Natural);
    }

    bool IsDiminished(Accidental hiAccid, Accidental lowAccid)
    {
        return ((hiAccid == Accidental.Flatto && lowAccid == Accidental.Natural) ||
                 lowAccid == Accidental.Sharp && hiAccid == Accidental.Natural);
    }

    bool IsSameInterval(Accidental hiAccid, Accidental lowAccid)
    {
        return ((hiAccid == Accidental.Sharp && lowAccid == Accidental.Sharp) ||
                 lowAccid == Accidental.Flatto && hiAccid == Accidental.Flatto);
    }

    bool IsDoubleAugmented(Accidental hiAccid, Accidental lowAccid)
    {
        return (hiAccid == Accidental.Sharp && lowAccid == Accidental.Flatto);
    }

    bool IsDoubleDiminished(Accidental hiAccid, Accidental lowAccid)
    {
        return (hiAccid == Accidental.Flatto && lowAccid == Accidental.Sharp);
    }

    /// <summary>
    /// どちらの音になんの記号がついているか取得し、その音程の異名同音を設定する
    /// </summary>
    void CalcEnharmonic(Note hi, Note low)
    {
        /* 変換の手順

        1.2音間の距離を測る(度数)
        2.音程の種類を判別する(長、短、完全)
        3.臨時記号で距離を調整する

        例えば五線譜上の音がC3、E#3なら増3度と言った感じ
        
        ===============長短系===============
        重減 <-> 減 <-> 短 <-> 長 <-> 増 <-> 重増
        ===============完全系===============
        重減 <-> 減 <-> 完全 <-> 増 <-> 重増
        
        で変化する
        */
        
        var hi_note_name_accid = hi.noteInfo.accidental;
        var low_note_name_accid = low.noteInfo.accidental;


        //高い音符の五線譜上の音(位置)を取得
        var hi_note_name_not_accid = GetNoteNameRemoveAccidental(hi_note_name_accid, hi.noteInfo.equalTemperament);

        //低い音符の五線譜上の音(位置)を取得
        var low_note_name_not_accid = GetNoteNameRemoveAccidental(low_note_name_accid, low.noteInfo.equalTemperament);

        //音程の性質を取得するために臨時記号がない状態で音程を算出
        MusicalInterval interval_not_accid;

        var semitone_num = ((int)hi_note_name_not_accid - (int)low_note_name_not_accid); //半音の数から算出

        //オクターブ内に納める
        if (semitone_num > 12)
        {
            semitone_num -= 12;
        }
        else if (semitone_num > 24)
        {
            semitone_num -= 24;
        }

        CreateMusicalInterval(Math.Abs(semitone_num), out interval_not_accid);

        if(interval_not_accid.quality == MusicalInterval.MusicalQuality.Perfect) //完全系
        {
            //高い音にのみシャープ。または低い音にのみフラットなら増○度
            if(IsAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Augmented;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にのみフラット。または低い音にのみシャープなら減○度
            else if (IsDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Diminished;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //両方の音にシャープ。または両方の音にフラットで完全○度(変わらない)
            else if(IsSameInterval(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Perfect;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
                if(semitone_num == 0)
                {
                    m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Unison;
                }
            }
            //高い音にシャープかつ低い音にフラットで重増○度
            else if (IsDoubleAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.DoubleAugmented;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にフラットかつ低い音にシャープで重減○度
            else if (IsDoubleDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.DoubleDiminished;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //臨時記号なし
            else
            {
                m_musicalIntervalEnharmonic = interval_not_accid;
                if(semitone_num == 0)
                {
                    m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Unison;
                }
            }
        }
        else if(interval_not_accid.quality == MusicalInterval.MusicalQuality.Major) //メジャー系
        {
            //高い音にのみシャープ。または低い音にのみフラットなら増○度
            if(IsAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Augmented;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にのみフラット。または低い音にのみシャープなら短○度
            else if (IsDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Minor;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //両方の音にシャープ。または両方の音にフラットで長○度(変わらない)
            else if(IsSameInterval(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Major;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にシャープかつ低い音にフラットで重増○度
            else if (IsDoubleAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.DoubleAugmented;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にフラットかつ低い音にシャープで減○度
            else if (IsDoubleDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Diminished;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //臨時記号なし
            else
            {
                m_musicalIntervalEnharmonic = interval_not_accid;
            }

        }
        else if(interval_not_accid.quality == MusicalInterval.MusicalQuality.Minor) //マイナー系
        {
            //高い音にのみシャープ。または低い音にのみフラットなら長○度
            if(IsAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Major;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にのみフラット。または低い音にのみシャープなら減○度
            else if (IsDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Diminished;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //両方の音にシャープ。または両方の音にフラットで長○度(変わらない)
            else if(IsSameInterval(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Minor;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にシャープかつ低い音にフラットで増○度
            else if (IsDoubleAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Augmented;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //高い音にフラットかつ低い音にシャープで重減○度
            else if (IsDoubleDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.DoubleDiminished;
                m_musicalIntervalEnharmonic.interval = interval_not_accid.interval;
            }
            //臨時記号なし
            else
            {
                m_musicalIntervalEnharmonic = interval_not_accid;
            } 
        }
        else //トライトーン(増4,減5)本当に以下の処理でいいのかかなり怪しい...プログラム的にはFとBのとき(増4度)にしか来ない？
        {
            //高い音にのみシャープ。または低い音にのみフラットなら重増4度？(完全5度の異名同音？)
            if(IsAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.DoubleAugmented;
                m_musicalIntervalEnharmonic.interval = 4;
            }
            //高い音にのみフラット。または低い音にのみシャープなら完全4度?
            else if (IsDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Perfect;
                m_musicalIntervalEnharmonic.interval = 4;
            }
            //両方の音にシャープ。または両方の音にフラット(変わらない)
            else if(IsSameInterval(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Tritone;
                m_musicalIntervalEnharmonic.interval = 4;
            }
            //高い音にシャープかつ低い音にフラットで完全4度から半音3つ分広がるので多分短6度でいいはず
            else if (IsDoubleAugmented(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Minor;
                m_musicalIntervalEnharmonic.interval = 6;

                //このような音程はまず出てこないので問題から除外する。
                InitQuiz();
                GenerateQuiz();
                
                return;
            }
            //高い音にフラットかつ低い音にシャープで長3度？(完全4度から半音3つ分狭くなるので多分長3度でいいはず)
            else if (IsDoubleDiminished(hi_note_name_accid, low_note_name_accid))
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Diminished;
                m_musicalIntervalEnharmonic.interval = 4;
            }            
            //臨時記号なし
            else
            {
                m_musicalIntervalEnharmonic.quality = MusicalInterval.MusicalQuality.Tritone;
                m_musicalIntervalEnharmonic.interval = 4;
            } 
        }

        if (semitone_num == 0)
        {
            interval_not_accid.quality = MusicalInterval.MusicalQuality.Unison;
        }

        m_naturalIntervalText.SetText("Natural:" + interval_not_accid.intervalName);
        m_enharmonicIntervalText.SetText("Enharmonic:" + m_musicalIntervalEnharmonic.intervalName);
    }

    void GenerateQuiz()
    {
  
// #if UNITY_EDITOR
//         //デバッグ用に特定の音を生成する
//         //ト音記号のみ
//         var left_index = 3;
//         var right_index = 6;
//         var left_note_name = NoteNames.F3;
//         var right_note_name = NoteNames.B3;
//         Accidental left_accid;
//         Accidental right_accid;

//         left_accid = Accidental.Sharp;
//         right_accid = Accidental.Natural;
        
// #else
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
            left_accid = Accidental.Natural;
            right_accid = Accidental.Natural;
        }

//#endif

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
