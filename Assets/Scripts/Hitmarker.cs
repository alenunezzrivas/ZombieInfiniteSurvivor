using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Hitmarker : MonoBehaviour
{
    public static Hitmarker Instance;

    [Header("Apariencia")]
    public float duracion = 0.15f;
    public float tamanoBase = 24f;
    public float tamanoPop = 36f;
    public float grosor = 3f;
    public float separacion = 6f;

    [Header("Headshot")]
    public float duracionHeadshot = 0.3f;
    public Color colorNormal = Color.white;
    public Color colorHeadshot = Color.red;

    private RectTransform hitmarkerRect;
    private Image hitmarkerImage;

    private void Awake()
    {
        Instance = this;
        CrearUI();
    }

    void CrearUI()
    {
        GameObject canvasGO = new GameObject("Hitmarker Canvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject container = new GameObject("Hitmarker Container");
        container.transform.SetParent(canvasGO.transform, false);

        hitmarkerRect = container.AddComponent<RectTransform>();
        hitmarkerRect.anchorMin = new Vector2(0.5f, 0.5f);
        hitmarkerRect.anchorMax = new Vector2(0.5f, 0.5f);
        hitmarkerRect.pivot = new Vector2(0.5f, 0.5f);
        hitmarkerRect.sizeDelta = new Vector2(tamanoBase, tamanoBase);

        hitmarkerImage = container.AddComponent<Image>();
        hitmarkerImage.color = new Color(colorNormal.r, colorNormal.g, colorNormal.b, 0f);
        hitmarkerImage.raycastTarget = false;

        CrearBrazos(hitmarkerRect);
    }

    void CrearBrazos(RectTransform parent)
    {
        float half = tamanoBase * 0.5f;
        float halfSep = separacion * 0.5f;
        float halfGrosor = grosor * 0.5f;

        CrearBrazo(parent, "Brazo Arriba", new Vector2(0, halfSep + halfGrosor), new Vector2(tamanoBase, grosor));
        CrearBrazo(parent, "Brazo Abajo", new Vector2(0, -halfSep - halfGrosor), new Vector2(tamanoBase, grosor));
        CrearBrazo(parent, "Brazo Izquierda", new Vector2(-halfSep - halfGrosor, 0), new Vector2(grosor, tamanoBase));
        CrearBrazo(parent, "Brazo Derecha", new Vector2(halfSep + halfGrosor, 0), new Vector2(grosor, tamanoBase));
    }

    void CrearBrazo(RectTransform parent, string nombre, Vector2 pos, Vector2 tam)
    {
        GameObject brazo = new GameObject(nombre);
        brazo.transform.SetParent(parent, false);

        RectTransform rt = brazo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;

        Image img = brazo.AddComponent<Image>();
        img.color = new Color(colorNormal.r, colorNormal.g, colorNormal.b, 0f);
        img.raycastTarget = false;
    }

    public void Mostrar(bool headshot)
    {
        if (hitmarkerImage == null)
            return;

        StopAllCoroutines();
        StartCoroutine(Animacion(headshot));
    }

    IEnumerator Animacion(bool headshot)
    {
        float tiempo = headshot ? duracionHeadshot : duracion;
        Color targetColor = headshot ? colorHeadshot : colorNormal;
        float targetSize = tamanoPop;

        float t = 0f;

        while (t < tiempo * 0.4f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / (tiempo * 0.4f);

            Color c = Color.Lerp(
                new Color(targetColor.r, targetColor.g, targetColor.b, 0f),
                targetColor,
                p
            );

            hitmarkerImage.color = c;
            foreach (Image img in hitmarkerRect.GetComponentsInChildren<Image>())
            {
                if (img != hitmarkerImage)
                    img.color = c;
            }

            hitmarkerRect.sizeDelta = Vector2.Lerp(
                new Vector2(tamanoBase, tamanoBase),
                new Vector2(targetSize, targetSize),
                p
            );

            yield return null;
        }

        yield return new WaitForSecondsRealtime(tiempo * 0.2f);

        t = 0f;

        while (t < tiempo * 0.4f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / (tiempo * 0.4f);

            Color c = Color.Lerp(
                targetColor,
                new Color(targetColor.r, targetColor.g, targetColor.b, 0f),
                p
            );

            hitmarkerImage.color = c;
            foreach (Image img in hitmarkerRect.GetComponentsInChildren<Image>())
            {
                if (img != hitmarkerImage)
                    img.color = c;
            }

            hitmarkerRect.sizeDelta = Vector2.Lerp(
                new Vector2(targetSize, targetSize),
                new Vector2(tamanoBase, tamanoBase),
                p
            );

            yield return null;
        }

        hitmarkerImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        foreach (Image img in hitmarkerRect.GetComponentsInChildren<Image>())
        {
            if (img != hitmarkerImage)
                img.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        }

        hitmarkerRect.sizeDelta = new Vector2(tamanoBase, tamanoBase);
    }
}
