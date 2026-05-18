using System.Collections;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator animator;

    private Rigidbody rb;
    private WaveManager waveManager;

    // =========================
    // SCORE / HEADSHOT
    // =========================
    private bool ultimoGolpeHeadshot = false;

    [Header("Movimiento")]
    public float speed = 2f;
    public float stoppingDistance = 2f;
    public float rotationSpeed = 5f;

    [Header("Evasion Obstaculos")]
    public float avoidanceForce = 3f;
    public float avoidanceDuration = 1.2f;

    private bool avoidingObstacle = false;
    private Vector3 avoidanceDirection;

    [Header("Ataque")]
    public float attackCooldown = 2f;
    public float damage = 10f;
    public float attackRangeExtra = 0.5f;

    private bool canAttack = true;

    [Header("Vida")]
    public bool esFuerte = false;
    public int vida = 3;

    [Header("Muerte")]
    public float tiempoDesaparecer = 3f;
    public float duracionFade = 1.5f;

    [Header("Audio")]
    public AudioClip[] attackClips;
    public AudioClip[] hurtClips;
    public AudioClip[] deathClips;
    public AudioClip[] headshotClips;

    private bool muerto = false;
    private AudioSource audioSource;
    private ObjectPool pool;
    private GameObject prefabSource;

    void Start()
    {
        // =========================
        // BUSCAR PLAYER
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

        // =========================
        // BUSCAR WAVEMANAGER
        // =========================
        waveManager =
            FindObjectOfType<WaveManager>();

        // =========================
        // COMPONENTES
        // =========================
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        // =========================
        // CONFIGURAR RIGIDBODY
        // =========================
        if (rb != null)
        {
            rb.useGravity = true;

            rb.isKinematic = false;

            rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }

        // =========================
        // CONFIGURAR AUDIO
        // =========================
        audioSource =
            GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (attackClips == null || attackClips.Length == 0)
        {
            attackClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_attack_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_attack_02"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_attack_03"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_attack_04"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_attack_05"
                ),
            };
        }

        if (hurtClips == null || hurtClips.Length == 0)
        {
            hurtClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_hurt_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_hurt_02"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_hurt_03"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_hurt_04"
                ),
            };
        }

        if (deathClips == null || deathClips.Length == 0)
        {
            deathClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_death_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/zombie_death_02"
                ),
            };
        }

        if (headshotClips == null || headshotClips.Length == 0)
        {
            headshotClips = new AudioClip[]
            {
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/headshot_01"
                ),
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/headshot_02"
                ),
            };
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void InitZombie(Transform playerTarget, ObjectPool ownerPool, GameObject prefab)
    {
        player = playerTarget;
        pool = ownerPool;
        prefabSource = prefab;
        ResetZombie();
    }

    void ResetZombie()
    {
        muerto = false;
        ultimoGolpeHeadshot = false;
        canAttack = true;
        avoidingObstacle = false;
        avoidanceDirection = Vector3.zero;

        if (esFuerte)
            vida = 3;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
            c.enabled = true;

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = 1f;
                    m.color = c;
                }
            }
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Attack");
            animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        if (muerto || player == null)
            return;

        float distancia =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // =========================
        // DIRECCION
        // =========================
        Vector3 direccion;

        if (avoidingObstacle)
        {
            direccion =
                avoidanceDirection;
        }
        else
        {
            direccion =
                player.position -
                transform.position;

            direccion.y = 0f;
        }

        // =========================
        // ROTACION
        // =========================
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(direccion);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotacionObjetivo,
                    rotationSpeed *
                    Time.fixedDeltaTime
                );
        }

        // =========================
        // MOVIMIENTO
        // =========================
        if (distancia > stoppingDistance)
        {
            Vector3 movimiento =
                transform.forward * speed;

            movimiento.y =
                rb.linearVelocity.y;

            rb.linearVelocity =
                movimiento;

            if (animator != null)
            {
                animator.SetFloat(
                    "Speed",
                    speed
                );
            }
        }
        else
        {
            rb.linearVelocity =
                new Vector3(
                    0f,
                    rb.linearVelocity.y,
                    0f
                );

            if (animator != null)
            {
                animator.SetFloat(
                    "Speed",
                    0f
                );
            }

            if (canAttack)
            {
                StartCoroutine(
                    Atacar()
                );
            }
        }
    }

    // =========================
    // DETECTAR OBSTACULOS
    // =========================
    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (muerto)
            return;

        // IGNORAR PLAYER Y ZOMBIES
        if (
            collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("Zombie")
        )
        {
            return;
        }

        // =========================
        // CALCULAR DIRECCION
        // =========================
        Vector3 direccionAleatoria =
            Random.value > 0.5f
            ? transform.right
            : -transform.right;

        avoidanceDirection =
            direccionAleatoria *
            avoidanceForce;

        StartCoroutine(
            EvitarObstaculo()
        );
    }

    IEnumerator EvitarObstaculo()
    {
        avoidingObstacle = true;

        yield return new WaitForSeconds(
            avoidanceDuration
        );

        avoidingObstacle = false;
    }

    // =========================
    // ATAQUE
    // =========================
    IEnumerator Atacar()
    {
        canAttack = false;

        int attack =
            Random.Range(0, 2);

        if (animator != null)
        {
            animator.SetInteger(
                "AttackType",
                attack
            );

            animator.SetTrigger(
                "Attack"
            );
        }

        if (
            audioSource != null &&
            attackClips != null &&
            attackClips.Length > 0
        )
        {
            AudioClip clip =
                attackClips[
                    Random.Range(
                        0,
                        attackClips.Length
                    )
                ];

            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        yield return new WaitForSeconds(
            0.5f
        );

        if (player != null && !muerto)
        {
            float distancia =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            if (
                distancia <=
                stoppingDistance +
                attackRangeExtra
            )
            {
                PlayerHealth ph =
                    player.GetComponent<PlayerHealth>();

                if (ph != null)
                {
                    ph.TakeDamage(
                        damage
                    );
                }
            }
        }

        yield return new WaitForSeconds(
            attackCooldown
        );

        canAttack = true;
    }

    // =========================
    // RECIBIR DISPARO
    // =========================
    public void RecibirDisparo(
        bool esHeadshot
    )
    {
        if (muerto)
            return;

        // =========================
        // GUARDAR HEADSHOT
        // =========================
        ultimoGolpeHeadshot =
            esHeadshot;

        if (esFuerte)
        {
            if (esHeadshot)
            {
                Morir();
            }
            else
            {
                vida--;

                if (vida <= 0)
                {
                    Morir();
                }
                else
                {
                    if (animator != null)
                    {
                        animator.SetTrigger(
                            "Hit"
                        );
                    }
                }
            }
        }
        else
        {
            Morir();
        }
    }

    // =========================
    // MORIR
    // =========================
    public void Morir()
    {
        if (muerto)
            return;

        muerto = true;

        StopAllCoroutines();

        // =========================
        // MUERTE FISICA
        // =========================
        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.constraints =
                RigidbodyConstraints.None;

            rb.AddForce(
                -transform.forward * 4f +
                Vector3.up * 2f,
                ForceMode.Impulse
            );

            rb.AddTorque(
                transform.right * 6f,
                ForceMode.Impulse
            );
        }

        if (animator != null)
        {
            animator.ResetTrigger(
                "Hit"
            );

            animator.ResetTrigger(
                "Attack"
            );

            animator.SetFloat(
                "Speed",
                0f
            );

            animator.enabled = false;
        }

        // =========================
        // AVISAR AL WAVEMANAGER
        // =========================
        if (waveManager != null)
        {
            waveManager.ZombieMuerto();
        }

        // =========================
        // AVISAR AL GAMEMANAGER
        // =========================
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ZombieKilled(
                ultimoGolpeHeadshot
            );
        }

        StartCoroutine(
            Desaparecer()
        );
    }

    // =========================
    // DESAPARECER
    // =========================
    IEnumerator Desaparecer()
    {
        yield return new WaitForSeconds(
            tiempoDesaparecer
        );

        // =========================
        // DESACTIVAR COLISIONES
        // =========================
        Collider[] colliders =
            GetComponentsInChildren<
                Collider>();

        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        // =========================
        // CONGELAR RIGIDBODY
        // =========================
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Renderer[] renderers =
            GetComponentsInChildren<
                Renderer>();

        float t = 0f;

        while (t < duracionFade)
        {
            t += Time.deltaTime;

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    t / duracionFade
                );

            foreach (Renderer r in renderers)
            {
                foreach (Material m in r.materials)
                {
                    if (
                        m.HasProperty("_Color")
                    )
                    {
                        Color c =
                            m.color;

                        c.a = alpha;

                        m.color = c;
                    }
                }
            }

            yield return null;
        }

        if (pool != null && prefabSource != null)
        {
            pool.ReturnToPool(prefabSource, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}