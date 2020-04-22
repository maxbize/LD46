using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    // Set in editor
    public int currentLevel;
    public Transform levels;
    public Moveable leftHand;
    public Moveable rightHand;
    public Moveable brain;
    public Transform player;
    public int hitsPerLevel;
    public GameObject heart;
    public GameObject startMenu;
    public GameObject endMenu;
    public Text endMenuBody1;
    public Text endMenuBody2;
    public Brain brainScript;
    public AudioSource songSource;
    public AudioManager audioManager;

    private List<Moveable> parts;
    private Moveable nextPart; // Next part player needs to touch to advance Player level sequence
    private int hitsLeft;
    private float gameStartTime;

    // Start is called before the first frame update
    void Start() {
        parts = new List<Moveable>() { leftHand, rightHand, brain };
        endMenu.SetActive(false);

        if (startMenu.activeSelf) {
            
        } else {
            // Debug mode - start at a specific level
            StartLevel(currentLevel);
        }
    }

    // Update is called once per frame
    void Update() {

        if (startMenu.activeSelf) {
            heart.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground";
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.X)) {
                songSource.Play();
                startMenu.SetActive(false);
                heart.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
                gameStartTime = Time.timeSinceLevelLoad;
                StartLevel(currentLevel);
            }
        } else if (endMenu.activeSelf) {
        } else {
            // Hack!
            if (player.position.y < 0.1f) {
                parts.ForEach(p => p.NotifyPlayerTouchedGround());
                nextPart = rightHand;
            }
        }

    }

    private void StartLevel(int index) {
        hitsLeft = hitsPerLevel;
        foreach (MoveConfig config in levels.GetChild(currentLevel).GetComponentsInChildren<MoveConfig>()) {
            if (config.part == MoveConfig.Part.LeftHand) {
                leftHand.SetConfig(config);
            } else if (config.part == MoveConfig.Part.RightHand) {
                rightHand.SetConfig(config);
            } else if (config.part == MoveConfig.Part.Brain) {
                brain.SetConfig(config);
            } else {
                Debug.LogError("You messed up your configs");
            }
        }
        nextPart = rightHand; // Sequences always start with left hand
    }

    public void NotifyPartAtTarget(Moveable part) {
        if (parts.All(p => p.atTarget)) {
            parts.ForEach(p => p.NotifyAllAtInitialTargets());
        }
    }

    public void NotifyPlayerTouchedPart(Moveable part) {
        /*if (parts.Any(p => !p.atTarget)) {
            return;
        }*/


        if (part == nextPart) {
            if (nextPart.OtherCanMoveToNextPlayerSpot()) {
                nextPart = nextPart == leftHand ? rightHand : leftHand;
                nextPart.NotifyPlayerTouchedOtherHand();
            }
        }
    }

    public void NotifyPlayerAttackedPart(Moveable part) {
        if (part == brain) {
            audioManager.PlayClip(audioManager.hurtClip);
            brainScript.NotifyHurt();
            hitsLeft--;
            if (hitsLeft <= 0) {
                Vector3 brainToPlayer = player.position - brain.transform.position;
                player.GetComponent<Rigidbody2D>().velocity = brainToPlayer.normalized * 10;
                currentLevel++;
                if (currentLevel < levels.childCount) {
                    StartLevel(currentLevel);
                } else {
                    ActivateEndState();
                }
            }
        }
    }

    private void ActivateEndState() {
        endMenu.SetActive(true);
        heart.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground";
        string time = (Time.timeSinceLevelLoad - gameStartTime).ToString("F1");
        string s = "thanks for playing!\ntime: " + time + " sec\nmade in 48 hours for LD46\n\"keep it alive\"\n@maxbize";
        endMenuBody1.text = s;
        endMenuBody2.text = s;
    }

}
