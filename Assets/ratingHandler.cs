using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ratingHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public RectTransform draggable;

    public Transform ListHolder;
    public List<Transform> items;
    public RectTransform LastLeftItem;
    public RectTransform LastRightItem;
    public int LastLeftIndex;
    public int LastRightIndex;
    
    private float offset = 400;
    private int sizeOffset;
    // Start is called before the first frame update
    void Awake()
    {
        Image img = new Image();
        items.Insert(items.Count/2, );
        sizeOffset = 1200 / (items.Count + 1);
        int counter = 0;
        foreach (var item in items)
        {
            if (item != null)
                item.transform.position = new Vector3(ListHolder.position.x - offset + sizeOffset * counter, ListHolder.position.y,
                0);
            //if (counter == (items.Count / 2) - 1) counter++;
            counter++;
        }
        /*
        int index = -1;
        int count = 0;
        foreach (var item in items)
        {
            if (item == LastLeftItem)
            {
                index = count;
                break;
            }
            else count++;
        }

        LastLeftIndex = index;
        LastRightIndex = index + 1;
        */
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggable.anchoredPosition += eventData.delta;
        /*
        int counter = 0;
        foreach (var item in items)
        {
            if (item != null)
            {
                if (item.transform.position.x < draggable.position.x)
                {
                    
                }
                item.transform.position = new Vector3(ListHolder.position.x - offset + sizeOffset * counter, ListHolder.position.y,
                    0);
            }
            //if (counter == (items.Count / 2) - 1) counter++;
            counter++;
        }*/
        /*
        if (LastLeftItem != null && draggable.position.x < LastLeftItem.position.x)
        {
            LastLeftItem.position = new Vector3(LastLeftItem.position.x + sizeOffset, LastLeftItem.position.y, 0);

            LastRightItem = LastLeftItem;
            LastRightIndex--;
            if (LastLeftIndex != 0)
            {
                LastLeftItem = items[LastLeftIndex - 1];
                LastLeftIndex--;
                
            }
            else
            {
                LastLeftItem = null;
            }
        } 
        else if (LastRightItem != null && draggable.position.x > LastRightItem.position.x)
        {
            LastRightItem.position = new Vector3(LastRightItem.position.x - sizeOffset, LastRightItem.position.y, 0);

            LastLeftItem = LastRightItem;
            LastLeftIndex++;
            if (LastRightIndex != items.Count - 1)
            {
                LastRightItem = items[LastRightIndex + 1];
                LastRightIndex++;
            }
            else
            {
                LastRightItem = null;
            }
            Debug.Log(LastRightItem);
        } 
        */
    }
}
