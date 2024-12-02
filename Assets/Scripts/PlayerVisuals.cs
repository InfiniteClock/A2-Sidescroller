using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private readonly int IdleHash = Animator.StringToHash("Idle");
    private readonly int WalkingHash = Animator.StringToHash("Walking");
    private readonly int JumpingHash = Animator.StringToHash("Jumping");
    private readonly int DeadHash = Animator.StringToHash("Dead");
    private readonly int Divehash = Animator.StringToHash("Dive");

    void FixedUpdate()
    {
        UpdateVisuals();

        switch (playerController.GetFacingDirection())
        {
            case PlayerController.FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.right:
                bodyRenderer.flipX = false;
                break;
        }
    }
    private void UpdateVisuals()
    {
        if (playerController.prevState != playerController.currentState)
        {
            switch (playerController.currentState)
            {
                case PlayerController.PlayerState.idle:
                    animator.CrossFade(IdleHash, 0f);
                    break;
                case PlayerController.PlayerState.walking:
                    animator.CrossFade(WalkingHash, 0f);
                    break;
                case PlayerController.PlayerState.jumping:
                    animator.CrossFade(JumpingHash, 0f);
                    break;
                case PlayerController.PlayerState.dead:
                    animator.CrossFade(DeadHash, 0f);
                    break;
                case PlayerController.PlayerState.groundPound:
                    animator.CrossFade(Divehash, 0f);
                    break;
            }
        }
    }
}
