using System;

[Serializable]
public class AddVideoRequest 
{
    public int lessonId;
    public string video;
    public void Init(int _lessonId, string _video)
    {
        lessonId = _lessonId;
        video = _video;
    }
}

