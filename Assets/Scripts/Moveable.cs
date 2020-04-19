using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    // Set in editor
    public LevelManager levelManager;
    public Transform player;
    public bool hover; // Move around a little so that the scene is not so static

    public MoveConfig config { get; set; }
    public bool atTarget { get; set; }
    private State state;
    private float stateStartTime;
    private float perlinSeed;
    private int targetIndex; // Only used for player dynamic states for now
    private Collider2D col;

    // Defining here to be the same for all moveables
    private const float INITIAL_LERP_SPEED = 2f;
    private const float PATROL_START_TIME = 1f;
    private const float PATROL_END_TIME = 2f;
    private const float PATROL_TO_SPEED = 40f;
    private const float PATROL_FROM_LERP_SPEED = 1f;
    private const float PLAYER_TO_LERP_SPEED = 30f;
    private const float PLAYER_TO_MIN_DIST = 3f;

    private enum State
    {
        MoveToInitialTarget,
        WaitingToStart,
        WaitingForNextConfig,
        PatrolStart,
        PatrolMoveTo,
        PatrolWaitEnd,
        PatrolMoveBack,
        PlayerMoveTo,
        PlayerWait
    }

    // Start is called before the first frame update
    void Start() {
        perlinSeed = Random.Range(0f, 100f);
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update() {
        if (config == null) {
            return;
        }

        // Initial Movement
        if (state == State.MoveToInitialTarget) {
            Vector3 toTarget = (Vector3)config.GetTarget(0) - transform.position;
            transform.position += toTarget * INITIAL_LERP_SPEED * Time.deltaTime;
            if (toTarget.magnitude < 2 / 16f) {
                transform.position = config.GetTarget(0);
                SwitchToState(State.WaitingToStart);
            }

        // Chillin'
        } else if (state == State.WaitingForNextConfig || (config.typ == MoveConfig.Type.Static && state == State.WaitingToStart)) {
            float noiseX = Mathf.PerlinNoise(perlinSeed + Time.timeSinceLevelLoad, 0) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0, perlinSeed + Time.timeSinceLevelLoad) - 0.5f;
            transform.position = config.GetTarget(0) + new Vector2(noiseX, noiseY);

        // Patrol states (loops)
        } else if (state == State.PatrolStart) {
            atTarget = false;
            transform.position = (Vector3)config.GetTarget(0) + Random.onUnitSphere * 2/16f;
            if (Time.timeSinceLevelLoad - stateStartTime > PATROL_START_TIME) {
                SwitchToState(State.PatrolMoveTo);
            }
        } else if (state == State.PatrolMoveTo) {
            Vector3 toTarget = (Vector3)config.GetTarget(1) - transform.position;
            Vector3 deltaPos = toTarget.normalized * PATROL_TO_SPEED * Time.deltaTime;
            if (deltaPos.magnitude > toTarget.magnitude) {
                deltaPos = toTarget;
            }
            transform.position += deltaPos;
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
        else if (state == State.PlayerMoveTo) {
            Vector3 toTarget = (Vector3)config.GetTarget(targetIndex) - transform.position;
            Vector3 deltaPos = toTarget.normalized * PLAYER_TO_LERP_SPEED * Time.deltaTime;
            if (deltaPos.magnitude > toTarget.magnitude) {
                deltaPos = toTarget;
            }
            transform.position += deltaPos;
            if (toTarget.magnitude < 1 / 16f) {
                SwitchToState(State.PlayerWait);
            } else {
                Vector3 toPlayer = player.position - col.bounds.center;
                if (toPlayer.magnitude < PLAYER_TO_MIN_DIST) {
                    transform.position -= toPlayer.normalized * (PLAYER_TO_MIN_DIST - toPlayer.magnitude);
                }
            }
        } else if (state == State.PlayerWait) {
            float noiseX = Mathf.PerlinNoise(perlinSeed + Time.timeSinceLevelLoad, 0) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0, perlinSeed + Time.timeSinceLevelLoad) - 0.5f;
            //transform.position = config.GetTarget(targetIndex) + new Vector2(noiseX, noiseY);
        }
    }

    public void SetConfig(MoveConfig config) {
        this.config = config;
        state = State.MoveToInitialTarget;
        atTarget = false;
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
            SwitchToState(State.PlayerWait);
        }
    }

    public void NotifyPlayerTouchedOtherHand() {
        if (config.typ != MoveConfig.Type.Player) {
            return;
        }

        if (targetIndex < config.NumTargets() - 1) {
            Debug.Log(name + " moving to target " + (targetIndex + 1));
            targetIndex++;
            SwitchToState(State.PlayerMoveTo);
        }
    }

    public void NotifyPlayerTouchedGround() {
        if (config.typ == MoveConfig.Type.Player && targetIndex != 0) {
            atTarget = false;
            targetIndex = 0;
            SwitchToState(State.MoveToInitialTarget);
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
