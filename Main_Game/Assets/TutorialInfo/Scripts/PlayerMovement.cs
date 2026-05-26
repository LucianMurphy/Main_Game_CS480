using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Command
{
    public abstract Vector3 Execute(Transform playerTransform, float speed);
}

public class MoveCommand : Command
{
    private Vector3 direction;
    public MoveCommand(Vector3 dir)
    {
        direction = dir;
    }

    public override Vector3 Execute(Transform playerTransform, float speed)
    {
        Vector3 worldDir = playerTransform.TransformDirection(direction);

        return worldDir * speed;        
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

        if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy) return;

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

        Vector3 finalMovement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) 
        {
            finalMovement += buttonW.Execute(controller.transform, currentSpeed);
        }
        if (Keyboard.current.sKey.isPressed) 
        {
            finalMovement += buttonS.Execute(controller.transform, currentSpeed);
        }
        if (Keyboard.current.aKey.isPressed) 
        {
            finalMovement += buttonA.Execute(controller.transform, currentSpeed);
        }
        if (Keyboard.current.dKey.isPressed) 
        {
            finalMovement += buttonD.Execute(controller.transform, currentSpeed);
        }

        finalMovement.y = velocityY;

        controller.Move(finalMovement * Time.deltaTime);
    
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
