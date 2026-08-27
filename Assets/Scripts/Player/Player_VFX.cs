using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家专属的特效组件（继承 Entity_VFX）。
/// 
/// 为什么单独派生 Player_VFX：残影（afterimage）是玩家冲刺等状态特有的表现，
/// 普通敌人不需要；继承让共用的受伤/元素特效逻辑留在基类，玩家独有逻辑收在子类。
/// 残影用协程驱动而非 Update：效果按需启动/停止，触发时还能干净地重启。
/// </summary>
public class Player_VFX : Entity_VFX
{
    // 残影间隔 0.05s（Range 限定 0.01~0.2s）：间隔太密会同时存在过多残影物体拖慢性能，
    // 太疏则拖不出连续的残影视觉；预制体是空白模板，实际精灵在生成时替换
    [Header("残影设置")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float imageEchoInterval = 0.05f;
    [SerializeField] private GameObject imageEchoPrefab;
    private Coroutine imageEchoCo;

    /// <summary>
    /// 在目标位置生成一次性特效：通用入口，供攻击命中、技能释放等场景复用。
    /// </summary>
    public void CreateEffectOf(GameObject effect, Transform target)
    {
        Instantiate(effect, target.position, Quaternion.identity);
    }

    /// <summary>
    /// 启动残影拖尾效果。
    /// 为什么先停掉旧协程再开新的：冲刺等技能可能连续触发，若叠加两个生成循环，
    /// 残影密度会翻倍且结束时机混乱；重启保证每次调用都从干净状态开始。
    /// </summary>
    public void DoImageEchoEffect(float duretion)
    {
        if (imageEchoCo != null)
        {
            StopCoroutine(imageEchoCo);
        }

        imageEchoCo = StartCoroutine(ImageEchoEffectCo(duretion));
    }

    /// <summary>
    /// 残影生成循环：duration 内每隔 imageEchoInterval 生成一个残影。
    /// 用 WaitForSeconds 固定节拍而非每帧累加 deltaTime：残影必须严格等间隔出现
    /// 拖尾才均匀；timeTracker 按间隔累加，与循环条件保持一致。
    /// </summary>
    private IEnumerator ImageEchoEffectCo(float duartion)
    {
        float timeTracker = 0;

        while (timeTracker < duartion)
        {
            CreateImageEcho();

            yield return new WaitForSeconds(imageEchoInterval);
            timeTracker += imageEchoInterval;
        }
    }

    /// <summary>
    /// 生成单个残影并把它的精灵替换为玩家当前动画帧的精灵。
    /// 为什么替换而不是在预制体里配好：残影要复刻玩家"此刻"的姿势（如冲刺帧），
    /// 用 sr.sprite 实时取当前帧，拖尾才能像玩家本体的连续快照。
    /// </summary>
    private void CreateImageEcho()
    {
        GameObject imageEcho = Instantiate(imageEchoPrefab, transform.position, transform.rotation);
        imageEcho.GetComponentInChildren<SpriteRenderer>().sprite = sr.sprite;      //把预制体中的精灵替换为Animator中的
    }
}
