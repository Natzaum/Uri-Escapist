using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Uma única autoridade para telas modais, pausa, cursor e transições.</summary>
[DefaultExecutionOrder(10000)]
public class GameInterface : MonoBehaviour
{
    public enum ScreenState { None, Pause, Settings, Quiz, Feedback, Result, Loading, Error, Confirm }
    private static GameInterface instance;
    public static GameInterface Instance {
        get {
            if (instance == null) instance = new GameObject("URI • Interface").AddComponent<GameInterface>();
            return instance;
        }
    }
    public static bool WorldPaused => instance != null && instance.Pauses;
    public static bool BlocksInput => instance != null && instance.State != ScreenState.None;
    public ScreenState State { get; private set; }
    public bool IsTerminal => State == ScreenState.Result || transitioning;
    private bool transitioning;
    private RectTransform root;
    private TMP_FontAsset serif;
    private TMP_FontAsset body;
    private Texture2D glowTexture;
    private Sprite glowSprite;
    private TMP_Text message;
    private TMP_Text progressText;
    private RectTransform progressFill;
    private float progress = -1;
    private Action renderCurrent;
    private Action resumePause;
    private Action returnSettings;
    private Action returnConfirmation;
    private readonly List<Selectable> controls = new List<Selectable>();
    private readonly Color uriBlue = new Color(0.28f, 0.59f, 0.96f);
    private readonly Color muted = new Color(0.64f, 0.73f, 0.85f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() { var ui = Instance; AudioListener.volume = GamePreferences.Volume; }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        glowTexture = new Texture2D(256,32,TextureFormat.RGBA32,false);
        var pixels = new Color[256*32];
        for(int y=0;y<32;y++) for(int x=0;x<256;x++)
            pixels[y*256+x]=new Color(1,1,1,Mathf.Pow(Mathf.Sin(Mathf.PI*x/255f),2)*Mathf.Exp(-Mathf.Pow((y-15.5f)/10f,2)));
        glowTexture.SetPixels(pixels);glowTexture.Apply();
        glowSprite=Sprite.Create(glowTexture,new Rect(0,0,256,32),new Vector2(.5f,.5f));
        body = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF") ?? TMP_Settings.defaultFontAsset;
        var font = Resources.Load<Font>("Fonts/Cinzel-Regular");
        serif = font != null ? TMP_FontAsset.CreateFontAsset(font) : body;
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        gameObject.AddComponent<GraphicRaycaster>();
        root = MakeRect("Tela", transform, Vector2.zero, Vector2.zero);
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
        root.gameObject.AddComponent<Image>().color = new Color(0.006f, 0.010f, 0.022f, 0.97f);
        root.gameObject.SetActive(false);
        gameObject.AddComponent<GameHud>().Build(body, serif);
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        transitioning = false;
        Hide();
        AudioListener.volume = GamePreferences.Volume;
    }

    private bool IsMenu => SceneManager.GetActiveScene().name == "UriMenu";
    private bool Pauses => State != ScreenState.None && ((State != ScreenState.Quiz && State != ScreenState.Feedback) || GameDifficulty.PausesReading);
    private void LateUpdate()
    {
        if (State == ScreenState.None) return;
        Time.timeScale = Pauses ? 0f : 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !transitioning)
        {
            if (State == ScreenState.Confirm) returnConfirmation?.Invoke();
            else if (State == ScreenState.Settings) CloseSettings();
            else if (State == ScreenState.Pause) resumePause?.Invoke();
            else if (State == ScreenState.None && !IsMenu) ShowPause();
            else if (State == ScreenState.Quiz) QuizManager.Instance?.CloseBook();
            else if (State == ScreenState.Feedback) Hide();
        }
        if (State == ScreenState.Loading && progressFill != null)
        {
            float width = progress < 0 ? 100f : 760f * Mathf.Clamp01(progress);
            progressFill.sizeDelta = new Vector2(width, 2);
            progressFill.anchoredPosition = new Vector2(progress < 0 ? Mathf.PingPong(Time.unscaledTime * 180f, 660f) : 0, 0);
        }
    }

    public void Hide()
    {
        State = ScreenState.None;
        root.gameObject.SetActive(false);
        renderCurrent = null;
        controls.Clear();
        Time.timeScale = 1;
        Cursor.lockState = IsMenu ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsMenu;
    }

    private RectTransform MakeRect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }

    private TMP_Text Text(string value, float y, float size = 24, float width = 1200, float height = 70, bool heading = false)
    {
        var text = MakeRect(value, root, new Vector2(width, height), new Vector2(0,y)).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = heading ? serif : body;
        text.fontSize = size;
        text.text = value;
        text.richText = false;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.color = heading ? uriBlue : muted;
        return text;
    }

    private void Begin(ScreenState state, string title, string subtitle)
    {
        foreach (Transform child in root) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        controls.Clear(); progressFill = null; progressText = null;
        State = state;
        root.gameObject.SetActive(true);
        if (EventSystem.current == null) new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        EventSystem.current.SetSelectedGameObject(null);
        Text("URI ESCAPIST", 450, 17, heading:true);
        Text(title, 345, 62, height:100, heading:true);
        var line = MakeRect("Ornamento", root, new Vector2(380,1),new Vector2(0,295)).gameObject.AddComponent<Image>();
        line.color = new Color(uriBlue.r,uriBlue.g,uriBlue.b,0.35f); line.raycastTarget = false;
        message = Text(subtitle, 205, 23, 1300, 100);
        Text(state == ScreenState.Quiz ? (GameDifficulty.PausesReading ? "LEITURA EM PAUSA  •  ESC FECHA O LIVRO" : GameDifficulty.CanLeaveBook ? "O MUNDO CONTINUA ATIVO  •  ESC FECHA O LIVRO" : "O MUNDO CONTINUA ATIVO  •  RESPONDA PARA SAIR") : "URI ESCAPIST  /  CIÊNCIA DA COMPUTAÇÃO",
            -465, 15, 1300, 40);
        Time.timeScale = Pauses ? 0 : 1;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
    }

    private Button Button(string label, float y, Action action, float width = 650, float height = 64, bool prose = false)
    {
        var rect = MakeRect(label, root, new Vector2(width,height), new Vector2(0,y));
        var image = rect.gameObject.AddComponent<Image>(); image.sprite=glowSprite; image.color = Color.clear;
        var button = rect.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(1,1,1,0);
        colors.highlightedColor = new Color(0.08f,0.28f,0.62f,0.65f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.13f,0.42f,0.85f,0.85f);
        button.colors = colors;
        // The tint controls highlight opacity; source image must remain white.
        image.color = Color.white;
        var text = Text(label,0,prose ? 25 : 27,width-48,height-10,!prose);
        text.transform.SetParent(rect,false);
        text.rectTransform.anchoredPosition = Vector2.zero;
        text.enableAutoSizing = true; text.fontSizeMin = 19; text.fontSizeMax = prose ? 25 : 27;
        if (prose) { text.alignment = TextAlignmentOptions.MidlineLeft; text.color = new Color(.84f,.90f,.98f); }
        button.onClick.AddListener(() => action());
        var hover = rect.gameObject.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry {eventID=EventTriggerType.PointerEnter};
        enter.callback.AddListener(_ => { if(button.interactable) EventSystem.current.SetSelectedGameObject(button.gameObject); });
        hover.triggers.Add(enter);
        controls.Add(button);
        return button;
    }

    private void Focus()
    {
        for(int i=0;i<controls.Count;i++)
            controls[i].navigation = new Navigation { mode=Navigation.Mode.Explicit,
                selectOnUp=controls[(i+controls.Count-1)%controls.Count], selectOnDown=controls[(i+1)%controls.Count] };
        if(controls.Count>0) EventSystem.current.SetSelectedGameObject(controls[0].gameObject);
    }

    public void ShowPause()
    {
        if (IsTerminal || State == ScreenState.Loading || State == ScreenState.Error ||
            (State == ScreenState.Quiz && !GameDifficulty.CanLeaveBook)) return;
        resumePause = renderCurrent ?? Hide;
        RenderPause();
    }
    private void RenderPause()
    {
        Begin(ScreenState.Pause,"Pausa","O tempo está suspenso. A saída pode esperar.");
        Button("Continuar",85,()=>resumePause?.Invoke());
        Button("Configurações",5,()=>ShowSettings(RenderPause));
        Button("Reiniciar andar",-75,()=>Confirm("Reiniciar este andar?",()=>LoadScene(SceneManager.GetActiveScene().name),RenderPause));
        Button("Voltar ao menu",-155,()=>Confirm("Voltar ao menu principal?",()=>LoadScene("UriMenu"),RenderPause));
        Focus();
    }
    private void Confirm(string title,Action confirm,Action back)
    {
        returnConfirmation=back;
        Begin(ScreenState.Confirm,title,"O progresso deste andar será perdido.");
        Button("Cancelar",30,back); Button("Confirmar",-55,confirm); Focus();
    }

    public void ShowSettings(Action onBack)
    {
        returnSettings = onBack;
        Begin(ScreenState.Settings,"Configurações","Ajuste o som e o movimento da câmera.");
        Slider("Volume geral",80,0,1,GamePreferences.Volume,GamePreferences.SetVolume, v=>Mathf.RoundToInt(v*100)+"%");
        Slider("Sensibilidade do mouse",-65,0.2f,3,GamePreferences.Sensitivity,GamePreferences.SetSensitivity,v=>v.ToString("0.0")+"×");
        Button("Voltar",-260,CloseSettings); Focus();
    }
    private void CloseSettings() { GamePreferences.Save(); var back=returnSettings; Hide(); back?.Invoke(); }
    private void Slider(string label,float y,float min,float max,float value,Action<float> changed,Func<float,string> format)
    {
        Text(label,y+50,24);
        var valueText=Text(format(value),y-42,19);
        var rect=MakeRect(label,root,new Vector2(720,36),new Vector2(0,y));
        rect.gameObject.AddComponent<Image>().color=Color.clear;
        var slider=rect.gameObject.AddComponent<Slider>(); slider.minValue=min; slider.maxValue=max;
        var background=MakeRect("Trilho",rect,new Vector2(720,3),Vector2.zero).gameObject.AddComponent<Image>();
        background.color=new Color(.14f,.23f,.37f);
        var handleArea=MakeRect("Área",rect,new Vector2(700,36),Vector2.zero);
        var handle=MakeRect("Indicador",handleArea,new Vector2(12,-12),Vector2.zero).gameObject.AddComponent<Image>();
        handle.color=uriBlue;
        slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.value=value;
        slider.onValueChanged.AddListener(v=>{changed(v);valueText.text=format(v);});
        controls.Add(slider);
    }

    public void ShowQuiz(BookQuiz book, Action<int> answer)
    {
        if (IsTerminal) return;
        renderCurrent = ()=>ShowQuiz(book,answer);
        Begin(ScreenState.Quiz,"O livro aguarda",book.question);
        message.enableAutoSizing=true; message.fontSizeMin=20; message.fontSizeMax=27;
        message.fontSize=27; message.rectTransform.sizeDelta=new Vector2(1360,160);
        for(int i=0;i<book.options.Length;i++)
        {
            int index=i;
            Button(((char)('A'+i))+"   "+book.options[i],65-i*105,()=>answer(index),1360,94,true);
        }
        if (GameDifficulty.CanLeaveBook)
            Button("Fechar livro · Esc",-355,()=>QuizManager.Instance?.CloseBook(),650,54);
        Text(GameDifficulty.CanLeaveBook ? "A pergunta fica guardada neste livro. Pressione E perto dele para retomar."
            : "Responda para sair. O mundo continua em movimento.",-418,17,1400,40);
        Focus();
    }

    public void ShowFeedback(bool correct,string answer,Action onContinue)
    {
        if (IsTerminal) return;
        renderCurrent=()=>ShowFeedback(correct,answer,onContinue);
        Begin(ScreenState.Feedback,correct ? "Conhecimento adquirido" : "Resposta incorreta",
            correct ? "Você encontrou a resposta." : "Seu limite de erros diminuiu. Mantenha a atenção.");
        Text("RESPOSTA CORRETA",60,18,heading:true);
        Text(answer,-35,30,1320,140);
        Button("Continuar",-220,onContinue);
        Focus();
    }

    public void ShowResult(bool victory,string detail=null)
    {
        if (State==ScreenState.Result || transitioning || State==ScreenState.Loading || State==ScreenState.Error) return;
        renderCurrent=null;
        if (QuizManager.Instance!=null) QuizManager.Instance.ForceCloseQuiz();
        Begin(ScreenState.Result,victory ? "Você escapou" : "A jornada chegou ao fim",
            detail ?? (victory ? "Os desafios foram vencidos. O conhecimento abriu sua saída." : "Ainda há uma chance de escapar."));
        if(BookManager.Instance!=null)
            Text("Acertos: "+BookManager.Instance.GetBooksCollected()+"    Erros: "+BookManager.Instance.GetErrors(),50,23);
        Button(victory ? "Jogar novamente" : "Tentar novamente",-50,()=>{
            if(victory) DoorGameEnd.ResetRequirement();
            LoadScene(victory ? "MainScene" : SceneManager.GetActiveScene().name);
        });
        Button("Voltar ao menu",-140,()=>LoadScene("UriMenu")); Focus();
    }

    public void ShowLoading(string detail)
    {
        renderCurrent=null; progress=-1;
        Begin(ScreenState.Loading,"Preparando a jornada",detail);
        var track=MakeRect("Progresso",root,new Vector2(760,2),new Vector2(0,-50)).gameObject.AddComponent<Image>();
        track.color=new Color(.12f,.20f,.34f);
        progressFill=MakeRect("Preenchimento",track.transform,new Vector2(100,2),Vector2.zero);
        progressFill.anchorMin=progressFill.anchorMax=progressFill.pivot=new Vector2(0,0.5f);
        progressFill.gameObject.AddComponent<Image>().color=uriBlue;
        progressText=Text("Aguardando o servidor…",-105,20);
    }
    public void SetProgress(float value)
    {
        progress=value;
        if(progressText!=null) progressText.text=value<0 ? "Aguardando o servidor…" : Mathf.RoundToInt(Mathf.Clamp01(value)*100)+"%";
    }
    public void ShowLoadError(string detail,Action retry)
    {
        Begin(ScreenState.Error,"A passagem está fechada",detail);
        Button("Tentar novamente",-20,retry);
        Button("Voltar ao menu",-105,()=>LoadScene("UriMenu")); Focus();
    }
    public void LoadScene(string scene)
    {
        if(transitioning) return;
        if(!Application.CanStreamedLevelBeLoaded(scene)) {
            ShowLoadError("Não foi possível abrir a próxima cena.",()=>LoadScene(scene)); return;
        }
        StartCoroutine(Load(scene));
    }
    private IEnumerator Load(string scene)
    {
        transitioning=true;
        GamePreferences.Save();
        if(QuizManager.Instance!=null) QuizManager.Instance.ForceCloseQuiz();
        ShowLoading("Abrindo a próxima passagem…");
        var operation=SceneManager.LoadSceneAsync(scene);
        while(!operation.isDone) { SetProgress(operation.progress/0.9f); yield return null; }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded-=SceneLoaded;
        if(instance==this) instance=null;
        if(glowSprite!=null) Destroy(glowSprite);
        if(glowTexture!=null) Destroy(glowTexture);
        if(serif!=null && serif!=body) {
            foreach(var atlas in serif.atlasTextures) if(atlas!=null) Destroy(atlas);
            Destroy(serif.material); Destroy(serif);
        }
    }
}
