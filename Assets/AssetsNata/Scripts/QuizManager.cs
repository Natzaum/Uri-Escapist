using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;
    // Referências antigas preservadas para compatibilidade com as cenas.
    public GameObject quizPanel;
    public TMP_Text questionText;
    public Button[] optionButtons;
    public TMP_Text feedbackText;
    public MonoBehaviour playerMovement;
    public MonoBehaviour playerLook;
    private BookQuiz currentBook;
    private bool answered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (quizPanel != null) quizPanel.SetActive(false);
    }

    public void OpenQuiz(BookQuiz book)
    {
        if (book == null || currentBook != null || GameInterface.BlocksInput ||
            BookManager.Instance == null || !BookManager.Instance.QuestionsReady) return;
        currentBook = book;
        answered = false;
        GameInterface.Instance.ShowQuiz(book, Answer);
    }

    public void Answer(int index)
    {
        if (currentBook == null || answered || index < 0 || index >= currentBook.options.Length) return;
        answered = true;
        BookQuiz book = currentBook;
        bool correct = index == book.correctIndex;
        string correctAnswer = book.options[book.correctIndex];
        book.Answer(index);
        currentBook = null;
        if (GameInterface.Instance.IsTerminal) return;
        GameInterface.Instance.ShowFeedback(correct, correctAnswer, () => GameInterface.Instance.Hide());
    }

    public void OnCorrectAnswer(BookQuiz book)
    {
        if (BookManager.Instance != null) BookManager.Instance.OnBookCorrect();
        RemoveBook(book);
    }

    public void OnWrongAnswer(BookQuiz book)
    {
        if (BookManager.Instance != null) BookManager.Instance.OnBookWrong();
        RemoveBook(book);
    }

    private void RemoveBook(BookQuiz book)
    {
        if (book == null) return;
        book.gameObject.SetActive(false);
        Destroy(book.gameObject);
    }

    public void CloseBook()
    {
        if (!GameDifficulty.CanLeaveBook || currentBook == null || answered ||
            GameInterface.Instance.State != GameInterface.ScreenState.Quiz) return;
        // A pergunta pertence ao BookQuiz e só é consumida ao responder.
        ForceCloseQuiz();
        GameInterface.Instance.Hide();
    }

    public void ForceCloseQuiz()
    {
        currentBook = null;
        answered = false;
        if (quizPanel != null) quizPanel.SetActive(false);
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
}
