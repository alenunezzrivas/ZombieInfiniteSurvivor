using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private Vector3 waveOriginalScale;

    [Header("Referencias")]
    public Transform player;
    public ObjectPool objectPool;

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

        if (objectPool == null)
        {
            objectPool = FindFirstObjectByType<ObjectPool>();
        }

        if (waveText != null)
            waveOriginalScale = waveText.transform.localScale;

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

        StartCoroutine(AnunciarOleada());

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
        GameObject zombie;

        if (objectPool != null)
        {
            zombie = objectPool.Get(prefab);
            zombie.transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );
        }
        else
        {
            zombie =
                Instantiate(
                    prefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );
        }

        // =========================
        // ASIGNAR PLAYER + POOL
        // =========================
        ZombieAI ai =
            zombie.GetComponent<ZombieAI>();

        if (ai != null)
        {
            ai.InitZombie(player, objectPool, prefab);
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
    // ANUNCIAR OLEADA ANIMADO
    // =========================
    IEnumerator AnunciarOleada()
    {
        if (waveText == null) yield break;

        waveText.text = "WAVE " + waveActual;

        waveText.transform.localScale = waveOriginalScale * 1.5f;

        Color c = waveText.color;
        c.a = 1f;
        waveText.color = c;

        float t = 0f;

        while (t < 0.3f)
        {
            t += Time.deltaTime;
            waveText.transform.localScale = Vector3.Lerp(
                waveOriginalScale * 1.5f,
                waveOriginalScale,
                t / 0.3f
            );
            yield return null;
        }

        waveText.transform.localScale = waveOriginalScale;

        yield return new WaitForSeconds(2.5f);

        t = 0f;

        while (t < 0.5f)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / 0.5f);
            waveText.color = c;
            yield return null;
        }

        c.a = 0f;
        waveText.color = c;
    }

    // =========================
    // UI
    // =========================
    void ActualizarUI()
    {
        if (enemiesText != null)
        {
            enemiesText.text =
                "ENEMIES: " +
                enemigosVivos;
        }
    }
}