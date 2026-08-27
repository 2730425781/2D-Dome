using UnityEngine;

/// <summary>
/// 铁匠 NPC：与商人一样继承 Object_NPC 获得翻转/提示框，
/// 并通过 IInteractable 接入玩家的交互键。
///
/// 为什么每种 NPC 单独建类而不是"一个类 + 类型枚举"：
/// 商店、锻造的交互逻辑完全不同且会各自扩展，拆开避免在一个类里
/// 堆满 if/switch 分支，也便于后续给铁匠加专属行为（如强化装备）。
/// </summary>
public class Object_Blacksmith : Object_NPC, IInteractable
{
    private Inventory_Player inventory;
    private Inventoty_Storage storage;

    protected override void Awake()
    {
        base.Awake();
        storage = GetComponent<Inventoty_Storage>();

    }

    public void Interact()
    {
        // 遗留的占位调试输出；后续应替换为真正的锻造/强化面板入口
        ui.storageUI.SetupStorageUI(storage);
        ui.craftUI.SetupCraftUI(storage);

        //ui.storageUI.gameObject.SetActive(true);
        ui.craftUI.gameObject.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        inventory = player.GetComponent<Inventory_Player>();
        storage.GetInventory(inventory);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        // ui 在播放模式切换中可能已被销毁（本项目关闭了域重载），
        // 在已销毁的 UI 上调用方法会抛异常，统一判空
        if (ui != null)
        {
            ui.SwitchoffAllTooltips();
            if (ui.storageUI != null) ui.storageUI.gameObject.SetActive(false);
            if (ui.craftUI != null) ui.craftUI.gameObject.SetActive(false);
        }
    }
}
