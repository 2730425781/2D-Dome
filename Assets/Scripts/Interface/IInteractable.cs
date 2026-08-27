using UnityEngine;

/// <summary>
/// 可交互对象接口：Player.TryInteract 用 OverlapCircleAll 扫描周围碰撞体，
/// 通过 GetComponent&lt;IInteractable&gt;() 判断"能否交互"，并对最近的一个调用 Interact()。
/// 为什么用接口而不是基类：宝箱/NPC/机关之间没有任何共享逻辑或状态，
/// 接口让任意类型的对象都能挂上交互能力，且不占用继承位。
/// </summary>
public interface IInteractable
{
    // 玩家按下交互键时被调用；具体行为（开箱、对话等）由各实现类自行决定
    public void Interact();
}
