using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 物品 ToolTip：展示物品名称、类型与详细属性，继承 UI_ToolTip 复用定位逻辑。
/// 为什么内容由代码按类型拼装而不是在 Inspector 里配死：
/// 材料/消耗品/装备展示的信息结构完全不同，运行时按类型拼装
/// 才能用同一个面板覆盖所有物品种类。
/// </summary>
public class UI_ItemToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemType;
    [SerializeField] private TextMeshProUGUI itemInfo;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Transform merchantInfo;

    public void ShowToolTip(bool show, RectTransform targetRect, Inventory_Item item, bool buyPrice = false, bool showMerchantInfo = false)
    {
        // 组件所在物体已被销毁时（关闭域重载的播放模式切换会残留托管引用），
        // base 里已提前返回，但这里还会访问 itemName 等子物体引用，
        // 必须先拦掉，否则会在已销毁的 TextMeshProUGUI 上继续抛异常
        if (this == null) return;

        base.ShowToolTip(show, targetRect);

        merchantInfo.gameObject.SetActive(showMerchantInfo);

        int price = buyPrice ? item.buyPrice : Mathf.FloorToInt(item.sellPride);

        string fullStackPrice = $"价格：{price}x{item.stackSize}";
        string singleStackPrice = $"价格：{price}";
        // 每次显示都重新填充文本：物品会在不同槽位间移动，
        // 不重置旧内容会让上一个物品的信息残留

        itemPrice.text = item.stackSize > 1 ? fullStackPrice : singleStackPrice;
        itemType.text = item.GetItemTypeName(item.itemDate.itemType);
        itemInfo.text = item.GetItemInfo();

        string color = GetColorByRarity(item.itemDate.itemRarity);
        itemName.text = GetColoredText(color, item.itemDate.itemName);
    }

    /// <summary>
    /// 按稀有度返回十六进制颜色（TMP 富文本可直接使用）。
    /// 用 hex 而非命名色：精确控制配色，避免 TMP 命名色的版本差异。
    /// 配色按暗色 ToolTip 背景挑选，保证文字清晰可读：
    /// 普通=浅灰白 / 优秀=鲜绿 / 稀有=亮蓝 / 史诗=亮紫 / 传说=暖橙（经典 RPG 稀有度体系）
    /// </summary>
    private string GetColorByRarity(int rarity)
    {
        if (rarity <= 100) return "#E8E8E8";  // 普通
        if (rarity <= 300) return "#58D68D";  // 优秀
        if (rarity <= 600) return "#4DA6FF";  // 稀有
        if (rarity <= 800) return "#C770FF";  // 史诗

        return "#FFB13D";                     // 传说
    }
}