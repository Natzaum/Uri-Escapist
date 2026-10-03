using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>HUD não interativo, no mesmo canvas e paleta dos menus.</summary>
public class GameHud : MonoBehaviour
{
    public static GameHud Instance { get; private set; }
    private RectTransform root, banner;
    private TMP_FontAsset body, heading;
    private TMP_Text books, errors, stamina, timer, noticeTitle, noticeDetail;
    private RectTransform booksFill, errorsFill, staminaFill;
    private PlayerStamina fullStamina;
    private PlayerStaminaSimple simpleStamina;
    private float noticeTime;
    private readonly Color blue = new Color(.28f,.59f,.96f);
    private readonly Color pale = new Color(.78f,.86f,.96f);

    public void Build(TMP_FontAsset bodyFont, TMP_FontAsset titleFont)
    {
        Instance = this; body = bodyFont; heading = titleFont;
        root = Rect("HUD",transform,Vector2.zero,Vector2.zero);
        root.anchorMin=Vector2.zero; root.anchorMax=Vector2.one;
        root.SetAsFirstSibling();
        Card("LIVROS",32,out books,out booksFill);
        Card("ERROS",214,out errors,out errorsFill);
        Card("FÔLEGO",396,out stamina,out staminaFill);
        var clock=Rect("Tempo",root,new Vector2(170,68),new Vector2(-32,-28));
        clock.anchorMin=clock.anchorMax=clock.pivot=new Vector2(1,1);
        Panel(clock);
        Label(clock,"TEMPO RESTANTE",new Vector2(0,18),13,body,150,24);
        timer=Label(clock,"",new Vector2(0,-6),22,heading,150,44);
        banner=Rect("Aviso",root,new Vector2(760,108),new Vector2(0,-124));
        banner.anchorMin=banner.anchorMax=banner.pivot=new Vector2(.5f,1);
        Panel(banner);
        noticeTitle=Label(banner,"",new Vector2(0,23),24,heading,724,48);
        noticeTitle.color=blue;
        noticeDetail=Label(banner,"",new Vector2(0,-23),18,body,712,48);
        banner.gameObject.SetActive(false);
        SceneManager.sceneLoaded+=SceneLoaded;
        Refresh();
    }

    private void OnDestroy() { SceneManager.sceneLoaded-=SceneLoaded; if(Instance==this) Instance=null; }
    private void SceneLoaded(Scene scene,LoadSceneMode mode) { noticeTime=0; Refresh(); }
    private void Refresh()
    {
        fullStamina=FindFirstObjectByType<PlayerStamina>();
        simpleStamina=FindFirstObjectByType<PlayerStaminaSimple>();
    }
    public void Notify(string title,string detail,float seconds=5)
    {
        noticeTitle.text=title; noticeDetail.text=detail; noticeTime=seconds;
    }
    private void LateUpdate()
    {
        var manager=BookManager.Instance;
        bool visible=manager!=null && manager.QuestionsReady && !GameInterface.BlocksInput && SceneManager.GetActiveScene().name!="UriMenu";
        root.gameObject.SetActive(visible);
        if(!visible) return;
        books.text=$"{manager.GetBooksCollected():00} / {manager.minBooksToWin:00}";
        errors.text=$"{manager.GetErrors():00} / {manager.maxErrors:00}";
        Fill(booksFill,(float)manager.GetBooksCollected()/manager.minBooksToWin);
        Fill(errorsFill,(float)manager.GetErrors()/manager.maxErrors);
        float energy=fullStamina!=null ? fullStamina.GetStaminaPercent() : simpleStamina!=null ? simpleStamina.GetStaminaPercent() : 0;
        stamina.text=fullStamina!=null || simpleStamina!=null ? "SHIFT · CORRER" : "—";
        Fill(staminaFill,energy);
        timer.transform.parent.gameObject.SetActive(TimerManager.Instance!=null);
        if(TimerManager.Instance!=null) timer.text=TimerManager.Instance.GetFormattedTime();
        banner.gameObject.SetActive(noticeTime>0);
        if(noticeTime>0) noticeTime-=Time.unscaledDeltaTime;
    }
    private void Card(string title,float x,out TMP_Text value,out RectTransform fill)
    {
        var card=Rect(title,root,new Vector2(170,68),new Vector2(x,-28));
        card.anchorMin=card.anchorMax=card.pivot=new Vector2(0,1);
        Panel(card);
        Label(card,title,new Vector2(0,18),13,body,150,24);
        value=Label(card,"",new Vector2(0,-6),title=="FÔLEGO" ? 13 : 22,heading,150,44);
        var track=Rect("Trilho",card,new Vector2(142,2),new Vector2(0,-26));
        track.gameObject.AddComponent<Image>().color=new Color(.12f,.22f,.36f);
        fill=Rect("Progresso",track,new Vector2(142,2),Vector2.zero);
        fill.anchorMin=fill.anchorMax=fill.pivot=new Vector2(0,.5f);
        var image=fill.gameObject.AddComponent<Image>();image.color=blue;image.raycastTarget=false;
    }
    private void Fill(RectTransform fill,float amount) { fill.sizeDelta=new Vector2(142*Mathf.Clamp01(amount),2); }
    private void Panel(RectTransform rect)
    {
        var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.008f,.02f,.045f,.88f);image.raycastTarget=false;
        var line=Rect("Linha",rect,new Vector2(rect.sizeDelta.x,2),Vector2.zero);
        line.anchorMin=line.anchorMax=line.pivot=new Vector2(.5f,1);
        var border=line.gameObject.AddComponent<Image>();border.color=blue;border.raycastTarget=false;
    }
    private TMP_Text Label(Transform parent,string text,Vector2 position,float size,TMP_FontAsset font,float width=220,float height=40)
    {
        var label=Rect(text,parent,new Vector2(width,height),position).gameObject.AddComponent<TextMeshProUGUI>();
        label.font=font;label.text=text;label.fontSize=size;label.color=pale;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
        return label;
    }
    private RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 position)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.sizeDelta=size;rect.anchoredPosition=position;return rect;
    }
}
