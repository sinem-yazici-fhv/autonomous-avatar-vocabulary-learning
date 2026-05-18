using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCOverlay : MonoBehaviour
{
    public GameObject overlayPanel;
    public Camera npcCamera;
    public NPCInteraction npcInteraction;
    public TMP_Text overlaySpeechText;
    public float typewriterSpeed = 0.03f;

    Button helpButton;
    TMP_Text helpButtonText;
    Image speechBubbleImage;
    RectTransform speechBubbleRect;
    RectTransform speechTextRect;
    TMP_Text overlayProgressText;
    RectTransform progressTextRect;
    const string DefaultHelpLabel = "Need help?\nAsk Mr. Lex";

    void Start()
    {
        overlayPanel.SetActive(false);
        npcCamera.gameObject.SetActive(false);

        if (overlayPanel != null)
        {
            helpButton = overlayPanel.GetComponentInChildren<Button>(true);
            if (helpButton != null)
                helpButtonText = helpButton.GetComponentInChildren<TMP_Text>(true);

            BuildOverlaySpeechBubble();
            StyleOverlay();
        }
    }

    public void PlayerLeftRoom()
    {
        overlayPanel.SetActive(true);
        npcCamera.gameObject.SetActive(true);
        SetHelpButtonVisible(npcInteraction != null && npcInteraction.IsTask1HintAvailable);
        SyncOverlayWithRoomSpeech();
    }

    public void PlayerEnteredRoom()
    {
        ResetOverlayText();
        overlayPanel.SetActive(false);
        npcCamera.gameObject.SetActive(false);
    }

    public void OnNPCClicked()
    {
        if (overlaySpeechText == null || npcInteraction == null) return;
        if (!npcInteraction.IsTask1HintAvailable) return;
        string hint = npcInteraction.GetCurrentHint();
        SetOverlayMessage("Hint: " + hint);
    }

    void BuildOverlaySpeechBubble()
    {
        if (overlayPanel == null || overlaySpeechText != null) return;

        GameObject bubble = new GameObject("OverlaySpeechBubble", typeof(RectTransform), typeof(Image));
        bubble.transform.SetParent(overlayPanel.transform, false);

        speechBubbleRect = bubble.GetComponent<RectTransform>();
        speechBubbleRect.anchorMin = new Vector2(0.5f, 1f);
        speechBubbleRect.anchorMax = new Vector2(0.5f, 1f);
        speechBubbleRect.pivot = new Vector2(0f, 0f);
        speechBubbleRect.anchoredPosition = new Vector2(-60f, 15f);
        speechBubbleRect.sizeDelta = new Vector2(280f, 95f);

        speechBubbleImage = bubble.GetComponent<Image>();
        speechBubbleImage.sprite = CreateSpeechBubbleSprite();
        speechBubbleImage.type = Image.Type.Sliced;
        speechBubbleImage.color = Color.white;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(bubble.transform, false);

        speechTextRect = textObject.GetComponent<RectTransform>();
        speechTextRect.anchorMin = new Vector2(0f, 0f);
        speechTextRect.anchorMax = new Vector2(1f, 1f);
        speechTextRect.offsetMin = new Vector2(20f, 26f);
        speechTextRect.offsetMax = new Vector2(-20f, -12f);

        overlaySpeechText = textObject.GetComponent<TextMeshProUGUI>();
        overlaySpeechText.text = "";
        overlaySpeechText.fontSize = 12f;
        overlaySpeechText.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        overlaySpeechText.alignment = TextAlignmentOptions.MidlineLeft;
        overlaySpeechText.enableWordWrapping = true;

        GameObject progressObj = new GameObject("OverlayProgressText", typeof(RectTransform), typeof(TextMeshProUGUI));
        progressObj.transform.SetParent(bubble.transform, false);
        
        progressTextRect = progressObj.GetComponent<RectTransform>();
        progressTextRect.anchorMin = new Vector2(0.5f, 1f);
        progressTextRect.anchorMax = new Vector2(0.5f, 1f);
        progressTextRect.pivot = new Vector2(0.5f, 1f);
        progressTextRect.anchoredPosition = new Vector2(0f, -8f);
        progressTextRect.sizeDelta = new Vector2(250f, 20f);
        
        overlayProgressText = progressObj.GetComponent<TextMeshProUGUI>();
        overlayProgressText.text = "";
        overlayProgressText.fontSize = 12f;
        overlayProgressText.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        overlayProgressText.alignment = TextAlignmentOptions.Center;
        overlayProgressText.gameObject.SetActive(false);

        bubble.SetActive(false);
    }

    private Sprite CreateSpeechBubbleSprite()
    {
        int width = 128;
        int height = 128;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];
        int radius = 16;
        int tailHeight = 18;

        float cx = 64f, cy = 73f, hx = 64f, hy = 55f, r = 16f;

        float mag1 = Mathf.Sqrt(18f * 18f + 10f * 10f);
        float nx1 = 18f / mag1; float ny1 = 10f / mag1;
        float mag2 = Mathf.Sqrt(18f * 18f + 14f * 14f);
        float nx2 = -18f / mag2; float ny2 = 14f / mag2;

        Color32 blueOutline = new Color32(100, 140, 220, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float px = x; float py = y;
                float dx = Mathf.Abs(px - cx) - hx + r;
                float dy = Mathf.Abs(py - cy) - hy + r;
                float distBoxOuter = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f)) + Mathf.Min(Mathf.Max(dx, dy), 0f);
                float sdfBox = r - distBoxOuter;

                float sdf1 = (px - 60f) * nx1 + py * ny1;
                float sdf2 = (px - 60f) * nx2 + py * ny2;
                float sdfTail = Mathf.Min(sdf1, sdf2);
                sdfTail = Mathf.Min(sdfTail, py + 0.5f);
                if (py > 20f) sdfTail = -1000f;

                float sdf = Mathf.Max(sdfBox, sdfTail);
                float alpha = Mathf.Clamp01(sdf + 0.5f);
                float blend = Mathf.Clamp01(sdf - 2f);

                byte rC = (byte)Mathf.Lerp(blueOutline.r, 255f, blend);
                byte gC = (byte)Mathf.Lerp(blueOutline.g, 255f, blend);
                byte bC = (byte)Mathf.Lerp(blueOutline.b, 255f, blend);
                byte aC = (byte)(alpha * 255f);

                pixels[y * width + x] = new Color32(rC, gC, bC, aC);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(80f, tailHeight + radius + 2, radius + 2, radius + 2));
    }

    void StyleOverlay()
    {
        RawImage portrait = overlayPanel != null ? overlayPanel.GetComponent<RawImage>() : null;
        if (portrait != null)
        {
            portrait.raycastTarget = false;
            portrait.color = Color.white;
        }

        RectTransform panelRect = overlayPanel != null ? overlayPanel.GetComponent<RectTransform>() : null;
        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(230f, 230f);

        if (helpButton == null) return;

        RectTransform buttonRect = helpButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 62f);
        buttonRect.sizeDelta = new Vector2(190f, 34f);

        Image buttonImage = helpButton.GetComponent<Image>();
        if (buttonImage != null)
            buttonImage.color = new Color(0.09f, 0.12f, 0.16f, 0.72f);

        ColorBlock colors = helpButton.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.92f, 0.95f, 1f, 1f);
        helpButton.colors = colors;

        if (helpButtonText != null)
        {
            helpButtonText.text = DefaultHelpLabel;
            helpButtonText.fontSize = 15f;
            helpButtonText.alignment = TextAlignmentOptions.Center;
            helpButtonText.color = Color.white;
        }

        SetHelpButtonVisible(false);
    }

    void ResetOverlayText()
    {
        if (overlaySpeechText != null && overlaySpeechText.transform.parent != null)
        {
            overlaySpeechText.text = "";
            if (overlayProgressText != null) overlayProgressText.text = "";
            overlaySpeechText.transform.parent.gameObject.SetActive(false);
        }

        if (helpButtonText != null)
            helpButtonText.text = DefaultHelpLabel;
    }

    void SyncOverlayWithRoomSpeech()
    {
        if (npcInteraction == null || npcInteraction.speechText == null)
        {
            ResetOverlayText();
            return;
        }

        string message = npcInteraction.speechText.text;
        if (string.IsNullOrWhiteSpace(message))
        {
            ResetOverlayText();
            return;
        }

        SetOverlayMessage(message);
    }

    public void SetOverlayProgress(string progressHtml)
    {
        if (overlayProgressText == null) return;
        overlayProgressText.text = progressHtml;
        
        bool hasProgress = !string.IsNullOrEmpty(progressHtml);
        overlayProgressText.gameObject.SetActive(hasProgress);
        
        if (speechTextRect != null)
        {
            float topMargin = hasProgress ? 25f : 0f;
            speechTextRect.offsetMax = new Vector2(-20f, -12f - topMargin);
        }
        ResizeSpeechBubble();
    }

    public void SetOverlayMessage(string message)
    {
        if (overlaySpeechText == null || overlaySpeechText.transform.parent == null) return;

        overlaySpeechText.transform.parent.gameObject.SetActive(true);
        overlaySpeechText.text = message;
        ResizeSpeechBubble();

        if (helpButtonText != null)
            helpButtonText.text = DefaultHelpLabel;
    }

    public void SetHelpButtonVisible(bool visible)
    {
        if (helpButton == null) return;

        helpButton.gameObject.SetActive(visible);

        if (visible && helpButtonText != null)
            helpButtonText.text = DefaultHelpLabel;
    }

    void ResizeSpeechBubble()
    {
        if (overlaySpeechText == null || speechBubbleRect == null || speechTextRect == null) return;

        float maxWidth = 420f;
        float minWidth = 250f;
        float minHeight = 85f;

        overlaySpeechText.fontSize = 14f;
        
        bool hasProgress = overlayProgressText != null && overlayProgressText.gameObject.activeSelf;
        float progressHeight = hasProgress ? 25f : 0f;
        
        Vector2 preferred = overlaySpeechText.GetPreferredValues(overlaySpeechText.text, maxWidth - 40f, 0f);
        float width = Mathf.Clamp(preferred.x + 46f, minWidth, maxWidth);
        float height = Mathf.Max(minHeight, preferred.y + 46f + progressHeight);

        speechBubbleRect.sizeDelta = new Vector2(width, height);
    }
}
