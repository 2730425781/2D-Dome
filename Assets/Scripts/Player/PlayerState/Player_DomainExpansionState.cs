using UnityEngine;

public class Player_DomainExpansionState : PlayerState
{
    private Vector2 originalPosition;
    private float originalGravity;
    private float finalRiseDistance;

    private bool isLevitating;
    private bool createdDomain;

    public Player_DomainExpansionState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        originalPosition = player.transform.position;
        originalGravity = rb.gravityScale;
        finalRiseDistance = GetRiseDistance();

        player.SetVelocity(0, player.riseSpeed);
        player.health.SetCanTakedamage(false);
    }

    public override void Update()
    {
        base.Update();

        if (Vector2.Distance(originalPosition, player.transform.position) >= finalRiseDistance && !isLevitating)
        {
            Levitate();
        }
        if (isLevitating)
        {
            skillManager.domainExpansion.DoSpellCasting();

            if (stateTimer <= 0)
            {
                isLevitating = false;
                rb.gravityScale = originalGravity;
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        createdDomain = false;
        player.health.SetCanTakedamage(true);
    }

    private void Levitate()
    {
        isLevitating = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        stateTimer = skillManager.domainExpansion.GetDomainDuration();

        if (createdDomain == false)
        {
            createdDomain = true;
            skillManager.domainExpansion.CreateDomain();
        }
    }

    private float GetRiseDistance()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(player.transform.position, Vector2.up, player.maxRiseDistance, player.groundLayer);

        return hit2D.collider != null ? hit2D.distance - 1 : player.maxRiseDistance;
    }
}
