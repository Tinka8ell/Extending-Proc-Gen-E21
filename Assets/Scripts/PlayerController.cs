using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    /*
    private RigidBody rigidBody;

    private void Awake(){
        rigidBody = GetComponent<RigidBody>();
    }
    */
    
    public void Jump(InputAction.CallbackContext context){
        if (context.performed){
            Debug.Log("Jump");
            // rigidBody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
}
