using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LvlCounter : MonoBehaviour
{
    public Text lvlCounter;

    // Start is called before the first frame update
    private void Awake()
    {
        lvlCounter.text = "Lvl " + SceneManager.GetActiveScene().buildIndex;
    }
}