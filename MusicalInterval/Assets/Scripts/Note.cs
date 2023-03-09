using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    public enum Accidental //臨時記号
    {
        None,
        Sharp,
        Flatto
    }
    Image m_sharp = null;
    Image m_flatto = null;
    
    Accidental m_currentAccidental = Accidental.None;

    [SerializeField] MusicalNoteInfo m_noteInfo = new();

    public void InitMusicalInfo(MusicalNoteInfo musicalNoteInfo)
    {
        m_noteInfo = musicalNoteInfo;
    }

    void Awake()
    {
        m_sharp = transform.GetChild(0).GetComponent<Image>();
        m_flatto = transform.GetChild(1).GetComponent<Image>();
        UpdateAccidental();
    }

    void UpdateAccidental()
    {
        switch (m_currentAccidental)
        {
            case Accidental.None:
                m_sharp.gameObject.SetActive(false);
                m_flatto.gameObject.SetActive(false);
                break;
            case Accidental.Sharp:
                m_sharp.gameObject.SetActive(true);
                m_flatto.gameObject.SetActive(false);
                break;
            case Accidental.Flatto:
                m_sharp.gameObject.SetActive(false);
                m_flatto.gameObject.SetActive(true);
                break;
        }
    }

    public void SetAccidental(Accidental accidental)
    {
        m_currentAccidental = accidental;
        UpdateAccidental();
    }
}
