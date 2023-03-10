using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
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
    [SerializeField] Clef m_currentClef = Clef.G;

    [SerializeField] List<Note> m_leftNote = new(14);
    [SerializeField] List<Note> m_rightNote = new(14);

    [SerializeField] MusicalNoteInfo m_firstNote = new() { noteName = "", accidental = Accidental.None };
    [SerializeField] MusicalNoteInfo m_secondNote = new() { noteName = "", accidental = Accidental.None };

    [SerializeField] MusicalInterval m_musicalInterval;

    // static readonly NoteNames G_CLEF_LOW = NoteNames.C2;
    // static readonly NoteNames F_CLEF_LOW = NoteNames.E1;

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
        foreach (var clef in m_clefList)
        {
            clef.gameObject.SetActive(false);
            if (m_currentClef == clef.clef)
            {
                clef.gameObject.SetActive(true);
            }
        }
    }

    void Awake()
    {
        SetClef();
        NoteAllHide();
        GenerateQuiz();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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


    EqualTemperament GetNoteName(Accidental accidental, string aplphabet)
    {
        int[] values = (int[])Enum.GetValues(typeof(EqualTemperament));
        int res = 0;
        //ドイツ読みに変換
        if(aplphabet == "B")
        {
            aplphabet = "H";
        }
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

    void ClacMusicalInterval(Note first, Note second, out MusicalInterval musicalInterval)
    {
        var max_degree = (EqualTemperament)Math.Max((int)first.noteInfo.equalTemperament, (int)second.noteInfo.equalTemperament);
        var min_degree = (EqualTemperament)Math.Min((int)first.noteInfo.equalTemperament, (int)second.noteInfo.equalTemperament);

        var degree = (max_degree - min_degree);

        musicalInterval = new() { quality = MusicalInterval.MusicalQuality.Major, interval = degree };
    }

    void GenerateQuiz()
    {
        var left_index = UnityEngine.Random.Range(0, m_leftNote.Count);
        var right_index = UnityEngine.Random.Range(0, m_rightNote.Count);
        var left_note_name = GetIndexToNoteName(left_index);
        var right_note_name = GetIndexToNoteName(right_index);

        Accidental left_accid = EnumCommon.Random<Accidental>((int)Accidental.Flatto, (int)Accidental.Sharp + 1);
        Accidental right_accid = EnumCommon.Random<Accidental>((int)Accidental.Flatto, (int)Accidental.Sharp + 1);

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

        left_temperament = GetNoteName(left_accid, left_alphabet_str);
        right_temperament = GetNoteName(right_accid, right_alphabet_str);

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

        ClacMusicalInterval(m_leftNote[left_index], m_rightNote[right_index], out m_musicalInterval);
    }
}
