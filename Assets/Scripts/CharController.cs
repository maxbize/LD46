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
    private bool jumping;
    private float jumpStartTime;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
    
    }

    private void FixedUpdate() {
        //FindCurves();

        Vector2 force = Vector2.zero;
        bool grounded = IsGrounded();

        // Gravity
        //float gravity = 9.8f * gravityScale;
        //force += Vector2.down * gravity;

        // Drag
        //float hDrag = rb.velocity.x * rb.velocity.x * moveDir.magnitude > 0.1 ? horizontalDrag : noMoveDrag;
        float hDrag = rb.velocity.x * rb.velocity.x * horizontalDrag;
        force += Vector2.left * Mathf.Sign(rb.velocity.x) * hDrag;

        // Input movement
        force += acceleration * moveDir;

        // Jump handling
        // 1. On ground, not jumping, want to jump, start new jump
        // 2. Continuing an existing jump
        // 3. No longer jumping
        if (jumpInput) {
            if (grounded && jumpStartTime == 0) {
                jumping = true;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpStartTime = Time.timeSinceLevelLoad;
            } else if (Time.timeSinceLevelLoad - jumpStartTime < jumpSustainTime) {
                force += Vector2.up * jumpSustainForce;
            }
        } else if (jumping) {
            jumpStartTime = 0;
        }

        // Friction
        if (grounded && moveDir.x == 0) {
            force -= Vector2.right * rb.velocity.x * frictionCoefficient;
        }

        // Apply forces
        rb.AddForce(force);

        if (Mathf.Abs(rb.velocity.x) > 0.1f) {
            sr.flipX = rb.velocity.x < 0;
        }

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

    private bool IsGrounded() {
        int layerMask = 1 << LayerMask.NameToLayer("Environment");
        RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, (col.bounds.extents.y + 1 / 16f), layerMask);

        Debug.DrawRay(col.bounds.center, Vector2.down * (col.bounds.extents.y + 1/16f), hit.collider == null ? Color.red : Color.green);
        return hit.collider != null;
    }

    public void Move(Vector2 dir, bool jump) {
        moveDir = dir;
        jumpInput = jump;
    }
}
