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
