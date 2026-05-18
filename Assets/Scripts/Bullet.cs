using System.Collections;
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
    private Collider col;
    private bool hasHit = false;
    private ObjectPool pool;
    private GameObject prefabSource;
    private Coroutine lifeTimer;

    public void Init(ObjectPool ownerPool, GameObject prefab)
    {
        pool = ownerPool;
        prefabSource = prefab;
    }

    void OnEnable()
    {
        hasHit = false;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearVelocity = transform.forward * speed;
        }

        col = GetComponent<Collider>();

        if (col != null)
            col.enabled = true;

        lifeTimer = StartCoroutine(DevolverTrasTiempo());
    }

    void OnDisable()
    {
        if (lifeTimer != null)
        {
            StopCoroutine(lifeTimer);
            lifeTimer = null;
        }
    }

    IEnumerator DevolverTrasTiempo()
    {
        yield return new WaitForSeconds(lifeTime);

        Devolver();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        hasHit = true;

        if (lifeTimer != null)
        {
            StopCoroutine(lifeTimer);
            lifeTimer = null;
        }

        ContactPoint contact = collision.contacts[0];

        ZombieAI zombie =
            collision.collider.GetComponentInParent<ZombieAI>();

        if (zombie != null)
        {
            if (impactParticles != null)
            {
                GameObject impact = Instantiate(
                    impactParticles,
                    contact.point,
                    Quaternion.LookRotation(contact.normal)
                );

                Destroy(impact, 2f);
            }
        }
        else if (bulletHolePrefab != null)
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

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Devolver();
    }

    void Devolver()
    {
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