using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniPXIHandler : MonoBehaviour
{
    [System.Serializable]
    public class PXIQuestion
    {
        public string questionText;
        public float response;
    }

    public GameObject questionPrefab; // A prefab with a Text and Slider
    public Transform questionsParent;
    public Button submitButton;

    public int yOffsetStart;
    public int yOffset;

    private List<PXIQuestion> questions = new List<PXIQuestion>();
    private List<Slider> sliders = new List<Slider>();

    void Start()
    {
        InitializeQuestions();
        submitButton.onClick.AddListener(OnSubmit);
    }

    void InitializeQuestions()
    {
        string[] prompts = new string[]
        {
            "I liked the look and feel of the game",
            "The game was not too easy and not too hard to play",
            "It was easy to know how to perform actions in the game",
            "The goals of the game were clear to me",
            "The game gave clear feedback on my progress towards the goals",
            "I felt free to play the game in my own way",
            "I wanted to explore how the game evolved",
            "I was fully focused on the game",
            "I felt I was good at playing this game",
            "Playing the game was meaningful to me",
            "I had a good time playing this game"
        };

        int counter = 0;
        foreach (string prompt in prompts)
        {
            var qObj = Instantiate(questionPrefab, questionsParent);
            qObj.transform.position = new Vector3(transform.position.x, transform.position.y + yOffsetStart + yOffset * counter, 0);
            counter++;
            qObj.transform.Find("QuestionText").GetComponent<Text>().text = prompt;

            Slider slider = qObj.transform.Find("ResponseSlider").GetComponent<Slider>();
            slider.minValue = 1;
            slider.maxValue = 7;
            slider.value = 4; // Neutral start
            sliders.Add(slider);

            questions.Add(new PXIQuestion { questionText = prompt });
        }
    }

    void OnSubmit()
    {
        bool allFours = true;
        for (int i = 0; i < questions.Count; i++)
        {
            if (((int)sliders[i].value) != 4)
            {
                allFours = false;
            }
        }

        if (!allFours)
        {
            Dictionary<string, int> PXImap = new Dictionary<string, int>();
            for (int i = 0; i < questions.Count; i++)
            {
                PXImap.Add(questions[i].questionText, (int) sliders[i].value);
            }
            DataManager.Instance.AddPXI(PXImap);
            Application.Quit(0);
        }
    }
}
