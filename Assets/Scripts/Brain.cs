using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    // Set in editor
    public Sprite frame_Normal;
    public Sprite frame_Flash;
    public Sprite frame_Hurt;
    public float hurtFlashTime;
    public float hurtAnimTime;
    public float hurtFreezeTime;
    public ParticleSystem heartParticles;

    private float hurtStartTime;
    private float hurtStartUnscaledTime;
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start() {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        float hurtTime = Time.timeSinceLevelLoad - hurtStartTime;
        if (hurtTime < hurtFlashTime) {
            sr.sprite = frame_Flash;
        } else if (hurtTime < hurtAnimTime) {
            sr.sprite = frame_Hurt;
        } else {
            sr.sprite = frame_Normal;
        }

        if (Time.unscaledTime - hurtStartUnscaledTime > hurtFreezeTime) {
            Time.timeScale = 1;
        transform.localScale = Vector3.one;
        }
    }

    public void NotifyHurt() {
        hurtStartTime = Time.timeSinceLevelLoad;
        hurtStartUnscaledTime = Time.unscaledTime;
        transform.localScale = Vector3.one * 0.9f;
        Time.timeScale = 0;
    }
}
