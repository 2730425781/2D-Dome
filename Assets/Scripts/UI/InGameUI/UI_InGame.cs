using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private Inventory_Player playerInventory;
    private UI_SkillSlot[] skillSlots;

    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("快捷栏设置")]
    [SerializeField] private float yoffsetQuickItemParent = 150;
    [SerializeField] private Transform quickItemOptionsParent;
    private UI_QuickItemSlotOption[] quickItemSlotOptions;
    private UI_QuickItemSlot[] quickItemSlots;


    private void Awake()
    {
        // 必须在 Awake 里解析（先于所有 Start）：
        // UI.Start 会立即解锁默认技能 → GetSkillSlot，若 skillSlots 还没填充，
        // GetSkillSlot 遍历空数组会抛 NullReferenceException。
        // 技能栏已被重构为 UI_InGame 的"同级"节点（Canvas/UI_SkillBarParent），
        // 不再从自身子物体查找，改为全局查找，避免层级变动导致 skillSlots 为空。
        player = FindAnyObjectByType<Player>();
        skillSlots = Object.FindObjectsByType<UI_SkillSlot>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        quickItemSlots = GetComponentsInChildren<UI_QuickItemSlot>();

        UpdateHealthBar();
        if (player != null && player.health != null)
        {
            player.health.OnHealthUpdate += UpdateHealthBar;
        }

        playerInventory = player.playerInventory;
        playerInventory.OnQuickSlotUsed += UpdateQuickSlotsUI;
        // 快捷键使用物品的变暗反馈（与点击一致）
        playerInventory.OnQuickItemUsed += PlayQuickItemUseFeedback;
    }

    private void PlayQuickItemUseFeedback(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= quickItemSlots.Length) return;
        quickItemSlots[slotNumber].FlashUseFeedback();
    }

    public void UpdateQuickSlotsUI(int slotNumber, Inventory_Item item)
    {
        quickItemSlots[slotNumber].UpdateQuickItemSlot(item);
    }

    public void OpenQuickItemOptions(UI_QuickItemSlot quickItemSlot, RectTransform targetRect)
    {
        if (quickItemSlotOptions == null)
        {
            quickItemSlotOptions = quickItemOptionsParent.GetComponentsInChildren<UI_QuickItemSlotOption>(true);
        }

        // 排除已占用其它快捷栏位的物品：避免同一实例占两格导致堆叠在使用后不同步
        List<Inventory_Item> consumables = playerInventory.itemList
            .FindAll(item => item.itemDate.itemType == ItemType.Consumable)
            .FindAll(item => !playerInventory.IsItemInQuickSlot(item));

        for (int i = 0; i < quickItemSlotOptions.Length; i++)
        {
            if (i < consumables.Count)
            {
                quickItemSlotOptions[i].gameObject.SetActive(true);
                quickItemSlotOptions[i].SetupOption(quickItemSlot, consumables[i]);
            }
            else
            {
                quickItemSlotOptions[i].gameObject.SetActive(false);
            }
        }

        quickItemOptionsParent.position = targetRect.position + Vector3.up * yoffsetQuickItemParent;
    }

    public void HideQuickItemOptions() => quickItemOptionsParent.position = new Vector3(0, 999);

    public UI_SkillSlot GetSkillSlot(SkillType skillType)
    {
        // 防御：万一 Awake 前被调用，懒加载一次（正常流程 Awake 已填充）
        if (skillSlots == null)
        {
            skillSlots = Object.FindObjectsByType<UI_SkillSlot>(FindObjectsInactive.Include);
        }

        foreach (var slot in skillSlots)
        {
            if (slot != null && slot.skillType == skillType)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }

        return null;
    }

    private void UpdateHealthBar()
    {
        float currentHealth = Mathf.RoundToInt(player.health.GetCurrentHealth());
        float maxHealth = player.stats.GetMaxHealth();
        float sizeDiffrence = Mathf.Abs(maxHealth - healthRect.sizeDelta.x);

        if (sizeDiffrence > 0.1f)
        {
            healthRect.sizeDelta = new Vector2(maxHealth + maxHealth * 0.1f, healthRect.sizeDelta.y);
        }

        healthText.text = currentHealth + "/" + maxHealth;
        healthSlider.value = player.health.GetHealthPercentage();
    }
}
