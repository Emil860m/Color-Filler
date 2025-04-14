using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance { get { if (_instance != null) return _instance; else return new DataManager(); } }

    public List<string> rankings;
    public List<int> levelNumbers;
    public List<int> inputCount;
    public List<int> resetCount;
    public List<long> timeCount;
    public int lvlCount = 1;

    public Dictionary<string, int> PXI;

    public void AddPXI(Dictionary<string, int> map)
    {
        PXI = map;
        // Set a variable to the Documents path.
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Append text to an existing file named "WriteLines.txt".
        using (StreamWriter outputFile = new StreamWriter("ColorFillerData.txt", true))
        {
            outputFile.WriteLine("NewEntry");
            outputFile.WriteLine("PXI");
            foreach (var key in PXI.Keys)
            {
                outputFile.WriteLine(key + "=" + PXI[key]);
            }
            outputFile.WriteLine("RANKINGS");
            for (int i = 0; i < rankings.Count; i++)
            {
                outputFile.WriteLine("Rank" + i + "=" + rankings[i]);
            }
            outputFile.WriteLine("STATS");
            Debug.Log("inputLength" + inputCount.Count);
            Debug.Log("resetLength" + resetCount.Count);
            Debug.Log("timeLength" + timeCount.Count);
            for (int i = 0;i < inputCount.Count; i++)
            {
                string lvl = levelManager.Instance.levels[i];
                outputFile.WriteLine(lvl + "?inputs=" + inputCount[i] + "&resets=" + resetCount[i] + "&time=" + timeCount[i]);
            }
        }
    }

    public void AddRanking(int index, string lvl)
    {
        if (rankings == null || rankings.Count == 0)
        {
            rankings = new List<string>();
            rankings.Add(lvl);
            levelNumbers = new List<int>();
            levelNumbers.Add(lvlCount);
        } else
        {
            rankings.Insert(index, lvl);
            levelNumbers.Insert(index, lvlCount);
        }
        lvlCount++;
    }
    public void NewLevel()
    {
        resetCount.Add(0);
        inputCount.Add(0);
        timeCount.Add(0);
    }

    public void AddInput(int inputs)
    {
        //if (inputCount == null || inputCount.Count == 0) inputCount.Add(0);
        inputCount[lvlCount - 1] += inputs;
        Debug.Log("Inputs total: " + inputCount[lvlCount - 1]);
    }
    
    public void AddTime(long time)
    {
        timeCount[lvlCount - 1] += time;
    }


    public void AddReset(int inputs, long time)
    {
        //if (resetCount == null || resetCount.Count == 0) resetCount.Add(0);
        inputCount[lvlCount - 1] += inputs;
        timeCount[lvlCount - 1] += time;
        resetCount[lvlCount-1]++;
        Debug.Log("Reset++");
    }


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
        if (resetCount == null || resetCount.Count == 0) resetCount.Add(0);
        if (inputCount == null || inputCount.Count == 0) inputCount.Add(0);
        if (timeCount == null || timeCount.Count == 0) timeCount.Add(0);
    }
}
