using UnityEngine;

/// <summary>
/// 可受伤接口。
/// 
/// 为什么用接口而不是基类：
/// 1. Chest 已经继承 MonoBehaviour，无法再继承一个自定义基类
/// 2. 任何物体都可以实现 IDamagable 来获得受伤能力，不受继承树约束
/// 3. Entity_ComBat.PerformAttack() 只依赖这个接口，不关心目标具体是什么
/// </summary>
public interface IDamagable
{
    public void TakeDamage(float damage, Transform damageDealar);
}