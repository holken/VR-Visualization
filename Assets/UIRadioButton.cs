using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRadioButton : MonoBehaviour, IUIButton {

    private bool highlighting;
    public string buttonFunction;
    public GameObject menuManager;
    private GameObject spaceManager;
    private GameObject rightController;
    public Sprite uncheckedImage;
    public Sprite checkedImage;
    private GameObject panel;
    public bool boxChecked = false;
    private bool firstTouch = true;
    public List<GameObject> radioButtons;

    void Start() {
        //uncheckedImage = GetComponent<Image>().sprite;
        spaceManager = menuManager.GetComponent<MenuHandler>().spaceManager;
        rightController = spaceManager.GetComponent<PlayerParts>().rightHand;
        panel = transform.parent.gameObject;
    }

    void LateUpdate() {
        if (!highlighting) {
            GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            transform.localScale = new Vector3(1, 1, 1);
            if (!firstTouch) {
                panel.GetComponent<LineRenderer>().sharedMaterial.color = new Color(1f, 0f, 1f);
                panel.GetComponent<CursorTarget>().animationAmount = transform.parent.GetComponent<CursorTarget>().GetOriginalAnimationAmount();
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
        if (firstTouch) {
            rightController.GetComponent<ControllerHandler>().VibrateController(1000);
            panel.GetComponent<CursorTarget>().PlayHoverSound();
            firstTouch = false;
        }

    }

    public void DeSelect() {
        boxChecked = false;
        GetComponent<Image>().sprite = uncheckedImage;
        Debug.Log("Oh no I am uncheckde: " + name);
    }

    public void SelectButton() {
        if (!boxChecked) {
            GetComponent<Image>().sprite = checkedImage;
            boxChecked = true;
        } else {
            GetComponent<Image>().sprite = uncheckedImage;
            boxChecked = false;
        }
        menuManager.GetComponent<MenuHandler>().buttonPressed(gameObject);
        panel.GetComponent<CursorTarget>().PlaySelectSound();

        foreach (GameObject o in radioButtons) {
            Debug.Log("Time to uncheck: " + o.name);
            o.GetComponent<UIRadioButton>().DeSelect();
        }
    }


}
