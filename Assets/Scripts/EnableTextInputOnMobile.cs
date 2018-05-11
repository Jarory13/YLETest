using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableTextInputOnMobile : MonoBehaviour {

    InputField input;
    private TouchScreenKeyboard keyboard;

    // Use this for initialization
    void Start () {
        input = gameObject.GetComponent<InputField>();
	}
	
	// Update is called once per frame
	void Update () {
        if (input.isFocused)
        {
            
            keyboard = TouchScreenKeyboard.Open(input.text, TouchScreenKeyboardType.Default, false, false);
            Debug.Log("We got focus");
            if (keyboard != null)
            {
                Debug.Log("we should have a keyboard");
            }
        }
	}

    
}
