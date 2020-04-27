using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Set in editor
    public float noiseScanRate;
    public float decayRate;

    private Vector3 targetPos;
    private float screenShake;


    // Start is called before the first frame update
    void Start() {
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update() {
        screenShake = Mathf.Clamp(screenShake - decayRate * Time.deltaTime, 0, 10);
     
        float noiseX = Mathf.PerlinNoise(-10 + Time.realtimeSinceStartup * noiseScanRate, -10) - 0.5f;
        float noiseY = Mathf.PerlinNoise(10, 10 + Time.realtimeSinceStartup * noiseScanRate) - 0.5f;
        Vector3 noise = new Vector3(noiseX, noiseY, 0);
        transform.position = targetPos + noise * screenShake;
    }

    public void AddScreenShake(float amount) {
        screenShake += amount;
    }
}
