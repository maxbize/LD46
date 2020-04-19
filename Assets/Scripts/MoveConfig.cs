using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveConfig : MonoBehaviour
{
    public Type typ;
    public Part part;

    private List<Vector2> targets;

    public enum Type
    {
        Static,
        Patrol,
        Player
    }

    public enum Part
    {
        LeftHand,
        RightHand,
        Brain,
        Heart
    }

    private void Start() {
        targets = new List<Vector2>();
        for (int i = 0; i < transform.childCount; i++) {
            targets.Add(transform.GetChild(i).transform.position);
        }

        if (transform.childCount == 0 && typ == Type.Static) {
            targets.Add(transform.position);
        } else {
            Debug.LogError("Non-static move type must have children!");
        }
    }

    public Vector2 GetTarget(int index) {
        return targets[index];
    }
}
