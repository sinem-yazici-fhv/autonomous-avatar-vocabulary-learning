using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubble : MonoBehaviour
{
    public TMP_Text speechText;
    public GameObject speechBubble;
    public float minimumDisplayTime = 8f;
    public float secondsPerCharacter = 0.08f;
    public float typewriterSpeed = 0.04f;

    private string[] messages = {
        "Welcome to the English\nVocabulary Classroom!",
        "I am Mr. Lex,\nyour English teacher!",
        "Look around and click on\nobjects to learn their names!",
        "When you are ready,\ncome to me and press E!",
        "Press E to start!"
    };

    private int currentIndex = 0;
    private float timer = 0f;
    private bool finished = false;
    private bool isTyping = false;

    void Start()
    {
        SetupBubbleStyle();
        speechBubble.SetActive(true);
        StartCoroutine(TypewriterEffect(messages[0]));
        ResizeBubble(messages[0]);
    }

    void SetupBubbleStyle()
    {
        RectTransform backgroundRect = speechText?.transform.parent as RectTransform;
        if (backgroundRect != null)
        {
            // Die Sprechblase bisschen nach unten schieben, damit sie nicht zu weit oben schwebt
            backgroundRect.anchoredPosition += new Vector2(0f, -30f);

            // Hintergrund stylen
            Image backgroundImage = backgroundRect.GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.sprite = CreateSpeechBubbleSprite();
                backgroundImage.type = Image.Type.Sliced;
                backgroundImage.color = Color.white; // We draw the color inside our texture
            }

            // Sicherstellen, dass keine alten Effekte mehr stören
            Shadow shadow = backgroundRect.gameObject.GetComponent<Shadow>();
            if (shadow != null) Destroy(shadow);

            Outline outline = backgroundRect.gameObject.GetComponent<Outline>();
            if (outline != null) Destroy(outline);
        }

        // Text stylen
        if (speechText != null)
        {
            speechText.color = new Color(0.1f, 0.1f, 0.2f, 1f);
            speechText.fontSize = 22f;
        }
    }

    private Sprite CreateSpeechBubbleSprite()
    {
        int width = 128;
        int height = 128;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];
        int radius = 16;
        int tailHeight = 18;
        
        float cx = 64f;
        float cy = 73f;
        float hx = 64f;
        float hy = 55f;
        float r = 16f;

        float mag1 = Mathf.Sqrt(18f*18f + 10f*10f);
        float nx1 = 18f / mag1; float ny1 = 10f / mag1;

        float mag2 = Mathf.Sqrt(18f*18f + 14f*14f);
        float nx2 = -18f / mag2; float ny2 = 14f / mag2;

        Color32 greenOutline = new Color32(100, 140, 220, 255); 
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float px = x; float py = y;

                // Signed Distance Field (SDF) für die rechteckige Box
                float dx = Mathf.Abs(px - cx) - hx + r;
                float dy = Mathf.Abs(py - cy) - hy + r;
                float distBoxOuter = Mathf.Sqrt(Mathf.Max(dx, 0f)*Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f)*Mathf.Max(dy, 0f)) + Mathf.Min(Mathf.Max(dx, dy), 0f);
                float sdfBox = r - distBoxOuter;

                // Signed Distance Field (SDF) für den spitz zulaufenden Schweif
                float sdf1 = (px - 60f) * nx1 + py * ny1;
                float sdf2 = (px - 60f) * nx2 + py * ny2;
                float sdfTail = Mathf.Min(sdf1, sdf2);
                sdfTail = Mathf.Min(sdfTail, py + 0.5f); // Abrunden an der Unterkante
                if (py > 20f) sdfTail = -1000f; // Oberhalb der Box ignorieren

                // Kombinieren der beiden Formen (Union)
                float sdf = Mathf.Max(sdfBox, sdfTail);

                // Anti-Aliasing & Umriss berechnen
                float alpha = Mathf.Clamp01(sdf + 0.5f);
                float blend = Mathf.Clamp01(sdf - 2f); // Der grüne Rand ist ca. 3 Pixel dick

                byte rC = (byte)Mathf.Lerp(greenOutline.r, 255f, blend);
                byte gC = (byte)Mathf.Lerp(greenOutline.g, 255f, blend);
                byte bC = (byte)Mathf.Lerp(greenOutline.b, 255f, blend);
                byte aC = (byte)(alpha * 255f);

                pixels[y * width + x] = new Color32(rC, gC, bC, aC);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius + 2, tailHeight + radius + 2, radius + 2, radius + 2));
    }

    void Update()
    {
        if (finished || isTyping) return;

        timer += Time.deltaTime;
        float currentMessageDuration = Mathf.Max(minimumDisplayTime, messages[currentIndex].Length * secondsPerCharacter);
        if (timer >= currentMessageDuration)
        {
            timer = 0f;
            if (currentIndex < messages.Length - 1)
            {
                currentIndex++;
                ResizeBubble(messages[currentIndex]);
                StartCoroutine(TypewriterEffect(messages[currentIndex]));
            }
            else
            {
                finished = true;
            }
        }
    }

    IEnumerator TypewriterEffect(string message)
    {
        isTyping = true;
        speechText.text = "";
        foreach (char c in message)
        {
            speechText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        isTyping = false;
    }

    void ResizeBubble(string message)
    {
        RectTransform backgroundRect = speechText?.transform.parent as RectTransform;
        RectTransform bubbleRect = backgroundRect?.parent as RectTransform;
        if (backgroundRect == null) return;

        float maxWidth = 340f;
        float minWidth = 220f;
        float minHeight = 72f;

        Vector2 preferred = speechText.GetPreferredValues(message, maxWidth - 42f, 0f);
        float width = Mathf.Clamp(preferred.x + 42f, minWidth, maxWidth);
        
        // +18 für den Bereich des Schweifs, den wir weiter unten durch Margin ignorieren
        float height = Mathf.Max(minHeight, preferred.y + 34f);

        backgroundRect.sizeDelta = new Vector2(width, height);
        if (bubbleRect != null)
            bubbleRect.sizeDelta = new Vector2(width, height);

        // Wir schieben den Text nach oben, damit er nicht im neuen Schweif landet
        if (speechText != null)
        {
            speechText.margin = new Vector4(0f, 0f, 0f, 18f); // (left, top, right, bottom)
        }
    }

    public void StopIntro()
    {
        StopAllCoroutines();
        finished = true;
        isTyping = false;
        enabled = false;
    }
}
