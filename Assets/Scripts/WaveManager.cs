using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    public GameObject zombieNormalPrefab;
    public GameObject zombieFuertePrefab;

    [Header("Spawn")]
    public float spawnRadius = 40f;
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
        StartCoroutine(IniciarOleada());
    }

    IEnumerator IniciarOleada()
    {
        yield return new WaitForSeconds(2f);

        int cantidad =
            enemigosBase + (waveActual * 2);

        waveText.text =
            "OLEADA " + waveActual;

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
        Vector2 randomCircle =
            Random.insideUnitCircle.normalized *
            spawnRadius;

        Vector3 spawnPos =
            player.position +
            new Vector3(
                randomCircle.x,
                0,
                randomCircle.y
            );

        GameObject prefab;

        // ENEMIGOS FUERTES
        bool spawnFuerte =
            waveActual >= 3 &&
            Random.value < 0.25f;

        prefab =
            spawnFuerte
            ? zombieFuertePrefab
            : zombieNormalPrefab;

        GameObject zombie =
            Instantiate(
                prefab,
                spawnPos,
                Quaternion.identity
            );

        ZombieAI ai =
            zombie.GetComponent<ZombieAI>();

        ai.player = player;

        enemigosVivos++;

        ActualizarUI();
    }

    public void ZombieMuerto()
    {
        enemigosVivos--;

        ActualizarUI();

        if (enemigosVivos <= 0)
        {
            waveActual++;

            StartCoroutine(IniciarOleada());
        }
    }

    void ActualizarUI()
    {
        enemiesText.text =
            "ENEMIGOS: " + enemigosVivos;
    }
}