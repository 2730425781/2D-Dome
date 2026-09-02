using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有背包（玩家背包、仓库）的公共基类。做成 MonoBehaviour 而非纯 C# 类，
/// 是为了让背包能作为组件挂在任意场景物体上（Player / 储物箱），并通过 virtual Awake 给子类一个初始化扩展点。
/// 用 C# event 而非直接调用 UI 刷新，是为了把“数据变更 → 界面刷新”解耦：
/// UI 只需订阅一次 OnInventoryChange，之后任何增删改都会自动触发刷新，新增其他监听方也无需改动这里。
/// </summary>
public class Inventory_Base : MonoBehaviour, ISaveable
{
    protected Player player;

    public event Action OnInventoryChange;
    public int maxInventorySixe = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    [Header("物品数据列表")]
    [SerializeField] protected ItemListDateSO itemDataBase;

    // Awake 做成 virtual 空实现：子类（如玩家背包）可安全调用 base.Awake() 来保留基类契约，
    // 同时基类本身没有必须的初始化，避免强制子类背负不必要的初始化顺序。
    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }

    public void TryUseItem(Inventory_Item itemToUse)
    {
        Inventory_Item consumable = itemList.Find(item => item == itemToUse);

        // 物品不在本背包（如已用光）时直接放弃：防止对空引用执行效果导致空引用异常
        if (consumable == null) return;

        if (consumable.itemEffect.CanBeUsed(player) == false) return;

        // 先执行效果再处理数量：效果只关心“用了这个物品”，与剩余数量无关
        consumable.itemEffect.ExecuteEffect();

        // 有堆叠余量时只减堆叠数，物品留在原位；否则整格移出列表
        if (consumable.stackSize > 1)
        {
            consumable.RemoveStack();
        }
        else
        {
            RemoveItem(consumable);
        }

        // 无论走哪个分支，最后统一通知一次 UI 刷新
        OnInventoryChange?.Invoke();
    }

    public bool CanAddItem(Inventory_Item item)
    {
        bool hasStackable = FindAddStack(item) != null;

        // 背包即使满了，能与现有堆叠合并的物品依然可以入包，所以两个条件取或
        return hasStackable || itemList.Count < maxInventorySixe;
    }

    /// <summary>
    /// 查找能与待入包物品合并的现有堆叠。按 itemDate（物品数据资产）判断“是否同种”，
    /// 而不是比较物品实例：每次拾取都会 new 一个新实例，只有共享同一数据资产的物品才应该合并。
    /// </summary>
    public Inventory_Item FindAddStack(Inventory_Item itemToAdd)
    {
        return itemList.Find(item => item.itemDate == itemToAdd.itemDate && item.CanAddStack());


    }

    /// <summary>
    /// 入包策略：优先并入已有堆叠（不占新格），没有可并入的堆叠时才追加为新格子。
    /// 顺序不能反过来，否则堆叠类物品会过早占满格子。
    /// </summary>
    public void AddItem(Inventory_Item item)
    {
        Inventory_Item inventoryItem = FindAddStack(item);

        if (inventoryItem != null)
        {
            inventoryItem.AddStack();
        }
        else
        {
            itemList.Add(item);
        }

        OnInventoryChange?.Invoke();
    }

    public void RemoveItem(Inventory_Item itemToRemove, bool removeFull = false)
    {
        // 不在本背包的物品直接忽略：调用方可能持有刷新前的旧引用，
        // 对游离实例做任何操作都是无效的
        if (!itemList.Contains(itemToRemove)) return;

        if (removeFull)
        {
            itemList.Remove(itemToRemove);
        }
        else
        {
            // 必须对传入的实例本身操作，而不是按 itemDate 找第一个匹配：
            // 玩家背包里同种物品可能有多个堆叠，按 itemDate Find 会扣错堆叠
            // （卖第 2 格却扣第 1 格；或找到的堆叠数量为 1 时，整格误删传入实例）
            if (itemToRemove.stackSize > 1)
            {
                itemToRemove.RemoveStack();
            }
            else
            {
                itemList.Remove(itemToRemove);
            }
        }
        // 物品从背包消失，UI 必须立刻刷新对应格子
        OnInventoryChange?.Invoke();
    }

    /// <summary>
    /// 按物品数据查找背包中的实际实例。调用方往往只有数据资产（例如要穿戴的装备定义），
    /// 必须通过这里拿到背包里真实存在的实例，后续的堆叠、移除操作才有意义。
    /// </summary>
    public Inventory_Item FindItem(ItemDataSO itemDate)
    {
        return itemList.Find(item => item.itemDate == itemDate);
    }

    public Inventory_Item FindSameItem(Inventory_Item itemToFind)
    {
        // 注意必须是 == 比较：之前误写成单等号 =（赋值），会覆盖每个物品的 itemDate，
        // 且 Find 的谓词返回 ItemDateSO 无法转 bool，直接导致编译错误、整个背包挂掉
        return itemList.Find(item => item.itemDate == itemToFind.itemDate);
    }

    // 公开一个语义化的包装方法而非直接暴露事件调用：外部只知道“数据变了请刷新 UI”，
    // 不需要关心事件是否为 null（?. 在这里被隐藏）
    public void TriggerUpdateUI() => OnInventoryChange?.Invoke();

    public virtual void LoadData(GameData data)
    {

    }

    public virtual void SaveData(ref GameData data)
    {

    }
}
