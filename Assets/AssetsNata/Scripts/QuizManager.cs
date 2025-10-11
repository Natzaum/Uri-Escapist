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

    [Header("Player")]
    public MonoBehaviour playerMovement;
    public MonoBehaviour playerLook;

    private BookQuiz currentBook;

    private void Awake()
    {
        Instance = this;
        quizPanel.SetActive(false);
    }

    public void OpenQuiz(BookQuiz book)
    {
        currentBook = book;
        quizPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovement)
            playerMovement.enabled = false;
        if (playerLook)
            playerLook.enabled = false;

        questionText.text = book.question;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = book.options[i];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => Answer(index));
        }
    }

    public void Answer(int index)
    {
        quizPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement)
            playerMovement.enabled = true;
        if (playerLook)
            playerLook.enabled = true;

        currentBook.Answer(index);
    }

    public void OnCorrectAnswer(BookQuiz book)
    {
        Debug.Log($"Acertou a questão do livro: {book.name}");
        Destroy(book.gameObject);
    }

    public void OnWrongAnswer(BookQuiz book)
    {
        Debug.Log($"Errou a questão do livro: {book.name}");
        StartCoroutine(EnemyAlert());
    }

    IEnumerator EnemyAlert()
    {
        EnemyAI enemy = FindObjectOfType<EnemyAI>();
        if (enemy != null)
        {
            enemy.ForceChasePlayer(30f);
        }
        yield return null;
    }
}
