using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;
    private Image skillIcon;
    private RectTransform rect;
    private Button button;
    private SkillDataSO skillData;

    public SkillType skillType;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private string inputKeyName;
    [SerializeField] private TextMeshProUGUI inputKeyText;
    [SerializeField] private GameObject conflictSlot;

    private void OnValidate()
    {
        gameObject.name = "技能槽位 - " + skillType.ToString();
    }

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        button = GetComponent<Button>();
        skillIcon = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    public void SetupSkillSlots(SkillDataSO skillData)
    {
        this.skillData = skillData;
        Color color = Color.black; color.a = 0.6f;
        cooldownImage.color = color;

        inputKeyText.text = inputKeyName;
        skillIcon.sprite = skillData.icon;

        if (conflictSlot != null)
        {
            conflictSlot.SetActive(false);
        }
    }

    public void StartCooldown(float cooldown)
    {
        cooldownImage.fillAmount = 1;
        StartCoroutine(CooldownCo(cooldown));
    }

    public void ResetCooldown() => cooldownImage.fillAmount = 0;

    private IEnumerator CooldownCo(float duration)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            cooldownImage.fillAmount = 1f - (timePassed / duration);
            yield return null;
        }

        cooldownImage.fillAmount = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData == null) return;

        ui.skillToolTip.ShowToolTip(true, rect, skillData, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(false, null);
    }
}
