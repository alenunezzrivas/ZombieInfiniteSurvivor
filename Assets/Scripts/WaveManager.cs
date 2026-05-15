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

    [Header("Limites")]
    public int maxZombiesEnPartida = 100;

    [Header("Oleadas")]
    public int waveActual = 1;
    public int enemigosBase = 5;

    [Header("Tiempo Entre Oleadas")]
    public float tiempoEntreOleadas = 90f;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;

    private int enemigosVivos = 0;

    private bool oleadaEnCurso = false;

    void Start()
    {
        // =========================
        // BUSCAR PLAYER AUTOMATICAMENTE
        // =========================
        if (player == null)
        {
            GameObject p =
                GameObject.FindGameObjectWithTag("Player");

            if (p != null)
            {
                player = p.transform;
            }
        }

        StartCoroutine(
            LoopOleadas()
        );
    }

    // =========================
    // LOOP PRINCIPAL OLEADAS
    // =========================
    IEnumerator LoopOleadas()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (!oleadaEnCurso)
            {
                StartCoroutine(
                    IniciarOleada()
                );
            }

            yield return new WaitForSeconds(
                tiempoEntreOleadas
            );

            waveActual++;
        }
    }

    // =========================
    // INICIAR OLEADA
    // =========================
    IEnumerator IniciarOleada()
    {
        oleadaEnCurso = true;

        int cantidad =
            enemigosBase +
            (waveActual * 2);

        if (waveText != null)
        {
            waveText.text =
                "OLEADA " +
                waveActual;
        }

        for (int i = 0; i < cantidad; i++)
        {
            // =========================
            // LIMITE MAXIMO ZOMBIES
            // =========================
            if (
                enemigosVivos >=
                maxZombiesEnPartida
            )
            {
                break;
            }

            SpawnZombie();

            yield return new WaitForSeconds(
                tiempoEntreSpawns
            );
        }

        oleadaEnCurso = false;
    }

    // =========================
    // SPAWN ZOMBIE
    // =========================
    void SpawnZombie()
    {
        // =========================
        // COMPROBAR LIMITE
        // =========================
        if (
            enemigosVivos >=
            maxZombiesEnPartida
        )
        {
            return;
        }

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

    // =========================
    // ZOMBIE MUERTO
    // =========================
    public void ZombieMuerto()
    {
        enemigosVivos--;

        if (enemigosVivos < 0)
        {
            enemigosVivos = 0;
        }

        ActualizarUI();
    }

    // =========================
    // UI
    // =========================
    void ActualizarUI()
    {
        if (enemiesText != null)
        {
            enemiesText.text =
                "ENEMIGOS: " +
                enemigosVivos +
                " / " +
                maxZombiesEnPartida;
        }
    }
}