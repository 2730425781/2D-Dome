using UnityEngine;

[CreateAssetMenu(menuName = "游戏设置/物品数据/物品数据", fileName = "物品数据设置-")]
public class ItemDateSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("物品效果")]
    public ItemEffectDateSO itemEffect;
}
