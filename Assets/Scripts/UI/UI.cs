using UnityEngine;

public class UI : MonoBehaviour
{

    public UI_SkillToolTip skillToolTip { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }
    private bool skillTreeEnable;
    private bool inventoryEnable;

    private void Awake()
    {
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);

        skillTreeEnable = skillTreeUI.gameObject.activeSelf;
        inventoryEnable = inventoryUI.gameObject.activeSelf;
    }

    public void ToggleSkillTreeUI()
    {
        skillTreeEnable = !skillTreeEnable;
        skillTreeUI.gameObject.SetActive(skillTreeEnable);
        skillToolTip.ShowToolTip(false, null);
    }

    public void ToggleInvemtoryUI()
    {
        inventoryEnable = !inventoryEnable;
        inventoryUI.gameObject.SetActive(inventoryEnable);
        statToolTip.ShowToolTip(false, null);
        itemToolTip.ShowToolTip(false, null);
    }
}
