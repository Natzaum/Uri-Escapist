using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        float move = Input.GetAxis("Vertical");

        if (move != 0)
            animator.SetBool("isWalking", true);
        else
            animator.SetBool("isWalking", false);
    }
}
