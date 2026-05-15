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

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

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

        MostrarTop5();

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    public void PlayGame()
    {
        if (audioSource != null && startClip != null)
        {
            audioSource.PlayOneShot(startClip);
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "GameScene"
        );
    }

    public void ExitGame()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
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