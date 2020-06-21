using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProgressViewer : MonoBehaviour
{
    public Text progressText;

    public Button continueButton;
    public Button lvlSelectButton;

    public int numberOfLvls;

    // Start is called before the first frame update
    private void Awake()
    {
        if (PlayerPrefs.GetInt("Progress") != 0)
        {
            progressText.text = "Progress:  " + (PlayerPrefs.GetInt("Progress") - 1) + "/" + numberOfLvls + " Lvls";
        }
        else
        {
            progressText.text = "Progress:  " + PlayerPrefs.GetInt("Progress") + "/" + numberOfLvls + " Lvls";
        }

        if (PlayerPrefs.GetInt("Progress") < 2 || PlayerPrefs.GetInt("Progress") > numberOfLvls)
        {
            continueButton.interactable = false;
        }
        if (PlayerPrefs.GetInt("Progress") < 2)
        {
            lvlSelectButton.interactable = false;
        }
    }
}