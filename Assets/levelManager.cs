using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelManager : MonoBehaviour
{
    private static levelManager _instance;
    public static levelManager Instance { get { return _instance; } }


    public List<string> levels;
    private int levelCount = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetLevelStrings(List<string> lst)
    {
        levels = lst;
    }

    public int levelsLeft()
    {
        return levels.Count - levelCount;
    }

    public string getlvlStr()
    {
        if (levelCount >= levels.Count) return "";
        string lvl = levels[levelCount];
        levelCount++;
        return lvl;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
