using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Throwing")]
    public float throwForce = 15f;
    public float upwardForce = 5f;

    [Header("Explosion")]
    public float fuseTime = 3f;
    public float explosionRadius = 10f;

    private Rigidbody rb;
    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.AddRelativeForce(Vector3.forward * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sc = gameObject.AddComponent<SphereCollider>();
            sc.radius = 0.4f;
        }
        col.isTrigger = false;

        Destroy(gameObject, fuseTime + 2f);

        StartCoroutine(ExplosionTimer());
    }

    IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(fuseTime);
        Explosion();
    }

    void Explosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        GameObject particles = Resources.Load<GameObject>("Prefabs/GrenadeParticles");

        if (particles != null)
        {
            GameObject fx = Instantiate(particles, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            ZombieAI zombie = hit.GetComponentInParent<ZombieAI>();
            if (zombie != null)
                zombie.Morir();

            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                player.TakeDamage(50f * (1f - dist / explosionRadius));
            }
        }

        Destroy(gameObject);
    }
}
