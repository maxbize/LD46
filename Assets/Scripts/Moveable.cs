using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    // Set in editor
    public LevelManager levelManager;
    public Transform player;
    public Transform heart;
    public ParticleSystem heartParticles;
    public AudioManager audioManager;
    public CameraManager cameraManager;

    public MoveConfig config { get; set; }
    public bool atTarget { get; set; }
    private State state;
    private float stateStartTime;
    private float perlinSeed;
    private int targetIndex; // Only used for player dynamic states for now
    private int playerToDir;
    private Vector2 startPointPlayerTo;
    private Collider2D col;

    // Defining here to be the same for all moveables
    private const float INITIAL_LERP_SPEED = 2f;
    private const float PATROL_START_TIME = 1f;
    private const float PATROL_END_TIME = 2f;
    private const float PATROL_TO_SPEED = 40f;
    private const float PATROL_FROM_LERP_SPEED = 1f;
    private const float PLAYER_TO_LERP_SPEED = 30f;
    private const float PLAYER_TO_MIN_DIST = 0f;

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

        if (config.part == MoveConfig.Part.Brain) {
            HandleHeartParticles();
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
            if (config.part == MoveConfig.Part.Brain && !heartParticles.isPlaying) {
                heartParticles.Play();
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
            Vector3 centerPoint = (config.GetTarget(targetIndex) + startPointPlayerTo) / 2;
            Vector3 centerToStart = startPointPlayerTo - (Vector2)centerPoint;
            float angle = Mathf.Min(180, (Time.timeSinceLevelLoad - stateStartTime) * 720);
            Vector3 delta = Quaternion.Euler(0, 0, angle * playerToDir) * centerToStart;
            transform.position = centerPoint + delta;
            if (angle >= 180) {
                transform.position = config.GetTarget(targetIndex);
                SwitchToState(State.PlayerWait);
            }
        } else if (state == State.PlayerWait) {
            float noiseX = Mathf.PerlinNoise(perlinSeed + Time.timeSinceLevelLoad, 0) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0, perlinSeed + Time.timeSinceLevelLoad) - 0.5f;
            //transform.position = config.GetTarget(targetIndex) + new Vector2(noiseX, noiseY);
        }
    }

    public void SetConfig(MoveConfig config) {
        this.config = config;
        atTarget = false;
        SwitchToState(State.MoveToInitialTarget);
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
            if (config.part == MoveConfig.Part.LeftHand && state != State.MoveToInitialTarget) {
                audioManager.PlayClip(audioManager.handReset);
            }
        }
    }

    private void SwitchToState(State state) {
        State lastState = this.state;
        this.state = state;

        stateStartTime = Time.timeSinceLevelLoad;

        if (state == State.WaitingToStart) {
            atTarget = true;
            levelManager.NotifyPartAtTarget(this);
        }

        // For actions where both hands would play
        if (config.part == MoveConfig.Part.LeftHand) {
            if (state == State.PatrolStart) {
                audioManager.PlayClip(audioManager.handPatrolShake);
            } else if (state == State.PatrolMoveTo) {
                audioManager.PlayClip(audioManager.handPatrolMove);
            } else if (state == State.PatrolWaitEnd) {
                audioManager.PlayClip(audioManager.handPatrolArrive);
                cameraManager.AddScreenShake(0.3f);
            }
        }

        if (state == State.PlayerMoveTo) {
            // Figure out direction using collider positions
            startPointPlayerTo = col.bounds.center;
            Vector3 startToNext = config.GetTarget(targetIndex) + Vector2.up * col.bounds.extents.y - startPointPlayerTo;
            Vector3 startToPlayer = player.GetComponent<Collider2D>().bounds.center - (Vector3)startPointPlayerTo;
            playerToDir = Vector3.Cross(startToNext, startToPlayer).z > 0 ? 1 : -1;
            // For actual movement, we need to use transform positions
            startPointPlayerTo = transform.position;

            audioManager.PlayClip(audioManager.handPatrolMove);
        } else if (state == State.PlayerWait && lastState == State.PlayerMoveTo) {
            cameraManager.AddScreenShake(0.3f);
        }

        if (state == State.MoveToInitialTarget && config.part == MoveConfig.Part.Brain) {
            audioManager.PlayClip(audioManager.headMove);
        }


        if (config.part == MoveConfig.Part.Brain) {
            Debug.Log("Brain Switching to state " + state);
            if (state == State.WaitingForNextConfig) {
                heartParticles.Play();
            } else {
                heartParticles.Stop();
            }
        }
    }

    public bool OtherCanMoveToNextPlayerSpot() {
        return state == State.PlayerWait || state == State.MoveToInitialTarget;
    }

    // Should really be in another class but out of time!
    private void HandleHeartParticles() {
        Vector2 heartToSelf = transform.position - heart.position;
        heartParticles.transform.position = heart.position + Vector3.back * 3;
        heartParticles.transform.rotation = Quaternion.LookRotation(heartToSelf);
    }

    // Debugging
    //private void OnDrawGizmos() {
    //    UnityEditor.Handles.Label(transform.position + Vector3.left * 1.5f, state.ToString());
    //}
}
