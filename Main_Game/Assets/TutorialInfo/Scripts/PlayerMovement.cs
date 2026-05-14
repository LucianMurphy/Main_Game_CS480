using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Command
{
    public abstract void Execute(CharacterController controller, float speed, float velocityY);
}

public class MoveCommand : Command
{
    private Vector3 direction;
    private float gravity = -20f;
    public MoveCommand(Vector3 dir)
    {
        direction = dir;
    }

    public override void Execute(CharacterController controller, float speed, float velocityY)
    {
        Vector3 worldDir = controller.transform.TransformDirection(direction);

        Vector3 movement = (worldDir * speed) + (Vector3.up * velocityY);
        
        controller.Move(movement * Time.deltaTime);

        
    }
}

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float baseSpeed = 5f;
    private float gravity = -20f;
    private float velocityY;

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

        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f; // Slight downward force to keep grounded
        }
        else
        {
            velocityY += Time.deltaTime * gravity;
        }

        

        if (Keyboard.current.wKey.isPressed) buttonW.Execute(controller, currentSpeed, velocityY);
        if (Keyboard.current.sKey.isPressed) buttonS.Execute(controller, currentSpeed, velocityY);
        if (Keyboard.current.aKey.isPressed) buttonA.Execute(controller, currentSpeed, velocityY);
        if (Keyboard.current.dKey.isPressed) buttonD.Execute(controller, currentSpeed, velocityY);

        if (!Keyboard.current.anyKey.isPressed)
        {
            controller.Move(Vector3.up * velocityY * Time.deltaTime);
        }
    
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
