using UnityEngine;

public class Pellet_sound : MonoBehaviour
{
    public AudioClip playerSound;
    public AudioClip enemySound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
            PlaySound(playerSound);
        else if (other.CompareTag("PacMan"))
            PlaySound(enemySound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}
