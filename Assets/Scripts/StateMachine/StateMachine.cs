using UnityEngine;

/// <summary>
/// 状态机。核心职责：管理当前状态的生命周期（Enter/Update/Exit）。
/// 
/// 关键设计选择：
/// 1. 用 canChangeState 开关来防止死亡等不可打断状态被意外覆盖
/// 2. ChangeState 是同步调用（立即 Exit → 立即 Enter），不是排队或延迟执行
///    这要求调用方确保在正确的时机切换状态（例如动画事件触发后）
/// 3. 没有状态栈或历史记录——当前游戏需求不需要回溯前一个状态
/// </summary>
public class StateMachine
{
    public EntityState currentState { get; private set; }
    public bool canChangeState = true;   // 设为 false 后 ChangeState 会被静默忽略

    /// <summary>
    /// 初始化状态机并进入起始状态。
    /// </summary>
    public void Initialize(EntityState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    /// <summary>
    /// 切换到新状态。如果 canChangeState 为 false 则忽略。
    /// 先 Exit 当前状态，再 Enter 新状态——保证每次只有一个状态处于活跃状态。
    /// </summary>
    public void ChangeState(EntityState newState)
    {
        if (!canChangeState)
        {
            return;
        }
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    /// <summary>
    /// 每帧由 Entity.Update 调用，驱动当前状态的 Update 逻辑。
    /// </summary>
    public void UpdateActiveState()
    {
        currentState.Update();
    }

    /// <summary>
    /// 关闭状态机（例如敌人死亡后不再需要状态切换）。
    /// </summary>
    public void SwitchOffStateMachine() => canChangeState = false;
}