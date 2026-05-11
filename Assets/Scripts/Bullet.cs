using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 80f;
    public float lifeTime = 3f;

    [Header("Impacto")]
    public GameObject impactParticles;
    public GameObject bulletHolePrefab;

    private Rigidbody rb;

    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode =
                CollisionDetectionMode.Continuous;

            rb.linearVelocity = transform.forward * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // EVITAR MULTIPLES IMPACTOS
        if (hasHit) return;

        hasHit = true;

        Debug.Log("Impacta en: " + collision.collider.name);

        ContactPoint contact = collision.contacts[0];

        // =========================
        // DETECTAR ZOMBIE
        // =========================
        ZombieAI zombie =
            collision.collider.GetComponentInParent<ZombieAI>();

        // =========================
        // SI IMPACTA EN ZOMBIE
        // =========================
        if (zombie != null)
        {
            // PARTICULAS SOLO EN ZOMBIES
            if (impactParticles != null)
            {
                GameObject impact = Instantiate(
                    impactParticles,
                    contact.point,
                    Quaternion.LookRotation(contact.normal)
                );

                Destroy(impact, 2f);
            }

            bool headshot =
                collision.collider.CompareTag("Head");

            zombie.RecibirDisparo(headshot);
        }
        else
        {
            // =========================
            // AGUJERO DE BALA
            // =========================
            if (bulletHolePrefab != null)
            {
                Quaternion rot =
                    Quaternion.LookRotation(-contact.normal);

                GameObject hole = Instantiate(
                    bulletHolePrefab,
                    contact.point + contact.normal * 0.002f,
                    rot
                );

                hole.transform.SetParent(collision.transform);

                Destroy(hole, 15f);
            }
        }

        // =========================
        // DESACTIVAR COLISIONES
        // =========================
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // =========================
        // DESTRUIR BALA
        // =========================
        Destroy(gameObject);
    }
}