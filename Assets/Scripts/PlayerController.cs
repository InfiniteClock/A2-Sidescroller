using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float topSpeed;
    public float accelSpeed;
    public float decelTime;

    private Rigidbody2D rb;
    private Vector2 playerInput;
    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        MovementUpdate(playerInput);
    }
    private void Update()
    {
        // Resets Vector2 for player input every frame, then determines intended direction from key presses
        // If both left and right are pressed, H movement should be 0
        playerInput = new Vector2();
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            playerInput += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            playerInput += Vector2.right;
        }
    }

    private void MovementUpdate(Vector2 playerInput)
    {   
        // Decelerate the player if input direction is 0
        if (playerInput.x == 0)
        {
            rb.velocity += new Vector2(-rb.velocity.x / decelTime, 0);
        }
        // Accelerate the player in intended direction towards top speed
        else if (rb.velocity.x < topSpeed && rb.velocity.x > -topSpeed)
        {
            rb.AddForce(new Vector2(playerInput.x * accelSpeed, 0));
        }
    }

    public bool IsWalking()
    {
        return false;
    }
    public bool IsGrounded()
    {
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        return FacingDirection.left;
    }
}
