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
    public List<GameObject> radioButtons;

    void Start() {
        //uncheckedImage = GetComponent<Image>().sprite;
        spaceManager = menuManager.GetComponent<MenuHandler>().spaceManager;
        rightController = spaceManager.GetComponent<PlayerParts>().rightHand;
        panel = transform.parent.gameObject;
    }

    

    public void HighLightButton() {
        GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        highlighting = true;
        panel.GetComponent<LineRenderer>().sharedMaterial.color = new Color(0f, 1f, 0f);
        panel.GetComponent<CursorTarget>().animationAmount = panel.GetComponent<CursorTarget>().GetOriginalAnimationAmount() * 2f;
        rightController.GetComponent<ControllerHandler>().VibrateController(1000);
        panel.GetComponent<CursorTarget>().PlayHoverSound();

    }

    public void DeSelect() {
        GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
        transform.localScale = new Vector3(1, 1, 1);
        panel.GetComponent<LineRenderer>().sharedMaterial.color = new Color(1f, 0f, 1f);
        panel.GetComponent<CursorTarget>().animationAmount = transform.parent.GetComponent<CursorTarget>().GetOriginalAnimationAmount();

        highlighting = false;
    }

    public void DeSelecting()
    {
        boxChecked = false;
        GetComponent<Image>().sprite = uncheckedImage;
    }

    public void DeHighlight()
    {

    }

    public void SelectButton() {
        if (!boxChecked) {
            GetComponent<Image>().sprite = checkedImage;
            boxChecked = true;
        } else {
            //GetComponent<Image>().sprite = uncheckedImage;
            //boxChecked = false;
            return;
        }
        menuManager.GetComponent<MenuHandler>().buttonPressed(gameObject);
        panel.GetComponent<CursorTarget>().PlaySelectSound();

        foreach (GameObject o in radioButtons) {
            o.GetComponent<UIRadioButton>().DeSelecting();
        }
    }


}
