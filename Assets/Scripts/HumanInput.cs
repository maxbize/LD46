using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInput : MonoBehaviour
{
    private CharController charController;

    private bool jumpDownFlag;

    // Start is called before the first frame update
    void Start() {
        charController = GetComponent<CharController>();
    }

    private void Update() {
        jumpDownFlag |= Input.GetKeyDown(KeyCode.Space);
    }

    // Update is called once per frame
    void FixedUpdate() {
        charController.Move(Vector2.right * Input.GetAxisRaw("Horizontal"), Input.GetKey(KeyCode.Space), jumpDownFlag);
        jumpDownFlag = false;
    }
}
