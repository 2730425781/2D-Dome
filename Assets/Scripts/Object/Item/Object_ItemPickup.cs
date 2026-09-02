using UnityEngine;

/// <summary>
/// 地面掉落物：玩家靠近即自动拾取，无需按交互键。
///
/// 为什么单独一个组件而不是把拾取逻辑写进背包系统：
/// 掉落物是"场景里的物"（有 Sprite、有触发器），背包是"玩家身上的数据"，
/// 用碰撞事件解耦后，任何带 Inventory_Base 的角色碰到都能拾取。
///
/// 为什么在 Awake 里 new 一个 Inventory_Item 而不是直接持有 ItemDateSO：
/// 背包系统操作的是带堆叠数与唯一 itemId 的运行时实例（见 Inventory_Item 构造），
/// 掉落物必须"铸造"自己的实例，否则两个掉落物会共享同一份数据、堆叠互相串。
/// </summary>
public class Object_ItemPickup : MonoBehaviour
{
    [SerializeField] private Vector2 dropForce = new Vector2(3, 10);
    // 仅在编辑器的 OnValidate 中用于预览图标，运行时用不到
    // 数据源用 ScriptableObject：同一配置可被多个掉落物引用，
    // 改一处所有引用同步生效，且不会随每个场景重复拷贝数据
    [SerializeField] private ItemDataSO itemDate;
    [Space]

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    /// <summary>
    /// 编辑器专用回调：在 Inspector 里改完 itemDate 立即刷新图标与物体名，
    /// 让策划不进 Play 模式就能在场景中看到掉落物长什么样。
    /// </summary>
    private void OnValidate()
    {
        // 尚未配置数据时跳过，避免用空值把已有图标/名字清掉
        if (itemDate == null) return;

        sr = GetComponent<SpriteRenderer>();
        // 直接同步数据资产的图标，保证预览与真实物品外观一致
        SetupVisuals();
    }

    public void SetupItem(ItemDataSO itemDate)
    {
        this.itemDate = itemDate;
        SetupVisuals();

        // 随机左右散开：dropForce.x = 水平扩散幅度（±x），dropForce.y = 上抛高度。
        // 不能写成 Random.Range(-dropForce.x, dropForce.y)——那会把两个字段混在一起，
        // 范围变成 (-3, 10) 严重偏向右侧，看起来就像没有随机偏移
        float xForce = Random.Range(-dropForce.x, dropForce.x);
        rb.linearVelocity = new Vector2(xForce, dropForce.y);
        col.isTrigger = false;
    }

    private void SetupVisuals()
    {
        sr.sprite = itemDate.itemIcon;
        // 用物品名命名场景物体，方便在 Hierarchy 里按名字找掉落物
        gameObject.name = itemDate.itemName;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // NameToLayer 大小写敏感：项目里地面层叫 "Ground"（大写 G），
        // 写成小写 "ground" 会返回 -1，永远匹配不上 → 掉落物永远不会变成触发器
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground") || col.isTrigger)
        {
            return;
        }

        // 必须等物品"基本静止"才转触发器：
        // 出生点与地面重叠时物理解算会立刻把它顶出来（此时速度还很大），
        // 如果用 OnCollisionEnter 判断，物品一出生就被冻结在原地，dropForce 完全失效。
        // 改用 Stay（接触期间每帧触发）+ 速度门槛，只有真正落停才转换。
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            return;
        }

        // 只有"落在表面顶部"才转触发器（看接触法线是否朝上）：
        // 否则物品撞到墙面/斜坡的瞬间也会冻结在半空，变成永远捡不到的东西
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                col.isTrigger = true;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                break;
            }
        }
    }

    /// <summary>
    /// 触碰即拾取。先取背包并判空：碰撞的可能是地面、墙壁等没有背包的对象，
    /// 不判空会在下面直接空引用崩溃。
    /// 先 CanAddItem 再 AddItem：背包满时掉落物留在原地而不是凭空消失，
    /// 玩家清出格子后还能回来捡。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只有带背包的角色（玩家）才响应拾取，如果不是就不用继续执行了
        Inventory_Player inventory = collision.GetComponent<Inventory_Player>();
        if (inventory == null) return;

        Inventory_Item item = new Inventory_Item(itemDate);
        Inventoty_Storage storage = inventory.storage;

        if (itemDate.itemType == ItemType.Material)
        {
            storage.AddMaterialToStash(item);
            Destroy(gameObject);
            return;
        }

        // 放不下（背包满且无可堆叠槽位）就不销毁，物品保留在地上
        if (inventory.CanAddItem(item))
        {
            // 添加成功后 OnInventoryChange 事件会通知 UI 刷新背包（见 Inventory_Base.AddItem）
            inventory.AddItem(item);
            // 确认放进背包后才销毁，避免物品在"入包前"就消失
            Destroy(gameObject);
        }
    }
}
