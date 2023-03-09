using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
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


public class GameManager : MonoBehaviour
{
    [SerializeField, ReadOnly] Key m_currentKey = Key.C;
    [SerializeField, ReadOnly] Clef m_currentClef = Clef.G;

    [SerializeField] List<Note> m_leftNote = new(14);
    [SerializeField] List<Note> m_rightNote = new(14);

    [SerializeField] MusicalNoteInfo m_firstNote = new(){ alphabet = MusicalAlphabet.C , noteName = NoteNames.C2};
    [SerializeField] MusicalNoteInfo m_secondNote = new(){ alphabet = MusicalAlphabet.C , noteName = NoteNames.C2};

    static readonly NoteNames G_CLEF_LOW = NoteNames.C2;
    static readonly NoteNames F_CLEF_LOW = NoteNames.E1;

    
    void Awake()
    {
        foreach (var note in m_leftNote)
        {
            note.gameObject.SetActive(false);
        }

        foreach (var note in m_rightNote)
        {
            note.gameObject.SetActive(false);
        }
       
        GenerateQuiz();
    }

    // Start is called before the first frame update
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //そもそもindexでいいのかわからんがとりあえずこのような形に
    //indexはト音記号の場合13 = A4。ヘ音記号の場合は13 = C3。
    //この関数で臨時記号は調整しない
    NoteNamesNotAccidental GetIndexToNoteName(int index)
    {
        int[] values = (int[])Enum.GetValues(typeof(NoteNamesNotAccidental));
        NoteNamesNotAccidental result = NoteNamesNotAccidental.Invalid;
        foreach(int value in values)
        {
            if(index == value)
            {
                result = (NoteNamesNotAccidental)value;
            }
        }

        switch (m_currentClef)
        {
            case Clef.G:
                result += 5 + 7; //ヘ音記号からト音記号に変換するには、五度上げるか四度下げる.+7はオクターブ
                break;
            default: break;  
        }

        return result;
    }


    void GenerateQuiz()
    {
        var left_index = UnityEngine.Random.Range(0, m_leftNote.Count);
        var right_index = UnityEngine.Random.Range(0, m_rightNote.Count);

        m_leftNote[left_index].gameObject.SetActive(true);
        m_leftNote[left_index].InitMusicalInfo(new()
        {
            alphabet = MusicalAlphabet.C,
            noteNameNotAccid = GetIndexToNoteName(left_index), 
            noteName = NoteNames.C2,
            currentKey = m_currentKey
        });

        m_rightNote[right_index].gameObject.SetActive(true);
        m_rightNote[right_index].InitMusicalInfo(new()
        {
            alphabet = MusicalAlphabet.C,
            noteNameNotAccid = GetIndexToNoteName(right_index), 
            noteName = NoteNames.C2,
            currentKey = m_currentKey
        });
    }
}
