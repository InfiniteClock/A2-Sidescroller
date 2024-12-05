using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal")]
    public float topSpeed = 6f;
    public float accelTime = 0.5f;
    public float decelTime = 0.1f;
    public float airDecelTime = 0.3f;
    public float quickTurnTime = 0.1f;

    [Header("Vertical")]
    public float terminalSpeed = 10f;
    public float coyoteTime = 0.1f;
    public float maxApexHeight = 4f;
    public float maxApexTime = 0.65f;
    public float variableJumpHeight = 2f;
    public float vJumpWindow = 0.1f;
    public float groundPoundFreezeDuration = 0.1f;

    [Header("Ground Checking")]
    public Vector2 boxSize;
    public float boxOffset = 0.55f;
    public LayerMask groundLayer;

    [Header("Player State")]
    public PlayerState currentState = PlayerState.idle;
    public PlayerState prevState = PlayerState.idle;

    private float accelRate;
    private float decelRate;
    private float airDecelRate;
    private float quickTurnRate;
    private float maxJumpVelocity;
    private float gravity;

    // Timers
    private float cTimer;
    private float vJumpTimer;
    private float GPTimer;
    private float QTtimer;

    private bool isGroundPounding = false;
    private bool isGrounded = false;
    private bool isDead = false;
    private bool quickTurning = false;

    private float variableJumpVelocity;

    private Rigidbody2D rb;
    private Vector2 playerInput;
    private Vector2 velocity;
    private FacingDirection currentDirection;
    private FacingDirection turnDirection;
    
    public enum FacingDirection
    {
        left, right
    }
    public enum PlayerState
    {
        idle, walking, jumping, dead, groundPound
    }

    private void OnValidate()
    {
        // Determine player gravity and jump velocity of max jump
        gravity = -2 * maxApexHeight / (maxApexTime * maxApexTime);
        maxJumpVelocity = 2 * maxApexHeight / maxApexTime;

        // Determine the jump velocity of the variable jump.
        // Because of the exponential effect of gravity, calculation must be performed by
        // squaring the regular jump velocity, applying the ratio of regular jump height to variable jump height,
        // then square rooting the entire thing. 
        variableJumpVelocity = Mathf.Sqrt(maxJumpVelocity * maxJumpVelocity * variableJumpHeight / maxApexHeight);
        
        // Debug.Log(maxJumpVelocity + " : " +  variableJumpVelocity);

        // Set the acceleration, deceleration, aerial deceleration and quick turning rates
        accelRate = topSpeed / accelTime;
        decelRate = topSpeed / decelTime;
        airDecelRate = topSpeed / airDecelTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        // Get the rigidbody, disable gravity
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void FixedUpdate()
    {
        // Check for ground collision before most other code
        CheckGround();

        // Apply movement and jumping, then set the rigidbody's velocity
        MovementUpdate(playerInput);
        JumpUpdate(playerInput.y);
        rb.velocity = velocity;
    }
    private void Update()
    {
        // Set the previous player state to last frame's, before any changes may be made this frame
        prevState = currentState;

        // Every frame, get the player's horizontal input for movementUpdate
        playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInput.y = Input.GetAxisRaw("Vertical");

        // Facing direction should equal input direction
        if (playerInput.x < 0) currentDirection = FacingDirection.left;
        if (playerInput.x > 0) currentDirection = FacingDirection.right;

        // Change player state according to player's actions/conditions
        if (isDead) currentState = PlayerState.dead;

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
                else if (isGroundPounding) currentState = PlayerState.groundPound;
                break;
            case PlayerState.groundPound:
                if (isGrounded && velocity.x != 0) currentState = PlayerState.walking;
                else if (isGrounded && velocity.x == 0) currentState = PlayerState.idle;
                break;

        }
        // Toggle death state
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState != PlayerState.dead) currentState = PlayerState.dead;
            else currentState = PlayerState.idle;
        }
    }

    private void MovementUpdate(Vector2 direction)
    {
        // When trying to move left or right
        if (direction.x != 0)
        {
            // If the player is moving fast enough one way and inputs the other way, enable the quick turn timer and set the direction
            if (direction.x > 0 && velocity.x < 0)
            {
                turnDirection = FacingDirection.right;
                QTtimer = 0;
            }
            else if (direction.x < 0 && velocity.x > 0)
            {
                turnDirection = FacingDirection.left;
                QTtimer = 0;
            }

            // If player is in quick turning state, is grounded, and is moving in the right direction for a quick turn, accelerate faster
            if (quickTurning && isGrounded && ((turnDirection == FacingDirection.right && direction.x > 0))
                    || (turnDirection == FacingDirection.left && direction.x < 0))
            {
                velocity.x += accelRate * 3f * direction.x * Time.deltaTime;        
            }
            // Otherwise normally accelerate player
            else
            {
                velocity.x += accelRate * direction.x * Time.deltaTime;
            }
            
            // Clamps velocity to not surpass top speed in either direction
            velocity.x = Mathf.Clamp(velocity.x, -topSpeed, topSpeed);  
        }

        // When not touching horizontal inputs
        else
        {
            // Apply regular deceleration when on the ground
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
            // Apply aerial deceleration when in the air
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

        // Run the quick turn timer when it is less than the max time
        if (QTtimer < quickTurnTime) 
        { 
            QTtimer += Time.deltaTime; 
            quickTurning = true; 
        }
        else quickTurning = false;

        // Run the coyote time timer when it is less than the max time
        if (cTimer < coyoteTime) cTimer += Time.deltaTime;
        
        // Apply gravity to the player while in the air, within terminal speed
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, -terminalSpeed);
        }
        // Stop downward velocity, reset coyote timer
        else
        {
            velocity.y = 0;
            // Keep the coyote timer reset to 0 while on ground
            cTimer = 0;
            // Keep ground pound state false when on ground
            isGroundPounding = false;
        }
    }
    private void JumpUpdate(float jumpInput)
    {
        // When the player is on the ground, or has only been off the ground for less than the coyote timer's max seconds, allow jumping
        if ((isGrounded || cTimer < coyoteTime) && jumpInput > 0)
        {
            velocity.y = maxJumpVelocity;
            isGrounded = false;

            // Once a jump is made, push coyote timer above max to disable it
            cTimer += coyoteTime;

            // Once a jump is made, begin the variable jump timer
            vJumpTimer = 0;
        }

        // When the variable jump timer is active...
        if (vJumpTimer < vJumpWindow)
        {
            // ...count up towards the time window for a variable jump to occur...
            vJumpTimer += Time.deltaTime;
            // ...and if the player releases the jump key (or presses down) before then, switch to the variable jump
            if (jumpInput <= 0 && vJumpTimer <= vJumpWindow)
            {
                // Set velocity to variable jump's initial velocity, minus the force of gravity that has occurred so far.
                velocity.y = variableJumpVelocity + (gravity * vJumpTimer);
            }
        }

        // When the player pushes a down-direction key while not grounded, and not already ground pounding
        if (!isGrounded && jumpInput < 0 && !isGroundPounding)
        {
            isGroundPounding = true;
            // Start the gp timer
            GPTimer = 0;
        }

        if (isGroundPounding)
        {
            GPTimer += Time.deltaTime;
            if (GPTimer < groundPoundFreezeDuration)
            {
                velocity = Vector2.zero;
            }
            else
            {
                velocity.y = -terminalSpeed;
            }
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
        // Returns true if ground layer collision is detected beneath the player
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * boxOffset, boxSize, 0, groundLayer);
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
