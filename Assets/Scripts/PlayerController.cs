using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    public void Jump(InputAction.CallbackContext context){
        if (context.performed){
            Debug.Log("Jump");
        }
    }
}
