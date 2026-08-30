using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 储物箱/仓库的背包：同样继承 Inventory_Base，免费获得堆叠、容量和变更事件这套背包逻辑，
/// 但它不参与装备穿戴。通过 SetInventory 注入玩家背包引用，为后续“背包↔仓库”转移做准备；
/// 之所以要注入引用，是因为仓库本身不持有玩家对象，用事件/引用注入避免它去场景里反查。
/// </summary>
public class Inventoty_Storage : Inventory_Base
{
    public Inventory_Player playerInventory { get; private set; }
    public List<Inventory_Item> materialStash;

    public void ConsumeMaterials(Inventory_Item itemToCraft)
    {
        foreach (var neededItem in itemToCraft.itemDate.craftRecipe)
        {
            // neededItem.stackSize 是配方要求的数量；依次从玩家背包、仓库、材料库扣除，
            // 每扣完一个来源就把剩余需求量传下去，避免跨来源重复/超额扣除
            int remaining = neededItem.stackSize;

            remaining -= ConsumedMaterialsAmount(playerInventory.itemList, neededItem.itemDate, remaining);
            if (remaining <= 0) continue;

            remaining -= ConsumedMaterialsAmount(itemList, neededItem.itemDate, remaining);
            if (remaining <= 0) continue;

            remaining -= ConsumedMaterialsAmount(materialStash, neededItem.itemDate, remaining);
        }

        // 材料数量已变化：主动通知 UI 刷新材料库/背包显示
        TriggerUpdateUI();
    }

    private int ConsumedMaterialsAmount(List<Inventory_Item> itemLists, ItemDateSO neededItemDate, int amountNeeded)
    {
        int consumedAmount = 0;
        // 迭代过程中不能直接 Remove（List 会在枚举时抛 InvalidOperationException），
        // 先把要删的实例收集起来，循环结束后统一删除
        List<Inventory_Item> toRemove = new List<Inventory_Item>();

        // 必须迭代传入的 itemLists 参数：之前误写成类字段 itemList（仓库自己的列表，通常为空），
        // 导致玩家背包和材料库的材料永远扣不掉，而制作却判定成功
        foreach (var item in itemLists)
        {
            if (item.itemDate != neededItemDate)
            {
                continue;
            }

            // 上限用"还剩多少需要扣"而不是完整需求量：
            // 否则当材料分散在多个来源时，后面的来源会按完整需求量超额扣除
            int removeAmount = Mathf.Min(item.stackSize, amountNeeded - consumedAmount);
            item.stackSize -= removeAmount;
            consumedAmount += removeAmount;

            if (item.stackSize <= 0)
            {
                toRemove.Add(item);
            }

            if (consumedAmount >= amountNeeded)
            {
                break;
            }
        }

        foreach (var item in toRemove)
        {
            itemLists.Remove(item);
        }

        return consumedAmount;
    }

    public bool HasEnoughMaterials(Inventory_Item itemToCraft)
    {
        foreach (var material in itemToCraft.itemDate.craftRecipe)
        {
            if (GetAvaliableAmount(material.itemDate) < material.stackSize)
            {
                return false;
            }
        }
        return true;
    }

    public int GetAvaliableAmount(ItemDateSO itemDate)
    {
        int amount = 0;

        foreach (var item in playerInventory.itemList)
        {
            if (item.itemDate == itemDate)
            {
                amount += item.stackSize;
            }
        }

        foreach (var item in itemList)
        {
            if (item.itemDate == itemDate)
            {
                amount += item.stackSize;
            }
        }

        foreach (var item in materialStash)
        {
            if (item.itemDate == itemDate)
            {
                amount += item.stackSize;
            }
        }

        return amount;
    }

    public void AddMaterialToStash(Inventory_Item itemToAdd)
    {
        // 材料库只接受材料：规则放在数据层（玩家脚本），
        // 任何入口（拾取/购买/拖动/面板拖放）都无法绕过
        if (itemToAdd == null || itemToAdd.itemDate == null || itemToAdd.itemDate.itemType != ItemType.Material)
        {
            Debug.Log("只有材料才能放入材料库");
            return;
        }

        var stackableItem = StackableInStash(itemToAdd);

        if (stackableItem != null)
        {
            stackableItem.AddStack();
        }
        else
        {
            // 存入前必须克隆一份新实例：传入的实例可能同时属于其他容器（如商店列表），
            // 直接把原实例存进材料库会让两个容器共享同一对象，数量互相污染
            materialStash.Add(new Inventory_Item(itemToAdd.itemDate) { stackSize = itemToAdd.stackSize });
        }

        TriggerUpdateUI();
    }

    public Inventory_Item StackableInStash(Inventory_Item itemToAdd)
    {
        return materialStash.Find(item => item.itemDate == itemToAdd.itemDate && item.CanAddStack());
    }

    public void GetInventory(Inventory_Player inventory) => playerInventory = inventory;

    // ---------- 拖拽转移（整组） ----------

    /// <summary>把整组物品移入材料库。sourceList 为物品当前所在的列表（玩家/仓库）。</summary>
    public void MoveToStash(List<Inventory_Item> sourceList, Inventory_Item item)
    {
        // 材料库只接受材料：非材料（武器/防具/消耗品）不能通过任何路径进入，
        // 且必须在移除源物品之前拦截，否则会把源物品弄丢
        if (item == null || item.itemDate == null || item.itemDate.itemType != ItemType.Material)
        {
            Debug.Log("只有材料才能放入材料库");
            return;
        }

        sourceList.Remove(item);

        // 材料库支持堆叠：有同类堆叠就合并（课程演示不处理超上限拆分），否则整体存入
        var stackable = StackableInStash(item);
        if (stackable != null)
        {
            stackable.stackSize += item.stackSize;
        }
        else
        {
            materialStash.Add(item);
        }

        TriggerUpdateUI();
    }

    /// <summary>把整组物品从材料库移到目标背包。目标放不下则不转移（物品留在材料库）。</summary>
    public void MoveStashTo(Inventory_Base target, Inventory_Item item)
    {
        if (!target.CanAddItem(item)) return;

        materialStash.Remove(item);
        target.itemList.Add(item);

        TriggerUpdateUI();
    }

    // 目前是占位空实现：转移逻辑待后续课程补齐，先保留方法签名以稳定外部调用
    public void PlayerToStorage(Inventory_Item item, bool transferStack)
    {
        int transferAmount = transferStack ? item.stackSize : 1;

        for (int i = 0; i < transferAmount; i++)
        {
            if (CanAddItem(item))
            {
                var itemToAdd = new Inventory_Item(item.itemDate);

                playerInventory.RemoveItem(item);
                AddItem(itemToAdd);
            }
        }

        TriggerUpdateUI();
    }

    public void StoargeToPlayer(Inventory_Item item, bool transferStack)
    {
        int transferAmount = transferStack ? item.stackSize : 1;

        for (int i = 0; i < transferAmount; i++)
        {
            if (playerInventory.CanAddItem(item))
            {
                var itemToAdd = new Inventory_Item(item.itemDate);

                RemoveItem(item);
                playerInventory.AddItem(itemToAdd);
            }
        }

        TriggerUpdateUI();
    }
}
