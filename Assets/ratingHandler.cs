using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ratingHandler : MonoBehaviour, IEndDragHandler, IDragHandler
{
    public RectTransform draggable;

    public Transform ListHolder;
    private List<Transform> items;
    public int itemCount = 0;
    private GameObject dragInsert;
    private int insertIndex;
    public Canvas canvas;
    public UnityEngine.UI.Button nextLevelButton;
    public GameManager gm;
    /*
    public RectTransform LastLeftItem;
    public RectTransform LastRightItem;
    public int LastLeftIndex;
    public int LastRightIndex;
    */
    public GameObject draggablePrefab;
    public GameObject miniPXI;

    private float offset = 800;
    private int sizeOffset;
    // Start is called before the first frame update
    void Awake()
    {
        //nextLevelButton = transform.Find("nextLevelButton");
        itemCount = DataManager.Instance.rankings.Count;
        items = new List<Transform>();
        sizeOffset = 1800 / (itemCount + 1);
        for (int i = 0; i < itemCount; i++)
        {
            var qObj = Instantiate(draggablePrefab, transform);
            var text = qObj.transform.Find("levelNumber");
            text.GetComponent<Text>().text = "Level " + DataManager.Instance.levelNumbers[i];
            items.Add(qObj.transform);
        }
        var drag = Instantiate(draggablePrefab, transform);
        draggable.transform.Find("levelNumber").GetComponent<Text>().text = "Level " + DataManager.Instance.lvlCount;
        dragInsert = drag;
        insertIndex = items.Count/2;
        items.Insert(items.Count/2, drag.transform);
        drag.SetActive(false);
        
        int counter = 0;
        foreach (var item in items)
        {
            item.transform.position = new Vector3(ListHolder.position.x - offset + sizeOffset * counter, ListHolder.position.y, 0);
            counter++;
        }
    }
    void Start()
    {
        nextLevelButton.onClick.AddListener(nextLevel);
    }

    void nextLevel()
    {
        Debug.Log("next level pressed");
        DataManager.Instance.AddRanking(insertIndex, gm.lvlStr);
        if (levelManager.Instance.levelsLeft() == 0)
        {
            miniPXI.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            DataManager.Instance.NewLevel();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }



    public void OnEndDrag(PointerEventData eventData)
    {
        draggable.position = dragInsert.transform.position;
        nextLevelButton.interactable = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggable.anchoredPosition += eventData.delta / canvas.scaleFactor;
        if (insertIndex > 0 && insertIndex < items.Count - 1)
        {
            if (items[insertIndex - 1].position.x > draggable.position.x)
            {
                items.Remove(dragInsert.transform);
                items.Insert(insertIndex - 1, dragInsert.transform);
                insertIndex--;
            }
            else if (items[insertIndex + 1].position.x < draggable.position.x) 
            {

                items.Remove(dragInsert.transform);
                items.Insert(insertIndex + 1, dragInsert.transform);
                insertIndex++;
            }
        }
        else if (insertIndex == 0)
        {
            if (items[insertIndex + 1].position.x < draggable.position.x)
            {

                items.Remove(dragInsert.transform);
                items.Insert(insertIndex + 1, dragInsert.transform);
                insertIndex++;
            }
        }
        else if (insertIndex == items.Count - 1)
        {
            if (items[insertIndex - 1].position.x > draggable.position.x)
            {
                items.Remove(dragInsert.transform);
                items.Insert(insertIndex - 1, dragInsert.transform);
                insertIndex--;
            }
        }
        int counter = 0;
        foreach (var item in items)
        {
            item.transform.position = new Vector3(ListHolder.position.x - offset + sizeOffset * counter, ListHolder.position.y, 0);
            counter++;
        }
    }
}
