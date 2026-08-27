using UnityEngine;

/// <summary>
/// 物品的基础数据定义：所有具体物品（消耗品、装备）的公共字段都在这里。
/// 为什么用 ScriptableObject：物品是“定义”而非场景里的“实例”，
/// 一份资源可被多个背包中的 Inventory_Item 引用，修改定义即全局生效。
/// </summary>
[CreateAssetMenu(menuName = "游戏设置/物品数据/物品数据", fileName = "物品数据设置-")]
public class ItemDateSO : ScriptableObject
{
    [Header("信息设置")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    // 默认最大堆叠 1：大多数物品不可堆叠，可堆叠物品（如药水）在子类或资源上调大
    public int maxStackSize = 1;

    [Header("效果设置")]
    // 效果独立成资源而不是内嵌在这里：
    // 同一效果（如“回复 10% 生命”）可被多个物品复用，避免每个物品重复配置一份
    public ItemEffectDateSO itemEffect;

    [Header("制作设置")]
    public Inventory_Item[] craftRecipe;

    [Header("掉落设置")]
    [Range(1, 1000)]
    public int itemRarity = 10;
    [Range(0, 100)]
    public float dropChance;
    [Range(0, 100)]
    public float maxDropChance = 50;

    [Header("商品信息")]
    [Range(0, 10000)]
    public int itemPrice = 100;
    public int minStackSizeInShop = 1;
    public int maxStackSizeInShop = 1;

    private void OnValidate()
    {
        dropChance = GetDropChance();
    }

    public float GetDropChance()
    {
        float maxRarity = 1000;
        float chance = (maxRarity - itemRarity + 1) / maxRarity * 100;

        return Mathf.Min(chance, maxDropChance);
    }
}
