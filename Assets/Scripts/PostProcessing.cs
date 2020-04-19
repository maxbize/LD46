using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessing : MonoBehaviour
{
    // Set in editor
    public Transform heart;

    private Camera cam;
    private Material material;

    // Start is called before the first frame update
    void Start() {
        material = new Material(Shader.Find("Custom/Pulse"));
        cam = Camera.main;
    }

    public void SendPulse(Vector2 pos) {
        Vector2 pulseScreenPos = cam.WorldToScreenPoint(heart.position);
        pulseScreenPos.x /= Screen.width;
        pulseScreenPos.y /= Screen.height;
        Shader.SetGlobalVector("_Pulse", new Vector4(pulseScreenPos.x, pulseScreenPos.y, Time.timeSinceLevelLoad + 0.1f));
    }

    private void Update() {
        Shader.SetGlobalFloat("_TimeSinceLevelLoad", Time.timeSinceLevelLoad);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }
}
