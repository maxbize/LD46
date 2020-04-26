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
    public GameObject startMenu;
    public GameObject endMenu;
    public ParticleSystem heartParticles;
    public float springCoeffient;
    public AudioManager audioManager;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private Vector2 targetPos;
    private float perlinSeed;
    private List<Vector2> bezierOffsets = new List<Vector2>();
    private SpriteRenderer playerSr;
    private int audioCounter;

    private float lastPulseTime;

    // Start is called before the first frame update
    void Start() {
        perlinSeed = Random.Range(-100, 100);
        heartParticles.Stop();
        playerSr = player.GetComponentInChildren<SpriteRenderer>();
        targetPos = transform.position;
        for (int i = 0; i < linkStarts.Count; i++) {
            GameObject childLink = new GameObject("Heart link " + i);
            LineRenderer lr = childLink.AddComponent<LineRenderer>();
            lineRenderers.Add(lr);
            lr.startWidth = 2f / 16f;
            lr.endWidth = 1f / 16f;
            lr.sharedMaterial = mat;
            lr.sortingLayerName = "Background";
            lr.startColor = new Color32(36, 34, 47, 255);
            lr.endColor = new Color32(143, 248, 226, 255);
            //lr.positionCount = segmentsPerLine + 2;
            lr.positionCount = 3;
            bezierOffsets.Add(Random.onUnitSphere * 2);
        }
    }

    // Update is called once per frame
    void Update() {
        //UpdateHeartLinksParabolic();
        Vector2 toPlayer = player.position - transform.position;
        for (int i = 0; i < linkStarts.Count; i++) {
            LineRenderer lr = lineRenderers[i];
            Vector2 p0 = (Vector2)transform.position + linkStarts[i];
            Vector2 p1 = (Vector2)transform.position + toPlayer / 2 + bezierOffsets[i];
            Vector2 p2 = (Vector2)player.position + linkEnds[i];
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);
            lr.SetPosition(2, p2);
        }

        if (!startMenu.activeSelf && !endMenu.activeSelf) {
            Vector2 offset = new Vector2(playerSr.flipX ? 2.5f : -2.5f, 1.5f);
            Vector2 toPlayerOffset = toPlayer + offset;
            targetPos += toPlayerOffset * springCoeffient * Time.deltaTime;
        } else {
            transform.position = new Vector2(1, 9.5f);
            targetPos = new Vector2(1, 9.5f);
            heartParticles.Stop();
        }

        float pulseTime = Time.timeSinceLevelLoad - lastPulseTime;
        if (pulseTime > 1.5f) {
            lastPulseTime = Time.timeSinceLevelLoad;
            audioCounter = 0;
            pp.SendPulse(transform.position); // heh heh :P
            audioManager.PlayClip(audioManager.heartbeat);
        } else if (pulseTime > 0.2f && audioCounter == 0) {
            audioManager.PlayClip(audioManager.heartbeat);
            audioCounter++;
        }

        float noiseX = Mathf.PerlinNoise(perlinSeed + Time.timeSinceLevelLoad, 0) - 0.5f;
        float noiseY = Mathf.PerlinNoise(0, perlinSeed + Time.timeSinceLevelLoad) - 0.5f;
        noiseX = Mathf.Round(noiseX * 4) / 4;
        noiseY = Mathf.Round(noiseY * 4) / 4;
        transform.position = targetPos + new Vector2(noiseX, noiseY) * 0.5f;
    }

    private void UpdateHeartLinksParabolic() {
        Vector2 toPlayer = player.position - transform.position;
        // If there's time later, do something cool like bezier or catenary
        for (int i = 0; i < linkStarts.Count; i++) {
            LineRenderer lr = lineRenderers[i];
            Vector2 p1 = (Vector2)transform.position + linkStarts[i];
            Vector2 p3 = (Vector2)player.position + linkEnds[i];
            Vector2 p2 = (Vector2)transform.position + toPlayer / 2 + bezierOffsets[i] * 0.2f + Vector2.down * 0.5f * Mathf.PerlinNoise(Time.timeSinceLevelLoad, 50);
            p2 = (p1 + p3) / 2 + Vector2.up * bezierOffsets[i].y * 0.2f;
            lr.SetPosition(0, p1);

            // Solve a, b, c of parabola
            float x1 = p1.x;
            float x2 = p2.x;
            float x3 = p3.x;
            float y1 = p1.y;
            float y2 = p2.y;
            float y3 = p3.y;
            float x12 = p1.x * p1.x;
            float x22 = p2.x * p2.x;
            float x32 = p3.x * p3.x;

            float a = (y1 * (x2 - x3) + y2 * (x3 - x1) + y3 * (x1 - x2)) / ((x1 - x3) * (x2 - x3) * (x1 - x2));
            float b = (y1 * (x32 - x22) + y2 * (x12 - x32) + y3 * (x22 - x12)) / ((x1 - x3) * (x2 - x3) * (x1 - x2));
            float c = (y1 * (x22 * x3 - x32 * x2) + y2 * (x32 * x1 - x12 * x3) + y3 * (x12 * x2 - x22 * x1)) / ((x1 - x3) * (x2 - x3) * (x1 - x2));

            for (int j = 1; j < segmentsPerLine + 1; j++) {
                float t = j / (segmentsPerLine + 1f);
                float x = x1 + (x3 - x1) * t;
                float y = a * x * x + b * x + c;
                lr.SetPosition(j, new Vector3(x, y, 0));
            }

            lr.SetPosition(segmentsPerLine + 1, p3);
        }
    }
}
