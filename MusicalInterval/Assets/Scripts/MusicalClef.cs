using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalClef : MonoBehaviour
{
    [SerializeField] Clef m_clef;

    public Clef clef { get {return m_clef;} private set {} }

}
