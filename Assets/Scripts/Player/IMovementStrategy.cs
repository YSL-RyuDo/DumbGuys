using UnityEngine;

public interface IMovementStrategy
{
    void ApplyMovement(CharacterMotor motor, Vector3 direction);
}

public class NormalMovement : IMovementStrategy
{
    public void ApplyMovement(CharacterMotor motor, Vector3 direction)
    {
        motor.Move(direction);
    }
}

public class SlipperyMovement : IMovementStrategy
{
    public void ApplyMovement(CharacterMotor motor, Vector3 direction)
    {
        motor.Move(direction * 1.2f);
    }
}

public class DashMovement : IMovementStrategy
{
    public void ApplyMovement(CharacterMotor motor, Vector3 direction)
    {
        motor.Move(direction * 2f);
    }
}
