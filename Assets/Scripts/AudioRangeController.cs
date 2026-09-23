using UnityEngine;

public class AudioRangeController : MonoBehaviour
{
    private AudioSource source;
    private Player player;
    private float maxVolume;
    private float minDistanceToHear = 12;

    private void Start()
    {
        player = Player.instance;
        source = GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogWarning("AudioRangeController 所在对象没有 AudioSource，脚本已失效：" + name);
            return;
        }

        maxVolume = source.volume;

        // 音高只在开始时随机一次。
        // 原写法把它放在 Update 里，等于每秒重随机约 60 次，
        // 声音会持续抖动、听起来像坏掉的磁带。
        source.pitch = Random.Range(0.8f, 1.2f);
    }

    private void Update()
    {
        if (player == null || source == null)
        {
            return;
        }

        float distance = Vector2.Distance(source.transform.position, player.transform.position);
        float t = Mathf.Clamp01(1 - (distance / minDistanceToHear));
        float targetVolume = Mathf.Lerp(0, maxVolume, t);

        // 音量按帧平滑插值是正确的（持续音源），保持原样
        source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * 3);
    }
}
