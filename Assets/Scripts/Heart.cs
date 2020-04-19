using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    // Set in editor
    public Transform player;
    public List<Vector2> linkStarts;
    public List<Vector2> linkEnds;
    public int segmentsPerLine;
    public Material mat;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < linkStarts.Count; i++) {
            GameObject childLink = new GameObject("Heart link " + i);
            LineRenderer lr = childLink.AddComponent<LineRenderer>();
            lineRenderers.Add(lr);
            lr.startWidth = 1.5f / 16f;
            lr.endWidth = 0.5f / 16f;
            lr.sharedMaterial = mat;
            lr.sortingLayerName = "Background";
            lr.startColor = new Color32(36, 34, 47, 255);
            lr.endColor = new Color32(36, 34, 47, 255);
        }
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < linkStarts.Count; i++) {
            LineRenderer lr = lineRenderers[i];
            lr.SetPosition(0, (Vector2)transform.position + linkStarts[i]);
            lr.SetPosition(1, (Vector2)player.position + linkEnds[i]);
        }
    }
}
