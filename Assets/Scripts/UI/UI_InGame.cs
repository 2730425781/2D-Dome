using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private UI_SkillSlot[] skillSlots;

    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

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
        UpdateHealthBar();
        if (player != null && player.health != null)
        {
            player.health.OnHealthUpdate += UpdateHealthBar;
        }
    }

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
