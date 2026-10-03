using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Porta de conclusão que mostra mensagem de parabéns
/// Somente funciona após completar o andar obrigatório
/// </summary>
public class DoorGameEnd : MonoBehaviour
{
    [Header("Referências")]
    public GameObject playerObject;

    [Header("Requisitos")]
    [Tooltip("Nome da cena que deve ser concluída antes")]
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
    private static readonly HashSet<string> completedFloors = new HashSet<string>();
    public static bool HasCompletedFloor(string scene) => completedFloors.Contains(scene);
    public static void MarkFloorCompleted(string scene) => completedFloors.Add(scene);

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
        Debug.Log($"   Requer conclusão de: {requiredSceneName}");
        Debug.Log($"   Status: {(HasCompletedFloor(requiredSceneName) ? "✅ CONCLUÍDO" : "❌ PENDENTE")}");
    }

    void OnTriggerEnter(Collider other)
    {
        bool isPlayer = (player != null && other.gameObject == player) || 
                       other.CompareTag("Player") || 
                       other.name == "Player";

        if (!isPlayer || gameEnded || GameInterface.Instance.IsTerminal)
            return;

        // Verificar se concluiu a cena obrigatória
        if (!HasCompletedFloor(requiredSceneName))
        {
            Debug.LogWarning($"❌ Acesso negado! Você precisa concluir o segundo andar primeiro!");
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
        GameInterface.Instance.ShowResult(true, "Você superou os dois andares e conquistou sua saída.\nO conhecimento foi sua chave para a liberdade.");
    }

    void ShowDeniedMessage()
    {
        GameHud.Instance?.Notify("A saída ainda está selada", "Conclua o segundo andar e retorne pela passagem para liberar a porta principal.", 6);
    }

    // Compatibilidade com SceneVisitMarker: visitar não equivale a concluir.
    public static void SetVisitedRequiredScene() { }

    public static void ResetRequirement() => completedFloors.Clear();

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = HasCompletedFloor(requiredSceneName) ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
}

