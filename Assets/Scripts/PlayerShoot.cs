using UnityEngine;
using StarterAssets;

public class PlayerShoot : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public ParticleSystem muzzleFlash;
    public GameObject hitEffect;

    private StarterAssetsInputs input;

    [Header("Disparo")]
    public float range = 100f;
    public float fireRate = 0.2f;

    private float nextFireTime = 0f;

    void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (input == null || cam == null) return;

        if (input.shoot && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Disparar();
        }
    }

    void Disparar()
    {
        // 🔫 Muzzle flash
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // 💣 Instanciar bala física (visual)
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }

        // 🎯 Raycast (lógica REAL)
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            // 💥 impacto visual
            if (hitEffect != null)
            {
                GameObject impact = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }

            ZombieAI zombie = hit.collider.GetComponentInParent<ZombieAI>();

            if (zombie != null)
            {
                bool headshot = hit.collider.CompareTag("Head");
                zombie.RecibirDisparo(headshot);
            }
        }
    }
}