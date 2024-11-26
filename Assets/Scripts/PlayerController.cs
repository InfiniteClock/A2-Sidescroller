using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal")]
    public float topSpeed;
    public float accelTime;
    public float decelTime;
    public float airDecelTime;

    [Header("Vertical")]
    public float terminalSpeed;
    public float coyoteTime;
    public float apexHeight;
    public float apexTime;

    [Header("Ground Checking")]
    public Vector2 boxSize;
    public float boxOffset;
    public LayerMask nonGround;

    [Header("Player State")]
    public PlayerState currentState = PlayerState.idle;
    public PlayerState prevState = PlayerState.idle;

    private float accelRate;
    private float decelRate;
    private float airDecelRate;
    private float jumpVelocity;
    private float gravity;
    private float currentGravity;
    private float cTimer;

    private bool isGrounded = false;
    private bool isDead = false;
    private Coroutine jumping;
    private Rigidbody2D rb;
    private Vector2 velocity;
    private FacingDirection currentDirection;
    
    public enum FacingDirection
    {
        left, right
    }
    public enum PlayerState
    {
        idle, walking, jumping, dead
    }

    private void OnValidate()
    {
        gravity = -2 * apexHeight / (apexTime * apexTime);
        currentGravity = gravity;
        jumpVelocity = 2 * apexHeight / apexTime;

        accelRate = topSpeed / accelTime;
        decelRate = topSpeed / decelTime;
        airDecelRate = topSpeed / airDecelTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void FixedUpdate()
    {
        prevState = currentState;

        CheckGround();
        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");

        if(isDead) currentState = PlayerState.dead;

        switch (currentState)
        {
            case PlayerState.dead:
                break;
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x != 0) currentState = PlayerState.walking;
                break;
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                break;
            case PlayerState.jumping:
                if (isGrounded && velocity.x != 0) currentState = PlayerState.walking;
                else if (isGrounded && velocity.x == 0) currentState = PlayerState.idle;
                break;

        }

        MovementUpdate(playerInput);
        JumpUpdate();
        rb.velocity = velocity;

        if (Input.GetKey(KeyCode.Space)) currentState = PlayerState.dead;
    }
    
    private void MovementUpdate(Vector2 direction)
    {
        if (direction.x < 0) currentDirection = FacingDirection.left;
        if (direction.x > 0) currentDirection = FacingDirection.right;

        if (direction.x != 0)
        {
            velocity.x += accelRate * direction.x * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -topSpeed, topSpeed);  // Clamps velocity to not surpass top speed in either direction
        }
        else
        {
            if (isGrounded)
            {
                if (velocity.x > 0)
                {
                    velocity.x -= decelRate * Time.deltaTime;
                    velocity.x = Mathf.Max(velocity.x, 0);      // Prevents decelerating to below 0
                }
                else if (velocity.x < 0)
                {
                    velocity.x += decelRate * Time.deltaTime;
                    velocity.x = Mathf.Min(velocity.x, 0);      // Prevents decelerating to above 0
                }
            }
            else
            {
                if (velocity.x > 0)
                {
                    velocity.x -= airDecelRate * Time.deltaTime;
                    velocity.x = Mathf.Max(velocity.x, 0);      // Prevents deceleratin to below 0
                }
                else if (velocity.x < 0)
                {
                    velocity.x += airDecelRate * Time.deltaTime;
                    velocity.x = Mathf.Min(velocity.x, 0);      // Prevents decelerating to above 0
                }
            }
        }


        // run the coyote time timer when it is less than the max time
        if (cTimer < coyoteTime) cTimer += Time.deltaTime;
        
        if (!isGrounded)
        {
            velocity.y += currentGravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, -terminalSpeed);
        }
        else
        {
            velocity.y = 0;
            // Keep the coyote timer reset to 0 while on ground
            cTimer = 0;
        }
    }
    private void JumpUpdate()
    {
        if ((isGrounded || cTimer < coyoteTime) && Input.GetAxisRaw("Vertical") > 0)
        {
            velocity.y = jumpVelocity;
            isGrounded = false;
            cTimer += coyoteTime;
        }
    }
    public bool IsWalking()
    {
        return velocity.x != 0;
    }

    private void OnDrawGizmos()
    {
        // Display the boxcast in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * boxOffset, boxSize);
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * boxOffset, boxSize, 0, nonGround);
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public FacingDirection GetFacingDirection()
    {
        return currentDirection;
    }
}
