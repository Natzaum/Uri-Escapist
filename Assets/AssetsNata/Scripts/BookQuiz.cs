using UnityEngine;

public class BookQuiz : MonoBehaviour
{
    [Header("Pergunta e respostas")]
    [TextArea(2, 4)]
    public string question;
    public string[] options = new string[4];
    public int correctIndex;

    [HideInInspector]
    public int remoteQuestionId;
    
    [Header("Detecção")]
    public float detectionRadius = 3f; // Raio de detecção (ajusta automaticamente com escala)
    public KeyCode interactKey = KeyCode.E; // Tecla para interagir
    
    private bool isAnswered = false;
    private bool playerInRange = false;
    private Transform playerTransform;

    private void Start()
    {
        // Validar configuração
        Debug.Log($"=== Book {name} Configuração ===");
        Debug.Log($"Pergunta: {question}");
        Debug.Log($"Opções: {string.Join(", ", options)}");
        Debug.Log($"Índice Correto: {correctIndex}");
        
        if (correctIndex < 0 || correctIndex >= options.Length)
        {
            Debug.LogError($"⚠️ ERRO: correctIndex ({correctIndex}) está fora do range! Deve ser 0-3");
        }
        
        // Ajustar o collider baseado na escala
        SphereCollider trigger = GetComponent<SphereCollider>();
        if (trigger != null)
        {
            // Ajustar o raio do trigger baseado na escala do objeto
            float scale = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            trigger.radius = detectionRadius / scale; // Dividir pela escala para manter o raio real
            trigger.isTrigger = true;
            
            Debug.Log($"Scale: {scale}, Trigger Radius: {trigger.radius}");
        }
        else
        {
            Debug.LogWarning($"Book {name} não tem SphereCollider! Adicionando um...");
            trigger = gameObject.AddComponent<SphereCollider>();
            trigger.radius = detectionRadius;
            trigger.isTrigger = true;
        }
        Debug.Log($"================================");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Book {name} - Trigger Enter com: {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Player") && !isAnswered)
        {
            playerInRange = true;
            playerTransform = other.transform;
            
            if (QuizManager.Instance != null)
            {
                Debug.Log($"📖 Abrindo quiz do livro: {name}");
                QuizManager.Instance.OpenQuiz(this);
            }
            else
            {
                Debug.LogError("QuizManager.Instance é null!");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
            Debug.Log($"Player saiu do alcance do livro {name}");
        }
    }

    public void Answer(int index)
    {
        isAnswered = true;
        
        Debug.Log($"BookQuiz.Answer chamado - Index: {index}, Correto: {correctIndex}");
        
        if (index == correctIndex)
        {
            Debug.Log("✓ Resposta CORRETA! Chamando OnCorrectAnswer...");
            if (QuizManager.Instance != null)
                QuizManager.Instance.OnCorrectAnswer(this);
            else
                Debug.LogError("QuizManager.Instance é null em OnCorrectAnswer!");
        }
        else
        {
            Debug.Log("✗ Resposta ERRADA! Chamando OnWrongAnswer...");
            if (QuizManager.Instance != null)
                QuizManager.Instance.OnWrongAnswer(this);
            else
                Debug.LogError("QuizManager.Instance é null em OnWrongAnswer!");
        }
    }
    
    public bool SetQuestionData(int questionId, string newQuestion, string[] newOptions, int newCorrectIndex)
    {
        if (string.IsNullOrWhiteSpace(newQuestion))
        {
            Debug.LogWarning($"Book {name}: pergunta remota vazia ignorada.");
            return false;
        }

        if (newOptions == null || newOptions.Length != 4)
        {
            Debug.LogWarning($"Book {name}: a pergunta remota precisa ter exatamente 4 alternativas.");
            return false;
        }

        if (newCorrectIndex < 0 || newCorrectIndex >= newOptions.Length)
        {
            Debug.LogWarning($"Book {name}: indice de resposta remota invalido ({newCorrectIndex}).");
            return false;
        }

        remoteQuestionId = questionId;
        question = newQuestion;
        options = (string[])newOptions.Clone();
        correctIndex = newCorrectIndex;

        Debug.Log($"Book {name} recebeu a pergunta remota #{remoteQuestionId}.");
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar área de detecção
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Linha para o player se estiver no alcance
        if (playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
