using UnityEngine;

public interface IInputProvider
{
    Vector2 MoveAxis { get; }
    bool JumpPressed { get; }
}

public class PlayerInputProvider : IInputProvider
{
    public Vector2 MoveAxis
    {
        get
        {
            if (GameUIManager.IsChatting)
                return Vector2.zero;

            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }

    public bool JumpPressed
    {
        get
        {
            // 채팅 중이면 점프 입력 무효화
            if (GameUIManager.IsChatting)
                return false;

            return Input.GetButtonDown("Jump");
        }
    }
}
