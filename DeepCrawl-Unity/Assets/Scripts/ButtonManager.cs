using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This class is used as wrapper to use the UI button element 
// (and its press events) with the ECS system. This button can be used the same 
// as the keyboard button in Unity
public class ButtonManager : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{

    bool isClicked = false;
    bool isPressed = false;
    Button button;

    protected void Start()
    {
        button = GetComponent<Button>();
        // When the button is pressed, start the courutine
        button.onClick.AddListener(() => StartCoroutine(setClicked()));
    }

    IEnumerator setClicked()
    {
        isClicked = true;
        // Wait the end of the frame to set the isCLicked bool to false.
        // This simulate the onClick() function and can be used with ECS system
        yield return new WaitForEndOfFrame();
        isClicked = false;
    }

    public bool GetButtonDown()
    {
        return isClicked;
    }

    public bool GetPressedDown()
    {
        return isPressed;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void disable()
    {
        if (button != null)
        {
            button.interactable = false;
        }

    }

    public void enable()
    {
        if (button != null)
            button.interactable = true;
    }
}
