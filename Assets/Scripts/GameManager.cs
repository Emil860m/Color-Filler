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
    public string lvlStr;
    public UnityEngine.Object playerObj;
    public UnityEngine.Object squareObj;
    public UnityEngine.Object holeObj;
    public UnityEngine.Object redHoleObj; 
    public UnityEngine.Object yellowHoleObj; 
    public UnityEngine.Object greenHoleObj; 
    public UnityEngine.Object blueHoleObj; 
    public UnityEngine.Object redBlockObj; 
    public UnityEngine.Object yellowBlockObj; 
    public UnityEngine.Object greenBlockObj; 
    public UnityEngine.Object blueBlockObj; 

    private int LvlNumber;


    public bool lvlSelector;
    public Camera camera;


    private PlayerController playerController;

    public GameObject undoHelpText;

    private int levelsLeft;


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

        lvlStr = levelManager.Instance.getlvlStr();
        levelsLeft = levelManager.Instance.levelsLeft();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        playerController.delayInput = true;
        playerController.input = true;

        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();
        GameObject lvl = GameObject.Find("Lvl Prefabs");
        lvlTrans = lvl.transform;
        if (lvlStr != null && lvlStr != "")
        {

            int startX = -1;
            int startY = -1;
            int xcounter = 2;
            int holeCount = 0;
            int blockCount = 0;
            List<UnityEngine.Object> holes = new List<UnityEngine.Object>();
            holes.Add(redHoleObj);
            holes.Add(blueHoleObj);
            holes.Add(yellowHoleObj);
            holes.Add(greenHoleObj);
            List<UnityEngine.Object> blocks = new List<UnityEngine.Object>();
            blocks.Add(redBlockObj);
            blocks.Add(blueBlockObj);
            blocks.Add(yellowBlockObj);
            blocks.Add(greenBlockObj);
            foreach (var row in lvlStr.Split('|'))
            {
                int ycounter = 0;
                foreach (var cell in row.Split(';'))
                {
                    if (cell.StartsWith("1"))
                    {
                        GameObject tile = Instantiate(squareObj) as GameObject;
                        tile.transform.parent = lvlTrans.transform;
                        Vector3 pos = new Vector3(startY + ycounter, startX + xcounter, 0);
                        tile.transform.localPosition = pos;
                        transInLvl.Add(tile.transform);
                        if (cell.Length > 1)
                        {
                            GameObject block;
                            if (cell.EndsWith("p"))
                            {
                                block = GameObject.Find("Player");
                            }
                            else
                            {
                                block = Instantiate(blocks[blockCount]) as GameObject;
                                blockCount++;
                            }
                            block.transform.parent = lvlTrans.transform;
                            block.transform.localPosition = pos;
                            transInLvl.Add(block.transform);
                        }

                    }
                    else if (cell.StartsWith("2"))
                    {
                        // disappearing block
                    }
                    else if (cell.StartsWith("4"))
                    {
                        GameObject tile;
                        if (cell.EndsWith("p"))
                        {
                            tile = Instantiate(holeObj) as GameObject;
                        }
                        else
                        {
                            tile = Instantiate(holes[holeCount]) as GameObject;
                            holeCount++;
                        }
                        tile.transform.parent = lvlTrans.transform;
                        Vector3 pos = new Vector3(startY + ycounter, startX + xcounter, 0);
                        tile.transform.localPosition = pos;
                        transInLvl.Add(tile.transform);
                    }
                    ycounter++;
                }
                xcounter--;
            }
        }
        else
        {
            foreach (Transform child in lvlTrans)
            {
                transInLvl.Add(child);
            }
        }
        transInLvl.Sort(delegate (Transform a, Transform b)
        {
            if (a.position.y < b.position.y) return 1;
            else if (a.position.y > b.position.y) return -1;
            else if (a.position.x > b.position.x) return 1;
            else if (a.position.x < b.position.x) return -1;
            else if (a.tag == "Player" || a.tag == "Pieces") return 1;
            else if (b.tag == "Ground") return 1;
            else if (a.tag == "Ground") return -1;
            return 0;
        });
        string lvlstrFromLevel = "";
        int lastX = (int) transInLvl[0].position.x;
        int lastY = (int) transInLvl[0].position.y;
        int highX = -999;
        int highY = -999;
        int lowX = 999;
        int lowY = 999;
        int itemCounter = 0;
        foreach (Transform child in transInLvl)
        {
            if (child.position.x > highX) highX = (int) child.position.x;
            else if (child.position.x < lowX) lowX = (int) child.position.x;
            if (child.position.y > highY) highY = (int) child.position.y;
            else if (child.position.y < lowY) lowY = (int) child.position.y;
        }
        for (int i = highY; i >= lowY; i--)
        {
            for (int j = lowX; j <= highX; j++)
            {
                if (itemCounter < transInLvl.Count)
                {
                    Transform child = transInLvl[itemCounter];
                    if (transInLvl[itemCounter].position.y == i && transInLvl[itemCounter].position.x == j)
                    {
                        if (child.name.Contains("Square")) lvlstrFromLevel += "1";
                        else if (child.name.Contains("Broken")) lvlstrFromLevel += "2";
                        else if (child.tag == "Holes")
                        {
                            lvlstrFromLevel += "4";
                            if (child.name.Contains("Red"))
                            {
                                lvlstrFromLevel += "a";
                            }
                            if (child.name.Contains("Yellow"))
                            {
                                lvlstrFromLevel += "b";
                            }
                            if (child.name.Contains("Blue"))
                            {
                                lvlstrFromLevel += "c";
                            }
                            if (child.name.Contains("Green"))
                            {
                                lvlstrFromLevel += "d";
                            }
                            if (child.name.Contains("Black"))
                            {
                                lvlstrFromLevel += "p";
                            }
                        }
                        if (itemCounter + 1 < transInLvl.Count)
                        {

                            child = transInLvl[itemCounter + 1];
                            if (transInLvl[itemCounter + 1].position.y == i && transInLvl[itemCounter + 1].position.x == j)
                            {
                                if (child.name.Contains("Red"))
                                {
                                    lvlstrFromLevel += "a";
                                }
                                if (child.name.Contains("Yellow"))
                                {
                                    lvlstrFromLevel += "b";
                                }
                                if (child.name.Contains("Blue"))
                                {
                                    lvlstrFromLevel += "c";
                                }
                                if (child.name.Contains("Green"))
                                {
                                    lvlstrFromLevel += "d";
                                }
                                if (child.tag == "Player")
                                {
                                    lvlstrFromLevel += "p";
                                }
                                itemCounter++;
                            }
                        }
                        itemCounter++;
                    }
                    else lvlstrFromLevel += "0";
                }
                else lvlstrFromLevel += "0";
                if (j != highX)
                    lvlstrFromLevel += ";";
            }
            if (i != lowY)
                lvlstrFromLevel += "|";
        }
        Debug.Log(lvlstrFromLevel);

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
        if (levelsLeft > 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
            // todo: show questionnaire
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