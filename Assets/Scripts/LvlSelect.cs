using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvlSelect : MonoBehaviour
{
    private GameManager gameManager;
        
    public SpriteRenderer SR;
    public SpriteRenderer childSR;

    public Color normal;
    public Color mouseOver;
    public Color mouseDown;
    public Color mouseDownText;
    public Color notUnlockedText;

    public bool specialLvl;
    public int specialLvlNum;
    public int adjacentLvlNum;

    private int lvlNum;
    public TextMesh lvlTextNum;


    private bool hasBeenClicked;

    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        
        if (!specialLvl)
        {
            lvlNum = Convert.ToInt32(lvlTextNum.text);
            if (PlayerPrefs.GetInt("Progress") < lvlNum)
            {
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                SR.color = mouseDown;
                childSR.color = mouseDown;
                lvlTextNum.color = notUnlockedText;
            }
        }
        else
        {
            lvlNum = specialLvlNum;
            if (PlayerPrefs.GetInt("Progress") < adjacentLvlNum)
            {
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                SR.color = mouseDown;
                childSR.color = mouseDown;
                lvlTextNum.color = notUnlockedText;
            }
        }



    }

    private void OnMouseEnter()
    {
        if (hasBeenClicked) return;

        StartCoroutine(ColorFader(mouseOver));
        
        //SR.color = mouseOver;
        //childSR.color = mouseOver;
    }

    private void OnMouseExit()
    {
        if (hasBeenClicked) return;
        
        StartCoroutine(ColorFader(normal));
        //SR.color = normal;
        //childSR.color = normal;
    }


    private void OnMouseDown()
    {
        StartCoroutine(ColorFader(mouseDown));
        //SR.color = mouseDown;
        //childSR.color = mouseDown;

        lvlTextNum.color = mouseDownText;
        
        Debug.Log("Mouse Down on " + lvlNum);
        
        hasBeenClicked = true;

        StartCoroutine(gameManager.GoToLvl(lvlNum));
    }

    private IEnumerator ColorFader(Color targetColor)
    {
        float timeLeft = 0.1f;
        while (timeLeft > 0)
        {
            SR.color = Color.Lerp(SR.color, targetColor, Time.deltaTime / timeLeft);
            childSR.color = Color.Lerp(childSR.color, targetColor, Time.deltaTime / timeLeft);
 
            // update the timer
            timeLeft -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SR.color = targetColor;
    }
}
