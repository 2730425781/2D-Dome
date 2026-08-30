using UnityEngine;

/// <summary>
/// 物品效果的抽象基类：所有具体效果（治疗、吸血、Buff、技能点……）都继承它。
/// 为什么用 ScriptableObject + 虚方法而不是 MonoBehaviour：
/// 效果是“数据 + 行为”的组合，做成资源后可以被多个物品复用，
/// 且不需要挂在场景物体上，由背包在运行时直接调用。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/物品效果", fileName = "物品效果设置-")]
public class ItemEffectDateSO : ScriptableObject
{
    // 当前持有该效果的玩家。
    // 为什么用 Subscribe 注入而不是在 ExecuteEffect 里临时查找：
    // 被动型效果（吸血、冰暴）要长期监听玩家事件，必须持有稳定引用；
    // 主动型效果（治疗）不依赖它，只走 ExecuteEffect。
    protected Player player;
    [TextArea]
    public string effectDescription;

    /// <summary>
    /// 使用前的预检：默认允许使用。
    /// 重写它的效果（如 Buff）在此判断“当前是否还能生效”，
    /// 让背包在真正消耗物品之前就拦截操作，而不是消耗完再回滚。
    /// </summary>
    public virtual bool CanBeUsed(Player player)
    {
        return true;
    }

    /// <summary>
    /// 主动使用物品时调用；空实现是“无副作用”的默认值，子类按需重写。
    /// </summary>
    public virtual void ExecuteEffect()
    {

    }

    /// <summary>
    /// 装备/持有物品时订阅玩家事件（被动效果用）。
    /// 为什么用事件订阅而不是每帧轮询：只在真正发生攻击/受伤时结算，省掉无效检查。
    /// </summary>
    public virtual void Subscribe(Player player)
    {
        this.player = player;
    }

    /// <summary>
    /// 卸下物品时取消订阅并清空引用。
    /// 必须与 Subscribe 成对调用，否则事件会重复回调，或引用悬挂导致空引用。
    /// </summary>
    public virtual void Unsubscribe()
    {

    }
}
