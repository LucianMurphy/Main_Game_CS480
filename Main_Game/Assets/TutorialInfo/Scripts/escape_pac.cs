using UnityEngine;

public class EscapeWall : MonoBehaviour
{
    // Fires when PacMan AI physically collides with the escape wall.
    // Treat this the same as the player being killed (PacMan escaped).
    private void OnCollisionEnter(Collision collision)
    {
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null) gm.Win();
    }
}
