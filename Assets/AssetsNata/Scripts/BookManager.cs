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
    public float baseChaseSpeed = 0.75f;
    public float basePatrolSpeed = 0.375f;
    public float speedIncreasePerCorrect = 0.05f;
    public float speedIncreasePerError = 0.1f;
    public float bonusSpeedEveryTwoCorrects = 0.05f;
    public float finalChaseSpeed = 1.1f; // Velocidade no 4º erro (mais lenta, visível)

    private int booksCollected = 0;
    private int errors = 0;
    private bool gameOverShown = false; // Flag para evitar game over múltiplo

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
        // Resetar flag de game over (importante para reiniciar cenas)
        gameOverShown = false;
        
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
        
        // Aumentar velocidade do inimigo (CHASE e PATROL)
        if (enemy != null)
        {
            // Base: +0.5 por acerto
            float speedIncrease = booksCollected * speedIncreasePerCorrect;
            
            // Bônus a cada 2 acertos: +1 adicional (+0.5 do bônus)
            int bonusCount = booksCollected / 2; // Divisão inteira (2 acertos = 1 bônus, 4 acertos = 2 bônus, etc)
            float bonusSpeed = bonusCount * bonusSpeedEveryTwoCorrects;
            
            // Atualizar CHASE SPEED
            float newChaseSpeed = baseChaseSpeed + speedIncrease + bonusSpeed;
            enemy.chaseSpeed = newChaseSpeed;
            
            // Atualizar PATROL SPEED (mesma proporção)
            float newPatrolSpeed = basePatrolSpeed + speedIncrease + bonusSpeed;
            enemy.patrolSpeed = newPatrolSpeed;
            
            if (bonusCount > 0 && booksCollected % 2 == 0)
            {
                Debug.Log($"🎉 BÔNUS! A cada 2 acertos: +{bonusSpeedEveryTwoCorrects} velocidade extra!");
            }
            
            Debug.Log($"🏃 Chase Speed: {newChaseSpeed} | 🚶 Patrol Speed: {newPatrolSpeed}");
            Debug.Log($"   (Base Chase: {baseChaseSpeed}, Base Patrol: {basePatrolSpeed} + Acertos: {speedIncrease} + Bônus: {bonusSpeed})");
        }
        
        UpdateUI();
        CheckWinCondition();
    }

    public void OnBookWrong()
    {
        errors++;
        
        Debug.Log($"✗ Livro errado! Total de erros: {errors}/{maxErrors + 1}");
        
        // Aumentar velocidade do inimigo por erro (CHASE e PATROL)
        if (enemy != null)
        {
            float errorSpeedIncrease = errors * speedIncreasePerError;
            
            // Recalcular velocidade total (acertos + erros + bônus)
            float correctSpeedIncrease = booksCollected * speedIncreasePerCorrect;
            int bonusCount = booksCollected / 2;
            float bonusSpeed = bonusCount * bonusSpeedEveryTwoCorrects;
            
            // Atualizar CHASE SPEED
            float newChaseSpeed = baseChaseSpeed + correctSpeedIncrease + bonusSpeed + errorSpeedIncrease;
            enemy.chaseSpeed = newChaseSpeed;
            
            // Atualizar PATROL SPEED (mesma proporção)
            float newPatrolSpeed = basePatrolSpeed + correctSpeedIncrease + bonusSpeed + errorSpeedIncrease;
            enemy.patrolSpeed = newPatrolSpeed;
            
            Debug.Log($"⚠️ Velocidade aumentada por ERRO!");
            Debug.Log($"   🏃 Chase Speed: {newChaseSpeed} | 🚶 Patrol Speed: {newPatrolSpeed}");
            Debug.Log($"   (+{speedIncreasePerError} por erro)");
            
            // NOVO: Ativar always chase após 2 erros (opcional)
            if (errors >= 2)
            {
                enemy.SetAlwaysChase(true);
                Debug.Log("🚨 2+ erros! Inimigo agora persegue SEMPRE!");
            }
        }
        
        UpdateUI();
        
        // 4º erro = game over com velocidade visível
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

    public void InstantGameOver()
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

        Debug.Log("💀 TEMPO ACABOU! Inimigo vindo direto até você!");

        // Garantir que o tempo está rodando
        if (Time.timeScale != 1f)
        {
            Debug.Log($"⏱️ Resetando Time.timeScale de {Time.timeScale} para 1f");
            Time.timeScale = 1f;
        }

        // NÃO teleportar, deixar ele vir da posição atual
        // O player pode VER ele chegando
        
        float speed = finalChaseSpeed; // Velocidade configurável, visível (padrão: 15)
        float duration = 60f; // 60 segundos para pegar o player
        float elapsed = 0f;
        
        Debug.Log($"🏃 Inimigo perseguindo a {speed} unidades/seg por {duration}s (você pode vê-lo chegando!)");

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
                if (distance < 5f)
                {
                    Debug.Log("💀 Inimigo pegou o player!");
                    // Pequeno delay antes de mostrar game over
                    yield return new WaitForSeconds(0.5f);
                    DirectGameOver();
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Se não pegou em 60 segundos, game over mesmo assim
        Debug.Log("⏱️ Tempo esgotado! Inimigo te alcançou!");
        DirectGameOver();
    }

    void DirectGameOver()
    {
        // Evitar chamar game over múltiplas vezes
        if (gameOverShown)
        {
            Debug.LogWarning("⚠️ Game Over já foi chamado! Ignorando...");
            return;
        }

        gameOverShown = true;
        Debug.Log("💀 GAME OVER! Mostrando tela de retry...");
        
        // Garantir que tempo está rodando para não travar
        if (Time.timeScale != 1f)
        {
            Debug.Log($"⏱️ Garantindo Time.timeScale = 1f para mostrar UI");
            Time.timeScale = 1f;
        }
        
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

    // Chamado quando o tempo acabar (TimerManager)
    public void OnTimeUp()
    {
        Debug.LogError("💀 TEMPO ACABOU! Inimigo vindo infinitamente!");
        
        // Chamar InstantChasePlayer para o inimigo vir infinitamente
        InstantGameOver();
    }

    // Getters para outras classes
    public int GetBooksCollected() => booksCollected;
    public int GetErrors() => errors;
    public int GetRemainingBooks() => totalBooks - booksCollected - errors;
}
