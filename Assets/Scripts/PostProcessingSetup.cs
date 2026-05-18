using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingSetup : MonoBehaviour
{
    public static PostProcessingSetup Instance;

    [Header("Config")]
    public bool activarBloom = true;
    public float bloomIntensity = 0.5f;
    public float bloomThreshold = 1f;

    public bool activarVigneta = true;
    public float vignetaIntensidad = 0.3f;
    public float vignetaSuavizado = 0.5f;

    public bool activarColor = true;
    public float saturacion = 0f;
    public float contraste = 0f;

    [Header("Damage Flash")]
    public float flashIntensity = 0.6f;
    public float flashDuration = 0.3f;

    private Vignette vignette;
    private float vignetteBase;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();

        if (cam != null)
        {
            UniversalAdditionalCameraData camData =
                cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
                camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
        }

        GameObject volumeGO = new GameObject("Global Volume");
        volumeGO.transform.SetParent(transform);

        Volume volume = volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = null;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = profile;

        if (activarBloom)
        {
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(bloomIntensity);
            bloom.threshold.Override(bloomThreshold);
        }

        if (activarVigneta)
        {
            vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(vignetaIntensidad);
            vignette.smoothness.Override(vignetaSuavizado);
            vignetteBase = vignetaIntensidad;
        }

        if (activarColor)
        {
            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(saturacion);
            color.contrast.Override(contraste);
        }
    }

    public void DamageFlash()
    {
        if (vignette == null) return;

        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        vignette.intensity.Override(flashIntensity);

        float t = 0f;

        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float intensity = Mathf.Lerp(flashIntensity, vignetteBase, t / flashDuration);
            vignette.intensity.Override(intensity);
            yield return null;
        }

        vignette.intensity.Override(vignetteBase);
    }
}
