using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 5;
    public int rows = 6;
    public float tileSize = 1.3f;
    public float gridOffsetY = 0.5f;

    [Header("Assets - Auto Loaded from Resources/Sprites")]
    public Sprite bgSprite;
    public Sprite tileIce;
    public Sprite tileSnow;
    public Sprite tileEmpty;
    public Sprite penguin;
    
    // UI Icons
    public Sprite iconSettings;
    public Sprite iconHelp;
    public Sprite btnUndo;
    public Sprite btnShuffle;
    public Sprite btnHint;

    private void OnValidate()
    {
        // Auto-load resources in Editor for convenience if null
        if (bgSprite == null) bgSprite = Resources.Load<Sprite>("Sprites/game_background");
        if (tileIce == null) tileIce = Resources.Load<Sprite>("Sprites/tile_ice_cube");
        if (tileSnow == null) tileSnow = Resources.Load<Sprite>("Sprites/tile_snowflake");
        if (tileEmpty == null) tileEmpty = Resources.Load<Sprite>("Sprites/tile_empty");
        if (penguin == null) penguin = Resources.Load<Sprite>("Sprites/penguin_character");
        
        if (iconSettings == null) iconSettings = Resources.Load<Sprite>("Sprites/ui_btn_settings");
        if (iconHelp == null) iconHelp = Resources.Load<Sprite>("Sprites/ui_btn_help");
        if (btnUndo == null) btnUndo = Resources.Load<Sprite>("Sprites/ui_btn_undo");
        if (btnShuffle == null) btnShuffle = Resources.Load<Sprite>("Sprites/ui_btn_shuffle");
        if (btnHint == null) btnHint = Resources.Load<Sprite>("Sprites/ui_btn_hint");
    }

    [ContextMenu("Build Level Visuals")]
    public void BuildLevel()
    {
        ClearLevel();
        OnValidate(); // Ensure assets loaded

        // 1. Setup Camera
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 9f; 
        Camera.main.backgroundColor = Color.white; 
        Camera.main.clearFlags = CameraClearFlags.SolidColor; // Ensure no dark skybox

        // 2. Setup Background
        GameObject bgObj = new GameObject("Background_Image");
        bgObj.transform.parent = this.transform;
        SpriteRenderer bgR = bgObj.AddComponent<SpriteRenderer>();
        bgR.sprite = bgSprite;
        // Brighten the background texture itself by tinting it slightly if it's too dark, 
        // but usually white is max brightness. 
        // We can't make it brighter than the texture pixels unless we use a shader.
        // Assuming the texture is fine, but let's ensure alpha is full.
        bgR.color = Color.white;

        // Scale BG to cover Screen
        if (bgSprite != null)
        {
            float targetHeight = 22f; // Increased coverage
            float spriteHeightUnits = bgSprite.rect.height / bgSprite.pixelsPerUnit;
            float scale = targetHeight / spriteHeightUnits;
            bgObj.transform.localScale = new Vector3(scale, scale, 1f);
        }
        bgObj.transform.position = new Vector3(0, 0, 10);

        // 3. Build Grid
        GameObject gridHolder = new GameObject("Grid_Holder");
        gridHolder.transform.parent = this.transform;
        
        // Colors for Grid - EVEN BRIGHTER / PASTEL
        Color cBlue = new Color(0.8f, 0.9f, 1f); // lighter blue
        Color cWhite = Color.white;
        Color cGrey = new Color(0.95f, 0.95f, 0.98f); // lighter grey

        // Center the grid: (Cols * Size) / 2
        float totalGridWidth = columns * tileSize;
        float totalGridHeight = rows * tileSize;
        
        float startX = -(totalGridWidth / 2f) + (tileSize / 2f);
        float startY = (totalGridHeight / 2f) - (tileSize / 2f) + gridOffsetY;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.parent = gridHolder.transform;
                tile.transform.position = new Vector3(startX + x * tileSize, startY - y * tileSize, 0);
                
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                
                Sprite targetSprite;
                Color targetColor;
                
                float rand = Random.value;
                if (rand > 0.6f) 
                { 
                    targetSprite = tileIce; 
                    targetColor = cBlue;
                }
                else if (rand > 0.3f) 
                { 
                    targetSprite = tileSnow; 
                    targetColor = cGrey;
                }
                else 
                { 
                    targetSprite = tileEmpty; 
                    targetColor = cWhite;
                }
                
                sr.sprite = targetSprite;
                sr.color = targetColor;
                SetSpriteSize(tile, targetSprite, tileSize);
            }
        }

        // 4. Place Penguin
        GameObject penguinObj = new GameObject("Penguin");
        penguinObj.transform.parent = gridHolder.transform;
        float pX = startX + 2 * tileSize;
        float pY = startY - 0 * tileSize;
        penguinObj.transform.position = new Vector3(pX, pY, -0.5f); // Closer to camera
        
        SpriteRenderer pSr = penguinObj.AddComponent<SpriteRenderer>();
        pSr.sprite = penguin;
        pSr.sortingOrder = 10;
        SetSpriteSize(penguinObj, penguin, tileSize * 0.85f);


        // 5. Create UI (Canvas Overlay)
        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.parent = this.transform;
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Top UI
        Color cBtnTop = new Color(0.8f, 0.9f, 1f);
        CreateImageButton("Btn_Settings", canvasObj.transform, iconSettings, AnchorType.TopLeft, new Vector2(50, -100), new Vector2(130, 130), cBtnTop);
        CreateImageButton("Btn_Help", canvasObj.transform, iconHelp, AnchorType.TopRight, new Vector2(-50, -100), new Vector2(130, 130), cBtnTop);
        CreateText("LevelText", canvasObj.transform, "LEVEL 5", AnchorType.TopCenter, new Vector2(0, -130), 75);

        // Progress Bar
        Sprite pbBg = Resources.Load<Sprite>("Sprites/ui_progressbar");
        Sprite pbFill = Resources.Load<Sprite>("Sprites/ui_progressbar_fill");
        if (pbBg != null && pbFill != null)
        {
            GameObject pbObj = new GameObject("ProgressBar");
            pbObj.transform.SetParent(canvasObj.transform, false);
            Image bgImg = pbObj.AddComponent<Image>();
            bgImg.sprite = pbBg;
            bgImg.type = Image.Type.Sliced;
            RectTransform pbRt = pbObj.GetComponent<RectTransform>();
            SetAnchor(pbRt, AnchorType.TopCenter);
            pbRt.anchoredPosition = new Vector2(0, -230);
            pbRt.sizeDelta = new Vector2(550, 70);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(pbObj.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = pbFill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0.35f;
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(8, 8); fillRt.offsetMax = new Vector2(-8, -8);
        }

        // Bottom UI Colors - Very Bright Pastels
        Color cPink = new Color(1f, 0.85f, 0.85f); 
        Color cPurple = new Color(0.9f, 0.9f, 1f); 
        Color cGreen = new Color(0.8f, 1f, 0.9f); 

        CreateImageButton("Btn_Undo", canvasObj.transform, btnUndo, AnchorType.BottomLeft, new Vector2(130, 220), new Vector2(180, 180), cPink);
        CreateImageButton("Btn_Shuffle", canvasObj.transform, btnShuffle, AnchorType.BottomRight, new Vector2(-130, 220), new Vector2(180, 180), cPurple);
        CreateImageButton("Btn_Hint", canvasObj.transform, btnHint, AnchorType.BottomCenter, new Vector2(0, 220), new Vector2(380, 180), cGreen);
        
        CreateText("HintText", canvasObj.transform, "HINT", AnchorType.BottomCenter, new Vector2(0, 220), 55, true);
    }

    private void SetSpriteSize(GameObject obj, Sprite sprite, float targetSizeWorldUnits)
    {
        if (sprite == null) return;
        float spriteSizeNative = sprite.rect.width / sprite.pixelsPerUnit;
        float scaleFactor = targetSizeWorldUnits / spriteSizeNative;
        obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    private enum AnchorType { TopLeft, TopRight, TopCenter, BottomLeft, BottomRight, BottomCenter, Center }

    void CreateImageButton(string name, Transform parent, Sprite sprite, AnchorType anchor, Vector2 offset, Vector2 size, Color? tint = null)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        if (tint.HasValue) img.color = tint.Value;
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = offset;
        rt.sizeDelta = size;
    }

    void CreateText(string name, Transform parent, string content, AnchorType anchor, Vector2 offset, int fontSize, bool isOverlay = false)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = isOverlay ? Color.white : new Color(0.2f, 0.4f, 0.6f);
        
        if (!isOverlay)
            obj.AddComponent<Outline>().effectDistance = new Vector2(2, -2);
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(400, 120);
    }
    
    public void ClearLevel()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
