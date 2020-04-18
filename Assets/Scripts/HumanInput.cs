using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInput : MonoBehaviour
{
    private CharController charController;

    // Start is called before the first frame update
    void Start() {
        charController = GetComponent<CharController>();
    }

    // Update is called once per frame
    void FixedUpdate() {

        charController.Move(Vector2.right * Input.GetAxisRaw("Horizontal"));

        if (Input.GetKey(KeyCode.Space)) {
            charController.Jump();
        }
    }
}
