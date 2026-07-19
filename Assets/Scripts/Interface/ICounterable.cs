using UnityEngine;

/// <summary>
/// 可被反击接口。
/// 
/// 与 IDamagable 分离的原因：
/// 反击和受伤是两个独立的交互，一个敌人可以同时支持受伤和反击，
/// 但逻辑不同（反击触发眩晕而非扣血），拆开让实现更清晰。
/// </summary>
public interface ICounterable
{
    public void HandleCounter();
}