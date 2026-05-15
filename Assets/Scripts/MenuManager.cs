using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text top5Text;

    [Header("Audio")]
    public AudioClip clickClip;
    public AudioClip startClip;
    public AudioClip menuMusic;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Start()
    {
        // SFX source
        sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
        {
            sfxSource =
                gameObject.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;

        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.5f;

        if (clickClip == null)
        {
            clickClip =
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_click_01"
                );
        }

        if (startClip == null)
        {
            startClip =
                Resources.Load<AudioClip>(
                    "Audio/UI/ui_start_01"
                );
        }

        if (menuMusic == null)
        {
            menuMusic =
                Resources.Load<AudioClip>(
                    "Audio/Music/menu_music"
                );
        }

        if (musicSource != null && menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.Play();
        }

        MostrarTop5();

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    public void PlayGame()
    {
        if (sfxSource != null && startClip != null)
        {
            sfxSource.PlayOneShot(startClip);
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "GameScene"
        );
    }

    public void ExitGame()
    {
        if (sfxSource != null && clickClip != null)
        {
            sfxSource.PlayOneShot(clickClip);
        }

        Application.Quit();

        Debug.Log("Salir del juego");
    }

    void MostrarTop5()
    {
        List<int> scores =
            SaveSystem.CargarScores();

        string texto =
            "   TOP FIVE\n\n";

        for (int i = 0; i < scores.Count; i++)
        {
            texto +=
                (i + 1) +
                ". " +
                scores[i] +
                "\n";
        }

        top5Text.text = texto;
    }
}