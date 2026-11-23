using UnityEngine;
using UnityEngine.UI;

public class VoidDetector : MonoBehaviour
{
    [Header("Fall Detection")]
    public float fallLimit = -10f;

    [Header("UI")]
    public Text messageText;
    public string fallMessage = "Eevee cayó al vacío!";

    [Header("Audio")]
    public AudioClip fallSound;

    private AudioSource audioSource;
    private bool hasFallen = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!hasFallen && transform.position.y < fallLimit)
        {
            hasFallen = true;
            ShowFallMessage();
        }
    }

    void ShowFallMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = fallMessage;
        }

        if (fallSound != null)
            audioSource.PlayOneShot(fallSound);

        // Reiniciar despues de 2 segundos
        Invoke(nameof(ResetPlayer), 2f);
    }

    void ResetPlayer()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);

        hasFallen = false;
        transform.position = new Vector3(0f, 1f, 0f);
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
}
