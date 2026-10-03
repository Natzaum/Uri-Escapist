using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>Menu principal inteiramente gerado em runtime; a cena fornece apenas ambiente e áudio.</summary>
public class MenuPrincipal : MonoBehaviour
{
    // Mantidos para compatibilidade com as referências serializadas da cena.
    public Button playButton;
    public Button exitButton;
    public Toggle nightmareToggle;
    public TextMeshProUGUI titleText;
    [Header("Sound")]
    [Range(0f, 1f)] public float ambientVolume = 0.12f;
    [Range(0f, 1f)] public float playFeedbackVolume = 0.45f;

    public static string GameMode { get; private set; } = "normal";
    private static bool isNightmareMode;
    private bool isStartingGame;
    private Coroutine revealRoutine;
    private string page = "main";
    private CanvasGroup screen;
    private RectTransform content;
    private RectTransform menu;
    private TMP_FontAsset serif;
    private TMP_FontAsset bodyFont;
    private TMP_Text description;
    private RawImage emblem;
    private Texture2D emblemTexture;
    private Texture2D glowTexture;
    private Sprite glowSprite;
    private GameObject generatedCanvas;
    private MenuAmbientAudio ambientAudio;
    private readonly List<Button> buttons = new List<Button>();
    private readonly List<Image> highlights = new List<Image>();
    private readonly List<TMP_Text> labels = new List<TMP_Text>();
    private readonly Color uriBlue = new Color(0.20f, 0.52f, 0.91f);
    private readonly Color muted = new Color(0.57f, 0.67f, 0.80f);

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Desliga só a apresentação antiga; não desativa o objeto que contém este script.
        Canvas oldCanvas = playButton != null ? playButton.GetComponentInParent<Canvas>() : null;
        if (oldCanvas != null)
        {
            oldCanvas.enabled = false;
            GraphicRaycaster raycaster = oldCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = false;
        }
        bodyFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (bodyFont == null) bodyFont = TMP_Settings.defaultFontAsset;
        Font font = Resources.Load<Font>("Fonts/Cinzel-Regular");
        if (font != null) serif = TMP_FontAsset.CreateFontAsset(font);
        if (serif == null) serif = bodyFont;
        BuildCanvas();
        ShowMain();
        ambientAudio = GetComponent<MenuAmbientAudio>();
        if (ambientAudio == null) ambientAudio = gameObject.AddComponent<MenuAmbientAudio>();
        ambientAudio.Initialize(ambientVolume, playFeedbackVolume);
        revealRoutine = StartCoroutine(Reveal());
    }

    private RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private TMP_Text Text(string name, Transform parent, string value, float size,
        Vector2 dimensions, Vector2 position, Color color, bool useSerif = false)
    {
        var text = Rect(name, parent, dimensions, position).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = useSerif ? serif : bodyFont;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private void BuildCanvas()
    {
        generatedCanvas = new GameObject("URI • Menu principal", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = generatedCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = generatedCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var background = Rect("Fundo", canvas.transform, Vector2.zero, Vector2.zero);
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.gameObject.AddComponent<Image>().color = new Color(0.006f, 0.010f, 0.022f);
        content = Rect("Composição", canvas.transform, new Vector2(1920, 1080), Vector2.zero);

        screen = content.gameObject.AddComponent<CanvasGroup>();
        screen.alpha = 0f;

        emblemTexture = CreateEmblem();
        emblem = Rect("Emblema • conhecimento e passagem", content, new Vector2(650, 650),
            new Vector2(0, 170)).gameObject.AddComponent<RawImage>();
        emblem.texture = emblemTexture;
        emblem.raycastTarget = false;
        emblem.color = new Color(1, 1, 1, 0.52f);
        titleText = (TextMeshProUGUI)Text("Título", content, "URI ESCAPIST", 116,
            new Vector2(1550, 170), new Vector2(0, 160), uriBlue, true);
        titleText.characterSpacing = 5f;
        titleText.color = Color.white;
        titleText.enableVertexGradient = true;
        titleText.colorGradient = new VertexGradient(new Color(0.65f, 0.82f, 1.00f),
            new Color(0.46f, 0.70f, 0.98f), new Color(0.08f, 0.27f, 0.58f), new Color(0.13f, 0.40f, 0.78f));
        Text("Subtítulo", content, "O CONHECIMENTO É A SUA ÚNICA SAÍDA", 17,
            new Vector2(950, 40), new Vector2(0, 60), muted).characterSpacing = 5;
        menu = Rect("Opções", content, new Vector2(600, 300), new Vector2(0, -185));
        description = Text("Descrição", content, "", 18, new Vector2(1000, 85),
            new Vector2(0, -380), muted);
        Text("Rodapé", content, "URI ESCAPIST  /  CIÊNCIA DA COMPUTAÇÃO", 13,
            new Vector2(1100, 30), new Vector2(0, -490), muted).characterSpacing = 2;
        Text("Controles", content, "↑ ↓  NAVEGAR     ENTER  SELECIONAR     ESC  VOLTAR", 12,
            new Vector2(1100, 30), new Vector2(0, -455), muted);

        glowTexture = new Texture2D(256, 32, TextureFormat.RGBA32, false);
        var pixels = new Color[256 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 256; x++)
            {
                float horizontal = Mathf.Pow(Mathf.Sin(Mathf.PI * x / 255f), 2);
                float vertical = Mathf.Exp(-Mathf.Pow((y - 15.5f) / 7f, 2));
                pixels[y * 256 + x] = new Color(0.10f, 0.39f, 0.86f, horizontal * vertical * 0.45f);
            }
        glowTexture.SetPixels(pixels);
        glowTexture.Apply();
        glowSprite = Sprite.Create(glowTexture, new Rect(0, 0, 256, 32), new Vector2(0.5f, 0.5f));
        if (EventSystem.current == null)
            new GameObject("Menu EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void ClearPage(string next)
    {
        EventSystem.current.SetSelectedGameObject(null);
        foreach (Transform child in menu) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        buttons.Clear();
        highlights.Clear();
        labels.Clear();
        page = next;
    }

    private Button Option(string label, float y, UnityEngine.Events.UnityAction action)
    {
        int index = buttons.Count;
        RectTransform rect = Rect(label, menu, new Vector2(520, 48), new Vector2(0, y));
        Image glow = rect.gameObject.AddComponent<Image>();
        glow.sprite = glowSprite;
        glow.color = new Color(1, 1, 1, 0);
        Button button = rect.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => { if (!isStartingGame) action(); });
        TMP_Text text = Text("Texto", rect, label, 23, new Vector2(500, 46), Vector2.zero, muted, true);
        buttons.Add(button);
        highlights.Add(glow);
        labels.Add(text);
        EventTrigger trigger = rect.gameObject.AddComponent<EventTrigger>();
        var hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hover.callback.AddListener(_ => {
            if (!isStartingGame) EventSystem.current.SetSelectedGameObject(button.gameObject);
        });
        trigger.triggers.Add(hover);
        var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        select.callback.AddListener(_ => Select(index));
        trigger.triggers.Add(select);
        return button;
    }

    private void Select(int index)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            highlights[i].color = new Color(1, 1, 1, i == index ? 1 : 0);
            labels[i].color = i == index ? new Color(0.80f, 0.90f, 1.00f) : muted;
        }
    }

    private void FinishPage(int selected = 0)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].navigation = new Navigation {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i + buttons.Count - 1) % buttons.Count],
                selectOnDown = buttons[(i + 1) % buttons.Count]
            };
        }
        EventSystem.current.SetSelectedGameObject(buttons[selected].gameObject);
    }

    private string ModeLabel() => GameMode == "facil" ? "Fácil" : GameMode == "dificil" ? "Difícil" : "Normal";

    private void ShowMain()
    {
        ClearPage("main");
        playButton = Option("Nova partida", 90, () => StartCoroutine(StartGame()));
        Option("Dificuldade  ·  " + ModeLabel(), 36, ShowDifficulty);
        Option("Configurações", -18, () => GameInterface.Instance.ShowSettings(ShowMain));
        Option("Créditos", -72, ShowCredits);
        exitButton = Option("Sair", -126, ShowExit);
        description.text = "Entre. Explore. Encontre a saída.";
        FinishPage();
    }

    private void ShowDifficulty()
    {
        ClearPage("difficulty");
        string[] modes = { "facil", "normal", "dificil" };
        string[] names = { "Fácil", "Normal", "Difícil" };
        for (int i = 0; i < modes.Length; i++)
        {
            string mode = modes[i];
            Option(names[i] + (GameMode == mode ? "  ·  selecionado" : ""), 90 - i * 54,
                () => { GameMode = mode; ShowMain(); });
        }
        Option("Voltar", -72, ShowMain);
        description.text = "Fácil: questões fáceis, depois fáceis e médias.\nNormal: fáceis, depois médias.  Difícil: somente difíceis.";
        FinishPage(System.Array.IndexOf(modes, GameMode));
    }

    private void ShowCredits()
    {
        ClearPage("credits");
        Text("Créditos", menu, "UM PROJETO DE CIÊNCIA DA COMPUTAÇÃO\n\nTrabalho de Conclusão de Curso\nURI Escapist", 21,
            new Vector2(820, 160), new Vector2(0, 40), uriBlue);
        Option("Voltar", -95, ShowMain);
        description.text = "Tipografia: Cinzel · The Cinzel Project Authors · SIL Open Font License";
        FinishPage();
    }

    private void ShowExit()
    {
        ClearPage("exit");
        Text("Confirmação", menu, "Deseja sair do jogo?", 24, new Vector2(600, 50), new Vector2(0, 90), uriBlue, true);
        Option("Voltar", 20, ShowMain);
        Option("Sair do jogo", -40, ExitGame);
        description.text = "";
        FinishPage();
    }

    private IEnumerator Reveal()
    {
        for (float t = 0; t < 1.2f; t += Time.unscaledDeltaTime)
        {
            screen.alpha = Mathf.SmoothStep(0, 1, t / 1.2f);
            yield return null;
        }
        screen.alpha = 1;
    }

    private void Update()
    {
        if (emblem != null)
            emblem.color = new Color(1, 1, 1, 0.47f + Mathf.Sin(Time.unscaledTime * 0.45f) * 0.07f);
        if (!isStartingGame && !GameInterface.BlocksInput && Input.GetKeyDown(KeyCode.Escape) && page != "main")
            ShowMain();
    }

    private IEnumerator StartGame()
    {
        if (isStartingGame) yield break;
        DoorGameEnd.ResetRequirement();
        isStartingGame = true;
        if (revealRoutine != null) StopCoroutine(revealRoutine);
        screen.interactable = false;
        description.text = "Abrindo os portões...";
        ambientAudio.PlayStartFeedback();
        StartCoroutine(ambientAudio.FadeOut(0.8f));
        // O Canvas continua bloqueando cliques durante a transição.
        yield return new WaitForSecondsRealtime(0.2f);
        for (float t = 0; t < 0.8f; t += Time.unscaledDeltaTime)
        {
            screen.alpha = 1 - Mathf.Clamp01(t / 0.8f);
            yield return null;
        }
        screen.alpha = 0;
        Time.timeScale = 1f;
        GameInterface.Instance.LoadScene("MainScene");
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static bool IsNightmareMode() => isNightmareMode;
    public static void ResetMenu() { isNightmareMode = false; }

    // Emblema original desenhado por código: arco, livro aberto e linhas de uma passagem.
    private Texture2D CreateEmblem()
    {
        const int size = 512;
        var pixels = new Color[size * size];
        System.Action<float, float, float> dot = (x, y, strength) => {
            for (int dy = -5; dy <= 5; dy++)
                for (int dx = -5; dx <= 5; dx++)
                {
                    int px = Mathf.RoundToInt(x) + dx, py = Mathf.RoundToInt(y) + dy;
                    if (px < 0 || py < 0 || px >= size || py >= size) continue;
                    float a = strength * Mathf.Exp(-(dx * dx + dy * dy) * 0.35f);
                    int index = py * size + px;
                    float alpha = Mathf.Clamp01(pixels[index].a + a);
                    pixels[index] = new Color(0.12f, 0.35f, 0.72f, alpha);
                }
        };
        System.Action<Vector2, Vector2, float> line = (a, b, intensity) => {
            int steps = Mathf.CeilToInt(Vector2.Distance(a, b));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(a, b, steps == 0 ? 0 : i / (float)steps);
                dot(p.x, p.y, intensity * (0.5f + 0.5f * Mathf.PerlinNoise(p.x * 0.13f, p.y * 0.13f)));
            }
        };
        for (int i = 0; i < 220; i++)
        {
            float angle = Mathf.PI * i / 219f;
            dot(256 + Mathf.Cos(angle) * 132, 255 + Mathf.Sin(angle) * 155, 0.24f);
        }
        line(new Vector2(124, 255), new Vector2(124, 100), 0.10f);
        line(new Vector2(388, 255), new Vector2(388, 100), 0.10f);
        line(new Vector2(256, 55), new Vector2(256, 462), 0.12f);
        for (int pageIndex = 0; pageIndex < 3; pageIndex++)
        {
            float y = pageIndex * 9;
            line(new Vector2(256, 174 + y), new Vector2(164, 207 + y), 0.18f);
            line(new Vector2(164, 207 + y), new Vector2(164, 287 + y), 0.13f);
            line(new Vector2(164, 287 + y), new Vector2(256, 254 + y), 0.18f);
            line(new Vector2(256, 174 + y), new Vector2(348, 207 + y), 0.18f);
            line(new Vector2(348, 207 + y), new Vector2(348, 287 + y), 0.13f);
            line(new Vector2(348, 287 + y), new Vector2(256, 254 + y), 0.18f);
        }
        line(new Vector2(205, 100), new Vector2(307, 100), 0.12f);
        line(new Vector2(226, 85), new Vector2(286, 85), 0.12f);
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        if (generatedCanvas != null) Destroy(generatedCanvas);
        if (emblemTexture != null) Destroy(emblemTexture);
        if (glowSprite != null) Destroy(glowSprite);
        if (glowTexture != null) Destroy(glowTexture);
        if (serif != null && serif != bodyFont)
        {
            foreach (Texture2D atlas in serif.atlasTextures) if (atlas != null) Destroy(atlas);
            if (serif.material != null) Destroy(serif.material);
            Destroy(serif);
        }
    }
}
