
[System.Serializable]
public struct MusicalInterval
{
    /// <summary>
    /// 長、短、増、減、完全などの結合辞。
    /// 英語のWikipediaの音程のページにQualityと書いてあったのでこうなった
    /// </summary>
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
        Tritone,            //三全音(増4,減5)
    }
    
    /// <summary>
    /// 結合辞
    /// </summary>
    public MusicalQuality quality;
    /// <summary>
    /// 度数
    /// </summary>
    public int interval;

    /// <summary>
    /// 音程を文字列で返す
    /// </summary>
    public string intervalName { get {return quality.ToString() + interval.ToString() ;} private set {} }
}
