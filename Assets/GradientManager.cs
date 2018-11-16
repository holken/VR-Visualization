using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientManager : MonoBehaviour {
    public Gradient[] gradients;
    private Gradient currGradient;
    private float fillAmountPerPart;
    private int nbrColors;
    private int count = 0;
    [SerializeField]
    private int currGradientIndex = 0;

    void Awake() {
        currGradient = gradients[Settings.Instance.currGradientIndex];
        nbrColors = currGradient.colorScheme.Length;
        fillAmountPerPart = 1f / (float)nbrColors;
    }
    

    public void setGradient(int index) {
        currGradient = gradients[index];
        nbrColors = currGradient.colorScheme.Length;
        fillAmountPerPart = 1f / (float) nbrColors;
    }

    public Gradient getGradient() {
        return currGradient;
    }

    public int getIndex() {
        return Settings.Instance.currGradientIndex;
    }

    //TODO Remove duplicate code etc
    public void GradientIncremention(int incr) {
        Settings.Instance.currGradientIndex += incr;
        if (Settings.Instance.currGradientIndex >= gradients.Length) {
            Settings.Instance.currGradientIndex = 0;
        } else if (Settings.Instance.currGradientIndex < 0) {
            Settings.Instance.currGradientIndex = gradients.Length - 1;
        }
        currGradient = gradients[Settings.Instance.currGradientIndex];
        nbrColors = currGradient.colorScheme.Length;
        fillAmountPerPart = 1f / (float)nbrColors;
    }


    public Color getColor(float colorIndex) {
        //Extreme cases
       if (colorIndex > 1) {
            return currGradient.colorScheme[nbrColors - 1];
       } else if (colorIndex < 0) {
            return currGradient.colorScheme[0];
        }

        Color color;
        float indexRough = colorIndex * nbrColors;
        float colorSpectrumSize = (1 / nbrColors);
        //int indexSmaller = Mathf.FloorToInt(indexRough / colorSpectrumSize);
        int indexLarger = Mathf.CeilToInt(indexRough);

        //More extreme cases
        if (indexLarger <= 0) {
            indexLarger = 1;
        }
        

        int indexSmaller = Mathf.FloorToInt(indexRough);

        float rest = indexRough - indexSmaller;
        float red = 0;
        float green = 0;
        float blue = 0;
       
        if (indexSmaller == 0) {
            //TODO think if this is a true representation, especially if we gonna take * rest and if we should use indexSmaller
            red = currGradient.colorScheme[indexSmaller].r * (1 - rest * (1f / 2f)) + currGradient.colorScheme[indexLarger].r * rest * (1f / 2f);
            green = currGradient.colorScheme[indexSmaller].g * (1 - rest * (1f / 2f)) + currGradient.colorScheme[indexLarger].g * rest * (1f / 2f);
            blue = currGradient.colorScheme[indexSmaller].b * (1 - rest * (1f / 2f)) + currGradient.colorScheme[indexLarger].b * rest * (1f / 2f);

        } else if (indexLarger == nbrColors) {
            red = currGradient.colorScheme[indexSmaller - 1].r * ((1f / 2f) - rest * (1f / 2f)) + currGradient.colorScheme[indexLarger - 1].r * ((1f / 2f) + rest * (1f / 2f));
            green = currGradient.colorScheme[indexSmaller - 1].g * ((1f / 2f) - rest * (1f / 2f)) + currGradient.colorScheme[indexLarger - 1].g * ((1f / 2f) + rest * (1f / 2f));
            blue = currGradient.colorScheme[indexSmaller - 1].b * ((1f / 2f) - rest * (1f / 2f)) + currGradient.colorScheme[indexLarger - 1].b * ((1f / 2f) + rest * (1f / 2f));
        } else if (rest <= 0.5) {

            red = currGradient.colorScheme[indexSmaller - 1].r * ((1f / 2f) - rest) + currGradient.colorScheme[indexLarger - 1].r * ((1f / 2f) + rest);
            green = currGradient.colorScheme[indexSmaller - 1].g * ((1f / 2f) - rest) + currGradient.colorScheme[indexLarger - 1].g * ((1f / 2f) + rest);
            blue = currGradient.colorScheme[indexSmaller - 1].b * ((1f / 2f) - rest) + currGradient.colorScheme[indexLarger - 1].b * ((1f / 2f) + rest);
        } else {
            red = currGradient.colorScheme[indexSmaller].r * (1 - (rest - 0.5f)) + currGradient.colorScheme[indexLarger].r * (rest - 0.5f);
            green = currGradient.colorScheme[indexSmaller].g * (1 - (rest - 0.5f)) + currGradient.colorScheme[indexLarger].g * (rest - 0.5f);
            blue = currGradient.colorScheme[indexSmaller].b * (1 - (rest - 0.5f)) + currGradient.colorScheme[indexLarger].b * (rest - 0.5f);
            
        }


        color = new Color(red, green, blue);
        /*if (count % 100 == 0)
        {
            Debug.Log("color: " + color);
        }*/
        count++;
            return color;

        
    }
	
}
