using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Porta que só abre se o player coletou o número mínimo de livros
/// Integra com BookManager para validação
/// </summary>
public class DoorWithBookRequirement : MonoBehaviour
{
    [Header("Requisito de Livros")]
    [Tooltip("Número mínimo de livros necessários")]
    public int booksRequired = 7;

    [Header("Cena de Destino")]
    [Tooltip("Nome da cena para carregar (ex: cena_ruan)")]
    public string targetSceneName = "cena_ruan";

    [Header("Referências")]
    [Tooltip("GameObject do player - arraste aqui")]
    public GameObject playerObject;

    [Header("Feedback Visual")]
    [Tooltip("Mensagem quando falta livros")]
    public TextMeshProUGUI feedbackText;
    
    [Tooltip("Duração da mensagem em segundos")]
    public float messageDuration = 3f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private bool isTransitioning = false;
    private Collider doorCollider;
    private GameObject player;
    private float messageTimer = 0f;

    void Start()
    {
        doorCollider = GetComponent<Collider>();
        
        if (doorCollider == null)
        {
            Debug.LogError("❌ DoorWithBookRequirement precisa ter um Collider com isTrigger = true!");
            return;
        }

        if (!doorCollider.isTrigger)
        {
            Debug.LogError("❌ O Collider DEVE ter 'Is Trigger' marcado!");
            doorCollider.isTrigger = true;
            Debug.Log("✓ Is Trigger ativado automaticamente!");
        }

        // Encontrar player
        if (playerObject != null)
        {
            player = playerObject;
            if (showDebugInfo)
                Debug.Log($"👤 Player arrastado: {player.name}");
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && showDebugInfo)
                Debug.Log($"👤 Player encontrado: {player.name}");
            else
                Debug.LogWarning("⚠️ Player não encontrado! Arraste o Player no Inspector!");
        }

        if (showDebugInfo)
        {
            Debug.Log($"🚪 Porta ativada!");
            Debug.Log($"📚 Requisito: {booksRequired} livros");
            Debug.Log($"🎯 Destino: {targetSceneName}");
        }
    }

    void Update()
    {
        // Gerenciar duração da mensagem
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Verificar se é o player
        bool isPlayer = (player != null && other.gameObject == player) || 
                       other.CompareTag("Player") || 
                       other.name == "Player";

        if (!isPlayer)
            return;

        if (isTransitioning)
            return;

        // Verificar se tem livros suficientes
        int booksCollected = GetBooksCollected();

        if (booksCollected >= booksRequired)
        {
            // ✅ Pode passar!
            Debug.Log($"✓ Player tem {booksCollected} livros! (Requisito: {booksRequired})");
            Debug.Log($"🌀 Carregando cena: {targetSceneName}");
            
            ShowMessage($"✓ Parabéns! Você completou o desafio!", 2f);
            
            isTransitioning = true;
            Invoke(nameof(LoadScene), 1f);
        }
        else
        {
            // ❌ Não pode passar
            int livrosFaltando = booksRequired - booksCollected;
            Debug.Log($"❌ Player tem apenas {booksCollected} livros! Faltam {livrosFaltando}");
            
            string message = $"❌ Você precisa de {booksRequired} livros!\nTem: {booksCollected}/{booksRequired}\nFaltam: {livrosFaltando}";
            ShowMessage(message, messageDuration);
        }
    }

    int GetBooksCollected()
    {
        // Tentar pegar do BookManager
        if (BookManager.Instance != null)
        {
            int collected = BookManager.Instance.GetBooksCollected();
            if (showDebugInfo)
                Debug.Log($"📖 Livros coletados (BookManager): {collected}");
            return collected;
        }
        else
        {
            Debug.LogWarning("⚠️ BookManager não encontrado! Retornando 0");
            return 0;
        }
    }

    void ShowMessage(string message, float duration)
    {
        if (feedbackText == null)
        {
            Debug.Log(message);
            return;
        }

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        messageTimer = duration;
    }

    void LoadScene()
    {
        Debug.Log($"🌀 Carregando cena: {targetSceneName}");
        
        // Garantir que o tempo está normal
        if (Time.timeScale != 1f)
        {
            Debug.Log($"⏱️ Resetando Time.timeScale de {Time.timeScale} para 1f");
            Time.timeScale = 1f;
        }
        
        SceneManager.LoadScene(targetSceneName);
    }

    void OnDrawGizmos()
    {
        // Desenhar a área de transição no editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
}
