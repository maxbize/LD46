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

    // Start is called before the first frame update
    void Start() {
        StartLevel(currentLevel);
    }

    // Update is called once per frame
    void Update() {

    }

    private void StartLevel(int index) {
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
    }

}
