using System.Collections;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator animator;

    private Rigidbody rb;

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
        // ROTACION
        // =========================
        Vector3 direccion =
            player.position - transform.position;

        direccion.y = 0f;

        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(direccion);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotacionObjetivo,
                    rotationSpeed * Time.fixedDeltaTime
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

            rb.linearVelocity = movimiento;

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
                StartCoroutine(Atacar());
            }
        }
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

            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(0.5f);

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
                    ph.TakeDamage(damage);
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
                    if (animator != null)
                    {
                        animator.SetTrigger("Hit");
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
    void Morir()
    {
        if (muerto)
            return;

        muerto = true;

        StopAllCoroutines();

        rb.linearVelocity = Vector3.zero;

        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Attack");

            animator.SetFloat(
                "Speed",
                0f
            );

            int death =
                Random.Range(0, 2);

            animator.SetInteger(
                "DeathType",
                death
            );

            animator.SetTrigger("Die");
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        StartCoroutine(Desaparecer());
    }

    // =========================
    // DESAPARECER
    // =========================
    IEnumerator Desaparecer()
    {
        yield return new WaitForSeconds(
            tiempoDesaparecer
        );

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