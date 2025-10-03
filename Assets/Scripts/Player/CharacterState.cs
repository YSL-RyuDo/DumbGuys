using UnityEngine;

public abstract class CharacterState
{
    protected PlayerCharacter player;

    public CharacterState(PlayerCharacter player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void HandleInput() { }
    public virtual void Update() { }
}

public class IdleState : CharacterState
{
    public IdleState(PlayerCharacter player) : base(player) { }

    public override void HandleInput()
    {
        if (player.InputProvider.MoveAxis.sqrMagnitude > 0.01f)
            player.ChangeState(new MoveState(player));

        if (player.InputProvider.JumpPressed && player.Motor.IsGrounded())
            player.ChangeState(new JumpState(player));
    }
}

public class MoveState : CharacterState
{
    public MoveState(PlayerCharacter player) : base(player) { }

    public override void Update()
    {
        Vector2 moveAxis = player.InputProvider.MoveAxis;
        Vector3 moveDir = player.GetCameraRelativeMove(moveAxis);

        // 이동 처리
        player.CurrentStrategy.ApplyMovement(player.Motor, moveDir);

        // 카메라 정면 기준으로 회전 유지
        Vector3 camForward = Vector3.Scale(player.CameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        if (camForward.sqrMagnitude > 0.01f)
            player.Motor.Rotate(camForward);
    }

    public override void HandleInput()
    {
        if (player.InputProvider.MoveAxis.sqrMagnitude < 0.01f)
            player.ChangeState(new IdleState(player));

        if (player.InputProvider.JumpPressed && player.Motor.IsGrounded())
            player.ChangeState(new JumpState(player));
    }
}

public class JumpState : CharacterState
{
    public JumpState(PlayerCharacter player) : base(player) { }

    public override void Enter()
    {
        player.Motor.Jump();
        player.SetJumping(true); // 점프 시작 애니메이션
    }

    public override void HandleInput()
    {
        if (player.Motor.IsGrounded())
        {
            player.SetJumping(false); // 착지 애니메이션
            player.ChangeState(new IdleState(player));
        }
    }
}
