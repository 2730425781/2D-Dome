using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品效果/物品效果", fileName = "物品效果设置-")]
public class ItemEffectDateSO : ScriptableObject
{
    protected Player player;
    [TextArea]
    public string effectDescription;

    public virtual bool CanBeUsed()
    {
        return true;
    }

    public virtual void ExecuteEffect()
    {

    }

    public virtual void Subscribe(Player player)
    {
        this.player = player;
    }

    public virtual void Unsubscribe()
    {

    }
}
