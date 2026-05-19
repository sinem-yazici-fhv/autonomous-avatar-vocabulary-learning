using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectLabel : MonoBehaviour
{
    public string englishWord;
    [Header("Anpassungen (Farbe & Position)")]
    public float yOffset = 0.2f;
    public float zOffset = 0.5f;
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    public Color textColor = new Color(0.95f, 0.95f, 0.95f, 1f);

    private GameObject labelObject;
    private TMP_Text labelText;
    private bool isVisible = false;
    private bool exploreMode = true;
    private string defaultText;

    void Start()
    {
        InitializeIfNeeded();
        if (!isVisible && labelObject != null)
        {
            labelObject.SetActive(false);
        }
    }

    public void InitializeIfNeeded()
    {
        if (labelObject == null)
        {
            BuildLabel();
        }
    }

    void BuildLabel()
    {
        labelObject = new GameObject("Label", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        
        labelObject.transform.position = transform.position;
        labelObject.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        Canvas canvas = labelObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(labelObject.transform, false);

        Image bgImage = bg.GetComponent<Image>();
        bgImage.color = backgroundColor;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(bg.transform, false);

        labelText = textObj.GetComponent<TextMeshProUGUI>();
        labelText.text = englishWord;
        defaultText = englishWord;
        labelText.fontSize = 22f;
        labelText.color = textColor;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableWordWrapping = false;

        Vector2 textSize = labelText.GetPreferredValues(englishWord);
        float width = textSize.x + 40f;
        float height = textSize.y + 30f;

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(width, height);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 5f);
        textRect.offsetMax = new Vector2(-10f, -5f);
    }

    void Update()
    {
        if (labelObject != null && labelObject.activeSelf)
        {
            labelObject.transform.LookAt(Camera.main.transform);
            labelObject.transform.Rotate(0, 180f, 0);

            float topY = transform.position.y;
            Vector3 centerPos = transform.position;
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                topY = col.bounds.max.y;
                centerPos = col.bounds.center;
            }
            else
            {
                topY += 1.5f;
            }

            Vector3 basePos = new Vector3(centerPos.x, topY + yOffset, centerPos.z);
            
            Vector3 dirToCamera = (Camera.main.transform.position - basePos).normalized;
            labelObject.transform.position = basePos + dirToCamera * zOffset; 
        }
    }

    void OnMouseDown()
    {
        if (!exploreMode) return;

        isVisible = !isVisible;
        labelObject.SetActive(isVisible);
    }

    public void SetExploreMode(bool active)
    {
        exploreMode = active;
        if (!active && labelObject != null)
            labelObject.SetActive(false);
    }

    public void ShowLabel()
    {
        isVisible = true;
        InitializeIfNeeded();
        if (labelObject != null)
            labelObject.SetActive(true);
    }

    public void HideLabel()
    {
        isVisible = false;
        if (labelObject != null)
            labelObject.SetActive(false);
    }

    public void SetLabelText(string text)
    {
        InitializeIfNeeded();
        if (labelText == null) return;

        labelText.text = text;
        Vector2 textSize = labelText.GetPreferredValues(text);
        float width = textSize.x + 40f;
        float height = textSize.y + 30f;

        RectTransform bgRect = labelText.transform.parent.GetComponent<RectTransform>();
        if (bgRect != null)
            bgRect.sizeDelta = new Vector2(width, height);
    }

    public void ResetLabelText()
    {
        InitializeIfNeeded();
        SetLabelText(defaultText);
    }
}
