using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    public bool hover; // Jitter around a little

    private MoveConfig config;
    private State state;

    // Defining here to be the same for all moveables
    private const float INITIAL_LERP_SPEED = 2f;

    private enum State
    {
        MoveToInitialTarget,
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (state == State.MoveToInitialTarget) {
            Vector3 toTarget = (Vector3)config.GetTarget(0) - transform.position;
            transform.position += toTarget * INITIAL_LERP_SPEED * Time.deltaTime;
            Debug.DrawRay(transform.position, toTarget, Color.white);
        }
    }

    public void SetConfig(MoveConfig config) {
        this.config = config;
        state = State.MoveToInitialTarget;
    }
}
