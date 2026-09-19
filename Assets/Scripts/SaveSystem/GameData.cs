using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档数据结构：被 JsonUtility 序列化写入单个存档文件（每个槽位一份）。
///
/// 为什么用 SerializableDictionary 而不是普通 Dictionary：
/// JsonUtility 不直接支持 Dictionary，需要可序列化的字典包装才能正常序列化/反序列化。
/// 各字段用字符串 / SkillType 作键而不是对象引用：对象引用在序列化后无法还原成
/// 同一实例，用稳定标识（物品名/枚举）才能在加载时正确对应回运行时数据。
/// </summary>
[Serializable]
public class GameData
{
    public int gold;                                    // 金币
    public int skillPoints;                             // 剩余技能点
    public List<Inventory_Item> itemList;               // 全部物品实例（含堆叠数）
    public SerializableDictionary<string, int> inventory;        // 背包：物品名 -> 数量
    public SerializableDictionary<string, int> storageItems;     // 仓库：物品名 -> 数量
    public SerializableDictionary<string, int> stroageMaterials; // 材料库：物品名 -> 数量
    public SerializableDictionary<string, ItemType> equipedItems;    // 已装备：槽位 -> 物品类型
    public SerializableDictionary<string, bool> skillTreeUI;         // 技能树节点解锁状态
    public SerializableDictionary<SkillType, SkillUpgradeType> skillUpgrades; // 各技能已升级分支
    public Vector3 savedCheckpoint;                     // 最近一次激活的检查点位置（重生点）
    public Vector3 lastPlayerPosition;

    public SerializableDictionary<string, bool> unlockedCheckpoints;
    public string lastScenePlayed;

    // 任务系统：以 questSaveID（资产 GUID）为键，避免保存对象引用。
    // activeQuests  = 已接取但未领奖的任务
    // questProgress = 任务当前累计进度（击杀/对话次数）
    // claimedQuests = 奖励已发放的任务（含自动发放与 NPC 领取两种）
    public SerializableDictionary<string, bool> activeQuests;
    public SerializableDictionary<string, int> questProgress;
    public SerializableDictionary<string, bool> claimedQuests;

    public GameData()
    {
        // 构造函数里初始化字典，避免在 Load 之前访问到 null
        inventory = new SerializableDictionary<string, int>();
        storageItems = new SerializableDictionary<string, int>();
        stroageMaterials = new SerializableDictionary<string, int>();
        equipedItems = new SerializableDictionary<string, ItemType>();

        skillTreeUI = new SerializableDictionary<string, bool>();
        skillUpgrades = new SerializableDictionary<SkillType, SkillUpgradeType>();

        unlockedCheckpoints = new SerializableDictionary<string, bool>();

        activeQuests = new SerializableDictionary<string, bool>();
        questProgress = new SerializableDictionary<string, int>();
        claimedQuests = new SerializableDictionary<string, bool>();
    }
}
