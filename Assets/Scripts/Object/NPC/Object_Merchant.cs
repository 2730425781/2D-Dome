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
public class Object_Merchant : Object_NPC
{
    [Header("任务与对话")]
    [SerializeField] private QuestDataSO[] questArray;
    [SerializeField] private DialogueLineSO firstDialogueLine;

    private Inventory_Player playerInventory;
    private Inventory_Merchant merchantInventory;
    private Inventoty_Storage materialInventory;

    /// <summary>商人发布的任务列表。QuestManager 也会从这里反查任务资产。</summary>
    public override QuestDataSO[] Quests => questArray;

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

    public override void Interact()
    {
        // 先上报"与商人对话"的任务进度
        ReportQuestTalk();

        // 触发区外也可能被交互（玩家交互用的是自己的范围检测），此时 playerInventory 还没在
        // OnTriggerEnter2D 里赋值，这里兜底解析，避免商店绑定到一个 null 玩家背包
        if (playerInventory == null && Player.instance != null)
        {
            playerInventory = Player.instance.playerInventory;
        }

        // 先把商店面板的数据源绑定好。
        // SetupMerchantUI 负责：绑定三个背包、订阅库存变更事件、把商人引用下发到每个槽位、
        // 并做一次初始刷新。不调用它的话，对话里选择"打开商店"后打开的是一个
        // 全空且拖放买卖完全失效的商店面板——这正是"商店界面打不开/打开了也没内容"的另一半原因
        if (ui != null && ui.merchantUI != null)
        {
            ui.merchantUI.SetupMerchantUI(merchantInventory, playerInventory, materialInventory);
        }

        if (merchantInventory != null && playerInventory != null)
        {
            merchantInventory.SetInventory(playerInventory);
        }

        ui.OpenDialogueUI(firstDialogueLine);
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
        ui.HideAllTooltips();

        if (ui == null || ui.merchantUI == null)
        {
            return;
        }
        else
        {
            ui.OpenMerchantUI(false);
        }
    }
}
