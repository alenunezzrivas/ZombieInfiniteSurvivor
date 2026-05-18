using StarterAssets;
using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] footstepClips;
    public AudioClip[] sprintClips;

    [Header("Config")]
    public float walkInterval = 0.5f;
    public float sprintInterval = 0.35f;
    public float minSpeed = 0.5f;

    private CharacterController controller;
    private StarterAssetsInputs input;
    private AudioSource audioSource;
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (controller == null || input == null || audioSource == null)
            return;

        bool isGrounded = controller.isGrounded;
        bool isMoving = controller.velocity.magnitude > minSpeed;

        if (isGrounded && isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                bool sprinting = input.sprint;
                AudioClip[] clips = sprinting ? sprintClips : footstepClips;

                if (clips != null && clips.Length > 0)
                {
                    AudioClip clip = clips[Random.Range(0, clips.Length)];
                    audioSource.PlayOneShot(clip);
                }

                stepTimer = sprinting ? sprintInterval : walkInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }
}
