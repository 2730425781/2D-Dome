using UnityEngine;

/// <summary>
/// 商人 NPC：复用 Object_NPC 的翻转与提示框表现，只补充"可交互"身份。
///
/// 为什么 IInteractable 加在子类而不是基类：基类只负责所有 NPC 共通的
/// 外观表现，"能否被交互"是各 NPC 自己的特性。Player.TryInteract 通过
/// GetComponent 扫描 IInteractable，实现该接口即自动接入玩家的交互键。
///
/// 现阶段 Interact 只是占位打印，后续会替换为打开商店面板。
/// </summary>
public class Object_Merchant : Object_NPC, IInteractable
{
    private Inventory_Player playerInventory;
    private Inventory_Merchant merchantInventory;
    private Inventoty_Storage materialInventory;

    protected override void Awake()
    {
        base.Awake();
        merchantInventory = GetComponent<Inventory_Merchant>();
        materialInventory = FindAnyObjectByType<Inventoty_Storage>();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            merchantInventory.FillShopList();
        }
    }

    public void Interact()
    {
        // 占位实现：先验证交互链路通不通，真正的商店 UI 在这里接入
        ui.merchantUI.SetupMerchantUI(merchantInventory, playerInventory, materialInventory);
        ui.merchantUI.gameObject.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        playerInventory = player.GetComponent<Inventory_Player>();
        merchantInventory.SetInventory(playerInventory);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        ui.SwitchoffAllTooltips();

        if (ui == null || ui.merchantUI == null)
        {
            return;
        }
        else
        {
            ui.merchantUI.gameObject.SetActive(false);
        }
    }
}
