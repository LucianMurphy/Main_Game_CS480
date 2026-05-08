using System.Collections;
using UnityEngine;

public class PacMan_sound : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clipA;
    [SerializeField] AudioClip clipB;
    [SerializeField] [Min(0f)] float delayBetweenSounds = 0.1f;
    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        StartCoroutine(LoopAlternating());
    }
    IEnumerator LoopAlternating()
    {
        while (true)
        {
            audioSource.clip = clipA;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(delayBetweenSounds);

            audioSource.clip = clipB;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(delayBetweenSounds);
        }
    }
}