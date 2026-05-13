using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    public GameObject zombieNormalPrefab;
    public GameObject zombieFuertePrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Spawn")]
    public float tiempoEntreSpawns = 1f;

    [Header("Oleadas")]
    public int waveActual = 1;
    public int enemigosBase = 5;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;

    private int enemigosVivos = 0;

    void Start()
    {
        // BUSCAR PLAYER AUTOMATICAMENTE
        if (player == null)
        {
            GameObject p =
                GameObject.FindGameObjectWithTag("Player");

            if (p != null)
            {
                player = p.transform;
            }
        }

        StartCoroutine(IniciarOleada());
    }

    IEnumerator IniciarOleada()
    {
        yield return new WaitForSeconds(2f);

        int cantidad =
            enemigosBase + (waveActual * 2);

        if (waveText != null)
        {
            waveText.text =
                "OLEADA " + waveActual;
        }

        for (int i = 0; i < cantidad; i++)
        {
            SpawnZombie();

            yield return new WaitForSeconds(
                tiempoEntreSpawns
            );
        }
    }

    void SpawnZombie()
    {
        // =========================
        // COMPROBAR SPAWN POINTS
        // =========================
        if (
            spawnPoints == null ||
            spawnPoints.Length == 0
        )
        {
            Debug.LogWarning(
                "No hay Spawn Points asignados."
            );

            return;
        }

        // =========================
        // ELEGIR SPAWN ALEATORIO
        // =========================
        Transform spawnPoint =
            spawnPoints[
                Random.Range(
                    0,
                    spawnPoints.Length
                )
            ];

        // =========================
        // ELEGIR PREFAB
        // =========================
        bool spawnFuerte =
            waveActual >= 3 &&
            Random.value < 0.25f;

        GameObject prefab =
            spawnFuerte
            ? zombieFuertePrefab
            : zombieNormalPrefab;

        // =========================
        // SPAWNEAR
        // =========================
        GameObject zombie =
            Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        // =========================
        // ASIGNAR PLAYER
        // =========================
        ZombieAI ai =
            zombie.GetComponent<ZombieAI>();

        if (ai != null)
        {
            ai.player = player;
        }

        enemigosVivos++;

        ActualizarUI();
    }

    public void ZombieMuerto()
    {
        enemigosVivos--;

        if (enemigosVivos < 0)
        {
            enemigosVivos = 0;
        }

        ActualizarUI();

        // =========================
        // NUEVA OLEADA
        // =========================
        if (enemigosVivos <= 0)
        {
            waveActual++;

            StartCoroutine(IniciarOleada());
        }
    }

    void ActualizarUI()
    {
        if (enemiesText != null)
        {
            enemiesText.text =
                "ENEMIGOS: " +
                enemigosVivos;
        }
    }
}