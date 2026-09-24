using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/音频数据/音频数据库", fileName = "音频数据库")]
public class AudioDataBaseSO : ScriptableObject
{
    public List<AudioClipData> player;
    public List<AudioClipData> ui;

    [Header("音乐列表")]
    public List<AudioClipData> mainMenuMusic;
    public List<AudioClipData> levelMusic;

    private Dictionary<string, AudioClipData> clipCollection;

    private void OnEnable()
    {
        RebuildCollection();
    }

    /// <summary>重建 音频名 → 数据 的查找表。</summary>
    private void RebuildCollection()
    {
        clipCollection = new Dictionary<string, AudioClipData>();

        AddToCollection(player);
        AddToCollection(ui);
        AddToCollection(mainMenuMusic);
        AddToCollection(levelMusic);
    }

    public AudioClipData GetAudioData(string audioName)
    {
        if (string.IsNullOrEmpty(audioName)) return null;

        // 兜底：OnEnable 尚未执行、或域重载后缓存被清空时重建一次，
        // 否则这里会对 null 字典调用 TryGetValue 直接空引用
        if (clipCollection == null) RebuildCollection();

        return clipCollection.TryGetValue(audioName, out var data) ? data : null;
    }

    private void AddToCollection(List<AudioClipData> listToAdd)
    {
        // 列表未配置（策划只填了一部分）时不应空引用
        if (listToAdd == null) return;

        foreach (var data in listToAdd)
        {
            // audioName 为空的数据进不了查找表，跳过避免脏数据
            if (data == null || string.IsNullOrEmpty(data.audioName)) continue;

            if (!clipCollection.ContainsKey(data.audioName))
            {
                clipCollection.Add(data.audioName, data);
            }
        }
    }
}


[System.Serializable]
public class AudioClipData
{
    public string audioName;
    public List<AudioClip> clips = new List<AudioClip>();
    [Range(0, 1f)] public float volume;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Count)];
    }
}