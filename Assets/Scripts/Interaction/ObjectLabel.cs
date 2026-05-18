using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectLabel : MonoBehaviour
{
    public string englishWord;
    [Header("Anpassungen (Farbe & Position)")]
    public float yOffset = 0.2f; // Abstand über dem Objekt nach oben
    public float zOffset = 0.5f; // Zieht das Label in Richtung Kamera (hilft bei Objekten in der Wand)
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f); // Dunkles, leicht transparentes Anthrazit
    public Color textColor = new Color(0.95f, 0.95f, 0.95f, 1f); // Helles Beige/Weiß

    private GameObject labelObject;
    private TMP_Text labelText;
    private bool isVisible = false;
    private bool exploreMode = true;
    private string defaultText;

    void Start()
    {
        InitializeIfNeeded();
        // Nur verstecken, wenn es nicht bereits explizit sichtbar gemacht wurde
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
        // Canvas erstellen
        labelObject = new GameObject("Label", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        
        // Initiale Position (wird in Update überschrieben)
        labelObject.transform.position = transform.position;
        labelObject.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        Canvas canvas = labelObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Hintergrund
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(labelObject.transform, false);

        Image bgImage = bg.GetComponent<Image>();
        bgImage.color = backgroundColor;

        // Text
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

        // Textgröße berechnen und Background anpassen
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
            // Immer zur Kamera schauen
            labelObject.transform.LookAt(Camera.main.transform);
            labelObject.transform.Rotate(0, 180f, 0);

            // Dynamische Höhe ermitteln basierend auf der Objekt-Größe (Collider)
            float topY = transform.position.y;
            Vector3 centerPos = transform.position;
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                topY = col.bounds.max.y; // Höchster Punkt der Hitbox (Collider)
                centerPos = col.bounds.center; // Mitte der Hitbox nutzen (nicht den Pivot, der könnte in der Wand sein!)
            }
            else
            {
                topY += 1.5f; // Fallback, falls das Objekt keinen Collider hat
            }

            // Position über dem Objekt berechnen (topY + yOffset)
            // Wir nutzen centerPos.x und centerPos.z, damit es wirklich zentriert ist
            Vector3 basePos = new Vector3(centerPos.x, topY + yOffset, centerPos.z);
            
            // Etwas in Richtung Kamera ziehen, damit nichts in der Wand verschwindet
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
