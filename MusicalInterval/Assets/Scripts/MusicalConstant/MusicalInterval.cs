
[System.Serializable]
public struct MusicalInterval
{
    //長、短、増、減、完全などの結合辞
    //英語のWikipediaの音程のページにQualityと書いてあったのでこうなった
    [System.Serializable]
    public enum MusicalQuality
    {
        Major,              //長
        Minor,              //短
        Perfect,            //完全
        Augmented,          //増
        Diminished,         //減
        DoubleAugmented,    //重増
        DoubleDiminished,   //重減
    }

    public MusicalQuality quality;
    public int interval;
    public string intervalName { get {return quality.ToString() + interval.ToString() ;} private set {} }
}
