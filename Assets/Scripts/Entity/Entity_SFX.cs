using UnityEngine;

public class Entity_SFX : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("音频名称")]
    // 传出距离：超过这个距离就听不到（用于 AudioManager 的距离衰减）
    // 注意字段名原为 sountDistance（笔误），已修正为 soundDistance。
    // 该字段此前从未被序列化到场景/预制体，改名不会丢配置。
    [SerializeField] private float soundDistance = 15f;
    [SerializeField] private PlayerSFXType attackHit;
    [SerializeField] private PlayerSFXType attackMiss;


    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayAttackHit()
    {
        AudioManager.instance.PlaySFX(attackHit.ToString(), audioSource, soundDistance);
    }

    public void PlayAttackMiss()
    {
        AudioManager.instance.PlaySFX(attackMiss.ToString(), audioSource, soundDistance);
    }
}
