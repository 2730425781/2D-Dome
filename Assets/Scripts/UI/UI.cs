using System;
using UnityEngine;

/// <summary>
/// UI 中枢：持有所有 ToolTip 与技能树/背包面板的引用，并负责开关面板。
/// 为什么集中在这里：ToolTip 的显隐被多个组件触发（槽位悬停、面板开关），
/// 由单一入口统一转发，避免各处持有各自的副本导致引用不一致或漏关。
/// </summary>
public class UI : MonoBehaviour
{
    public static UI instance;

    [SerializeField] private GameObject[] uiElements;

    public bool alternativeInput { get; private set; }
    private PlayerInputSet input;

    public UI_Craft craftUI { get; private set; }
    public UI_Quest questUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    public UI_Storage storageUI { get; private set; }
    public UI_Merchant merchantUI { get; private set; }
    public UI_Dialogue dialogueUI { get; private set; }
    public UI_PauseMenu pauseMenu { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }
    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_FadeScreen fadeScreenUI { get; private set; }
    public UI_DeathScreen deathScreenUI { get; private set; }
    public UI_SkillToolTip skillToolTip { get; private set; }

    private bool skillTreeEnable;
    private bool inventoryEnable;

    private void Awake()
    {
        instance = this;

        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        // GetComponentInChildren 的 true 参数：技能树/背包面板默认是隐藏的，
        // 不带 true 会跳过未激活的子物体，引用就会变成 null
        craftUI = GetComponentInChildren<UI_Craft>(true);
        questUI = GetComponentInChildren<UI_Quest>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        pauseMenu = GetComponentInChildren<UI_PauseMenu>(true);
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true);
        deathScreenUI = GetComponentInChildren<UI_DeathScreen>(true);

        // 以场景中的初始显隐状态为准记录开关标志，
        // 这样设计师在场景里把面板设成隐藏也能正确切换到"打开"
        skillTreeEnable = skillTreeUI.gameObject.activeSelf;
        inventoryEnable = inventoryUI.gameObject.activeSelf;

        // 任务面板只能由 NPC 交互打开，开局必须隐藏。
        // 场景里曾被保存成激活状态，会让任务面板一进游戏就盖住整个画面
        if (questUI != null) questUI.gameObject.SetActive(false);
    }

    private void Start()
    {
        skillTreeUI.UnlockDefaultSkills();
    }

    public void SetupControlsUI(PlayerInputSet inputSet)
    {
        input = inputSet;
        input.UI.SkillTreeUI.performed += ctx => ToggleSkillTreeUI();
        input.UI.InventoryUI.performed += ctx => ToggleInvemtoryUI();

        input.UI.AlternativeInput.performed += ctx => alternativeInput = true;
        input.UI.AlternativeInput.canceled += ctx => alternativeInput = false;

        input.UI.ToggleOptionsUI.performed += ctx =>
        {
            // 保留原有"ESC 关闭已打开的 UI"功能：
            // 有任一面板打开 → 全部关闭并回到游戏；否则 → 打开暂停菜单
            if (AnyPanelOpen())
            {
                Time.timeScale = 1;
                SwitchToGameUI();
                return;
            }

            pauseMenu.Toggle();
        };
    }

    // 是否有任一面板处于打开状态（用于 ESC 优先关闭而非打开菜单）。
    // 不依赖 uiElements 配置，直接检查各面板活动状态，避免漏配导致 ESC 无法关闭。
    private bool AnyPanelOpen()
    {
        if (skillTreeUI != null && skillTreeUI.gameObject.activeSelf) return true;
        if (inventoryUI != null && inventoryUI.gameObject.activeSelf) return true;
        if (optionsUI != null && optionsUI.gameObject.activeSelf) return true;
        // 任务面板不在 uiElements 配置里，必须单独判断，否则 ESC 关不掉任务面板
        if (questUI != null && questUI.gameObject.activeSelf) return true;

        if (uiElements != null)
        {
            foreach (var element in uiElements)
            {
                if (element != null && element.activeSelf) return true;
            }
        }
        return false;
    }

    public void OpenDeathScreenUI()
    {
        SwitchTo(deathScreenUI.gameObject);
        input.Disable();
    }

    public void OpenOptionsUI()
    {
        HideAllTooltips();
        StopPlayerControls(true);
        SwitchTo(optionsUI.gameObject);
    }

    public void SwitchToGameUI()
    {
        HideAllTooltips();
        StopPlayerControls(false);
        SwitchTo(inGameUI.gameObject);

        // 任务面板不在 uiElements 里（见场景配置），SwitchTo 不会关它，
        // 必须在这里单独关闭，否则 ESC/关闭面板后任务面板会一直盖在游戏上
        if (questUI != null) questUI.gameObject.SetActive(false);

        skillTreeEnable = false;
        inventoryEnable = false;
    }

    private void SwitchTo(GameObject objectToSwitchOn)
    {
        foreach (var element in uiElements)
        {
            element.gameObject.SetActive(false);
        }

        objectToSwitchOn.SetActive(true);
    }

    private void StopPlayerControls(bool stopControls)
    {
        if (stopControls)
        {
            input.Player.Disable();
        }
        else
        {
            input.Player.Enable();
        }
    }

    public void HideAllTooltips()
    {
        // 逐个判空：引用可能未绑定（组件缺失），也可能在播放模式切换中被销毁
        // （本项目关闭了域重载），在已销毁的 ToolTip 上调用方法会抛异常
        if (itemToolTip != null) itemToolTip.ShowToolTip(false, null);
        if (skillToolTip != null) skillToolTip.ShowToolTip(false, null);
        if (statToolTip != null) statToolTip.ShowToolTip(false, null);
    }

    // 关闭技能树时一并隐藏技能 ToolTip：
    // 节点随面板一起隐藏后悬停退出事件不会再触发，不手动关会残留在屏幕上
    public void ToggleSkillTreeUI()
    {
        skillTreeEnable = !skillTreeEnable;
        skillTreeUI.gameObject.SetActive(skillTreeEnable);
        HideAllTooltips();

        StopPlayerControls(skillTreeEnable);
    }

    // 关闭背包时清掉物品/属性 ToolTip，理由同上：
    // 它们指向的槽位随面板一起消失，靠悬停退出事件关掉并不可靠
    public void ToggleInvemtoryUI()
    {
        inventoryEnable = !inventoryEnable;
        inventoryUI.gameObject.SetActive(inventoryEnable);
        HideAllTooltips();

        StopPlayerControls(inventoryEnable);
    }

    public void OpenQuestUI(QuestDataSO[] questArray)
    {
        // 面板引用缺失时直接返回：否则会在已销毁/未绑定的对象上调用而抛异常
        if (questUI == null) return;

        StopPlayerControls(true);
        HideAllTooltips();

        questUI.gameObject.SetActive(true);
        questUI.SetupQuestUI(questArray);
    }

    public void OpenDialogueUI(DialogueLineSO firstLine)
    {
        // 面板引用缺失时直接返回：否则会在 null 上调用而抛异常
        if (dialogueUI == null) return;

        StopPlayerControls(true);
        HideAllTooltips();

        dialogueUI.gameObject.SetActive(true);
        dialogueUI.PlayDialogueLine(firstLine);
    }

    /// <summary>关闭对话面板并恢复玩家操作。</summary>
    public void CloseDialogueUI()
    {
        if (dialogueUI != null) dialogueUI.gameObject.SetActive(false);

        HideAllTooltips();
        StopPlayerControls(false);
    }

    /// <summary>关闭任务面板并恢复玩家操作。供 ESC 与面板内返回按钮复用。</summary>
    public void CloseQuestUI()
    {
        if (questUI != null) questUI.gameObject.SetActive(false);

        HideAllTooltips();
        StopPlayerControls(false);
    }

    public void OpenStorageUI(bool openStrageUI)
    {
        storageUI.gameObject.SetActive(openStrageUI);
        StopPlayerControls(openStrageUI);

        if (!openStrageUI)
        {
            craftUI.gameObject.SetActive(false);
            HideAllTooltips();
        }
    }

    public void OpenMerchantUI(bool openMerchantUI)
    {
        merchantUI.gameObject.SetActive(openMerchantUI);
        StopPlayerControls(openMerchantUI);

        if (!openMerchantUI)
        {
            HideAllTooltips();
        }
    }
}
