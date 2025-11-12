using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void ShowGameOver()
    {
        Debug.Log("💀 ShowGameOver chamado!");
        
        // Fechar quiz se estiver aberto
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.ForceCloseQuiz();
            Debug.Log("✓ Quiz fechado pelo Game Over");
        }
        
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("✓ Game Over UI ativada");
        }
        else
        {
            Debug.LogError("⚠️ Game Over UI não está atribuída!");
        }

        // NÃO PAUSAR O TEMPO! Deixar o inimigo vindo enquanto mostra UI
        // Time.timeScale = 0f;
        Debug.Log("⚠️ Tempo NÃO foi pausado! Inimigo continua vindo!");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("✓ Cursor liberado para clicar em Retry");
    }

    public void Retry()
    {
        Debug.Log("🔄 Retry pressionado!");
        Debug.Log("⏱️ Aguardando 1 segundo antes de recarregar...");
        
        // Garantir que o tempo volte ao normal ANTES de recarregar
        Time.timeScale = 1f;

        // Fechar quiz se ainda estiver aberto
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.ForceCloseQuiz();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Delay para garantir que a UI fecha corretamente
        Invoke(nameof(ReloadScene), 1f);
    }

    void ReloadScene()
    {
        Debug.Log("🔄 Recarregando cena agora...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Sair do jogo!");
        Application.Quit();
    }
}
