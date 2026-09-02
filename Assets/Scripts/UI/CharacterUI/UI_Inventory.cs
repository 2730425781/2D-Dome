using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 背包 UI：把 Inventory_Player 的数据同步到场景中的槽位。
/// 为什么用事件驱动而不是每帧轮询：背包内容只在拾取/使用/穿戴时变化，
/// 频率极低，订阅 OnInventoryChange 事件能在变化瞬间刷新，省去每帧开销。
/// </summary>
public class UI_Inventory : MonoBehaviour
{
    private Inventory_Player inventory;

    [SerializeField] private TextMeshProUGUI gold;
    [SerializeField] private UI_ItemSlotParent itemSlotParent;
    [SerializeField] private UI_EquipSlotParent equipSlotParent;

    private void Awake()
    {
        // 场景中只有单一玩家，按类型查找即可拿到数据源
        inventory = FindAnyObjectByType<Inventory_Player>();

        // 订阅事件而非轮询：拾取/使用/穿戴都会触发一次刷新，
        // 保证 UI 与数据始终同步，且只在数据真的变化时更新
        inventory.OnInventoryChange += UpdateUI;

        // 订阅后立即完整刷新一次，让界面以当前数据初始化，
        // 避免事件注册之前已存在的数据要等下一次变化才显示
        UpdateUI();
    }

    private void Update()
    {
        gold.text = inventory.gold.ToString();
    }

    private void OnEnable()
    {
        if (inventory == null)
        {
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        itemSlotParent.UpdateSlots(inventory.itemList);
        equipSlotParent.UpdateEquimentSlots(inventory.equipList);
    }
}
