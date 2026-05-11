using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Command
{
    public abstract void Execute(CharacterController controller, float speed);
}

public class MoveCommand : Command
{
    private Vector3 direction;

    public MoveCommand(Vector3 dir)
    {
        direction = dir;
    }

    public override void Execute(CharacterController controller, float speed)
    {
        Vector3 worldDir = controller.transform.TransformDirection(direction);
        controller.Move(worldDir * speed * Time.deltaTime);
    }
}

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float baseSpeed = 12f;

    // Set by EnergyDrink effect via ItemBarUI; reset to 1 when effect expires.
    [HideInInspector] public float speedMultiplier = 1f;

    private Command buttonW;
    private Command buttonA;
    private Command buttonS;
    private Command buttonD;

    void Start()
    {
        buttonW = new MoveCommand(Vector3.forward);
        buttonA = new MoveCommand(Vector3.left);
        buttonS = new MoveCommand(Vector3.back);
        buttonD = new MoveCommand(Vector3.right);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        float currentSpeed = baseSpeed * speedMultiplier;

        if (Keyboard.current.wKey.isPressed) buttonW.Execute(controller, currentSpeed);
        if (Keyboard.current.sKey.isPressed) buttonS.Execute(controller, currentSpeed);
        if (Keyboard.current.aKey.isPressed) buttonA.Execute(controller, currentSpeed);
        if (Keyboard.current.dKey.isPressed) buttonD.Execute(controller, currentSpeed);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("escape_walls"))
        {
            GameManager gm = Object.FindAnyObjectByType<GameManager>();
            if (gm != null) gm.PlayerEscaped();
        }
    }
}
