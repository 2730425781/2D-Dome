using System;
using UnityEngine;

/// <summary>
/// UI 中枢：持有所有 ToolTip 与技能树/背包面板的引用，并负责开关面板。
/// 为什么集中在这里：ToolTip 的显隐被多个组件触发（槽位悬停、面板开关），
/// 由单一入口统一转发，避免各处持有各自的副本导致引用不一致或漏关。
/// </summary>
public class UI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiElements;

    public bool alternativeInput { get; private set; }
    private PlayerInputSet input;

    public UI_Craft craftUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    public UI_Storage storageUI { get; private set; }
    public UI_Merchant merchantUI { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }
    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_SkillToolTip skillToolTip { get; private set; }

    private bool skillTreeEnable;
    private bool inventoryEnable;

    private void Awake()
    {
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        // GetComponentInChildren 的 true 参数：技能树/背包面板默认是隐藏的，
        // 不带 true 会跳过未激活的子物体，引用就会变成 null
        craftUI = GetComponentInChildren<UI_Craft>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);

        // 以场景中的初始显隐状态为准记录开关标志，
        // 这样设计师在场景里把面板设成隐藏也能正确切换到"打开"
        skillTreeEnable = skillTreeUI.gameObject.activeSelf;
        inventoryEnable = inventoryUI.gameObject.activeSelf;
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
            foreach (var element in uiElements)
            {
                if (element.activeSelf)
                {
                    Time.timeScale = 1;
                    SwitchToGameUI();
                    return;
                }
            }

            Time.timeScale = 0;
            OpenOptionsUI();
        };
    }

    public void OpenOptionsUI()
    {
        foreach (var element in uiElements)
        {
            element.gameObject.SetActive(false);
        }

        HideAllTooltips();
        StopPlayerControls(true);
        optionsUI.gameObject.SetActive(true);
    }

    public void SwitchToGameUI()
    {
        foreach (var element in uiElements)
        {
            element.gameObject.SetActive(false);
        }

        HideAllTooltips();
        StopPlayerControls(false);
        inGameUI.gameObject.SetActive(true);

        skillTreeEnable = false;
        inventoryEnable = false;
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
