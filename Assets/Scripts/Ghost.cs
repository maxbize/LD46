using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    // Set in editor
    public int numGhosts;
    public int ghostDelay; // frames

    private List<Transform> ghosts = new List<Transform>();
    private List<Vector2> history; // Ring buffer
    private int historyIndex;
    private int numSnapshots;

    // Start is called before the first frame update
    void Start() {
        //numSnapshots = Mathf.CeilToInt(numGhosts * ghostDelay / snapshotFrequency);
        numSnapshots = numGhosts * ghostDelay;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        for (int i = 0; i < numGhosts; i++) {
            GameObject ghost = new GameObject("Ghost " + i);
            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.flipX = GetComponent<SpriteRenderer>().flipX;
            sr.color = new Color(1, 1, 1, 0.2f + i * 0.1f);
            ghosts.Add(ghost.transform);
        }
        history = new List<Vector2>();
        for (int i = 0; i < numSnapshots; i++) {
            history.Add(transform.position);
        }
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < numGhosts; i++) {
            ghosts[i].position = history[(historyIndex + i * ghostDelay) % numSnapshots];
        }

        history[historyIndex] = transform.position;
        historyIndex++;
        if (historyIndex >= numSnapshots) {
            historyIndex = 0;
        }
    }
}
