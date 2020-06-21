using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleController : MonoBehaviour
{
    public string holeColor;
    private GameManager m_gameManager;

    private GameObject m_playerController;

    private Transform m_object;

    public bool completed;
    public bool filled;
    public bool blankHole;

    private SoundManager m_soundManager;



    // Start is called before the first frame update
    private void Awake()
    {
        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();

        //set variable game manager
        m_gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (filled == false)
        {
            filled = true;

            m_object = other.gameObject.transform;

            StartCoroutine(Fill());

            //if object in collider2D is a piece
            if (other.CompareTag("Pieces"))
            {
                //disable piece's collider2D
                other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                other.gameObject.GetComponent<PieceController>().pieceCompleted = true;
                //GetComponent<Collider2D>().enabled = false;

                //if the piece is the same color as the hole
                if (holeColor == other.gameObject.GetComponent<PieceController>().pieceColor)
                {
                    m_soundManager.PlayFillSound();
                    //tell game that that the hole is completed

                    gameObject.tag = "Ground";
                    
                    completed = true;

                }
                else
                {

                    //tell game that player has failed the lvl;
                    m_gameManager.LvlCompletionCheck();
                }
            }
            else if (other.CompareTag("Player"))
            {

                //disable player's collider2D
                other.gameObject.GetComponent<BoxCollider2D>().enabled = false;

                other.gameObject.GetComponent<PlayerController>().onlyRetry = true;

                //if the player is the same color as the hole
                if (holeColor == "black")
                {
                    //tell game that that the hole is completed
                    completed = true;
                }

                //check if player has completed the lvl;
                m_gameManager.LvlCompletionCheck();
            }
        }
    }

    public IEnumerator disFilled()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Debug.Log("disabled Filled");
        filled = false;
    }

    private IEnumerator Fill()
    {
        yield return new WaitUntil(() => transform.position == m_object.position);

        if (m_object.CompareTag("Pieces"))
        {
            //change the piece's sprite to filled
            PieceController PC = m_object.gameObject.GetComponent<PieceController>();
            SpriteRenderer SR = m_object.gameObject.GetComponent<SpriteRenderer>();

            for (int i = 0; i < PC.sprAnim.Count; i++)
            {
                SR.sprite = PC.sprAnim[i];
                yield return new WaitForSeconds(0.01f);
            }

            //put the piece in the floor
            m_object.gameObject.GetComponent<SpriteRenderer>().sortingOrder =
                m_object.gameObject.GetComponent<SpriteRenderer>().sortingOrder - 1;
        }
        else if (m_object.CompareTag("Player"))
        {
            //change the player's sprite to filled
            PlayerController PC = m_object.gameObject.GetComponent<PlayerController>();
            SpriteRenderer SR = m_object.gameObject.GetComponent<SpriteRenderer>();

                for (int i = 0; i < PC.sprAnim.Count; i++)
                {
                    SR.sprite = PC.sprAnim[i];
                    yield return new WaitForSeconds(0.01f);
                }
            
            //put the piece in the floor
            m_object.gameObject.GetComponent<SpriteRenderer>().sortingOrder =
                m_object.gameObject.GetComponent<SpriteRenderer>().sortingOrder - 1;
        }
    }
}