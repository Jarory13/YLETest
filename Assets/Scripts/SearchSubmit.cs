using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ListView;

/*
 * This script should be attached to the submit button
 * Use this to add the call back functions for on submit
 * We're using a submit button so that we don't have to
 * make an API call on every key entered. This script
 * may become obsolete if such behaviour turns out to be
 * desired. 
 **/
public class SearchSubmit : MonoBehaviour
{

    //A reference to the input field where the use will be typing their search
    private InputField inputField;

    //A reference to the button we're attached to
    private Button submitButton;

    //A reference to the GameObject that holds the scrollView.
    private GameObject listScroller;


    // Use this for initialization
    void Start()
    {

        //For safety reasons let's just grab the game object then we can 
        //check if it's null before setting input field. We'll also grab the disabled scroll view.
        GameObject inputFieldGameObject = GameObject.FindGameObjectWithTag(Constants.inputTag);
        listScroller = GameObject.FindGameObjectWithTag(Constants.listViewTag);

        //Set this to be disabled. We'll renable it when we search
        if (listScroller)
        {
            listScroller.SetActive(false);
        }

        if (inputFieldGameObject)
        {
            inputField = inputFieldGameObject.GetComponent<InputField>();
        }


        //Get a reference to the button we're attached to
        submitButton = gameObject.GetComponent<Button>();

        if (submitButton)
        {
            //Add our custom on submit function dynamically
            submitButton.onClick.AddListener(delegate { onSubmitButtonPressed(); });
        }
    }

    private void onSubmitButtonPressed()
    {

        //If for some reason the scroll view isn't in the scene we need to get out of this method.
        if (!listScroller)
        {
            throw new MissingReferenceException("The Scroller is missing. Please ensure it is in the scene");
        }

        //do nothing if we have no string
        if (inputField.text.Length == 0)
        {
            return;
        }

        if (!listScroller.activeSelf)
        {
            listScroller.SetActive(true);
        }
        else
        {
            //we've already done a search and turned the weblist on, but
            //We need to do everything over again. OnNewSearch can handle this
            listScroller.GetComponent<WebList>().OnNewSearch.Invoke();
        }

    }
}
