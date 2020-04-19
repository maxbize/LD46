using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    // Set in editor
    public LevelManager levelManager;
    public bool hover; // Move around a little so that the scene is not so static

    public MoveConfig config { get; set; }
    public bool atTarget { get; set; }
    private State state;
    private float stateStartTime;

    // Defining here to be the same for all moveables
    private const float INITIAL_LERP_SPEED = 2f;
    private const float PATROL_START_TIME = 1f;
    private const float PATROL_END_TIME = 1f;
    private const float PATROL_TO_LERP_SPEED = 3f;
    private const float PATROL_FROM_LERP_SPEED = 1f;

    private enum State
    {
        MoveToInitialTarget,
        WaitingToStart,
        WaitingForNextConfig,
        PatrolStart,
        PatrolMoveTo,
        PatrolWaitEnd,
        PatrolMoveBack
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        // Initial Movement
        if (state == State.MoveToInitialTarget) {
            Vector3 toTarget = (Vector3)config.GetTarget(0) - transform.position;
            transform.position += toTarget * INITIAL_LERP_SPEED * Time.deltaTime;
            if (toTarget.magnitude < 1 / 16f) {
                SwitchToState(State.WaitingToStart);
            }

        // Patrol states (loops)
        } else if (state == State.PatrolStart) {
            atTarget = false;
            transform.position = (Vector3)config.GetTarget(0) + Random.onUnitSphere * 2/16f;
            if (Time.timeSinceLevelLoad - stateStartTime > PATROL_START_TIME) {
                SwitchToState(State.PatrolMoveTo);
            }
        } else if (state == State.PatrolMoveTo) {
            Vector3 toTarget = (Vector3)config.GetTarget(1) - transform.position;
            transform.position += toTarget * PATROL_TO_LERP_SPEED * Time.deltaTime;
            if (toTarget.magnitude < 1 / 16f) {
                SwitchToState(State.PatrolWaitEnd);
            }
        } else if (state == State.PatrolWaitEnd) {
            if (Time.timeSinceLevelLoad - stateStartTime > PATROL_END_TIME) {
                SwitchToState(State.PatrolMoveBack);
            }
        } else if (state == State.PatrolMoveBack) {
            Vector3 toTarget = (Vector3)config.GetTarget(0) - transform.position;
            transform.position += toTarget * PATROL_FROM_LERP_SPEED * Time.deltaTime;
            if (toTarget.magnitude < 1 / 16f) {
                SwitchToState(State.WaitingToStart);
            }
        }

        // Player reactive states
    }

    public void SetConfig(MoveConfig config) {
        this.config = config;
        state = State.MoveToInitialTarget;
    }

    public void NotifyAllAtInitialTargets() {
        if (state != State.WaitingToStart && state != State.WaitingForNextConfig) {
            Debug.LogError("Bad state transition from " + state + " - " + name + " wasn't ready");
        }

        if (config.typ == MoveConfig.Type.Static) {
            atTarget = true;
            SwitchToState(State.WaitingForNextConfig);
        } else if (config.typ == MoveConfig.Type.Patrol) {
            SwitchToState(State.PatrolStart);
        } else {
            Debug.LogError("Player Moveable state not yet implemented");
        }
    }

    private void SwitchToState(State state) {
        this.state = state;
        stateStartTime = Time.timeSinceLevelLoad;

        if (state == State.WaitingToStart) {
            atTarget = true;
            levelManager.NotifyPartAtTarget(this);
        }
    }
}
