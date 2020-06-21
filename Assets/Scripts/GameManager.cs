using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Animator m_transitionAnimator;


    public GameObject canvas;
    public GameObject fader;
    public float timeDelay;

    private SoundManager m_soundManager;

    [Header("Game Hue")] public Material gameHueMaterial;
    [Range(0, 270)] public float gameHue = 0;
    public bool activeHueControl;

    [Header("Start Animation")] public List<Transform> transInLvl = new List<Transform>();

    public bool[] arrFinished;
    public Transform lvlTrans;

    private int LvlNumber;


    public bool lvlSelector;
    public Camera camera;


    private PlayerController playerController;

    public GameObject undoHelpText;


    private void Awake()
    {
        try
        {
            Regex regex = new Regex("[0-9]+");
            Match match = regex.Match(SceneManager.GetActiveScene().name);
            LvlNumber = Convert.ToInt32(match.Value);
            Debug.Log("lvl number is " + LvlNumber);


            gameHueMaterial.SetFloat("_HueShift", 1.5f * LvlNumber);
        }
        catch
        {
            Debug.Log("Unable to find lvl number in scene name");
        }

        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        playerController.delayInput = true;
        playerController.input = true;

        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();

        lvlTrans = GameObject.Find("Lvl Prefabs").transform;
        foreach (Transform child in lvlTrans)
        {
            transInLvl.Add(child);
        }

        arrFinished = new bool[transInLvl.Count];

        StartLvlAnim();
        playerController.retryCounter = GameObject.FindGameObjectWithTag("Retry Counter").GetComponent<Text>();

        if (!m_soundManager.noStartAnim)
        {
            StartCoroutine(DelayInput(arrFinished));
        }
    }


    private void Start()
    {
        undoHelpText = GameObject.FindWithTag("undoHelpText");
    }

    public void LvlCompletionCheck()
    {
        Debug.Log("Checking LVL COMPLETE");
        //put pieces in scene into an array
        GameObject[] m_holes = GameObject.FindGameObjectsWithTag("Holes");
        //set blank holes to complete
        foreach (var hole in m_holes)
        {
            if (hole.GetComponent<HoleController>().blankHole)
            {
                hole.GetComponent<HoleController>().completed = true;
            }
        }

        //if there all pieces is completed
        if (m_holes.All(hole => hole.GetComponent<HoleController>().completed))
        {
            m_soundManager.PlayWonSound();

            //up anim
            if (transInLvl.Count == 0)
            {
                foreach (Transform child in lvlTrans)
                {
                    transInLvl.Add(child);
                }
            }


            for (int u = 0; u < transInLvl.Count; u++)
            {
                arrFinished[u] = false;
                try
                {
                    StartCoroutine(EndUpAnim(transInLvl[u], u));
                }
                catch
                {
                }
            }

            m_soundManager.noStartAnim = false;

            //load next lvl
            StartCoroutine(nameof(NextLvl));
        }
        else
        {
            undoHelpText.GetComponent<Text>().enabled = true;

            m_soundManager.PlayFailSound();


            if (playerController.remainingUndoes <= 0 || m_holes.All(hole => hole.GetComponent<HoleController>().filled))
            {
                Debug.Log("Failed lvl test");

                //reset current lvl
                StartCoroutine(nameof(ResetLvl));
            }
        }
    }


    private void Update()
    {
        if (lvlSelector)
        {
            Vector2 cameraPos = new Vector2(camera.transform.position.x, camera.transform.position.y);
            int gameHue = Convert.ToInt32(7 * cameraPos.x + 150);
            gameHueMaterial.SetFloat("_HueShift", gameHue);
        }

        if (activeHueControl)
        {
            gameHueMaterial.SetFloat("_HueShift", gameHue);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject.Find("Player").GetComponent<PlayerController>().input = false;
            StartCoroutine(nameof(GoToMenu));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("reseting lvl");
            m_soundManager.PlayFailSound();

            StartCoroutine(nameof(ResetLvl));
        }

        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.T) && Input.GetKey(KeyCode.L) &&
            Input.GetKeyDown(KeyCode.N))
        {
            GameObject.Find("Player").GetComponent<PlayerController>().input = false;
            SkipLvl();
        }
    }

    private bool IsAllBoolsTrue(bool[] bools)
    {
        for (int i = 0; i < bools.Length; ++i)
        {
            if (bools[i] == false)
            {
                return false;
            }
        }

        return true;
    }

    private void StartLvlAnim()
    {
        //activate fader
        canvas.SetActive(true);
        //set transition animator
        m_transitionAnimator = fader.GetComponent<Animator>();
        //start the start animation
        m_transitionAnimator.Play("Start Transition");

        //drop anim
        if (!m_soundManager.noStartAnim)
        {
            for (int u = 0; u < transInLvl.Count; u++)
            {
                arrFinished[u] = false;
                try
                {
                    StartCoroutine(StartDropAnim(transInLvl[u], u));
                }
                catch
                {
                    Debug.Log("u is " + u);
                }
            }
        }
    }

    private IEnumerator DelayInput(bool[] bools)
    {
        if (!m_soundManager.noStartAnim)
        {
            GameObject.Find("Player").GetComponent<PlayerController>().input = false;
            GameObject.Find("Player").GetComponent<PlayerController>().delayInput = false;
            yield return new WaitUntil(() => IsAllBoolsTrue(bools) == true);
            Debug.Log("Input Delay is over.");

            m_soundManager.noStartAnim = true;
        }

        GameObject.Find("Player").GetComponent<PlayerController>().delayInput = true;
        GameObject.Find("Player").GetComponent<PlayerController>().input = true;
    }

    private IEnumerator StartDropAnim(Transform trans, int u)
    {
        float yPos = trans.position.y;
        trans.position = new Vector3(trans.position.x, trans.position.y + 20, trans.position.z);

        yield return new WaitForSeconds((trans.position.x + trans.position.y + 8) / 25f);

        for (float i = -3f; i < 4; i = i + 0.1f)
        {
            trans.position = new Vector3(trans.position.x, yPos + (Mathf.Round(Mathf.Pow(2, -i) * 100) / 100) - 0.0625f,
                trans.position.z);
            yield return new WaitForSeconds(0.01f);
        }

        trans.position = new Vector3(trans.position.x, yPos, trans.position.z);
        arrFinished[u] = true;
    }

    private IEnumerator EndUpAnim(Transform trans, int u)
    {
        float yPos = trans.position.y;

        yield return new WaitForSeconds((trans.position.x - trans.position.y + 8) / 15f);

        for (float i = -1f; i < 6; i = i + 0.1f)
        {
            trans.position = new Vector3(trans.position.x, yPos + (Mathf.Round(Mathf.Pow(2, i) * 100) / 100) - 0.5f,
                trans.position.z);
            yield return new WaitForSeconds(0.01f);
        }

        arrFinished[u] = true;
    }

    private IEnumerator GoToMenu()
    {
        //up anim
        if (transInLvl.Count == 0)
        {
            foreach (Transform child in lvlTrans)
            {
                transInLvl.Add(child);
            }
        }


        for (int u = 0; u < transInLvl.Count; u++)
        {
            arrFinished[u] = false;
            try
            {
                StartCoroutine(EndUpAnim(transInLvl[u], u));
            }
            catch
            {
            }
        }

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

    private IEnumerator ResetLvl()
    {
        Debug.Log("reseting lvl");

        GameObject.Find("Player").GetComponent<PlayerController>().delayInput = false;

        yield return new WaitForSeconds(0.5f);
        //start the end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish + time delay
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length / 2);
        yield return new WaitForSeconds(timeDelay);
        //reset current lvl
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator NextLvl()
    {
        //progress
        if (PlayerPrefs.GetInt("Progress") == LvlNumber)
        {
            Debug.Log("increasing progess now");
            PlayerPrefs.SetInt("Progress", LvlNumber + 1);
            PlayerPrefs.Save();
        }

        Debug.Log("progress is " + PlayerPrefs.GetInt("Progress"));


        yield return new WaitForSeconds(1f);
        Debug.Log("going to next lvl");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);

        //load next lvl
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void SkipLvl()
    {
        Debug.Log("Skipping to next lvl");
        //gave progress
        PlayerPrefs.SetInt("Progress", PlayerPrefs.GetInt("Progress") + 1);
        PlayerPrefs.Save();
        
        //load next lvl
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public IEnumerator GoToLvl(int lvlNum)
    {
        //up anim
        if (transInLvl.Count == 0)
        {
            foreach (Transform child in lvlTrans)
            {
                transInLvl.Add(child);
            }
        }


        for (int u = 0; u < transInLvl.Count; u++)
        {
            arrFinished[u] = false;
            try
            {
                StartCoroutine(EndUpAnim(transInLvl[u], u));
            }
            catch
            {
            }
        }

        m_soundManager.noStartAnim = false;

        yield return new WaitForSeconds(1f);
        Debug.Log("going to next lvl");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //load next lvl
        SceneManager.LoadScene(lvlNum);
    }
}