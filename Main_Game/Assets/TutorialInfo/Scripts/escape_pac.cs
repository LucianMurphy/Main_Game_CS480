using UnityEngine;

public class EscapeWall : MonoBehaviour
{
    // This part handles the AI (Pac-Man)
    private void OnCollisionEnter(Collision collision)
    {
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null) 
        {
            gm.RestartGame();
        }
    }

    // This part handles the Player (Ghost with CharacterController)
    // Note: This must be on the PLAYER script OR use a trigger check
    // To keep it simple, we can use OnTriggerStay if you make a 
    // secondary thin trigger collider, OR just add this to PlayerMovement.cs:
}