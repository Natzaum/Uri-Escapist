using UnityEngine;

public class TestCharacterAnim : MonoBehaviour
{
    public Animator animator;

    private float timer = 0f;
    private bool isWalking = false;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 2f)
        {
            isWalking = !isWalking;
            animator.SetBool("isWalking", isWalking);
            timer = 0f;
        }
    }
}
