using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();
        }
    }
}
