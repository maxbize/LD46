using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    // Set in editor
    public float acceleration;
    public float gravityScale;
    public float horizontalDrag;
    public float noMoveDrag;
    public float verticalDrag;
    public float jumpForce;
    public float jumpSustainForce;
    public float jumpSustainTime;
    public float frictionCoefficient;
    public float maxYVel;
    public float maxYVelWalled;
    public float maxXVelGround;
    public float maxXVelAir;
    public float maxXVelAirWallJump;
    public float wallJumpXForce;
    public float wallJumpYForce;
    public float wallJumpMoveRestrictTime;
    public float noAirFrictionTime;
    public float wallStickTime;

    public AnimationCurve horizontalGroundVelocityRampUp;
    public AnimationCurve horizontalGroundVelocityRampDown;
    public AnimationCurve verticalFreefallVelocityRamp;
    public AnimationCurve verticalGroundedVelocityRamp;
    public AnimationCurve jumpVelocityRamp;

    // Ramp up/down horizontal velocity while grounded/aired
    // Jump vertical velocity
    // Wall jump horizontal/vertical velocity
    // Air strike velocity

    // Curve x = time, y = velocity

    // Velocity movement test
    private AnimationCurve currentHorizontalCurve;
    private float horizontalCurveStartTime;
    private AnimationCurve currentVerticalCurve;
    private float verticalCurveStartTime;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private bool jumpInput;
    private Vector2 moveDir;
    private JumpState jumping;
    private float jumpStartTime;
    private float wallStickStartTime;
    private bool newJumpInput = true;

    private enum JumpState
    {
        None,
        Ground,
        WallJumpLocked,
        WallJumpFree
    }

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {

    }

    // This is an absolute mess. If I had more time, I'd want to simplify as much as possible.
    // Maybe just a single moveState powering a state machine and a single stateStart timer
    private void FixedUpdate() {
        //FindCurves();

        Vector2 force = Vector2.zero;
        bool grounded = IsGrounded();
        int walled = IsWalled();
        float jumpTime = Time.timeSinceLevelLoad - jumpStartTime;

        // Gravity
        //float gravity = 9.8f * gravityScale;
        //force += Vector2.down * gravity;

        // Drag
        //float hDrag = rb.velocity.x * rb.velocity.x * horizontalDrag;
        //force += Vector2.left * Mathf.Sign(rb.velocity.x) * hDrag;

        // Input movement
        if (jumping == JumpState.WallJumpLocked && jumpTime < noAirFrictionTime) {

        } else if (!grounded && Time.timeSinceLevelLoad - wallStickStartTime < wallStickTime) {

        } else {
            force += acceleration * moveDir;
        }

        // Handle jump state transitions
        if (jumpTime > noAirFrictionTime && jumping == JumpState.WallJumpLocked) {
            jumpStartTime = 0;
            jumping = JumpState.WallJumpFree;
        }
        if (grounded && jumping == JumpState.WallJumpFree) {
            jumpStartTime = 0;
            jumping = JumpState.None;
        }

        // Jump handling
        // 1. On ground, not jumping, want to jump, start new jump
        // 2. Continuing an existing jump
        // 3. No longer jumping
        if (jumpInput) {
            if (grounded && jumpStartTime == 0 && newJumpInput) {
                // Check ground jump
                jumping = JumpState.Ground;
                jumpTime = 0;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpStartTime = Time.timeSinceLevelLoad;
            } else if (walled != 0 && !grounded && jumpStartTime == 0 && newJumpInput) {
                // Check wall jump
                jumping = JumpState.WallJumpLocked;
                jumpTime = 0;
                rb.velocity = Vector2.zero;
                rb.AddForce(Vector2.up * wallJumpYForce, ForceMode2D.Impulse);
                rb.AddForce(Vector2.left * walled * wallJumpXForce, ForceMode2D.Impulse);
                jumpStartTime = Time.timeSinceLevelLoad;
            } else if (Time.timeSinceLevelLoad - jumpStartTime < jumpSustainTime) {
                force += Vector2.up * jumpSustainForce;
            }
        } else if (jumping != JumpState.None && jumping != JumpState.WallJumpLocked) {
            jumpStartTime = 0;
            jumping = JumpState.None;
        }

        // Friction
        if (moveDir.x == 0 && (grounded || Time.timeSinceLevelLoad - jumpStartTime > noAirFrictionTime)) {
            force -= Vector2.right * rb.velocity.x * frictionCoefficient;
        }

        // Apply forces
        rb.AddForce(force);

        // Handle sprite direction
        if (Mathf.Abs(rb.velocity.x) > 0.1f) {
            sr.flipX = rb.velocity.x < 0;
        }

        // Limit y velocity
        if (walled != 0 && moveDir.x != 0 && Mathf.Sign(walled) == Mathf.Sign(moveDir.x) && -rb.velocity.y > maxYVelWalled && rb.velocity.y < 0) {
            wallStickStartTime = Time.timeSinceLevelLoad;
            rb.velocity = new Vector2(rb.velocity.x, -maxYVelWalled);
        } else if (rb.velocity.y < 0 && -rb.velocity.y > maxYVel) {
            rb.velocity = new Vector2(rb.velocity.x, -maxYVel);
        }

        // Limit x velocity
        if (jumping == JumpState.WallJumpLocked && Mathf.Abs(rb.velocity.x) > maxXVelAirWallJump) {
            rb.velocity = new Vector2(maxXVelAirWallJump * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        } else if (jumping == JumpState.WallJumpFree && Mathf.Abs(rb.velocity.x) > maxXVelAir) {
            rb.velocity = new Vector2(maxXVelAir * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        } else if ((jumping == JumpState.Ground || jumping == JumpState.None) && Mathf.Abs(rb.velocity.x) > maxXVelGround) {
            rb.velocity = new Vector2(maxXVelGround * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        }
        /*
        if (grounded && Mathf.Abs(rb.velocity.x) > maxXVelGround) {
            rb.velocity = new Vector2(maxXVelGround * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        } else if (!grounded && Mathf.Abs(rb.velocity.x) > maxXVelAir) {
            rb.velocity = new Vector2(maxXVelAir * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        }
        */

        //if (Mathf.Abs(rb.velocity.x) < 0.5 && moveDir.x == 0) {
        //    rb.velocity = Vector2.up * rb.velocity.y; // Clear x velocity
        //} 

        /*
        // Horizontal velocity curve
        Vector2 velocity = rb.velocity;
        float horizontalCurveTime = Time.timeSinceLevelLoad - horizontalCurveStartTime;
        if (currentHorizontalCurve == horizontalGroundVelocityRampUp) {
            velocity.x = moveDir.x * horizontalGroundVelocityRampUp.Evaluate(horizontalCurveTime);
        } else if (currentHorizontalCurve == horizontalGroundVelocityRampDown) {
            velocity.x = Mathf.Sign(velocity.x) * horizontalGroundVelocityRampDown.Evaluate(horizontalCurveTime);
        }

        // Vertical velocity curve
        float verticalCurveTime = Time.timeSinceLevelLoad - verticalCurveStartTime;
        velocity.y = currentVerticalCurve.Evaluate(verticalCurveTime);

        if (Mathf.Abs(velocity.x) > 0.1f) {
            sr.flipX = velocity.x < 0;
        }

        rb.velocity = velocity;
        */
    }

    // Figure out which curve we're on
    /*
    private void FindCurves() {
        AnimationCurve candidateCurve = null;
        bool grounded = IsGrounded();

        // Horizontal
        if (moveDir.magnitude > 0.1) {
            candidateCurve = horizontalGroundVelocityRampUp;
        } else {
            candidateCurve = horizontalGroundVelocityRampDown;
        }

        if (candidateCurve != currentHorizontalCurve) {
            currentHorizontalCurve = candidateCurve;
            horizontalCurveStartTime = Time.timeSinceLevelLoad;
        }

        // Vertical
        candidateCurve = null;

        if (grounded) {
            if (jumpInput) {
                candidateCurve = jumpVelocityRamp;
            } else {
                candidateCurve = verticalGroundedVelocityRamp;
            }
        } else {
            if (!(currentVerticalCurve == jumpVelocityRamp && jumpInput)) {
                candidateCurve = verticalFreefallVelocityRamp;
            }
        }

        if (candidateCurve != currentVerticalCurve && candidateCurve != null) {
            currentVerticalCurve = candidateCurve;
            verticalCurveStartTime = Time.timeSinceLevelLoad;
        }
    }
    */

    // -1 left wall, 0 not walled, 1 right wall
    private int IsWalled() {
        int layerMask = 1 << LayerMask.NameToLayer("Environment");
        RaycastHit2D hit = Physics2D.Raycast((Vector2)col.bounds.center - Vector2.up * col.bounds.extents.y * 0.9f, Vector2.right, (col.bounds.extents.x + 1 / 16f), layerMask);
        if (hit.collider != null) {
            return 1;
        }
        hit = Physics2D.Raycast((Vector2)col.bounds.center + Vector2.up * col.bounds.extents.y * 0.9f, Vector2.right, (col.bounds.extents.x + 1 / 16f), layerMask);
        if (hit.collider != null) {
            return 1;
        }

        hit = Physics2D.Raycast((Vector2)col.bounds.center - Vector2.up * col.bounds.extents.y * 0.9f, Vector2.left, (col.bounds.extents.x + 1 / 16f), layerMask);
        if (hit.collider != null) {
            return -1;
        }
        hit = Physics2D.Raycast((Vector2)col.bounds.center + Vector2.up * col.bounds.extents.y * 0.9f, Vector2.left, (col.bounds.extents.x + 1 / 16f), layerMask);
        if (hit.collider != null) {
            return -1;
        }

        return 0;
    }

    private bool IsGrounded() {
        int layerMask = 1 << LayerMask.NameToLayer("Environment");
        RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, (col.bounds.extents.y + 1 / 16f), layerMask);

        //Debug.DrawRay(col.bounds.center, Vector2.down * (col.bounds.extents.y + 1/16f), hit.collider == null ? Color.red : Color.green);
        return hit.collider != null;
    }

    public void Move(Vector2 dir, bool jump, bool jumpDown) {
        moveDir = dir;
        jumpInput = jump;
        newJumpInput = jumpDown;
    }
}
