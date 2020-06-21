using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class EndScene : MonoBehaviour
{
    private Animator m_transitionAnimator;
    
    public GameObject canvas;
    public GameObject fader;
    public float timeDelay;
    
    public Material gameHueMaterial;

    private void Awake()
    {
        fader.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartLvlAnim();
        StartCoroutine(nameof(colorChange));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(nameof(GoToMenu));
        }
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
    
    private IEnumerator GoToMenu()
    {
        yield return new WaitForSeconds(0.25f);
        Debug.Log("going to menu");
        yield return new WaitForSeconds(timeDelay);
        //start the end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish + time delay
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //reset current lvl
        SceneManager.LoadScene(0);
    }
    
    private IEnumerator colorChange()
    {
        while (true)
        {
            int rand = Random.RandomRange(0, 50);
            Debug.Log(rand);
            gameHueMaterial.SetFloat("_HueShift", rand);
            yield return new WaitForSeconds(3);
        }
    }
    
}
