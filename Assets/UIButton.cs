using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IUIButton {

    private bool highlighting;
    private bool selected = false;
    public string buttonFunction;
    private GameObject spaceManager;
    private GameObject rightController;
    private bool firstTouch = true;
    public GameObject menuManager;
    private GameObject panel;

    void Start() {
        spaceManager = menuManager.GetComponent<MenuHandler>().spaceManager;
        rightController = spaceManager.GetComponent<PlayerParts>().rightHand;
        panel = transform.parent.gameObject;
    }

    void LateUpdate() {
        if (!highlighting &&!selected) {
            GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            transform.localScale = new Vector3(1, 1, 1);
            if (!firstTouch) {
                panel.GetComponent<LineRenderer>().sharedMaterial.color = new Color(1f, 0f, 1f);
                panel.GetComponent<CursorTarget>().animationAmount = transform.parent.GetComponent<CursorTarget>().GetOriginalAnimationAmount();
                menuManager.GetComponent<MenuHandler>().DeactivateHoverText();
            }
            firstTouch = true;
            
        }
        
        highlighting = false;
    }

    public void HighLightButton() {
        GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        highlighting = true;
        panel.GetComponent<LineRenderer>().sharedMaterial.color = new Color(0f, 1f, 0f);
        panel.GetComponent<CursorTarget>().animationAmount = panel.GetComponent<CursorTarget>().GetOriginalAnimationAmount() * 2f;
        menuManager.GetComponent<MenuHandler>().ActivateHoverText(gameObject.name);
        if (firstTouch) {
            rightController.GetComponent<ControllerHandler>().VibrateController(1000);
            panel.GetComponent<CursorTarget>().PlayHoverSound();
            firstTouch = false;
        }
    }

    public void SelectButton() {
        GameObject selectedBackground = menuManager.GetComponent<MenuHandler>().selectedBackground;
        bool selected = menuManager.GetComponent<MenuHandler>().buttonPressed(gameObject);
        //TODO make it work for example make it disapear when changing page
        /*if (selected) {
            selectedBackground.SetActive(true);
            selectedBackground.transform.position = transform.position;
            selectedBackground.transform.localScale = transform.localScale;
        }*/      
        panel.GetComponent<CursorTarget>().PlaySelectSound();
    }

    public void DeSelect() {
        GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
        transform.localScale = new Vector3(1, 1, 1);
        highlighting = false;
        selected = false;
    }

    public void Select() {
        selected = true;
    }
}
