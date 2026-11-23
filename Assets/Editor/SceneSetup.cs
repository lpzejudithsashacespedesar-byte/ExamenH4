using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSetup : Editor
{
    [MenuItem("ExamenH4/Configurar Escena")]
    static void SetupScene()
    {
        if (Application.isPlaying) { Debug.LogError("Sal del Play mode primero (Ctrl+P)."); return; }

        ImportarSprite("Assets/Sprites/background.png", 100);
        ImportarSprite("Assets/Sprites/eevee.png", 800);
        AssetDatabase.Refresh();

        AudioClip sonido = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/Audio/freesound_community-pokemon-cry-parody-46225.mp3");

        Sprite bgSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/background.png");
        Sprite eeveeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/eevee.png");

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);

        // ===== CAMARA =====
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.4f, 0.75f, 1f);
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ===== FONDO =====
        var fondo = new GameObject("Fondo");
        var srF = fondo.AddComponent<SpriteRenderer>(); srF.sortingOrder = -10;
        fondo.transform.position = Vector3.zero;
        if (bgSprite != null)
        {
            srF.sprite = bgSprite;
            fondo.transform.localScale = new Vector3(
                17.8f / bgSprite.bounds.size.x,
                10.2f / bgSprite.bounds.size.y, 1);
        }
        else { srF.sprite = MkSpr(new Color(0.4f,0.75f,1f)); fondo.transform.localScale=new Vector3(20,12,1); }

        // ===== TECHO (con sonido al tocarlo) =====
        var techo = new GameObject("Techo");
        techo.transform.position  = new Vector3(0, 5.3f, 0);
        techo.transform.localScale = new Vector3(20f, 0.3f, 1);
        techo.AddComponent<BoxCollider2D>();
        techo.AddComponent<ObstacleController>();
        var techoAudio = techo.AddComponent<AudioSource>();
        techoAudio.playOnAwake = false;
        if (sonido != null) techoAudio.clip = sonido;

        // ===== PAREDES LATERALES (invisibles) =====
        Pared("ParedIzq", new Vector3(-9.5f, 0, 0), new Vector3(0.3f, 20f, 1));
        Pared("ParedDer", new Vector3( 9.5f, 0, 0), new Vector3(0.3f, 20f, 1));

        // ===== SUELO 3 segmentos con 2 HUECOS =====
        Plat("Suelo1", new Vector3(-6f,  -4.3f, 0), new Vector3(7f, 0.6f, 1));
        // ---- HUECO 1 (vacio, Eevee cae aqui) ----
        Plat("Suelo2", new Vector3( 1f,  -4.3f, 0), new Vector3(4f, 0.6f, 1));
        // ---- HUECO 2 (vacio, Eevee cae aqui) ----
        Plat("Suelo3", new Vector3( 7f,  -4.3f, 0), new Vector3(4f, 0.6f, 1));

        // ===== PLATAFORMAS ELEVADAS =====
        Plat("Plat1",  new Vector3(-4f,  -1.8f, 0), new Vector3(4f, 0.5f, 1));
        Plat("Plat2",  new Vector3( 4f,  -1.8f, 0), new Vector3(4f, 0.5f, 1));

        // ===== OBSTACULOS en plataformas elevadas (con sonido) =====
        Obstaculo("Obs1", new Vector3(-5.3f, -0.9f, 0), sonido);
        Obstaculo("Obs2", new Vector3(-2.8f, -0.9f, 0), sonido);
        Obstaculo("Obs3", new Vector3( 2.8f, -0.9f, 0), sonido);
        Obstaculo("Obs4", new Vector3( 5.3f, -0.9f, 0), sonido);

        // ===== EEVEE =====
        // Suelo1 top = -4.3 + 0.3 = -4.0  → Eevee arranca parado ahi
        var player = new GameObject("Eevee");
        player.transform.position = new Vector3(-7f, -4.0f, 0); // en el suelo
        player.transform.localScale = Vector3.one;               // escala 1 para physics
        player.tag = "Player";

        // Sprite en hijo separado (no afecta al collider)
        var spriteChild = new GameObject("EeveeSprite");
        spriteChild.transform.SetParent(player.transform);
        spriteChild.transform.localPosition = new Vector3(0, 0.7f, 0); // centrado en el collider
        spriteChild.transform.localScale    = Vector3.one;
        var srP = spriteChild.AddComponent<SpriteRenderer>();
        srP.sortingOrder = 5;
        srP.sprite = eeveeSprite != null
            ? eeveeSprite
            : MkSpr(new Color(0.65f, 0.45f, 0.2f));

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f; rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        var col = player.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(1.0f, 1.4f);
        col.offset = new Vector2(0f, 0.7f); // bottom al nivel del suelo

        var pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 6f; pc.jumpForce = 15f;

        var gc = new GameObject("GroundCheck");
        gc.transform.SetParent(player.transform);
        gc.transform.localPosition = new Vector3(0, -0.1f, 0); // justo debajo
        pc.groundCheck = gc.transform;
        pc.groundLayer = LayerMask.GetMask("Default");

        player.AddComponent<AudioSource>().playOnAwake = false;

        var vd = player.AddComponent<VoidDetector>();
        vd.fallLimit   = -5.5f;
        vd.fallMessage = "¡Eevee cayó al vacío!";
        if (sonido != null) vd.fallSound = sonido;

        // ===== GAMEMANAGER =====
        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        // ===== CANVAS UI =====
        var canvas = new GameObject("Canvas");
        var cv = canvas.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var scoreUI = Texto(canvas, "PuntajeTexto", "Puntos: 0", 36,
            new Vector2(0,1), new Vector2(0,1), new Vector2(250,55), new Vector2(15,-15), Color.white);

        var msgUI = Texto(canvas, "MensajeTexto", "¡Eevee cayó al vacío!", 52,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(750,110), Vector2.zero, Color.yellow);
        msgUI.gameObject.SetActive(false);

        // Nombre en esquina inferior derecha
        Texto(canvas, "NombreTexto", "lpzejudithsashacespedesar-byte", 22,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(500, 40), new Vector2(-10, 10), Color.white);

        vd.messageText = msgUI;
        var gmC = gm.GetComponent<GameManager>();
        gmC.scoreText = scoreUI; gmC.messageText = msgUI;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Escena lista! Ctrl+S, luego Play.");
    }

    static void ImportarSprite(string path, int ppu)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.spritePixelsPerUnit = ppu;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    static void Plat(string n, Vector3 pos, Vector3 esc)
    {
        var go = new GameObject(n);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MkSpr(new Color(0.2f, 0.6f, 0.1f));
        sr.sortingOrder = 2;
        go.transform.position = pos; go.transform.localScale = esc;
        go.AddComponent<BoxCollider2D>();
    }

    static void Obstaculo(string n, Vector3 pos, AudioClip clip)
    {
        var go = new GameObject(n);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MkSpr(new Color(0.4f, 0.35f, 0.3f));
        sr.sortingOrder = 3;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.4f, 0.9f, 1);
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<ObstacleController>();
        var au = go.AddComponent<AudioSource>();
        au.playOnAwake = false;
        if (clip != null) au.clip = clip;
    }

    static void Pared(string n, Vector3 pos, Vector3 esc)
    {
        var go = new GameObject(n); go.transform.position = pos; go.transform.localScale = esc;
        go.AddComponent<BoxCollider2D>();
    }

    static UnityEngine.UI.Text Texto(GameObject canvas, string nombre, string txt,
        int size, Vector2 ancMin, Vector2 ancMax, Vector2 sd, Vector2 ap, Color color)
    {
        var go = new GameObject(nombre); go.transform.SetParent(canvas.transform, false);
        var t = go.AddComponent<UnityEngine.UI.Text>();
        t.text = txt; t.fontSize = size; t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter; t.color = color;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = ancMin; rt.sizeDelta = sd; rt.anchoredPosition = ap;
        return t;
    }

    static Sprite MkSpr(Color c)
    {
        var tex = new Texture2D(1,1); tex.SetPixel(0,0,c); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1);
    }
}
