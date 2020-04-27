using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessing : MonoBehaviour
{
    // Set in editor
    public Transform heart;
    public RenderTexture postRT;

    private Camera cam;
    private Material material;
    private RenderTexture tempRT;
    private const int MAX_PULSES = 5;
    private List<Vector4> pulses;
    private int pulseIndex;

    // Start is called before the first frame update
    void Start() {
        material = new Material(Shader.Find("Custom/Pulse"));
        cam = Camera.main;
        pulses = new List<Vector4>();
        // Array sizes can only be set once so set to max first
        for (int i = 0; i < MAX_PULSES; i++) {
            pulses.Add(new Vector4(-100, -100, 0, 0));
        }
        Shader.SetGlobalVectorArray("_Pulses", pulses);
    }

    public void SendPulse(Vector2 pos, float distance) {
        Vector2 pulseScreenPos = cam.WorldToScreenPoint(pos);
        pulseScreenPos.x = ((pulseScreenPos.x / Screen.width) - cam.rect.x) / cam.rect.width;
        pulseScreenPos.y = ((pulseScreenPos.y / Screen.height) - cam.rect.y) / cam.rect.height;

        pulses[pulseIndex] = new Vector4(pulseScreenPos.x, pulseScreenPos.y, Time.timeSinceLevelLoad, distance);

        pulseIndex = (pulseIndex + 1) % pulses.Count;

        Shader.SetGlobalVectorArray("_Pulses", pulses);
    }

    private void Update() {
        Shader.SetGlobalFloat("_TimeSinceLevelLoad", Time.timeSinceLevelLoad);
    }

    /*
    private void OnRenderObject() {
        tempRT = RenderTexture.GetTemporary(Screen.width, Screen.height);
        tempRT.filterMode = FilterMode.Point;

        //RenderTexture active = RenderTexture.active;

        Graphics.Blit(cam.activeTexture, tempRT, material);

        //cam.targetTexture = null;
        //Graphics.Blit(tempRT, null as RenderTexture);

        //Graphics.Blit(tempRT, cam.activeTexture);

        RenderTexture.ReleaseTemporary(tempRT);

        //Graphics.Blit(source, postRT, material);
        //Graphics.Blit(postRT, destination, material);

    }
    */

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }
}
