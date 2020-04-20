using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Set in editor
    public AudioClip jumpClip;
    public AudioClip hurtClip;
    public AudioClip headMove;
    public AudioClip handPatrolShake;
    public AudioClip handPatrolMove;
    public AudioClip handReset;

    private Vector3 clipPositions;

    // Start is called before the first frame update
    void Start() {
        clipPositions = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update() {

    }

    public void PlayClip(AudioClip clip) {
        GameObject go = new GameObject("Audioclip " + clip.name);
        go.transform.position = clipPositions;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = Random.Range(0.8f, 1.2f);
        source.Play();
        go.AddComponent<SelfDestruct>();
    }
}
