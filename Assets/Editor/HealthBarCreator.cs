// ============================================================
//  HealthBarCreator.cs — Editor Utility
//  Unity menüsünden tek tıkla health bar hiyerarşisi oluşturur.
//
//  Kullanım: Unity menüsünden → FightingGame → Create Health Bars
//
//  NOT: Bu script Assets/Editor klasöründe olmalıdır.
//       Sadece Editor'da çalışır, build'e dahil olmaz.
// ============================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class HealthBarCreator
{
    [MenuItem("FightingGame/Create Health Bars")]
    public static void CreateHealthBars()
    {
        // Canvas bul veya oluştur
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Player 1 — sol üst
        GameObject hb1 = CreateSingleHealthBar(canvas.transform, "HealthBarP1",
            anchor: new Vector2(0f, 1f),
            pivot: new Vector2(0f, 1f),
            position: new Vector2(20f, -20f),
            fillOriginLeft: true);

        // Player 2 — sağ üst
        GameObject hb2 = CreateSingleHealthBar(canvas.transform, "HealthBarP2",
            anchor: new Vector2(1f, 1f),
            pivot: new Vector2(1f, 1f),
            position: new Vector2(-20f, -20f),
            fillOriginLeft: false);

        // HealthBarUI scriptlerini ekle
        var script1 = hb1.AddComponent<FightingGame.UI.HealthBarUI>();
        var script2 = hb2.AddComponent<FightingGame.UI.HealthBarUI>();

        // Fill Image referanslarını ata
        Image fill1 = hb1.transform.Find("FillMask/Fill").GetComponent<Image>();
        Image fill2 = hb2.transform.Find("FillMask/Fill").GetComponent<Image>();

        // SerializedObject ile private field'ları ata
        SerializedObject so1 = new SerializedObject(script1);
        so1.FindProperty("fillImage").objectReferenceValue = fill1;
        so1.ApplyModifiedProperties();

        SerializedObject so2 = new SerializedObject(script2);
        so2.FindProperty("fillImage").objectReferenceValue = fill2;
        so2.ApplyModifiedProperties();

        // Special Meter kurulumunu otomatik çağır
        SetupSpecialMeterTool.SetupSpecialMeter();

        Selection.activeGameObject = hb1;
        Debug.Log("[HealthBarCreator] HealthBarP1 ve HealthBarP2 (Aynalı) oluşturuldu! Frame.png otomatik atandı.");
    }

    private static Sprite GetOrFixSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateSingleHealthBar(
        Transform parent, string name,
        Vector2 anchor, Vector2 pivot, Vector2 position,
        bool fillOriginLeft)
    {
        // ── Ana Container ────────────────────────────────────
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = anchor;
        rootRT.anchorMax = anchor;
        rootRT.pivot     = pivot;
        rootRT.anchoredPosition = position;
        rootRT.sizeDelta = new Vector2(400f, 40f);

        // ── FillMask (RectMask2D — kırpma alanı) ────────────
        GameObject fillMask = new GameObject("FillMask", typeof(RectTransform));
        fillMask.transform.SetParent(root.transform, false);
        fillMask.AddComponent<RectMask2D>();

        RectTransform maskRT = fillMask.GetComponent<RectTransform>();
        maskRT.anchorMin = Vector2.zero;
        maskRT.anchorMax = Vector2.one;
        maskRT.offsetMin = new Vector2(15f, 5f);   // left, bottom padding
        maskRT.offsetMax = new Vector2(-15f, -5f);  // right, top padding

        // ── Fill (can dolgusu — Filled Image) ────────────────
        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillMask.transform, false);

        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f, 1f); // yeşil

        fillImg.type = Image.Type.Simple;
        fillImg.raycastTarget = false;

        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // ── Frame (çerçeve PNG) ──────────────────────────────
        GameObject frame = new GameObject("Frame", typeof(RectTransform));
        frame.transform.SetParent(root.transform, false);

        Image frameImg = frame.AddComponent<Image>();
        frameImg.raycastTarget = false;
        
        // Frame'i otomatik olarak yükle ve ata
        Sprite frameSprite = GetOrFixSprite("Assets/Bar/Frame.png");
        if (frameSprite != null)
        {
            frameImg.sprite = frameSprite;
            frameImg.preserveAspect = true;
        }

        RectTransform frameRT = frame.GetComponent<RectTransform>();
        frameRT.anchorMin = Vector2.zero;
        frameRT.anchorMax = Vector2.one;
        frameRT.offsetMin = Vector2.zero;
        frameRT.offsetMax = Vector2.zero;

        frame.transform.SetAsLastSibling();

        // ── P2 Aynalama (Mirrored) ───────────────────────────
        if (!fillOriginLeft) // fillOriginLeft false ise bu P2 demektir
        {
            // Hem çerçeveyi hem de dolguyu X ekseninde ters çevir (aynala)
            frame.transform.localScale = new Vector3(-1, 1, 1);
            fillMask.transform.localScale = new Vector3(-1, 1, 1);
        }

        return root;
    }
}
#endif
