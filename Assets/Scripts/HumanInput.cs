using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInput : MonoBehaviour
{
    private CharController charController;

    private bool jumpDownFlag;
    private bool attackDownFlag;

    // Start is called before the first frame update
    void Start() {
        charController = GetComponent<CharController>();
    }

    private void Update() {
        jumpDownFlag |= Input.GetKeyDown(KeyCode.X);
        attackDownFlag |= Input.GetKeyDown(KeyCode.C);
    }

    // Update is called once per frame
    void FixedUpdate() {
        charController.Move(Vector2.right * Input.GetAxisRaw("Horizontal"), Input.GetKey(KeyCode.X), jumpDownFlag, attackDownFlag);
        jumpDownFlag = false;
        attackDownFlag = false;
    }
}
