using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    Image m_sharp = null;
    Image m_flatto = null;
    [SerializeField] MusicalNoteInfo m_noteInfo = new();

    public MusicalNoteInfo noteInfo { get {return m_noteInfo;} private set {} }

    public void InitMusicalInfo(MusicalNoteInfo musicalNoteInfo)
    {
        m_noteInfo = musicalNoteInfo;
        UpdateAccidental();
    }

    void Awake()
    {
        m_sharp = transform.GetChild(0).GetComponent<Image>();
        m_flatto = transform.GetChild(1).GetComponent<Image>();
    }

    void UpdateAccidental()
    {
        switch (m_noteInfo.accidental)
        {
            case Accidental.Natural:
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
}
