using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float shakeAmount = 0f;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeAmount > 0f)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shakeAmount = Mathf.MoveTowards(shakeAmount, 0f, Time.unscaledDeltaTime * 5f);
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float intensity, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        shakeAmount = intensity;
        yield return new WaitForSecondsRealtime(duration);
        shakeAmount = 0f;
        transform.localPosition = originalPos;
    }
}
