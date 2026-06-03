using UnityEngine;

public class pip_sound : MonoBehaviour
{
    public AudioClip playerSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
            PlaySound(playerSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}
