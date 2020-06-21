using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHoleController : MonoBehaviour
{
    public bool newGame;
    public bool continueGame;

    private Animator m_transitionAnimator;

    public GameObject canvas;
    public GameObject fader;
    public float timeDelay;

    private SoundManager m_soundManager;

    private Transform m_object;

    public bool endOfGame;


    private void Awake()
    {
        //activate fader
        canvas.SetActive(true);
        //set transition animator
        m_transitionAnimator = fader.GetComponent<Animator>();
        //set sound manager variable
        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();

        //so it only gets triggered once by "new game" object
        if (newGame)
        {
            //start the start animation
            StartCoroutine(nameof(StartLvl));
        }

        if (endOfGame)
        {
            PlayerPrefs.SetInt("Progress", 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        m_soundManager.PlayFillSound();

        m_object = other.gameObject.transform;

        StartCoroutine(nameof(Fill));

        if (newGame)
        {
            StartCoroutine(nameof(NewGame));
        }
        else if (continueGame)
        {
            StartCoroutine(nameof(ContinueGame));
        }
    }

    private IEnumerator StartLvl()
    {
        //start the start animation
        m_transitionAnimator.Play("Start Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length / 2);
        //reset current lvl
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().input = true;
    }

    private IEnumerator NewGame()
    {
        Debug.Log("New Game");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //start new game
        PlayerPrefs.SetInt("Progress", 1);
        PlayerPrefs.Save();
        //load next lvl
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator ContinueGame()
    {
        Debug.Log("Continue Game");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);

        //load next lvl
        SceneManager.LoadScene(PlayerPrefs.GetInt("Progress"));
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
            m_object.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
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
            m_object.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }
}