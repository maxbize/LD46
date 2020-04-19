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
    public PostProcessing pp;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private Vector2 targetPos;
    private float perlinSeed;

    private float lastPulseTime;

    // Start is called before the first frame update
    void Start() {
        targetPos = transform.position;
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

        if (Time.timeSinceLevelLoad - lastPulseTime > 1.5f) {
            lastPulseTime = Time.timeSinceLevelLoad;
            pp.SendPulse(transform.position); // heh heh :P
        }

        float noiseX = Mathf.PerlinNoise(perlinSeed + Time.timeSinceLevelLoad, 0) - 0.5f;
        float noiseY = Mathf.PerlinNoise(0, perlinSeed + Time.timeSinceLevelLoad) - 0.5f;
        noiseX = Mathf.Round(noiseX * 4) / 4;
        noiseY = Mathf.Round(noiseY * 4) / 4;
        transform.position = targetPos + new Vector2(noiseX, noiseY) * 0.5f;
    }
}
