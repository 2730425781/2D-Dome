using UnityEngine;

/// <summary>
/// 可存档接口：任何想被 SaveManager 保存/恢复的对象实现它。
///
/// 为什么用接口而不是基类：玩家背包、技能树、存储、检查点等对象类型各异，
/// 但它们有共同的"把数据写入 GameData / 从 GameData 恢复"需求。
/// 接口让这些对象无需继承同一个基类即可被 SaveManager 统一遍历调用，
/// 也避免为保存逻辑强行引入一个公共继承层级。
/// </summary>
public interface ISaveable
{
    /// <summary>从存档数据恢复自身状态（会覆盖运行时状态）。</summary>
    public void LoadData(GameData data);

    /// <summary>把自身状态写入存档数据。ref 让各对象往同一份数据上累加，而非各持一份拷贝。</summary>
    public void SaveData(ref GameData data);
}
