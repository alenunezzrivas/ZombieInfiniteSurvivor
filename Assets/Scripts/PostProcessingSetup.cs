using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingSetup : MonoBehaviour
{
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

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
        }

        if (cam != null)
        {
            UniversalAdditionalCameraData camData =
                cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
            {
                camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
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
            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(vignetaIntensidad);
            vignette.smoothness.Override(vignetaSuavizado);
        }

        if (activarColor)
        {
            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(saturacion);
            color.contrast.Override(contraste);
        }
    }
}
