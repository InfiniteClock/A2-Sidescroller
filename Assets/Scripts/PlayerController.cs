using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float topSpeed;
    public float accelSpeed;
    public float decelTime;
    public float AirDrag;
    public float AirTime;
    private float jumpForce;
    private float gravity;

    [Space(10)]
    public float apexHeight;
    public float apexTime;
    [Space(10)]

    public Vector2 boxSize;
    public float boxOffset;
    public LayerMask nonGround;

    private Coroutine jumping;
    private Rigidbody2D rb;
    private Vector2 playerInput;
    private FacingDirection lastDirection;
    public enum FacingDirection
    {
        left, right
    }

    private void OnValidate()
    {
        gravity = 2 * apexHeight / (apexTime * apexTime);
        jumpForce = 2 * apexHeight / apexTime;
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
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            if (IsGrounded())
            {
                playerInput += Vector2.up;
            }
        }
    }
    private IEnumerator Jump()
    {
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

        // Buffer time hang time
        yield return new WaitForSeconds(apexTime);
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.gravityScale = 0;
        yield return new WaitForSeconds(AirTime);
        rb.gravityScale = gravity;
    }
    private void MovementUpdate(Vector2 playerInput)
    {   
        // Decelerate the player if input direction is 0
        if (playerInput.x == 0)
        {
            if (IsGrounded())
            {
                rb.velocity += new Vector2(-rb.velocity.x / decelTime, 0);
            }
            else
            {
                rb.velocity += new Vector2(-rb.velocity.x / AirDrag, 0);
            }
        }
        // Accelerate the player in intended direction towards top speed
        else if (rb.velocity.x < topSpeed && rb.velocity.x > -topSpeed)
        {
            rb.AddForce(new Vector2(playerInput.x * accelSpeed, 0));
        }

        if (playerInput.y > 0)
        {
            jumping = StartCoroutine(Jump());

            // Jump will use impulse mode for a snappier jump
            //rb.AddForce(new Vector2(0, playerInput.y * jumpForce), ForceMode2D.Impulse);
        }
    }

    public bool IsWalking()
    {
        if (playerInput.x != 0)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        // Display the boxcast in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position - transform.up * boxOffset, boxSize);
    }
    public bool IsGrounded()
    {
        // Use a boxcast to detect ground beneath the player
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, boxOffset, nonGround))
        {
            rb.gravityScale = gravity;
            return true;
        }
        // Returns false if the boxcast fails to detect ground
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        if (playerInput.x < 0)
        {
            lastDirection = FacingDirection.left;
            return FacingDirection.left;
        }
        else if (playerInput.x > 0) 
        {
            lastDirection = FacingDirection.right;
            return FacingDirection.right;
        }
        else
        {
            return lastDirection;
        }
        
    }
}
