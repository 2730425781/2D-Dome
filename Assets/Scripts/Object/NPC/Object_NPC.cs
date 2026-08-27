using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.Universal;

/// <summary>
/// NPC 基类：统一处理所有 NPC 的通用表现——面向玩家翻转 + 头顶交互提示框浮动。
///
/// 为什么用基类而不是接口：翻转与提示框是每个 NPC 都会用到的纯表现逻辑，
/// 放进基类避免商人和铁匠等子类重复实现；子类只需补充各自的交互
/// （实现 IInteractable），职责更清晰。
///
/// 为什么用触发碰撞体（OnTriggerEnter2D/OnTriggerExit2D）而不是每帧轮询距离：
/// 走进/走出触发范围时自然拿到 player 引用，提示框的显示与隐藏完全由
/// 物理事件驱动，不需要额外状态管理。
/// </summary>
public class Object_NPC : MonoBehaviour
{
    // 由 OnTriggerEnter2D 填充的玩家引用，供翻转判断使用，子类也可能需要它
    protected Transform player;
    // 场景唯一 UI 引用，供子类（打开商店/铁匠面板）使用；
    // 在基类解析一次，避免每个子类重复 FindObjectByType
    protected UI ui;

    // 实际翻转的视觉模型；与触发碰撞体分离，方便"碰撞范围"和"外观"用不同子物体
    [SerializeField] private Transform npc;
    // 交互提示框。它必须放在世界空间，代码才能直接改 transform.position 让它浮动
    [SerializeField] private GameObject InteractToolTip;

    [Header("NPC提示框")]
    // 浮动速度：约每秒 1.6 个正弦周期，视觉上是"温和上下飘"而非急促抖动
    [SerializeField] private float floatSpeed = 1.6f;
    // 浮动幅度：0.2 世界单位，够明显又不至于遮挡角色
    [SerializeField] private float floatRange = 0.2f;
    // 浮动基准点：提示框每帧都在移动，必须在 Awake 记住初始位置，
    // 否则每次都在"当前位置+偏移"上叠加，提示框会越飘越远
    private Vector3 startPosition;
    // 当前朝向记录，保证只在方向真正变化时旋转一次（见 HandleNpcFlip）
    private bool facingRight = true;

    /// <summary>
    /// 用 Awake 而非 Start：隐藏提示框、记录浮动基准点必须在第一帧 Update
    /// 浮动之前完成；同时让子类在 Start 中使用 ui 时它已被解析。
    /// </summary>
    protected virtual void Awake()
    {
        // 场景里只有唯一一个 UI，直接按类型查找最省事
        ui = FindAnyObjectByType<UI>();
        startPosition = InteractToolTip.transform.position;
        // 默认隐藏，只有玩家进入触发范围才显示
        InteractToolTip.SetActive(false);
    }

    /// <summary>
    /// 翻转与浮动都依赖持续刷新（正弦随时间变化），且开销极小，用 Update 每帧驱动。
    /// </summary>
    protected virtual void Update()
    {
        HandleNpcFlip();
        HandleToolTip();
    }

    /// <summary>
    /// 用 Mathf.Sin 生成平滑往复的偏移而非自行累加位置：
    /// 正弦保证浮动首尾衔接无跳变；且只在提示框显示时才计算，隐藏时零开销。
    /// </summary>
    private void HandleToolTip()
    {
        if (InteractToolTip.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            InteractToolTip.transform.position = startPosition + new Vector3(0, yOffset);
        }
    }

    /// <summary>
    /// 用 Y 轴旋转 180° 翻转 2D 精灵而非缩放 x=-1：
    /// 旋转不会改变碰撞体与子物体的形状，避免缩放翻转让子物体变形。
    /// facingRight 标志保证只在朝向变化时旋转一次，否则每帧 Rotate 会让模型转个没完。
    /// </summary>
    private void HandleNpcFlip()
    {
        // 玩家可能还没进过触发区（player 为 null），或 Inspector 未挂 npc 模型；
        // Update 每帧都会走到这里，不判空会直接空引用崩溃
        if (player == null || npc == null)
        {
            return;
        }
        if (npc.position.x > player.position.x && facingRight)
        {
            npc.transform.Rotate(0, 180, 0);
            facingRight = false;
        }
        else if (npc.position.x < player.position.x && !facingRight)
        {
            npc.transform.Rotate(0, 180, 0);
            facingRight = true;
        }
    }

    /// <summary>
    /// 玩家进入触发范围：同时拿到 player 引用（供翻转用）并显示提示框，
    /// 提示时机与物理事件同步，无需额外轮询。
    /// 注意：这里无条件把碰撞者当作玩家，要求场景中只有玩家会碰到触发器。
    /// </summary>
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.transform;
        InteractToolTip.SetActive(true);
    }

    /// <summary>
    /// 离开范围即隐藏提示框。故意不把 player 置空：
    /// 玩家离开后 NPC 保持最后朝向，避免在触发区边界反复翻转抖动。
    /// </summary>
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        InteractToolTip.SetActive(false);
    }
}
