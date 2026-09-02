using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ESC 暂停菜单：按下 ESC 弹出带四个入口的菜单（技能树 / 人物 / 存档 / 选项）。
/// 菜单免去场景/预制体编辑，运行时动态构建。
/// "存档"入口打开一个存档槽位选择面板（槽位 1/2/3，显示有无存档，可保存/加载/删除）。
/// ESC 键由 UI.SetupControlsUI 里的 ToggleOptionsUI 输入动作转发到本组件的 Toggle()。
/// </summary>
public class UI_PauseMenu : MonoBehaviour
{
    private UI ui;
    private GameObject menuRoot;
    private bool isOpen;

    // 主菜单面板 与 存档面板：切换显示
    private RectTransform mainPanel;
    private RectTransform savePanel;
    private Text[] slotStatusTexts;

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
        var canvasGo = new GameObject("PauseMenuCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        RectTransform canvasRt = canvasGo.GetComponent<RectTransform>();
        canvasRt.SetParent(transform, false);

        mainPanel = CreatePanel(canvasRt, "MainPanel");
        BuildMainMenu(mainPanel);

        savePanel = CreatePanel(canvasRt, "SavePanel");
        savePanel.gameObject.SetActive(false);
        BuildSavePanel(savePanel);

        menuRoot = canvasGo;
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
            string label = buttonLabels[i];
            AddButton(panel, label, new Vector2(0, startY - i * stepY), new Vector2(320, 70), () => OnButton(i));
        }
    }

    private void BuildSavePanel(RectTransform panel)
    {
        AddText(panel, "存档", 34, new Vector2(0, 240), new Vector2(360, 50));

        int slotCount = SaveManager.instance != null ? SaveManager.instance.SlotCount : 3;
        slotStatusTexts = new Text[slotCount];

        float y = 120f;
        for (int slot = 1; slot <= slotCount; slot++)
        {
            BuildSlotRow(panel, slot, y);
            y -= 100f;
        }

        AddButton(panel, "返回", new Vector2(0, y), new Vector2(160, 60), () =>
        {
            savePanel.gameObject.SetActive(false);
            mainPanel.gameObject.SetActive(true);
        });
    }

    private void BuildSlotRow(RectTransform panel, int slot, float y)
    {
        RectTransform row = CreateRect("SlotRow_" + slot, panel);
        row.anchorMin = new Vector2(0.5f, 0);
        row.anchorMax = new Vector2(0.5f, 0);
        row.anchoredPosition = new Vector2(0, y);
        row.sizeDelta = new Vector2(620, 80);

        AddText(row, "槽位 " + slot, 26, new Vector2(-210, 0), new Vector2(140, 50));
        slotStatusTexts[slot - 1] = AddText(row, "无存档", 24, new Vector2(-50, 0), new Vector2(140, 50));
        AddButton(row, "保存", new Vector2(110, 0), new Vector2(110, 60), () => SaveToSlot(slot));
        AddButton(row, "加载", new Vector2(235, 0), new Vector2(110, 60), () => LoadFromSlot(slot));
        AddButton(row, "删除", new Vector2(360, 0), new Vector2(110, 60), () => DeleteSlot(slot));
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

    private Text AddText(RectTransform parent, string content, int fontSize, Vector2 pos, Vector2 size)
    {
        var rt = CreateRect("Text", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var text = rt.gameObject.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = GetFont();
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
        Text text = textRt.gameObject.AddComponent<Text>();
        text.text = label;
        text.fontSize = Mathf.Clamp((int)(size.x * 0.2f), 16, 28);
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = GetFont();
        text.raycastTarget = false;

        return button;
    }

    private Font GetFont()
    {
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (f == null) { try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
        return f;
    }
}
