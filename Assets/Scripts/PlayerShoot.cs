using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerShoot : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public StarterAssets.StarterAssetsInputs input;

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
    }

    void Update()
    {
        bool shootInput = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            shootInput = true;

        if (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.2f)
            shootInput = true;
#else
        shootInput = Input.GetMouseButton(0);
#endif

        if (shootInput && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Disparar();
        }
    }

    void Disparar()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );
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
            Debug.Log("Impacto en: " + hit.collider.name);

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