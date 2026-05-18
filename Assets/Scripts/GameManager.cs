using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    private Button newGameButton;
    private Button menuButton;
    private Button resumeButton;
    private Button pauseMenuButton;
    private GunSelector gunSelector;

    [Header("Grenades")]
    public int grenadeCount = 0;
    public TMP_Text grenadeText;
    public GameObject grenadePickupPrefab;
    public float grenadePickupInterval = 15f;
    public float grenadeSpawnRadius = 40f;

    [Header("Combo")]
    public float comboDecayTime = 3f;
    public TMP_Text comboMilestoneText;

    private float comboTimer;
    private int lastMilestone;
    private Vector3 comboOriginalScale;

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

        if (comboText != null)
            comboOriginalScale = comboText.transform.localScale;

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
        if (mainCam != null)
        {
            if (mainCam.GetComponent<PostProcessingSetup>() == null)
                mainCam.gameObject.AddComponent<PostProcessingSetup>();

            gunSelector = mainCam.GetComponent<GunSelector>();
            if (gunSelector == null)
                gunSelector = mainCam.gameObject.AddComponent<GunSelector>();
        }

        CrearBotonesGameOver();
        ActualizarUIGranada();
        InvokeRepeating(nameof(SpawnearPickupGranada), grenadePickupInterval, grenadePickupInterval);
    }

    void SpawnearPickupGranada()
    {
        if (gameEnded || isPaused) return;

        WaveManager wm = FindFirstObjectByType<WaveManager>();
        if (wm == null || wm.spawnPoints == null || wm.spawnPoints.Length == 0) return;

        Transform spawnPoint = wm.spawnPoints[Random.Range(0, wm.spawnPoints.Length)];
        Vector3 spawnPos = spawnPoint.position + Vector3.up * 0.5f;

        GameObject pickup;

        if (grenadePickupPrefab != null)
        {
            pickup = Instantiate(grenadePickupPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            GameObject gbPrefab = Resources.Load<GameObject>("Blasters/grenade-b");
            if (gbPrefab == null) return;

            pickup = Instantiate(gbPrefab, spawnPos, Quaternion.identity);
            pickup.transform.localScale = Vector3.one * 0.5f;

            if (pickup.GetComponent<Collider>() == null)
            {
                SphereCollider sc = pickup.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 1f;
            }
        }

        pickup.AddComponent<GrenadePickup>();
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
            return;
        }

        if (isPaused) return;

        if (Input.GetKeyDown(KeyCode.Q) && grenadeCount > 0 && !gameEnded)
        {
            grenadeCount--;
            LanzarGranada();
            ActualizarUIGranada();
        }

        if (comboText != null && comboText.transform.localScale != comboOriginalScale)
        {
            comboText.transform.localScale = Vector3.Lerp(
                comboText.transform.localScale,
                comboOriginalScale,
                Time.deltaTime * 10f
            );

            if (Vector3.Distance(comboText.transform.localScale, comboOriginalScale) < 0.01f)
                comboText.transform.localScale = comboOriginalScale;
        }

        if (comboMilestoneText != null && comboMilestoneText.gameObject.activeSelf)
        {
            Color c = comboMilestoneText.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 2f);
            comboMilestoneText.color = c;

            if (c.a <= 0f)
                comboMilestoneText.gameObject.SetActive(false);
        }

        if (combo > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0f)
            {
                combo = 0;
                lastMilestone = 0;
                ActualizarUI();

                if (comboText != null)
                    comboText.transform.localScale = comboOriginalScale;
            }
        }
    }

    // =========================
    // ZOMBIE MUERTO
    // =========================
    public void ZombieKilled(bool headshot)
    {
        combo++;
        comboTimer = comboDecayTime;

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

        if (comboText != null)
        {
            comboText.transform.localScale = comboOriginalScale * 1.3f;
        }

        int milestone = (combo / 5) * 5;

        if (milestone >= 5 && milestone > lastMilestone)
        {
            lastMilestone = milestone;

            if (comboMilestoneText != null)
            {
                comboMilestoneText.text = "COMBO x" + milestone + "!";
                Color mc = comboMilestoneText.color;
                mc.a = 1f;
                comboMilestoneText.color = mc;
                comboMilestoneText.gameObject.SetActive(true);
            }
        }

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
        lastMilestone = 0;

        if (comboText != null)
            comboText.transform.localScale = comboOriginalScale;

        if (comboMilestoneText != null)
            comboMilestoneText.gameObject.SetActive(false);

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

        if (newGameButton != null)
            newGameButton.gameObject.SetActive(false);

        if (menuButton != null)
            menuButton.gameObject.SetActive(false);

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(true);

        if (pauseMenuButton != null)
            pauseMenuButton.gameObject.SetActive(true);

        if (audioSource != null && pauseClip != null)
        {
            audioSource.PlayOneShot(pauseClip);
        }

        if (musicSource != null)
        {
            musicSource.Pause();
        }

        if (gunSelector != null)
            gunSelector.SetVisible(true);
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

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);

        if (pauseMenuButton != null)
            pauseMenuButton.gameObject.SetActive(false);

        if (audioSource != null && resumeClip != null)
        {
            audioSource.PlayOneShot(resumeClip);
        }

        if (musicSource != null)
        {
            musicSource.UnPause();
        }

        if (gunSelector != null)
            gunSelector.SetVisible(false);
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

        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);

        if (pauseMenuButton != null)
            pauseMenuButton.gameObject.SetActive(false);

        if (newGameButton != null)
            newGameButton.gameObject.SetActive(true);

        if (menuButton != null)
            menuButton.gameObject.SetActive(true);

        if (gunSelector != null)
            gunSelector.SetVisible(false);
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
    // GRANADAS
    // =========================
    public void AddGrenade()
    {
        grenadeCount++;
        ActualizarUIGranada();
    }

    void ActualizarUIGranada()
    {
        if (grenadeText != null)
            grenadeText.text = "GRENADES: " + grenadeCount;
    }

    void LanzarGranada()
    {
        Camera cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;

        GameObject prefab = Resources.Load<GameObject>("Prefabs/grenade");
        if (prefab == null) return;

        GameObject grenade = Instantiate(prefab, cam.transform.position + cam.transform.forward * 1.5f, cam.transform.rotation);
        grenade.transform.localScale = Vector3.one * 2f;

        if (grenade.GetComponent<Collider>() == null)
        {
            SphereCollider sc = grenade.AddComponent<SphereCollider>();
            sc.isTrigger = false;
            sc.radius = 0.4f;
        }

        grenade.AddComponent<Grenade>();
    }

    // =========================
    // BOTONES (GAME OVER + PAUSA)
    // =========================
    void CrearBotonesGameOver()
    {
        if (panel == null) return;

        newGameButton = CrearBoton(
            "NewGameBtn",
            "NEW GAME",
            new Vector2(0, -110),
            () =>
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/UI/ui_click_01"));
                RestartGame();
            }
        );

        menuButton = CrearBoton(
            "MenuBtn",
            "MAIN MENU",
            new Vector2(0, -200),
            () =>
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/UI/ui_click_01"));
                VolverMenu();
            }
        );

        resumeButton = CrearBoton(
            "ResumeBtn",
            "RESUME",
            new Vector2(0, -155),
            () =>
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/UI/ui_click_01"));
                ReanudarJuego();
            }
        );

        pauseMenuButton = CrearBoton(
            "PauseMenuBtn",
            "MAIN MENU",
            new Vector2(0, -245),
            () =>
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(Resources.Load<AudioClip>("Audio/UI/ui_click_01"));
                VolverMenu();
            }
        );

        newGameButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(false);
        pauseMenuButton.gameObject.SetActive(false);
    }

    Button CrearBoton(string name, string texto, Vector2 anchoredPos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(panel.transform, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(390, 83);
        rt.anchoredPosition = anchoredPos;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.7f, 0.1f, 0.1f, 0.8f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.7f, 0.1f, 0.1f, 0.8f);
        colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.4f, 0.05f, 0.05f, 0.9f);
        btn.colors = colors;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
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