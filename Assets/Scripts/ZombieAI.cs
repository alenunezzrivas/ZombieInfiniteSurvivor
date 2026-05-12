using System.Collections;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator animator;

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

    void Update()
    {
        if (muerto || player == null) return;

        float distancia =
            Vector3.Distance(
                transform.position,
                player.position
            );

        RotarHaciaPlayer();

        if (distancia > stoppingDistance)
        {
            Mover();
        }
        else
        {
            animator.SetFloat("speed", 0f);

            if (canAttack)
            {
                StartCoroutine(Atacar());
            }
        }
    }

    void RotarHaciaPlayer()
    {
        Vector3 dir =
            (player.position - transform.position)
            .normalized;

        dir.y = 0;

        Quaternion rot =
            Quaternion.LookRotation(dir);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rot,
                rotationSpeed * Time.deltaTime
            );
    }

    void Mover()
    {
        transform.position +=
            transform.forward *
            speed *
            Time.deltaTime;

        animator.SetFloat("Speed", speed);
    }

    IEnumerator Atacar()
    {
        canAttack = false;

        // ATAQUE ALEATORIO
        int attack = Random.Range(0, 2);

        animator.SetInteger("AttackType", attack);
        animator.SetTrigger("Attack");

        // ESPERAR AL GOLPE
        yield return new WaitForSeconds(0.5f);

        // COMPROBAR DISTANCIA
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

        // COOLDOWN
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    // =========================
    // RECIBIR DISPARO
    // =========================
    public void RecibirDisparo(bool esHeadshot)
    {
        if (muerto) return;

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
        if (muerto) return;

        muerto = true;

        StopAllCoroutines();

        // LIMPIAR TRIGGERS
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Attack");

        // PARAR MOVIMIENTO
        animator.SetFloat("speed", 0f);

        // DESACTIVAR COLLIDER
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        // ANIMACION MUERTE
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