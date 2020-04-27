using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public PostProcessing pp;
    public CameraManager cameraManager;

    private List<Moveable> parts;
    private Moveable nextPart; // Next part player needs to touch to advance Player level sequence
    private int hitsLeft;
    private float gameStartTime;
    private bool releasedBoth;

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
            if (!releasedBoth) {
                releasedBoth = !Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.C);
            } else if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.C)) {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
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
        if (part == nextPart) {
            if (nextPart.OtherCanMoveToNextPlayerSpot()) {
                nextPart = nextPart == leftHand ? rightHand : leftHand;
                nextPart.NotifyPlayerTouchedOtherHand();
            }
        }
    }

    public void NotifyPlayerAttackedPart(Moveable part) {
        if (part == brain) {
            cameraManager.AddScreenShake(0.3f);
            audioManager.PlayClip(audioManager.hurtClip);
            brainScript.NotifyHurt();
            hitsLeft--;
            if (hitsLeft <= 0) {
                cameraManager.AddScreenShake(0.8f);
                pp.SendPulse(brain.GetComponent<Collider2D>().bounds.center, 200f);
                Vector3 brainToPlayer = player.position - brain.transform.position;
                player.GetComponent<CharController>().ForceJump(brainToPlayer.normalized);
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
        releasedBoth = false;
        endMenu.SetActive(true);
        heart.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground";
        string time = (Time.timeSinceLevelLoad - gameStartTime).ToString("F1");
        string s = "Thanks for playing!\nTime: " + time + " sec\nx + c to restart\n\n@maxbize";
        endMenuBody1.text = s;
        endMenuBody2.text = s;
    }

    public bool InMenus() {
        return startMenu.activeSelf || endMenu.activeSelf;
    }
}
