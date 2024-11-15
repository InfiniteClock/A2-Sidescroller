using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float topSpeed;
    public float accelSpeed;

    private Rigidbody2D rb;
    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.

        // Resets Vector2 for player input every frame, then determines intended direction from key presses
        // If both left and right are pressed, H movement should be 0
        Vector2 playerInput = new Vector2();
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            playerInput += Vector2.left;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            playerInput += Vector2.right;
        }
        
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {   
        if (playerInput.x == 0)
        {
            rb.AddForce(new Vector2(0 - rb.velocity.x, rb.velocity.y));
        }
        else if (rb.velocity.x < topSpeed && rb.velocity.x > -topSpeed)
        {
            rb.AddForce(new Vector2((playerInput.x * accelSpeed), 0));
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
