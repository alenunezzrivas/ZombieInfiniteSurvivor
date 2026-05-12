using TMPro;
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
    public GameObject gameOverText;

    private CharacterController controller;

    private float lastDamageTime;

    private bool dead = false;

    void Start()
    {
        currentHealth = maxHealth;

        controller = GetComponent<CharacterController>();

        UpdateUI();
    }

    void Update()
    {
        if (dead) return;

        RegenerarVida();
    }

    // =========================
    // RECIBIR DAÑO
    // =========================
    public void TakeDamage(float damage)
    {
        if (dead) return;

        currentHealth -= damage;

        lastDamageTime = Time.time;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        UpdateUI();

        Debug.Log("PLAYER VIDA: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =========================
    // REGENERACION
    // =========================
    void RegenerarVida()
    {
        // QUIETO
        bool quieto =
            controller.velocity.magnitude < 0.1f;

        // ESPERAR
        if (quieto &&
            Time.time > lastDamageTime + regenDelay)
        {
            currentHealth +=
                regenSpeed * Time.deltaTime;

            currentHealth = Mathf.Clamp(
                currentHealth,
                0,
                maxHealth
            );

            UpdateUI();
        }
    }

    // =========================
    // UI
    // =========================
    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    // =========================
    // MUERTE
    // =========================
    void Die()
    {
        dead = true;

        Debug.Log("GAME OVER");

        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }

        // BLOQUEAR CURSOR
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        // PARAR TIEMPO
        Time.timeScale = 0f;
    }
}