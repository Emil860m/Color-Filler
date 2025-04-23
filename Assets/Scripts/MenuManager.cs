using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class HttpResponse
{
    public string level { get; set; }
    public decimal entropy { get; set; }

}


public class MenuManager : MonoBehaviour
{
    public Material gameHueMaterial;
    
    public bool dropAnim;

    private Animator m_transitionAnimator;

    public GameObject canvas;
    public GameObject fader;
    public float timeDelay;

    private SoundManager m_soundManager;

    private Transform m_object;
    
    [Header("Start Animation")] public List<RectTransform> transInLvl = new List<RectTransform>();
    public Transform lvlTrans;

    public GameObject Graphy;

    private IEnumerator colorChange()
    {
        int rand = Random.RandomRange(0, 50);
        while (true)
        {
            int newRand = Random.RandomRange(0, 50);
            
            if (newRand < rand && newRand > rand -10 )
            {
                newRand -= 15;
                if (newRand < 0)
                {
                    newRand = 0;
                }
            }
            else if (newRand > rand && newRand < rand + 10)
            {
                newRand += 15;
                if (newRand > 50)
                {
                    newRand = 50;
                }
            }

            rand = newRand;
            Debug.Log(rand);
            gameHueMaterial.SetFloat("_HueShift", rand);
            yield return new WaitForSeconds(4);
        }
    }
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.U) && Input.GetKeyDown(KeyCode.A))
        {
            PlayerPrefs.SetInt("Progress", 46);
            PlayerPrefs.Save();

            Debug.Log("Progress set to: " + PlayerPrefs.GetInt("Progress"));
        }
        if (Input.GetKey(KeyCode.G) && Input.GetKeyDown(KeyCode.E))
        {
            Graphy.SetActive(!Graphy.activeSelf);
        }
    }

    private void Awake()
    {
        if (PlayerPrefs.GetInt("Progress") < 1)
        {
            PlayerPrefs.SetInt("Progress", 1);
        }

        Application.targetFrameRate = 60;
        
        StartCoroutine(nameof(colorChange));
        
        fader.SetActive(true);
        
        Debug.Log("Progress: " + PlayerPrefs.GetInt("Progress"));

        //set sound manager variable
        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();


        
        StartLvlAnim();

    }

    public IEnumerator RequestApi(string url, System.Action<List<HttpResponse>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError($"Request failed: {request.error}");
                callback?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;

            // Wrap array for JsonUtility
            string wrappedJson = "{ \"responses\": " + json + " }";

            HttpResponseList responseList = JsonUtility.FromJson<HttpResponseList>(wrappedJson);

            callback?.Invoke(responseList.responses);
        }
    }

    // Wrapper class for deserialization with JsonUtility
    [System.Serializable]
    private class HttpResponseList
    {
        public List<HttpResponse> responses;
    }

    private void HandleResponses(List<HttpResponse> responses)
    {
        if (responses == null)
        {
            Debug.Log("No responses received.");
            return;
        }

        foreach (var response in responses)
        {

            Debug.Log($"Status: {response}");
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
        

        if (dropAnim)
        {
            foreach (RectTransform child in lvlTrans)
            {
                transInLvl.Add(child);
            }
            
            //drop anim
            if (!m_soundManager.noStartAnim)
            {
                for (int u = 0; u < transInLvl.Count; u++)
                {
                    try
                    {
                        StartCoroutine(StartDropAnim(transInLvl[u]));
                    }
                    catch
                    {
                        Debug.Log("u is " + u);
                    }
                }
            }
        }
    }
    
    private IEnumerator StartDropAnim(RectTransform trans)
    {
        float yPos = trans.position.y;
        trans.position = new Vector3(trans.position.x, trans.position.y + 200, trans.position.z);

        yield return new WaitForSeconds((trans.position.x / 100 + trans.position.y / 100 + 8) / 25f);

        for (float i = -3f; i < 4; i = i + 0.1f)
        {
            trans.position = new Vector3(trans.position.x, yPos + (Mathf.Round(Mathf.Pow(2, -i) * 100) / 100) - 0.0625f,
                trans.position.z);
            yield return new WaitForSeconds(0.01f);
        }

        //trans.position = new Vector3(trans.position.x, yPos, trans.position.z);
    }
    
    public void NewGame()
    {
        List<string> levels = new List<string>();
        levels.Add("1;1;1;1|1;1;1a;1|1;1;1b;4p|4b;1p;1;4a");
        levels.Add("0;2;2;1|2;1p;1b;1|2;2;1a;4b|0;4p;4a;2");
        levels.Add("0;0;1;1;0|1;1;1;1;4c|1;4a;1;1b;4p|1;1a;1c;1;1p|4b;2;1;1;1");
        levels.Add("2;2;0;4c|1p;1b;1;4b|1a;2;2;1c|4a;4p;1;1");
        levels.Add("0;0;0;0;0|4p;1;4a;4b;1|2;1p;1;1;0|1;1b;1;1a;1|2;1;1;1;2");
        levels.Add("1;1;1;1;4b|1;1;1a;1;1p|2;1;1;1c;2|0;1;1;1b;4a|0;4p;4c;2;2");
        levels.Add("4b;1;1;0;0|1;1;1;0;0|1b;1;2;4p;0|1;1a;1;0;0|4a;1p;0;1;0");
        levels.Add("2;1;2;0;0|1p;1a;1;1;1|1b;1;1;1;1|1;4p;2;4a;2|4b;1;1;1;2");
        levels.Add("2;2;2;4c;4p|4b;1;1b;1p;4a|1d;1a;1c;2;2|1;1;0;1;0|4d;1;1;0;0");
        Shuffle(levels);
        levelManager.Instance.SetLevelStrings(levels);
        StartCoroutine(nameof(NewGameCo));
    }
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    private IEnumerator NewGameCo()
    {
        Debug.Log("New Game");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //start new game
        //PlayerPrefs.SetInt("Progress", 1);
        //load next lvl
        SceneManager.LoadScene(4);
    }
    
    public void ContinueGame()
    {
        StartCoroutine(nameof(ContinueGameCo));
    }
    private IEnumerator ContinueGameCo()
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

    public void LvlSelect()
    {
        StartCoroutine(nameof(LvlSelectCo));
    }
    private IEnumerator LvlSelectCo()
    {
        Debug.Log("Lvl select");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);

        //load next lvl
        SceneManager.LoadScene(47);
    }
    
    public void Controls()
    {
        StartCoroutine(nameof(ControlsCo));
    }
    private IEnumerator ControlsCo()
    {
        Debug.Log("Controls");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);

        //load next lvl
        SceneManager.LoadScene(48);
    }
    
    
    public void ResetProgress()
    {
        StartCoroutine(nameof(ResetProgressCo));
    }
    private IEnumerator ResetProgressCo()
    {
        Debug.Log("Reset Progress");
        yield return new WaitForSeconds(timeDelay);
        //start the start end animation
        m_transitionAnimator.Play("End Transition");
        //wait for animation to finish
        yield return new WaitForSeconds(m_transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        //load next lvl
        SceneManager.LoadScene(49);
    }
}
    
