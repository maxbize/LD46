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
        heartParticles.Stop();
        playerSr = player.GetComponent<SpriteRenderer>();
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
            lr.positionCount = 3;
            bezierOffsets.Add(Random.onUnitSphere);
        }
    }

    // Update is called once per frame
    void Update() {
        Vector2 toPlayer = player.position - transform.position;
        // If there's time later, do something cool like bezier or catenary
        for (int i = 0; i < linkStarts.Count; i++) {
            LineRenderer lr = lineRenderers[i];
            Vector2 p0 = (Vector2)transform.position + linkStarts[i];
            Vector2 p1 = (Vector2)transform.position + toPlayer/2 + bezierOffsets[i];
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
}
