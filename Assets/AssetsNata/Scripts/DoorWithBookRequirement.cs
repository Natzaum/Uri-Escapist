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
    [Tooltip("Nome da cena para carregar (ex: andar2)")]
    public string targetSceneName = "andar2";

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

    void Start()
    {
        booksRequired = GameDifficulty.BooksRequired(MenuPrincipal.GameMode);
        if (feedbackText != null) feedbackText.enabled = false;
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

        var manager = BookManager.Instance;
        booksRequired = manager != null ? manager.minBooksToWin : GameDifficulty.BooksRequired(MenuPrincipal.GameMode);
        if (manager == null || !manager.QuestionsReady)
        {
            GameHud.Instance?.Notify("Aguarde", "As perguntas deste andar ainda estão sendo preparadas.");
            return;
        }
        if (manager.CanProgress())
        {
            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                GameHud.Instance?.Notify("Passagem indisponível", "Não foi possível encontrar o próximo andar. Verifique as cenas do jogo.");
                return;
            }
            isTransitioning = true;
            GameHud.Instance?.Notify("Andar superado", targetSceneName == "andar2"
                ? "O próximo desafio espera por você. Avançando ao segundo andar…"
                : "O caminho de volta está aberto. Retornando para a saída principal…", 3);
            Invoke(nameof(LoadScene), 2f);
        }
        else
        {
            int missing = Mathf.Max(0, booksRequired - manager.GetBooksCollected());
            GameHud.Instance?.Notify("Passagem selada", $"Acerte mais {missing} livro(s) para avançar. Progresso: {manager.GetBooksCollected()}/{booksRequired}.", messageDuration);
        }
    }

    void LoadScene()
    {
        if (!GameInterface.Instance.IsTerminal)
        {
            DoorGameEnd.MarkFloorCompleted(SceneManager.GetActiveScene().name);
            GameInterface.Instance.LoadScene(targetSceneName);
        }
        else
            isTransitioning = false;
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
