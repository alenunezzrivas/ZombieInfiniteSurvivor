using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerShoot : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public StarterAssets.StarterAssetsInputs input;
    public ObjectPool objectPool;

    [Header("Disparo")]
    public float range = 100f;
    public float fireRate = 0.15f;
    public int damage = 1;

    [Header("Capas")]
    public LayerMask hitLayers;

    [Header("Efectos")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Audio")]
    public AudioClip gunshotClip;

    [Header("Screen Shake")]
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.1f;

    private float nextFireTime;
    private AudioSource audioSource;
    private CameraShake cameraShake;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        if (gunshotClip == null)
        {
            gunshotClip =
                Resources.Load<AudioClip>(
                    "Audio/Gameplay/desert_eagle_shot"
                );
        }

        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>();
        }

        if (cam != null)
        {
            cameraShake = cam.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = cam.gameObject.AddComponent<CameraShake>();
            }
        }

        if (objectPool == null)
        {
            objectPool = FindFirstObjectByType<ObjectPool>();
        }
    }

    void Update()
    {
        if (Time.time < nextFireTime) return;

        bool shootInput = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            shootInput = true;

        if (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.2f)
            shootInput = true;
#else
        shootInput = Input.GetMouseButton(0);
#endif

        if (shootInput)
        {
            nextFireTime = Time.time + fireRate;
            Disparar();
        }
    }

    void Disparar()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = null;

            if (objectPool != null)
            {
                bullet = objectPool.Get(bulletPrefab);
                bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
            }
            else
            {
                bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            }

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null && objectPool != null)
            {
                b.Init(objectPool, bulletPrefab);
            }
        }

        if (audioSource != null && gunshotClip != null)
        {
            audioSource.PlayOneShot(gunshotClip);
        }

        if (cameraShake != null)
        {
            cameraShake.Shake(shakeIntensity, shakeDuration);
        }

        RaycastHit hit;

        if (Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out hit,
                range,
                hitLayers
            ))
        {
            ZombieAI zombie =
                hit.collider.GetComponentInParent<ZombieAI>();

            if (zombie != null)
            {
                bool headshot =
                    hit.collider.CompareTag("Head");

                zombie.RecibirDisparo(headshot);

                if (Hitmarker.Instance != null)
                {
                    Hitmarker.Instance.Mostrar(headshot);
                }
            }
        }
    }
}