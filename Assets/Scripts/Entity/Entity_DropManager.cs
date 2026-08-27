using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity_DropManager : MonoBehaviour
{
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemListDateSO dropDate;

    [Header("掉落列表设置")]
    [SerializeField] private int maxRarityAmount = 1200;
    [SerializeField] private int maxItemToDrop = 3;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            DropItems();
        }
    }

    public virtual void DropItems()
    {
        List<ItemDateSO> itemToDrops = RollDrops();
        int amountToDrop = Mathf.Min(itemToDrops.Count, maxItemToDrop);

        for (int i = 0; i < amountToDrop; i++)
        {
            CreateItemDrop(itemToDrops[i]);
        }
    }

    protected void CreateItemDrop(ItemDateSO itemDate)
    {
        // 出生点抬高 1 单位：物品碰撞器是 1x1，直接生成在脚下会有一半埋进地面，
        // 物理解算立刻把它顶出来并判为"落地"，还没起飞就冻结在出生点
        Vector3 spawnPos = transform.position + Vector3.up * 1f;
        GameObject newItem = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
        newItem.GetComponent<Object_ItemPickup>().SetupItem(itemDate);
    }

    public List<ItemDateSO> RollDrops()
    {
        List<ItemDateSO> possibleDrops = new List<ItemDateSO>();
        List<ItemDateSO> finalDrops = new List<ItemDateSO>();
        float maxRarityAmount = this.maxRarityAmount;

        foreach (var item in dropDate.itemList)
        {
            float dropChance = item.GetDropChance();

            if (Random.Range(0, 100) <= dropChance)
            {
                possibleDrops.Add(item);
            }
        }

        possibleDrops = possibleDrops.OrderByDescending(item => item.itemRarity).ToList();

        foreach (var item in possibleDrops)
        {
            if (maxRarityAmount > item.itemRarity)
            {
                finalDrops.Add(item);
                maxRarityAmount -= item.itemRarity;
            }
        }

        return finalDrops;
    }
}
