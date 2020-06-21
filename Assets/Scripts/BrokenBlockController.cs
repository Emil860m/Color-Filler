using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenBlockController : MonoBehaviour
{
    private Transform player;

    public GameObject blockEdge;

    private bool playerIsHere;
    public bool skipDisable;

    private void Awake()
    {
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

        player = GameObject.FindWithTag("Player").transform;

    }

    private void Update()
    {
        if (player.position == gameObject.transform.position && !playerIsHere)
        {
            playerIsHere = true;
        }
        else if (player.position != gameObject.transform.position && playerIsHere)
        {
            if (!skipDisable)
            {
                Debug.Log("disabling X ground");
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                blockEdge.GetComponent<SpriteRenderer>().enabled = false;
                gameObject.GetComponent<BrokenBlockController>().enabled = false;
                playerIsHere = false;

            }
            else
            {
                playerIsHere = false;
                StartCoroutine(nameof(WaitToEnable));
            }

        }
    }

    private IEnumerator WaitToEnable()
    {
        yield return new WaitForSeconds(0.1f);
        skipDisable = false;
    }
}