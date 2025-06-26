using UnityEngine;

public class AnimationContoller : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

    }

    public void PlayAnimation(string name)
    {
        animator.SetTrigger(name);
    }
}
