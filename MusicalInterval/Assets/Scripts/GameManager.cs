using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [System.Serializable]
    public struct NotePair
    {
        public MusicalAlphabet alphabet;
        public NoteNames noteName;
    }

    [SerializeField, ReadOnly] Key m_currentKey = Key.C;
    [SerializeField, ReadOnly] Clef m_currentClef = Clef.G;

    [SerializeField] List<Note> m_leftNote = new(14);
    [SerializeField] List<Note> m_rightNote = new(14);

    [SerializeField] NotePair m_firstNote = new(){ alphabet = MusicalAlphabet.C , noteName = NoteNames.C2};
    [SerializeField] NotePair m_secondNote = new(){ alphabet = MusicalAlphabet.C , noteName = NoteNames.C2};

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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void GenerateQuiz()
    {

    }
}
