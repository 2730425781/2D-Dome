using UnityEngine;

/// </summary>
/// 迷你血条UI组件，挂载为 Entity 子物体。
/// 
/// 为什么用事件的 OnFlipped 而不在 Entity.Update 中轮询 rotation：
/// Entity.Flip() 通过 transform.Rotate 旋转整个对象（包括子物体），
/// 血条需要保持正面朝上，所以必须在翻转后重置 rotation。
/// 即使血条是子物体，由于 Entity 整个翻转，血条也跟着转了180度，
/// 必须主动旋转回来——事件通知比每帧检查朝向更高效。
/// </summary>
public class UI_MiniHealthBar : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    private void OnEnable()
    {
        if (entity == null)
            return;

        entity.OnFlipped += HandleFlip;
    }

    private void OnDisable()
    {
        if (entity == null)
            return;

        entity.OnFlipped -= HandleFlip;
    }

    private void HandleFlip() => transform.rotation = Quaternion.identity;
}
