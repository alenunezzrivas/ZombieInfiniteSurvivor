using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator animator;

    private NavMeshAgent agent;

    [Header("Movimiento")]
    public float speed = 2f;
    public float stoppingDistance = 2f;
    public float rotationSpeed = 5f;

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

    private bool muerto = false;

    void Start()
    {
        // Buscar player automáticamente
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        // Obtener NavMeshAgent
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = speed;
            agent.stoppingDistance = stoppingDistance;
            agent.angularSpeed = rotationSpeed * 100f;
            agent.updateRotation = true;
        }
    }

    void Update()
    {
        if (muerto || player == null)
            return;

        float distancia =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // =========================
        // MOVIMIENTO
        // =========================
        if (distancia > stoppingDistance)
        {
            Mover();

            animator.SetFloat("Speed", speed);
        }
        else
        {
            // Parar zombie
            if (agent != null)
            {
                agent.ResetPath();
            }

            animator.SetFloat("Speed", 0f);

            if (canAttack)
            {
                StartCoroutine(Atacar());
            }
        }
    }

    // =========================
    // MOVER
    // =========================
    void Mover()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);
        }
    }

    // =========================
    // ATAQUE
    // =========================
    IEnumerator Atacar()
    {
        canAttack = false;

        // Ataque aleatorio
        int attack = Random.Range(0, 2);

        animator.SetInteger("AttackType", attack);
        animator.SetTrigger("Attack");

        // Esperar golpe
        yield return new WaitForSeconds(0.5f);

        // Comprobar distancia
        if (player != null && !muerto)
        {
            float distancia =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            if (distancia <= stoppingDistance + attackRangeExtra)
            {
                PlayerHealth ph =
                    player.GetComponent<PlayerHealth>();

                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
            }
        }

        // Cooldown
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    // =========================
    // RECIBIR DISPARO
    // =========================
    public void RecibirDisparo(bool esHeadshot)
    {
        if (muerto)
            return;

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
                    animator.SetTrigger("Hit");
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
    void Morir()
    {
        if (muerto)
            return;

        muerto = true;

        StopAllCoroutines();

        // Parar agente
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Limpiar triggers
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Attack");

        // Parar animación movimiento
        animator.SetFloat("Speed", 0f);

        // Desactivar colliders
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        // Animación muerte
        int death = Random.Range(0, 2);

        animator.SetInteger("DeathType", death);
        animator.SetTrigger("Die");

        StartCoroutine(Desaparecer());
    }

    // =========================
    // DESAPARECER
    // =========================
    IEnumerator Desaparecer()
    {
        yield return new WaitForSeconds(tiempoDesaparecer);

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

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
                    if (m.HasProperty("_Color"))
                    {
                        Color c = m.color;
                        c.a = alpha;
                        m.color = c;
                    }
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}