using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Regeneracion")]
    public float regenDelay = 3f;
    public float regenSpeed = 10f;

    [Header("UI")]
    public Slider healthBar;

    private CharacterController controller;
    private float lastDamageTime;
    private bool dead = false;

    private WaveManager waveManager;

    [Header("Audio")]
    public AudioClip damageClip;

    [Header("Screen Shake")]
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.15f;

    private AudioSource audioSource;
    private CameraShake cameraShake;

    void Start()
    {
        currentHealth = maxHealth;

        controller =
            GetComponent<CharacterController>();

        waveManager =
            FindFirstObjectByType<WaveManager>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        if (damageClip == null)
        {
            damageClip =
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/bullet_impact"
                );
        }

        Camera mainCam = GetComponentInChildren<Camera>();
        if (mainCam != null)
        {
            cameraShake = mainCam.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = mainCam.gameObject.AddComponent<CameraShake>();
            }
        }

        if (healthBar != null)
        {
            healthBar.maxValue =
                maxHealth;

            healthBar.value =
                currentHealth;
        }
    }

    void Update()
    {
        if (dead)
            return;

        RegenerarVida();
    }

    public void TakeDamage(float damage)
    {
        if (dead)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maxHealth
            );

        if (audioSource != null && damageClip != null)
        {
            audioSource.PlayOneShot(damageClip);
        }

        if (cameraShake != null)
        {
            cameraShake.Shake(shakeIntensity, shakeDuration);
        }

        lastDamageTime = Time.time;

        UpdateUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDamaged();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void RegenerarVida()
    {
        bool quieto =
            controller.velocity.magnitude < 0.1f;

        float delayFinal =
            regenDelay;

        if (waveManager != null)
        {
            delayFinal +=
                waveManager.waveActual * 0.5f;
        }

        if (
            quieto &&
            Time.time >
            lastDamageTime + delayFinal
        )
        {
            currentHealth +=
                regenSpeed * Time.deltaTime;

            currentHealth =
                Mathf.Clamp(
                    currentHealth,
                    0,
                    maxHealth
                );

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value =
                currentHealth;
        }
    }

    void Die()
    {
        dead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}