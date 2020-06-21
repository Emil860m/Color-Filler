using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetProgressManager : MonoBehaviour
{
    private Animator m_transitionAnimator;
    
    public GameObject canvas;
    public GameObject fader;


    private void Awake()
    {
        fader.SetActive(true);
    }

    private void Start()
    {
        StartLvlAnim();
    }
    
    private void StartLvlAnim()
    {
        //activate fader
        canvas.SetActive(true);
        //set transition animator
        m_transitionAnimator = fader.GetComponent<Animator>();
        //start the start animation
        m_transitionAnimator.Play("Start Transition");
    }

    private IEnumerator ResetProgressCo()
    {
        Debug.Log("Reset Progress");
        yield return new WaitForSeconds(0.25f);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //reset progress
        PlayerPrefs.SetInt("Progress", 1);
        PlayerPrefs.Save();
        //load next lvl
        SceneManager.LoadScene(0);
    }
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(nameof(GoToMenu));
        }
        if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.P))
        {
            StartCoroutine(nameof(ResetProgressCo));
        }
    }
    
    private IEnumerator GoToMenu()
    {
        yield return new WaitForSeconds(0.25f);
        Debug.Log("going to menu");
        //start the end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish + time delay
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //reset current lvl
        SceneManager.LoadScene(0);
    }
}
