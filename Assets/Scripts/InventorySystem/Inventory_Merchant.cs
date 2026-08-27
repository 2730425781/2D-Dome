using System.Collections.Generic;
using UnityEngine;

public class Inventory_Merchant : Inventory_Base
{
    private Inventory_Player playerInventory;

    [SerializeField] private int minItemsAmount = 4;
    [SerializeField] private ItemListDateSO shopDate;

    protected override void Awake()
    {
        base.Awake();
        FillShopList();
    }

    public void TryBuyItem(Inventory_Item itemToBuy, bool buyFullStack)
    {
        int amountToBuy = buyFullStack ? itemToBuy.stackSize : 1;

        for (int i = 0; i < amountToBuy; i++)
        {
            if (playerInventory.gold < itemToBuy.buyPrice)
            {
                Debug.Log("资金不足");
                return;
            }

            if (itemToBuy.itemDate.itemType == ItemType.Material)
            {
                // 必须传一份新实例而不是直接把商店的物品塞进材料库：
                // 直接存入会让商店列表和材料库共享同一对象，之后商店扣数量、
                // 材料库加数量会互相污染——商店物品的数量被"锁住"不减少、也不会消失
                playerInventory.storage.AddMaterialToStash(new Inventory_Item(itemToBuy.itemDate));
            }
            else
            {
                // 背包放不下就整笔取消，不能继续扣钱和删商店库存：
                // 否则会出现"钱花了、商店物品没了、但背包里什么都没多"的情况
                if (!playerInventory.CanAddItem(itemToBuy))
                {
                    Debug.Log("背包已满，无法购买");
                    return;
                }

                playerInventory.AddItem(new Inventory_Item(itemToBuy.itemDate));
            }

            playerInventory.gold -= itemToBuy.buyPrice;
            RemoveItem(itemToBuy);
        }

        TriggerUpdateUI();
        Debug.Log("成功购买物品");
    }

    public void TrySellItem(Inventory_Item itemToSell, bool sellFullStack)
    {
        int amountToSell = sellFullStack ? itemToSell.stackSize : 1;

        for (int i = 0; i < amountToSell; i++)
        {
            // 先确认物品确实在玩家背包里再给钱：
            // 否则卖"不在背包里的物品"（如材料库里的材料）会只加钱不删物，形成金币漏洞
            if (!playerInventory.itemList.Contains(itemToSell))
            {
                Debug.Log("该物品不在背包中，无法出售");
                return;
            }

            int sellPrice = Mathf.FloorToInt(itemToSell.sellPride);

            playerInventory.gold += sellPrice;
            playerInventory.RemoveItem(itemToSell);
        }

        TriggerUpdateUI();
        Debug.Log("成功卖出物品");
    }

    public void FillShopList()
    {
        itemList.Clear();
        List<Inventory_Item> possibleItems = new List<Inventory_Item>();

        foreach (var itemDate in shopDate.itemList)
        {
            int randmoziedStack = Random.Range(itemDate.minStackSizeInShop, itemDate.maxStackSizeInShop + 1);
            int finalStack = Mathf.Clamp(randmoziedStack, 1, itemDate.maxStackSizeInShop);

            Inventory_Item itemToShop = new Inventory_Item(itemDate);
            itemToShop.stackSize = finalStack;

            possibleItems.Add(itemToShop);
        }

        int randomItemAmount = Random.Range(minItemsAmount, maxInventorySixe + 1);
        int finalItemAmount = Mathf.Clamp(randomItemAmount, 1, possibleItems.Count);

        for (int i = 0; i < finalItemAmount; i++)
        {
            var index = Random.Range(0, possibleItems.Count);
            var item = possibleItems[index];

            if (CanAddItem(item))
            {
                possibleItems.Remove(item);
                AddItem(item);
            }
        }

        TriggerUpdateUI();
    }

    public void SetInventory(Inventory_Player inventory) => playerInventory = inventory;

    /// <summary>出售材料库（商店材料箱）里的材料。材料库是普通 List，不能走 TrySellItem（那只卖玩家背包）。</summary>
    public void TrySellStashItem(Inventory_Item itemToSell, bool sellFullStack)
    {
        if (playerInventory == null || playerInventory.storage == null || itemToSell == null) return;
        List<Inventory_Item> stash = playerInventory.storage.materialStash;
        if (stash == null) return;

        int amountToSell = sellFullStack ? itemToSell.stackSize : 1;

        for (int i = 0; i < amountToSell; i++)
        {
            // 先确认物品仍在材料库再给钱，防止只加钱不删物的漏洞
            if (!stash.Contains(itemToSell)) return;

            int sellPrice = Mathf.FloorToInt(itemToSell.sellPride);
            playerInventory.gold += sellPrice;

            if (itemToSell.stackSize > 1)
            {
                itemToSell.RemoveStack();
            }
            else
            {
                stash.Remove(itemToSell);
            }
        }

        // 材料库变了：刷新存储界面与商店材料箱；再刷新商店面板（金币/玩家格）
        playerInventory.storage.TriggerUpdateUI();
        TriggerUpdateUI();
        Debug.Log("成功卖出材料");
    }

    /// <summary>供 UI 层（拖拽判定目标列表）读取玩家背包引用。</summary>
    public Inventory_Player PlayerInventory => playerInventory;
}
