using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveSystem
{
    private const int MAX_TOP = 5;

    // =========================
    // GUARDAR SCORE
    // =========================
    public static void GuardarScore(int nuevoScore)
    {
        List<int> scores =
            CargarScores();

        scores.Add(nuevoScore);

        scores =
            scores
            .OrderByDescending(x => x)
            .Take(MAX_TOP)
            .ToList();

        for (int i = 0; i < MAX_TOP; i++)
        {
            int valor =
                i < scores.Count
                ? scores[i]
                : 0;

            PlayerPrefs.SetInt(
                "TOP" + i,
                valor
            );
        }

        PlayerPrefs.Save();
    }

    // =========================
    // CARGAR SCORES
    // =========================
    public static List<int> CargarScores()
    {
        List<int> scores =
            new List<int>();

        for (int i = 0; i < MAX_TOP; i++)
        {
            scores.Add(
                PlayerPrefs.GetInt(
                    "TOP" + i,
                    0
                )
            );
        }

        return scores;
    }
}