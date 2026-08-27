using UnityEngine;

/// <summary>
/// 属性面板：在背包数据变化时刷新所有属性槽的数值。
/// 为什么独立成一个组件而不是让每个 UI_StatSlot 各自监听事件：
/// 属性受装备/增益影响，都由背包事件驱动，集中一个刷新入口
/// 能保证所有属性槽在同一帧读到一致的数据。
/// </summary>
public class UI_PlayerStats : MonoBehaviour
{
    private UI_StatSlot[] uiStatSolts;
    private Inventory_Player inventory;

    private void Awake()
    {
        uiStatSolts = GetComponentsInChildren<UI_StatSlot>();

        // 订阅背包变化事件：穿戴/卸下装备、增益生效或结束都会改属性，
        // 事件驱动保证面板只在属性可能变化的那一刻刷新
        inventory = FindAnyObjectByType<Inventory_Player>();
        inventory.OnInventoryChange += UpdateStatsUI;
    }

    private void Start()
    {
        // 首次刷新放在 Start 而不是 Awake：
        // 保证玩家属性、背包等其他组件的 Awake 已全部执行完，
        // 首次渲染前数值（含装备加成）就已就绪
        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        // 每个属性槽自行从玩家属性读取并格式化数值，
        // 这里只负责遍历触发，避免重复维护属性到数值的映射逻辑
        foreach (var statSlot in uiStatSolts)
        {
            statSlot.UpdateStatValue();
        }
    }
}
