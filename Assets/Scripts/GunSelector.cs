using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GunSelector : MonoBehaviour
{
    public static int SelectedIndex
    {
        get => PlayerPrefs.GetInt("SelectedBlaster", 0);
        set => PlayerPrefs.SetInt("SelectedBlaster", value);
    }

    private MeshFilter gunMeshFilter;
    private Mesh[] blasterMeshes;
    private string[] blasterNames;
    private int currentIndex = 0;
    private TMP_Text nameText;
    private TMP_Text countText;
    private bool inMenu;
    private GameObject selectorPanel;
    private GameObject selectorCount;

    void Start()
    {
        inMenu = SceneManager.GetActiveScene().name == "MenuScene";
        LoadBlasters();
        currentIndex = SelectedIndex;

        CrearUI();
        SetVisible(inMenu);

        if (!inMenu)
        {
            AplicarSkin();
        }
    }

    public void SetVisible(bool visible)
    {
        if (selectorPanel != null)
            selectorPanel.SetActive(visible);
        if (selectorCount != null)
            selectorCount.SetActive(visible);
    }

    void LoadBlasters()
    {
        string[] ids = { "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r" };
        blasterNames = new string[ids.Length];
        blasterMeshes = new Mesh[ids.Length];

        for (int i = 0; i < ids.Length; i++)
        {
            blasterNames[i] = "Blaster " + ids[i].ToUpper();
            GameObject prefab = Resources.Load<GameObject>("Blasters/blaster-" + ids[i]);

            if (prefab != null)
            {
                MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                    blasterMeshes[i] = mf.sharedMesh;
            }
        }
    }

    void AplicarSkin()
    {
        Transform gunParent = transform.parent;

        if (gunParent != null)
        {
            foreach (Transform child in gunParent)
            {
                if (child.name.StartsWith("blaster") || child.name.StartsWith("Blaster"))
                {
                    gunMeshFilter = child.GetComponent<MeshFilter>();
                    break;
                }
            }
        }

        if (gunMeshFilter != null && blasterMeshes[currentIndex] != null)
            gunMeshFilter.sharedMesh = blasterMeshes[currentIndex];
    }

    void CrearUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject panel = new GameObject("GunSelectorPanel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);
        selectorPanel = panel;

        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0f, 0f);
        panelRt.anchorMax = new Vector2(0f, 0f);
        panelRt.pivot = new Vector2(0f, 0f);
        panelRt.anchoredPosition = new Vector2(10f, 10f);
        panelRt.sizeDelta = new Vector2(280f, 40f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);

        GameObject prevBtnObj = new GameObject("PrevBtn", typeof(RectTransform));
        prevBtnObj.transform.SetParent(panel.transform, false);
        RectTransform prevRt = prevBtnObj.GetComponent<RectTransform>();
        prevRt.anchorMin = new Vector2(0f, 0f);
        prevRt.anchorMax = new Vector2(0f, 1f);
        prevRt.sizeDelta = new Vector2(30f, 0f);
        Image prevImg = prevBtnObj.AddComponent<Image>();
        prevImg.color = new Color(0.7f, 0.1f, 0.1f, 0.8f);
        Button prevBtn = prevBtnObj.AddComponent<Button>();
        prevBtn.onClick.AddListener(PrevBlaster);

        GameObject prevTextObj = new GameObject("Text", typeof(RectTransform));
        prevTextObj.transform.SetParent(prevBtnObj.transform, false);
        RectTransform prevTxtRt = prevTextObj.GetComponent<RectTransform>();
        prevTxtRt.anchorMin = Vector2.zero;
        prevTxtRt.anchorMax = Vector2.one;
        prevTxtRt.sizeDelta = Vector2.zero;
        TMP_Text prevTmp = prevTextObj.AddComponent<TextMeshProUGUI>();
        prevTmp.text = "<";
        prevTmp.fontSize = 20;
        prevTmp.alignment = TextAlignmentOptions.Center;
        prevTmp.color = Color.white;

        GameObject nextBtnObj = new GameObject("NextBtn", typeof(RectTransform));
        nextBtnObj.transform.SetParent(panel.transform, false);
        RectTransform nextRt = nextBtnObj.GetComponent<RectTransform>();
        nextRt.anchorMin = new Vector2(1f, 0f);
        nextRt.anchorMax = new Vector2(1f, 1f);
        nextRt.sizeDelta = new Vector2(30f, 0f);
        Image nextImg = nextBtnObj.AddComponent<Image>();
        nextImg.color = new Color(0.7f, 0.1f, 0.1f, 0.8f);
        Button nextBtn = nextBtnObj.AddComponent<Button>();
        nextBtn.onClick.AddListener(NextBlaster);

        GameObject nextTextObj = new GameObject("Text", typeof(RectTransform));
        nextTextObj.transform.SetParent(nextBtnObj.transform, false);
        RectTransform nextTxtRt2 = nextTextObj.GetComponent<RectTransform>();
        nextTxtRt2.anchorMin = Vector2.zero;
        nextTxtRt2.anchorMax = Vector2.one;
        nextTxtRt2.sizeDelta = Vector2.zero;
        TMP_Text nextTmp = nextTextObj.AddComponent<TextMeshProUGUI>();
        nextTmp.text = ">";
        nextTmp.fontSize = 20;
        nextTmp.alignment = TextAlignmentOptions.Center;
        nextTmp.color = Color.white;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(panel.transform, false);
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(1f, 1f);
        labelRt.offsetMin = new Vector2(30f, 0f);
        labelRt.offsetMax = new Vector2(-30f, 0f);

        GameObject labelTextObj = new GameObject("Text", typeof(RectTransform));
        labelTextObj.transform.SetParent(labelObj.transform, false);
        RectTransform labelTxtRt = labelTextObj.GetComponent<RectTransform>();
        labelTxtRt.anchorMin = Vector2.zero;
        labelTxtRt.anchorMax = Vector2.one;
        labelTxtRt.sizeDelta = Vector2.zero;
        nameText = labelTextObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 16;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;

        GameObject countObj = new GameObject("GunCount", typeof(RectTransform));
        countObj.transform.SetParent(canvas.transform, false);
        selectorCount = countObj;
        RectTransform countRt = countObj.GetComponent<RectTransform>();
        countRt.anchorMin = new Vector2(0f, 0f);
        countRt.anchorMax = new Vector2(0f, 0f);
        countRt.pivot = new Vector2(0f, 0f);
        countRt.anchoredPosition = new Vector2(10f, 55f);
        countRt.sizeDelta = new Vector2(280f, 25f);
        Image countBg = countObj.AddComponent<Image>();
        countBg.color = new Color(0f, 0f, 0f, 0.5f);

        GameObject countTextObj = new GameObject("Text", typeof(RectTransform));
        countTextObj.transform.SetParent(countObj.transform, false);
        RectTransform countTxtRt = countTextObj.GetComponent<RectTransform>();
        countTxtRt.anchorMin = Vector2.zero;
        countTxtRt.anchorMax = Vector2.one;
        countTxtRt.sizeDelta = Vector2.zero;
        countText = countTextObj.AddComponent<TextMeshProUGUI>();
        countText.fontSize = 14;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = Color.white;

        UpdateBlasterDisplay();
    }

    public void NextBlaster()
    {
        currentIndex = (currentIndex + 1) % blasterMeshes.Length;
        SelectedIndex = currentIndex;
        UpdateBlasterDisplay();
        if (!inMenu) AplicarSkin();
    }

    public void PrevBlaster()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = blasterMeshes.Length - 1;
        SelectedIndex = currentIndex;
        UpdateBlasterDisplay();
        if (!inMenu) AplicarSkin();
    }

    void UpdateBlasterDisplay()
    {
        if (nameText != null)
            nameText.text = blasterNames[currentIndex];

        if (countText != null)
            countText.text = (currentIndex + 1) + " / " + blasterMeshes.Length;
    }
}
