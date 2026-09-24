using System;
using UnityEngine;

/// <summary>
/// 音频设置数据：全局共用一份，独立于存档槽。
///
/// 为什么单独一个文件、而不是塞进 GameData：
/// GameData 是"每个存档槽一份"的数据，而音量属于玩家偏好设置——
/// 读取另一个存档不该把音量也一起换掉，主菜单在还没选存档时也应该能调音量。
/// 因此它写在自己的 AudioSettings.json 里，与槽位无关。
/// </summary>
[Serializable]
public class AudioSettingsData
{
    // 存的是滑块值(0~1)，不是混音器的 dB 值：
    // dB 是对数换算的结果，存滑块值才能在恢复时原样还原滑块位置，
    // 以后想改换算公式（比如换掉 Log10 系数）也不会让旧存档里的数值失去意义。
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
}
