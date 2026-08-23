using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script para porta que leva a outra cena ao player encostar
/// </summary>
public class DoorTransition : MonoBehaviour
{
    [Header("Referências (Arraste aqui)")]
    [Tooltip("GameObject do player - arraste aqui")]
    public GameObject playerObject;

    [Header("Cena de Destino")]
    [Tooltip("Nome exato da cena (ex: andar1) - aparece em Assets/Scenes/")]
    public string targetSceneName = "andar1";

    [Header("Visual/Feedback")]
    [Tooltip("Mostrar mensagem ao encostar")]
    public bool showMessage = true;
    
    [Tooltip("Tempo em segundos antes de transicionar")]
    [Range(0f, 3f)]
    public float transitionDelay = 0f;

    private bool isTransitioning = false;
    private Collider doorCollider;
    private GameObject player;

    void Start()
    {
        doorCollider = GetComponent<Collider>();
        
        if (doorCollider == null)
        {
            Debug.LogError("❌ DoorTransition precisa ter um Collider com isTrigger = true!");
            return;
        }

        if (!doorCollider.isTrigger)
        {
            Debug.LogError("❌ O Collider DEVE ter 'Is Trigger' marcado!");
            doorCollider.isTrigger = true;
            Debug.Log("✓ Is Trigger ativado automaticamente!");
        }

        // Se playerObject foi arrastado, usar como referência
        if (playerObject != null)
        {
            player = playerObject;
            Debug.Log($"👤 Player arrastado: {player.name}");
        }
        else
        {
            // Procurar player automaticamente
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                Debug.Log($"👤 Player encontrado: {player.name}");
            else
                Debug.LogWarning("⚠️ Player não encontrado! Arraste o Player no Inspector!");
        }

        // Verificar se a cena existe
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("❌ Nome da cena não definido!");
            return;
        }

        Debug.Log($"🚪 Porta ativada! Levará para: {targetSceneName}");
    }

    void OnTriggerEnter(Collider other)
    {
        // Verificar se é o player por GameObject ou por Tag
        bool isPlayer = (player != null && other.gameObject == player) || 
                       other.CompareTag("Player") || 
                       other.name == "Player";

        if (isPlayer)
        {
            if (!isTransitioning)
            {
                Debug.Log($"✓ Player tocou na porta!");
                Debug.Log($"🌀 Carregando cena: {targetSceneName}");
                
                isTransitioning = true;
                
                // Transicionar após delay
                if (transitionDelay > 0)
                {
                    Invoke(nameof(LoadScene), transitionDelay);
                }
                else
                {
                    LoadScene();
                }
            }
        }
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
            Gizmos.color = isTransitioning ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
}
