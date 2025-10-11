using UnityEngine;

public class BookQuiz : MonoBehaviour
{
    [Header("Pergunta e respostas")]
    [TextArea(2, 4)]
    public string question;
    public string[] options = new string[4];
    public int correctIndex;

    private bool isAnswered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isAnswered)
        {
            QuizManager.Instance.OpenQuiz(this);
        }
    }

    public void Answer(int index)
    {
        isAnswered = true;
        if (index == correctIndex)
        {
            QuizManager.Instance.OnCorrectAnswer(this);
        }
        else
        {
            QuizManager.Instance.OnWrongAnswer(this);
        }
    }
}
