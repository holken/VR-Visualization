using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using pointcloud_objects;

public class MenuHandler : MonoBehaviour {

    public GameObject playerController;
    public GameObject spaceManager;
    public GameObject currActiveButton;
    public GameObject[] menus;
    public GameObject activeMenu;
    public GameObject hoverText;
    public GameObject nbrColumns;
    public GameObject columnWidth;
    public GameObject gradientText;
    public GameObject selectedBackground;
    public GameObject graphVariableText;

    public static MenuHandler Instance;

    void Start() {
        if (Instance == null)
            Instance = this;

        activeMenu.SetActive(true);
        
        UpdateColumnSize();
        UpdateNbrColumns();
        gradientText.GetComponent<Text>().text = spaceManager.GetComponent<GradientManager>().getGradient().name;
    }

    private int currentMenu;
    /*
     * 0 = empty menu
     * 1 = tool menu
     * 2 = settings
     * 3 = Measure Tool Settings
     * 4 = Box Tool Settings
     */

    public bool buttonPressed(GameObject buttonObj) {
        

        string button = "";
        if (buttonObj.GetComponent<UIButton>()) {
            button = buttonObj.GetComponent<UIButton>().buttonFunction; 
        } else if (buttonObj.GetComponent<UICheckBox>()) {
            button = buttonObj.GetComponent<UICheckBox>().buttonFunction;
        } else if (buttonObj.GetComponent<UIRadioButton>()) {
            button = buttonObj.GetComponent<UIRadioButton>().buttonFunction;
        }

        if (button.Equals("Measure"))
        {
            pickTool(1);
            DeSelect();
            buttonObj.GetComponent<UIButton>().Select();
            currActiveButton = buttonObj;
            return true;
        }
        else if (button.Equals("SelectionBox"))
        {
            pickTool(2);
            DeSelect();
            buttonObj.GetComponent<UIButton>().Select();
            currActiveButton = buttonObj;
            return true;
        }
        else if (button.Equals("Grip"))
        {
            pickTool(3);
            DeSelect();
            buttonObj.GetComponent<UIButton>().Select();
            currActiveButton = buttonObj;
            return true;
        }
        else if (button.Equals("Photo"))
        {
            pickTool(4);
            DeSelect();
            buttonObj.GetComponent<UIButton>().Select();
            currActiveButton = buttonObj;
            return true;
        }
        else if (button.Equals("SaveObjects"))
        {
            saveObjects();
        }
        else if (button.Equals("Delete"))
        {
            pickTool(5);
            DeSelect();
            buttonObj.GetComponent<UIButton>().Select();
            currActiveButton = buttonObj;
            return true;
        }
        else if (button.Equals("Save"))
        {
            SaveGame();
        }
        else if (button.Equals("Load"))
        {
            LoadGame();
        }
        else if (button.Equals("Note"))
        {
            pickTool(6);
            DeSelect();
            buttonObj.GetComponent<UIButton>().Select();
            currActiveButton = buttonObj;
            return true;
        }
        else if (button.Equals("Music"))
        {
            if (!spaceManager.GetComponent<AudioSource>().enabled)
            {
                spaceManager.GetComponent<AudioSource>().enabled = true;
            }
            else
            {
                spaceManager.GetComponent<AudioSource>().enabled = false;
            }
        }
        else if (button.Equals("ToolSetting"))
        {
            activeMenu.SetActive(false);

            if (!currActiveButton)
            {
                menus[0].SetActive(true);
                activeMenu = menus[0];
            }
            if (currActiveButton)
            {
                if (currActiveButton.GetComponent<UIButton>().buttonFunction.Equals("Measure"))
                {
                    menus[3].SetActive(true);
                    activeMenu = menus[3];
                    Debug.Log("Measure");
                }
                else if (currActiveButton.GetComponent<UIButton>().buttonFunction.Equals("SelectionBox"))
                {
                    menus[4].SetActive(true);
                    activeMenu = menus[4];
                }
                else
                {
                    menus[0].SetActive(true);
                    activeMenu = menus[0];
                }
            }


        }
        else if (button.Equals("Settings"))
        {
            activeMenu.SetActive(false);
            menus[2].SetActive(true);
            activeMenu = menus[2];
        }
        else if (button.Equals("Tools"))
        {
            activeMenu.SetActive(false);
            menus[1].SetActive(true);
            activeMenu = menus[1];
        }
        else if (button.Equals("MeasureSizeNeg"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateSize(-1);
        }
        else if (button.Equals("MeasureSizePos"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateSize(1);
        }
        else if (button.Equals("MeasureFontColorLeft"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateFontColor(-1);
        }
        else if (button.Equals("MeasureFontColorRight"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateFontColor(1);
        }
        else if (button.Equals("MeasureLineColorRight"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateLineColor(1);
        }
        else if (button.Equals("MeasureLineColorLeft"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateLineColor(-1);
        }
        else if (button.Equals("ChangeUnitRight"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateUnitPointer(-1);
        }
        else if (button.Equals("ChangeUnitLeft"))
        {
            spaceManager.GetComponent<MeasureTool>().UpdateUnitPointer(-1);
        }
        else if (button.Equals("SaveMeasurement"))
        {
            spaceManager.GetComponent<MeasureTool>().ToggleSaveObjects();
        }
        else if (button.Equals("ResetGalaxy"))
        {
            spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().ReLoadScene();
        }
        else if (button.Equals("Resize"))
        {
            playerController.GetComponent<ControllerHandler>().SetMode(7);
            return true;
        }
        else if (button.Equals("RiemannSum"))
        {
            spaceManager.GetComponent<RiemannSumTool>().CreateGraph(DataLoader.dataLoader.minData[SpaceUtilities.Instance.currentVariableForGraph],
                DataLoader.dataLoader.maxData[SpaceUtilities.Instance.currentVariableForGraph]); //TODO fix for multidata
        }
        else if (button.Equals("Analytics"))
        {
            activeMenu.SetActive(false);
            menus[5].SetActive(true);
            activeMenu = menus[5];
        }
        else if (button.Equals("Plus1Column"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateNbrOfColumns(1);
            UpdateNbrColumns();
        }
        else if (button.Equals("Plus10Column"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateNbrOfColumns(10);
            UpdateNbrColumns();
        }
        else if (button.Equals("Plus100Column"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateNbrOfColumns(100);
            UpdateNbrColumns();
        }
        else if (button.Equals("Minus1Column"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateNbrOfColumns(-1);
            UpdateNbrColumns();
        }
        else if (button.Equals("Minus10Column"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateNbrOfColumns(-10);
            UpdateNbrColumns();
        }
        else if (button.Equals("Minus100Column"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateNbrOfColumns(-100);
            UpdateNbrColumns();
        }
        else if (button.Equals("Plus1ColumnWidth"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateColumnSize(0.001f);
            UpdateColumnSize();
        }
        else if (button.Equals("Plus10ColumnWidth"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateColumnSize(0.01f);
            UpdateColumnSize();
        }
        else if (button.Equals("Plus100ColumnWidth"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateColumnSize(0.1f);
            UpdateColumnSize();
        }
        else if (button.Equals("Minus1ColumnWidth"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateColumnSize(-0.001f);
            UpdateColumnSize();
        }
        else if (button.Equals("Minus10ColumnWidth"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateColumnSize(-0.01f);
            UpdateColumnSize();
        }
        else if (button.Equals("Minus100ColumnWidth"))
        {
            spaceManager.GetComponent<RiemannSumTool>().UpdateColumnSize(-0.1f);
            UpdateColumnSize();
        }
        else if (button.Equals("SelectData"))
        {
            Debug.Log("I made it to selectData");
            spaceManager.GetComponent<BoxSelection>().SetSaveObjects(false);
        }
        else if (button.Equals("MarkData"))
        {
            Debug.Log("I made it to MarkData");
            spaceManager.GetComponent<BoxSelection>().SetSaveObjects(true);
        }
        else if (button.Equals("ChangeGradientLeft"))
        {
            spaceManager.GetComponent<GradientManager>().GradientIncremention(-1);
            gradientText.GetComponent<Text>().text = spaceManager.GetComponent<GradientManager>().getGradient().name;
        }
        else if (button.Equals("ChangeGradientRight"))
        {
            spaceManager.GetComponent<GradientManager>().GradientIncremention(1);
            gradientText.GetComponent<Text>().text = spaceManager.GetComponent<GradientManager>().getGradient().name;
        }
        else if (button.Equals("LinearScaleGraph"))
        {
            spaceManager.GetComponent<RiemannSumTool>().SetIsLogarithmic(false);
        }
        else if (button.Equals("LogScaleGraph"))
        {
            spaceManager.GetComponent<RiemannSumTool>().SetIsLogarithmic(true);
        }
        else if (button.Equals("SpawnNewDataSet"))
        {
            spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().AddNewDataSet();
        }
        else if (button.Equals("DeleteDataSet"))
        {
            spaceManager.GetComponent<SpaceUtilities>().dataLoader.GetComponent<DataLoader>().DeleteDataSet();
        }
        else if (button.Equals("Undo"))
        {
            Debug.Log("Undo pressed");
            if (!(ActionBuffer.actionBuffer.Count <= 1))
            {
                ActionBuffer.actionBuffer[ActionBuffer.actionBuffer.Count - 2].Undo();
                Debug.Log("Undone");
            }
        }
        else if (button.Equals("3DHistGraph"))
        {
            spaceManager.GetComponent<DDDHistogram>().CreateGraph();
        } else if (button.Equals("ChangeVariableLeft"))
        {
            spaceManager.GetComponent<SpaceUtilities>().variableIncremention(1);
            graphVariableText.GetComponent<Text>().text = DataLoader.dataLoader.labelNames[spaceManager.GetComponent<SpaceUtilities>().currentVariableForGraph];
        }
        else if (button.Equals("ChangeVariableRight"))
        {
            spaceManager.GetComponent<SpaceUtilities>().variableIncremention(-1);
            graphVariableText.GetComponent<Text>().text = DataLoader.dataLoader.labelNames[spaceManager.GetComponent<SpaceUtilities>().currentVariableForGraph];
        } else if (button.Equals("DimData"))
        {
            Settings.Instance.dimStars = !Settings.Instance.dimStars;
        }
        return false;
    }
    

    private void pickTool(int index) {
        playerController.GetComponent<ControllerHandler>().SetMode(index);
   }

    private void saveObjects() {
        playerController.GetComponent<ControllerHandler>().ToggleSaveObject();
    }

    private void SaveGame() {
		spaceManager.GetComponent<SaveHandler>().SaveGame(1);
    }

    private void LoadGame() {
		spaceManager.GetComponent<SaveHandler>().LoadGame(1);
    }

    private void DeSelect() {
        if (currActiveButton) {
            //selectedBackground.SetActive(false);
            if (currActiveButton.GetComponent<UIButton>()) {
                currActiveButton.GetComponent<UIButton>().DeSelect();
            }
        }
    }

    public void ActivateHoverText(string text) {
        hoverText.GetComponent<Text>().text = text;
        hoverText.SetActive(true);
    }

    public void DeactivateHoverText() {
        hoverText.SetActive(false);
    }

    public void UpdateNbrColumns() {
        nbrColumns.GetComponent<Text>().text = spaceManager.GetComponent<RiemannSumTool>().nbrOfColumns.ToString();
    }

    public void UpdateColumnSize() {
        columnWidth.GetComponent<Text>().text = spaceManager.GetComponent<RiemannSumTool>().columnSize.ToString();
    }
}
