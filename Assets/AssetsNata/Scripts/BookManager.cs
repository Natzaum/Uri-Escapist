using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance;

    [Header("Configurações")]
    public int totalBooks = 10;
    public int minBooksToWin = 7;
    public int maxErrors = 3; // 4º erro = game over instantâneo

    [Header("UI")]
    public TMP_Text booksCounterText;
    public TMP_Text errorsCounterText;

    [Header("Inimigo")]
    public EnemyAI enemy;
    public float baseChaseSpeed = 4f;
    public float speedIncreasePerCorrect = 0.5f;

    private int booksCollected = 0;
    private int errors = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
        
        // Contar quantos livros existem na cena
        BookQuiz[] books = FindObjectsOfType<BookQuiz>();
        totalBooks = books.Length;
        
        Debug.Log($"📚 BookManager iniciado: {totalBooks} livros na cena");
        Debug.Log($"🎯 Objetivo: Acertar no mínimo {minBooksToWin} livros");
        Debug.Log($"⚠️ Máximo de erros permitidos: {maxErrors}");
        
        UpdateUI();
    }

    public void OnBookCorrect()
    {
        booksCollected++;
        
        Debug.Log($"✓ Livro correto! Total: {booksCollected}/{totalBooks}");
        
        // Aumentar velocidade do inimigo gradualmente
        if (enemy != null)
        {
            float newSpeed = baseChaseSpeed + (booksCollected * speedIncreasePerCorrect);
            enemy.chaseSpeed = newSpeed;
            Debug.Log($"🏃 Velocidade do inimigo aumentada para: {newSpeed}");
        }
        
        UpdateUI();
        CheckWinCondition();
    }

    public void OnBookWrong()
    {
        errors++;
        
        Debug.Log($"✗ Livro errado! Total de erros: {errors}/{maxErrors + 1}");
        
        UpdateUI();
        
        // 4º erro = game over instantâneo
        if (errors > maxErrors)
        {
            Debug.Log("💀 MUITOS ERROS! Inimigo vindo à velocidade da luz!");
            InstantGameOver();
        }
    }

    void UpdateUI()
    {
        if (booksCounterText != null)
        {
            booksCounterText.text = $"Livros: {booksCollected}/{minBooksToWin}";
            
            // Cor verde se já atingiu o mínimo
            if (booksCollected >= minBooksToWin)
            {
                booksCounterText.color = Color.green;
            }
        }

        if (errorsCounterText != null)
        {
            errorsCounterText.text = $"Erros: {errors}/{maxErrors + 1}";
            
            // Cor vermelha conforme se aproxima do limite
            if (errors >= maxErrors)
            {
                errorsCounterText.color = Color.red;
            }
            else if (errors >= maxErrors - 1)
            {
                errorsCounterText.color = Color.yellow;
            }
        }
    }

    void CheckWinCondition()
    {
        // Verificar se coletou todos os livros OU atingiu o mínimo
        if (booksCollected >= minBooksToWin)
        {
            int remainingBooks = totalBooks - booksCollected - errors;
            
            if (remainingBooks <= 0)
            {
                // Não há mais livros para coletar
                Debug.Log("🎉 VITÓRIA! Acertou o mínimo de livros!");
                WinLevel();
            }
            else
            {
                Debug.Log($"✓ Já atingiu o mínimo! Pode continuar ou sair. ({remainingBooks} livros restantes)");
            }
        }
    }

    void WinLevel()
    {
        Debug.Log("🏆 NÍVEL COMPLETO!");
        // Aqui você pode:
        // - Carregar próxima cena
        // - Mostrar tela de vitória
        // - Etc
        
        // Exemplo:
        // SceneManager.LoadScene("NextLevel");
    }

    void InstantGameOver()
    {
        if (enemy != null)
        {
            // Fazer o inimigo vir MUITO rápido e atravessar paredes
            enemy.enabled = false; // Desabilitar comportamento normal
            
            StartCoroutine(InstantChasePlayer());
        }
        else
        {
            // Se não tem inimigo, game over direto
            DirectGameOver();
        }
    }

    System.Collections.IEnumerator InstantChasePlayer()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("Player não encontrado!");
            yield break;
        }

        Debug.Log("⚡ Inimigo vindo à velocidade da luz!");

        // Teleportar inimigo mais perto do player primeiro
        Vector3 directionToPlayer = (player.position - enemy.transform.position).normalized;
        enemy.transform.position = player.position - directionToPlayer * 20f;

        float speed = 100f; // MUITO rápido
        float duration = 5f; // 5 segundos para pegar o player
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (player != null && enemy != null)
            {
                // Mover diretamente em direção ao player, ignorando NavMesh
                Vector3 direction = (player.position - enemy.transform.position).normalized;
                enemy.transform.position += direction * speed * Time.deltaTime;

                // Rotacionar para olhar para o player
                enemy.transform.LookAt(player);

                // Verificar distância para game over
                float distance = Vector3.Distance(enemy.transform.position, player.position);
                if (distance < 3f)
                {
                    Debug.Log("💀 Inimigo pegou o player!");
                    DirectGameOver();
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Se não pegou em 5 segundos, game over mesmo assim
        DirectGameOver();
    }

    void DirectGameOver()
    {
        Debug.Log("💀 GAME OVER!");
        
        GameOverManager gom = FindObjectOfType<GameOverManager>();
        if (gom != null)
        {
            gom.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager não encontrado! Recarregando cena...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Método público para verificar se pode avançar
    public bool CanProgress()
    {
        return booksCollected >= minBooksToWin;
    }

    // Getters para outras classes
    public int GetBooksCollected() => booksCollected;
    public int GetErrors() => errors;
    public int GetRemainingBooks() => totalBooks - booksCollected - errors;
}
