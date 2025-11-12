using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Referências de UI")]
    public GameObject quizPanel;
    public TMP_Text questionText;
    public Button[] optionButtons;
    public TMP_Text feedbackText; // Opcional: texto para mostrar "ACERTOU!" ou "ERROU!"

    [Header("Player")]
    public MonoBehaviour playerMovement;
    public MonoBehaviour playerLook;

    private BookQuiz currentBook;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Múltiplos QuizManagers na cena! Destruindo o duplicado.");
            Destroy(gameObject);
            return;
        }
        
        if (quizPanel != null)
            quizPanel.SetActive(false);
        else
            Debug.LogError("QuizPanel não está atribuído no QuizManager!");
    }

    public void OpenQuiz(BookQuiz book)
    {
        currentBook = book;
        quizPanel.SetActive(true);
        
        // Esconder feedback anterior
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovement)
            playerMovement.enabled = false;
        if (playerLook)
            playerLook.enabled = false;

        questionText.text = book.question;
        
        Debug.Log($"=== Abrindo Quiz ===");
        Debug.Log($"Pergunta: {book.question}");
        Debug.Log($"Resposta Correta Index: {book.correctIndex}");

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            string optionText = book.options[i];
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = optionText;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index));
            
            Debug.Log($"Botão {i}: {optionText} {(i == book.correctIndex ? "✓ CORRETO" : "")}");
        }
        Debug.Log($"===================");
    }

    public void Answer(int index)
    {
        if (currentBook == null)
        {
            Debug.LogError("currentBook é null!");
            return;
        }
        
        // Desabilitar botões para evitar cliques múltiplos
        foreach (Button btn in optionButtons)
        {
            btn.interactable = false;
        }
        
        Debug.Log($"Resposta selecionada: {index}, Resposta correta: {currentBook.correctIndex}");
        
        // Processar resposta - isso vai chamar OnCorrectAnswer ou OnWrongAnswer
        currentBook.Answer(index);
        
        // Fechar quiz IMEDIATAMENTE (sem delay)
        StartCoroutine(CloseQuizAfterDelay(0f));
    }
    
    IEnumerator CloseQuizAfterDelay(float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        
        quizPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement)
            playerMovement.enabled = true;
        if (playerLook)
            playerLook.enabled = true;
            
        // Reabilitar botões para próximo quiz
        foreach (Button btn in optionButtons)
        {
            btn.interactable = true;
        }
    }

    public void OnCorrectAnswer(BookQuiz book)
    {
        Debug.Log($"✓ ACERTOU! Livro: {book.name}");
        
        // Mostrar feedback visual se existir
        if (feedbackText != null)
        {
            feedbackText.text = "✓ CORRETO!";
            feedbackText.color = Color.green;
            feedbackText.gameObject.SetActive(true);
        }
        
        // Notificar o BookManager
        if (BookManager.Instance != null)
        {
            BookManager.Instance.OnBookCorrect();
        }
        
        // Fazer o livro sumir imediatamente
        if (book != null && book.gameObject != null)
        {
            book.gameObject.SetActive(false);
            Destroy(book.gameObject, 0.1f);
        }
    }

    public void OnWrongAnswer(BookQuiz book)
    {
        Debug.Log($"✗ ERROU! Livro: {book.name} - Inimigo alertado!");
        
        // Mostrar feedback visual se existir
        if (feedbackText != null)
        {
            feedbackText.text = "✗ ERRADO!";
            feedbackText.color = Color.red;
            feedbackText.gameObject.SetActive(true);
        }
        
        // Notificar o BookManager
        if (BookManager.Instance != null)
        {
            BookManager.Instance.OnBookWrong();
        }
        
        // Fazer o livro sumir
        if (book != null && book.gameObject != null)
        {
            book.gameObject.SetActive(false);
            Destroy(book.gameObject, 0.1f);
        }
        
        // Alertar o inimigo (só se não for o 4º erro)
        if (BookManager.Instance == null || BookManager.Instance.GetErrors() <= 3)
        {
            StartCoroutine(EnemyAlert());
        }
    }

    IEnumerator EnemyAlert()
    {
        EnemyAI enemy = FindObjectOfType<EnemyAI>();
        if (enemy != null)
        {
            Debug.Log("🚨 Inimigo foi ativado e está perseguindo!");
            enemy.ForceChasePlayer(30f);
        }
        else
        {
            Debug.LogWarning("⚠️ Nenhum inimigo encontrado na cena!");
        }
        yield return null;
    }
    
    // Método público para fechar o quiz forçadamente (usado pelo GameOver)
    public void ForceCloseQuiz()
    {
        Debug.Log("Quiz fechado forçadamente!");
        
        StopAllCoroutines();
        
        if (quizPanel != null)
            quizPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovement)
            playerMovement.enabled = true;
        if (playerLook)
            playerLook.enabled = true;
            
        // Reabilitar botões
        foreach (Button btn in optionButtons)
        {
            if (btn != null)
                btn.interactable = true;
        }
    }
}
