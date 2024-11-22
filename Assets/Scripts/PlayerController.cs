using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float topSpeed;
    public float accelSpeed;
    public float decelTime;
    public float AirDrag;
    public float AirTime;
    public float terminalSpeed;
    public float coyoteTime;

    [Space(10)]
    public float apexHeight;
    public float apexTime;
    [Space(10)]

    public Vector2 boxSize;
    public float boxOffset;
    public LayerMask nonGround;

    private float cTimer;
    private float jumpVelocity;
    private float gravity;
    private float gravityScale;
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
        gravity = -2 * apexHeight / (apexTime * apexTime);
        jumpVelocity = 2 * apexHeight / apexTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
    }

    private void FixedUpdate()
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
            if (IsGrounded() || cTimer < coyoteTime)
            {
                playerInput += Vector2.up;
                cTimer += coyoteTime;   // Removes ability for coyote time to work
            }
        }

        // run the coyote time timer when it is less than the max time
        if(cTimer < coyoteTime)
        {
            cTimer += Time.deltaTime;
        }
        if (IsGrounded())
        {
            // Keep the coyote timer reset to 0 while on ground
            cTimer = 0;
        }

        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        MovementUpdate(playerInput);
    }
    
    private void MovementUpdate(Vector2 direction)
    {   
        // Decelerate the player if input direction is 0
        if (direction.x == 0)
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
        else
        {
            rb.AddForce(new Vector2(direction.x * accelSpeed, 0));

            // Prevent surpassing topsSpeed
            if(rb.velocity.x > topSpeed)
            {
                rb.velocity = new Vector2(topSpeed, rb.velocity.y);
            }
            if (rb.velocity.x < -topSpeed)
            {
                rb.velocity = new Vector2(-topSpeed, rb.velocity.y);
            }
        }

        if (direction.y > 0)
        {
            if (jumping != null)
            {
                StopCoroutine(jumping);
            }
            jumping = StartCoroutine(Jump());
        }
        if (rb.velocity.y < -terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -terminalSpeed);
        }
    }
    private IEnumerator Jump()
    {
        rb.gravityScale = 0;

        for (float timer = Time.deltaTime; timer < apexTime; timer += Time.deltaTime)
        {
            rb.velocity = new Vector2(rb.velocity.x, gravity * timer + jumpVelocity);
            yield return null;
        }
        rb.velocity = new Vector2(rb.velocity.x, 0);

        yield return new WaitForSeconds(AirTime);
        rb.gravityScale = gravityScale;
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
