using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Porta de conclusão que mostra mensagem de parabéns
/// Somente funciona se o player já visitou o andar2
/// </summary>
public class DoorGameEnd : MonoBehaviour
{
    [Header("Referências")]
    public GameObject playerObject;

    [Header("Requisitos")]
    [Tooltip("Nome da cena que deve ser visitada antes")]
    public string requiredSceneName = "andar2";

    [Header("Mensagem")]
    [TextArea(3, 5)]
    public string victoryMessage = "Parabéns você completou a faculdade";
    
    [TextArea(2, 3)]
    public string deniedMessage = "você precisa completar o 2 andar primeiro";
    
    [Range(30, 100)]
    public int fontSize = 60;

    [Header("Cores")]
    public Color messageColor = Color.white;
    public Color deniedColor = Color.red;

    private bool gameEnded = false;
    private GameObject player;
    private static bool visitedRequiredScene = false; // Flag persistente entre cenas

    void Start()
    {
        Collider col = GetComponent<Collider>();
        
        if (col == null)
        {
            Debug.LogError("❌ DoorGameEnd precisa ter um Collider!");
            return;
        }

        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }

        if (playerObject != null)
            player = playerObject;
        else
            player = GameObject.FindGameObjectWithTag("Player");

        Debug.Log($"🎓 Porta de conclusão ativada!");
        Debug.Log($"   Requer visita a: 2");
        Debug.Log($"   Status: {(visitedRequiredScene ? "✅ VISITADO" : "❌ NÃO VISITADO")}");
    }

    void Update()
    {
        if (gameEnded && Input.anyKeyDown)
        {
            Debug.Log("👋 Saindo do jogo...");
            Application.Quit();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        bool isPlayer = (player != null && other.gameObject == player) || 
                       other.CompareTag("Player") || 
                       other.name == "Player";

        if (!isPlayer || gameEnded)
            return;

        // Verificar se visitou a cena obrigatória
        if (!visitedRequiredScene)
        {
            Debug.LogWarning($"❌ Acesso negado! Você precisa visitar o 2 andar primeiro!");
            ShowDeniedMessage();
            return;
        }

        gameEnded = true;
        Debug.Log("🎉 PARABÉNS! Jogo concluído!");

        Time.timeScale = 0f;
        ShowVictoryMessage();
    }

    void ShowVictoryMessage()
    {
        // Criar Canvas
        GameObject canvasObj = new GameObject("VictoryCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.9f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Texto
        GameObject textObj = new GameObject("VictoryText");
        textObj.transform.SetParent(canvasObj.transform, false);

        TextMeshProUGUI victoryText = textObj.AddComponent<TextMeshProUGUI>();
        victoryText.text = victoryMessage;
        victoryText.alignment = TextAlignmentOptions.Center;
        victoryText.fontSize = fontSize;
        victoryText.color = messageColor;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4, 4);

        Debug.Log("✅ Mensagem de vitória exibida!");
        Debug.Log("👆 Clique em qualquer lugar para sair");
    }

    void ShowDeniedMessage()
    {
        // Criar Canvas
        GameObject canvasObj = new GameObject("DeniedCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Texto
        GameObject textObj = new GameObject("DeniedText");
        textObj.transform.SetParent(canvasObj.transform, false);

        TextMeshProUGUI deniedText = textObj.AddComponent<TextMeshProUGUI>();
        deniedText.text = deniedMessage;
        deniedText.alignment = TextAlignmentOptions.Center;
        deniedText.fontSize = fontSize;
        deniedText.color = deniedColor;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4, 4);

        // Remover mensagem após 3 segundos
        Destroy(canvasObj, 3f);

        Debug.Log("❌ Acesso negado! Mensagem exibida por 3 segundos");
    }

    // Método estático para registrar que visitou a cena obrigatória
    public static void SetVisitedRequiredScene()
    {
        visitedRequiredScene = true;
        Debug.Log("✅ Cena obrigatória visitada! Porta de conclusão agora está acessível!");
    }

    // Reset quando voltar à cena inicial (opcional)
    public static void ResetRequirement()
    {
        visitedRequiredScene = false;
        Debug.Log("🔄 Requisito resetado");
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = visitedRequiredScene ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
}

