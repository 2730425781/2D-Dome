using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ESC 暂停菜单：按下 ESC 弹出带四个入口的菜单（技能树 / 人物 / 存档 / 选项）。
/// 菜单免去场景/预制体编辑，运行时动态构建。
/// "存档"入口打开一个存档槽位选择面板（槽位 1/2/3，显示有无存档，可保存/加载/删除）。
/// ESC 键由 UI.SetupControlsUI 里的 ToggleOptionsUI 输入动作转发到本组件的 Toggle()。
///
/// 文本用 TextMeshProUGUI 而非 legacy Text：
/// 项目统一使用 AlibabaPuHuiTi SDF 字体，legacy Text 无法直接使用 TMP 字体资产；
/// TMP 还顺带带来更清晰的字号缩放（中文字体在 legacy Text 下会明显发虚）。
/// </summary>
public class UI_PauseMenu : MonoBehaviour
{
    private UI ui;
    private GameObject menuRoot;
    private bool isOpen;

    // 主菜单面板 与 存档面板：切换显示
    private RectTransform mainPanel;
    private RectTransform savePanel;
    private TextMeshProUGUI[] slotStatusTexts;

    [Header("菜单按钮")]
    [SerializeField] private string title = "菜单";
    [SerializeField] private string[] buttonLabels = { "技能树", "人物", "存档", "选项" };

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        BuildMenu();
        menuRoot.SetActive(false);
    }

    /// <summary>开关菜单（由 ESC 的 ToggleOptionsUI 输入动作调用）。</summary>
    public void Toggle()
    {
        isOpen = !isOpen;
        menuRoot.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;
        if (isOpen && ui != null) ui.HideAllTooltips();
    }

    // ---------- 主菜单入口 ----------

    private void OpenSkillTree() => OpenAndClose(() => ui?.ToggleSkillTreeUI());
    private void OpenCharacter() => OpenAndClose(() => ui?.ToggleInvemtoryUI());
    private void OpenOptions() => OpenAndClose(() => ui?.OpenOptionsUI());

    private void OpenSave()
    {
        // 切换到存档槽位面板，并刷新"有/无存档"状态
        mainPanel.gameObject.SetActive(false);
        savePanel.gameObject.SetActive(true);
        RefreshSlotStatus();
    }

    private void OpenAndClose(System.Action opener)
    {
        opener?.Invoke();
        isOpen = false;
        menuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    // ---------- 存档槽位面板 ----------

    private void RefreshSlotStatus()
    {
        var sm = SaveManager.instance;
        if (sm == null || slotStatusTexts == null) return;

        for (int i = 0; i < slotStatusTexts.Length; i++)
        {
            if (slotStatusTexts[i] != null)
            {
                slotStatusTexts[i].text = sm.HasSaveData(i + 1) ? "有存档" : "无存档";
            }
        }
    }

    private void SaveToSlot(int slot)
    {
        SaveManager.instance?.SaveGame(slot);
        RefreshSlotStatus();
    }

    private void LoadFromSlot(int slot)
    {
        SaveManager.instance?.LoadGame(slot);
        // 加载后关闭面板，回到游戏
        OpenAndClose(null);
    }

    private void DeleteSlot(int slot)
    {
        SaveManager.instance?.DeleteSaveData(slot);
        RefreshSlotStatus();
    }

    // ---------- 构建 UI ----------

    private void BuildMenu()
    {
        // 直接挂到主 Canvas（UI 对象）下，复用其 GraphicRaycaster 与 CanvasScaler。
        // 不要创建"嵌套的子 Canvas"：ScreenSpaceOverlay 的子 Canvas 不会被主 EventSystem
        // 正确命中，导致按钮点击无反应（RaycastAll 返回 0）。
        RectTransform container = CreateRect("PauseMenu", transform);
        container.SetAsLastSibling();          // 渲染在最上，盖住其它 UI
        container.anchorMin = Vector2.zero;    // 铺满屏幕作为容器的定位基准
        container.anchorMax = Vector2.one;
        container.offsetMin = container.offsetMax = Vector2.zero;

        mainPanel = CreatePanel(container, "MainPanel");
        BuildMainMenu(mainPanel);

        savePanel = CreatePanel(container, "SavePanel");
        savePanel.gameObject.SetActive(false);
        BuildSavePanel(savePanel);

        menuRoot = container.gameObject;
    }

    private RectTransform CreatePanel(RectTransform parent, string name)
    {
        RectTransform panel = CreateRect(name, parent);
        panel.anchorMin = new Vector2(0.3f, 0.25f);
        panel.anchorMax = new Vector2(0.7f, 0.75f);
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        Image img = panel.gameObject.AddComponent<Image>();
        img.color = new Color(0.05f, 0.06f, 0.12f, 0.92f);
        return panel;
    }

    private void BuildMainMenu(RectTransform panel)
    {
        AddText(panel, title, 34, new Vector2(0, 240), new Vector2(360, 50));

        float startY = 120f;
        float stepY = 120f;
        for (int i = 0; i < buttonLabels.Length; i++)
        {
            // 用每层迭代独立的局部变量 index 捕获，避免 C# for 循环变量被所有 lambda
            // 共享：否则所有按钮都指向最后一个 i，OnButton(4) 无对应 case -> 点击无反应。
            int index = i;
            string label = buttonLabels[i];
            AddButton(panel, label, new Vector2(0, startY - i * stepY), new Vector2(320, 70), () => OnButton(index));
        }
    }

    private void BuildSavePanel(RectTransform panel)
    {
        // 存档面板布局与主菜单一致：全部用"面板中心"锚定，自上而下按均匀间距排布。
        // 原来标题/返回用中心锚、行用底部锚，混用导致错位——标题偏高一大截、返回压到行上。
        AddText(panel, "存档", 34, new Vector2(0, 200), new Vector2(360, 50));

        int slotCount = SaveManager.instance != null ? SaveManager.instance.SlotCount : 3;
        slotStatusTexts = new TextMeshProUGUI[slotCount];

        // 依次下移 110，把 3 行 + 返回 均匀铺在面板中心下方
        float startY = 90f;
        float stepY = 110f;
        for (int slot = 1; slot <= slotCount; slot++)
        {
            BuildSlotRow(panel, slot, startY - (slot - 1) * stepY);
        }

        AddButton(panel, "返回", new Vector2(0, startY - slotCount * stepY), new Vector2(160, 60), () =>
        {
            savePanel.gameObject.SetActive(false);
            mainPanel.gameObject.SetActive(true);
        });
    }

    private void BuildSlotRow(RectTransform panel, int slot, float y)
    {
        // 行同样用面板中心锚定，使其与标题/返回处于同一中心垂直线。
        RectTransform row = CreateRect("SlotRow_" + slot, panel);
        row.anchorMin = new Vector2(0.5f, 0.5f);
        row.anchorMax = new Vector2(0.5f, 0.5f);
        row.anchoredPosition = new Vector2(0, y);
        row.sizeDelta = new Vector2(760, 80);

        // 行内元素水平铺开，行宽 760 保证最右的"删除"按钮也落在面板内
        // （原 620 过窄会把"删除"挤出面板右缘）。
        AddText(row, "槽位 " + slot, 26, new Vector2(-275, 0), new Vector2(140, 50));
        slotStatusTexts[slot - 1] = AddText(row, "无存档", 24, new Vector2(-115, 0), new Vector2(140, 50));
        AddButton(row, "保存", new Vector2(70, 0), new Vector2(110, 60), () => SaveToSlot(slot));
        AddButton(row, "加载", new Vector2(190, 0), new Vector2(110, 60), () => LoadFromSlot(slot));
        AddButton(row, "删除", new Vector2(310, 0), new Vector2(110, 60), () => DeleteSlot(slot));
    }

    private void OnButton(int index)
    {
        switch (index)
        {
            case 0: OpenSkillTree(); break;
            case 1: OpenCharacter(); break;
            case 2: OpenSave(); break;
            case 3: OpenOptions(); break;
        }
    }

    // ---------- 运行时 UI 助手 ----------

    private RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    private TextMeshProUGUI AddText(RectTransform parent, string content, float fontSize, Vector2 pos, Vector2 size)
    {
        var rt = CreateRect("Text", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.font = GetFontAsset();
        text.raycastTarget = false;
        return text;
    }

    private Button AddButton(RectTransform parent, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var rt = CreateRect("Btn_" + label, parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.4f, 1f);

        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(onClick);

        var textRt = CreateRect("Text", rt);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textRt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = Mathf.Clamp(size.x * 0.2f, 16f, 28f);
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.font = GetFontAsset();
        text.raycastTarget = false;

        return button;
    }

    /// <summary>
    /// 取项目统一字体。优先用 TMP 全局默认字体（项目里已设为 AlibabaPuHuiTi SDF），
    /// 这样以后换字体只改 TMP Settings 一处，运行时构建的面板自动跟随。
    /// </summary>
    private TMP_FontAsset GetFontAsset()
    {
        return TMP_Settings.defaultFontAsset;
    }
}
