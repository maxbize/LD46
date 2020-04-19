using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Set in editor
    public int currentLevel;
    public Transform levels;
    public Moveable leftHand;
    public Moveable rightHand;
    public Moveable brain;
    public Transform player;

    private List<Moveable> parts;
    private bool firstReady;
    private Moveable nextPart; // Next part player needs to touch to advance Player level sequence

    // Start is called before the first frame update
    void Start() {
        StartLevel(currentLevel);
    }

    // Update is called once per frame
    void Update() {
        parts = new List<Moveable>() { leftHand, rightHand, brain };

        // Hack!
        if (player.position.y < 0.1f) {
            parts.ForEach(p => p.NotifyPlayerTouchedGround());
            nextPart = rightHand;
        }
    }

    private void StartLevel(int index) {
        firstReady = true;
        foreach (MoveConfig config in levels.GetChild(currentLevel).GetComponentsInChildren<MoveConfig>()) {
            if (config.part == MoveConfig.Part.LeftHand) {
                leftHand.SetConfig(config);
            } else if (config.part == MoveConfig.Part.RightHand) {
                rightHand.SetConfig(config);
            } else if (config.part == MoveConfig.Part.Brain) {
                brain.SetConfig(config);
            } else {
                Debug.LogError("You messed up your configs");
            }
        }
        nextPart = rightHand; // Sequences always start with left hand
    }

    public void NotifyPartAtTarget(Moveable part) {
        if (parts.All(p => p.atTarget)) {
            parts.ForEach(p => p.NotifyAllAtInitialTargets());
        }
    }

    public void NotifyPlayerTouchedPart(Moveable part) {
        if (part == nextPart) {
            if (part == rightHand) {
                nextPart = leftHand;
            } else if (part == leftHand) {
                nextPart = rightHand;
            } else {
                Debug.LogError("Unexpected nextPart: " + part);
            }
            nextPart.NotifyPlayerTouchedOtherHand();
        }
    }

}
