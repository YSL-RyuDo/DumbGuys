using UnityEngine;

public interface IInputProvider
{
    Vector2 MoveAxis { get; }
    bool JumpPressed { get; }
}

public class PlayerInputProvider : IInputProvider
{
    public Vector2 MoveAxis => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    public bool JumpPressed => Input.GetButtonDown("Jump");
}
