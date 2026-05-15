using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;

    [Header("UI HUD")]
    public GameObject hud;

    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text multiplierText;

    [Header("Wave UI")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;

    [Header("Panels")]
    public GameObject panel;

    [Header("Game Over")]
    public TMP_Text gameOverText;

    [Header("Pause")]
    public TMP_Text pauseText;
    public TMP_Text pauseTitle;

    [Header("Audio")]
    public AudioClip[] scoreClips;
    public AudioClip[] comboClips;
    public AudioClip pauseClip;
    public AudioClip resumeClip;
    public AudioClip[] gameOverClips;
    public AudioClip gameMusic;

    private bool isPaused = false;
    private bool gameEnded = false;
    private AudioSource audioSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.4f;

        if (gameMusic == null)
        {
            gameMusic =
                Resources.Load<AudioClip>(
                    "Audio/Music/game_music"
                );
        }

        if (musicSource != null && gameMusic != null)
        {
            musicSource.clip = gameMusic;
            musicSource.Play();
        }

        if (scoreClips == null || scoreClips.Length == 0)
        {
            scoreClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_score_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_score_02"
                ),
            };
        }

        if (comboClips == null || comboClips.Length == 0)
        {
            comboClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_combo_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_combo_02"
                ),
            };
        }

        if (pauseClip == null)
        {
            pauseClip =
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_pause_01"
                );
        }

        if (resumeClip == null)
        {
            resumeClip =
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_resume_01"
                );
        }

        if (gameOverClips == null || gameOverClips.Length == 0)
        {
            gameOverClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_gameover_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_gameover_02"
                ),
            };
        }

        ActualizarUI();

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(false);
        }

        if (pauseTitle != null)
        {
            pauseTitle.gameObject.SetActive(false);
        }

        if (Hitmarker.Instance == null)
        {
            new GameObject("Hitmarker", typeof(Hitmarker));
        }

        Camera mainCam = FindFirstObjectByType<Camera>();
        if (mainCam != null && mainCam.GetComponent<PostProcessingSetup>() == null)
        {
            mainCam.gameObject.AddComponent<PostProcessingSetup>();
        }
    }

    void Update()
    {
        if (gameEnded)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    // =========================
    // ZOMBIE MUERTO
    // =========================
    public void ZombieKilled(bool headshot)
    {
        combo++;

        if (combo > maxCombo)
        {
            maxCombo = combo;
        }

        int multiplicador =
            Mathf.Clamp(
                1 + combo / 5,
                1,
                10
            );

        int puntos =
            100 * multiplicador;

        if (headshot)
        {
            puntos += 50;
        }

        score += puntos;

        if (audioSource != null)
        {
            if (scoreClips != null && scoreClips.Length > 0)
            {
                AudioClip clip =
                    scoreClips[
                        Random.Range(
                            0,
                            scoreClips.Length
                        )
                    ];

                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }

            if (combo > 1 &&
                comboClips != null &&
                comboClips.Length > 0)
            {
                AudioClip clip =
                    comboClips[
                        Random.Range(
                            0,
                            comboClips.Length
                        )
                    ];

                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }

        ActualizarUI();
    }

    // =========================
    // PLAYER DA�ADO
    // =========================
    public void PlayerDamaged()
    {
        combo = 0;

        ActualizarUI();
    }

    // =========================
    // PAUSA
    // =========================
    void PausarJuego()
    {
        isPaused = true;

        Time.timeScale = 0f;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        if (hud != null)
        {
            hud.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(true);
        }

        if (pauseTitle != null)
        {
            pauseTitle.gameObject.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (audioSource != null && pauseClip != null)
        {
            audioSource.PlayOneShot(pauseClip);
        }

        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    // =========================
    // REANUDAR
    // =========================
    void ReanudarJuego()
    {
        isPaused = false;

        Time.timeScale = 1f;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        if (hud != null)
        {
            hud.SetActive(true);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(false);
        }

        if (pauseTitle != null)
        {
            pauseTitle.gameObject.SetActive(false);
        }

        if (audioSource != null && resumeClip != null)
        {
            audioSource.PlayOneShot(resumeClip);
        }

        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    // =========================
    // GAME OVER
    // =========================
    public void GameOver()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        SaveSystem.GuardarScore(score);

        Time.timeScale = 0f;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        if (hud != null)
        {
            hud.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(false);
        }

        if (pauseTitle != null)
        {
            pauseTitle.gameObject.SetActive(false);
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (audioSource != null &&
            gameOverClips != null &&
            gameOverClips.Length > 0)
        {
            AudioClip clip =
                gameOverClips[
                    Random.Range(
                        0,
                        gameOverClips.Length
                    )
                ];

            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    // =========================
    // REINICIAR
    // =========================
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager
            .GetActiveScene()
            .buildIndex
        );
    }

    // =========================
    // MENU
    // =========================
    public void VolverMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "MenuScene"
        );
    }

    // =========================
    // UI
    // =========================
    void ActualizarUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "SCORE: " + score;
        }

        if (comboText != null)
        {
            comboText.text =
                "COMBO: x" + combo;
        }

        if (multiplierText != null)
        {
            int multiplicador =
                Mathf.Clamp(
                    1 + combo / 5,
                    1,
                    10
                );

            multiplierText.text =
                "MULTI: x" +
                multiplicador;
        }
    }
}