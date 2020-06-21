using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverController : MonoBehaviour
{
    private const float k_RayCastLength = 0.1f;
    private const float k_PositionOffset = 0.5f;

    public Vector2 m_move;


    public Transform m_object;

    private SoundManager m_soundManager;

    private float moveSpeed;

    public List<GameObject> piecesInFront = new List<GameObject>();

    private PlayerController playerController;

    public GameObject border;

    private bool _waiting = false;

    // Start is called before the first frame update
    private void Awake()
    {
        moveSpeed = GameObject.Find("Player").GetComponent<PlayerController>().moveSpeed;
        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        
        FindRotation();
        //make a border under the mover regardless of rotation
        AddBorder();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        playerController.input = false;
        m_object = other.gameObject.transform;
        piecesInFront.Add(m_object.gameObject);
        StartCoroutine(WaitForObject());
    }

    private IEnumerator WaitForObject()
    {
        yield return new WaitUntil(() => m_object.position == transform.position);
        Mover();
    }

    private void Mover()
    {
        nextSpace:

        //if there is an object in front
        if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move))
        {
            if (_waiting)
            {
                _waiting = false;
                StopCoroutine(nameof(WaitUntilAbleToMove));
                Debug.Log("Waiting stopped!");
            }
            
            //if that object is a Piece or player
            if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                    .CompareTag("Pieces") || RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider
                    .gameObject.CompareTag("Player"))
            {
                //add Piece object to the end of the list
                piecesInFront.Add(
                    RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject);
                //goto nextSpace, check next block
                goto nextSpace;
            }

            if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                    .CompareTag("Holes") ||
                RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                    .CompareTag("Ground") || RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider
                    .gameObject.CompareTag("Mover"))
            {

                m_soundManager.PlayMoveSound();
                Debug.Log("mover moving");

                //move all objects in array
                for (var i = piecesInFront.Count - 1; i >= 0; i--)
                {
                    if (piecesInFront[i].gameObject.CompareTag("Player"))
                    {
                        playerController.prevMoves[playerController.prevMoves.Count - 1] = m_move;
                    }
                    
                    //move object in front of player in the direction that the player is moving in
                    StartCoroutine(SmoothMove(piecesInFront[i], piecesInFront[i].transform,
                        new Vector3(piecesInFront[i].transform.position.x + m_move.x,
                            piecesInFront[i].transform.position.y + m_move.y, 0)));
                }
                
                playerController.movedPieces[playerController.movedPieces.Count - 1] += piecesInFront.Count - 1;
            }

            if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                .CompareTag("Border")) m_soundManager.PlayMoveFailSound();
        }
        else //if there isn't an object, do nothing
        {
            playerController.input = true;
            playerController.moverMoving = false;


            if (!_waiting)
            {
                _waiting = true;
                StartCoroutine(nameof(WaitUntilAbleToMove));
            }
        }

        piecesInFront.Clear();
    }

    private IEnumerator WaitUntilAbleToMove()
    {

            Debug.Log("Waiting started!");
            while (_waiting)
            {
                yield return new WaitUntil(() => playerController.input);
                Debug.Log("1!");
                yield return new WaitUntil(() => !playerController.input);
                Debug.Log("2!");
                yield return new WaitUntil(() => playerController.input);

                if (RayCheckCenterBool(transform))
                {
            
                    playerController.input = false;
                    m_object = RayCheckCenter(transform).collider.gameObject.transform;
                    Debug.Log(RayCheckCenter(transform).collider.gameObject.transform.position);
                    piecesInFront.Add(m_object.gameObject);
            
                    Debug.Log("Waiting: tried to move!");
                    Mover();
                }
        }
    }
    

    RaycastHit2D RayCheck(Transform center, Vector2 direction)
    {
        RaycastHit2D[] rayCheckHits =
            Physics2D.RaycastAll(new Vector2(center.position.x + m_move.x, center.position.y + m_move.y),
                new Vector2(direction.x * k_PositionOffset, direction.y * k_PositionOffset), k_RayCastLength);

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                return hit;
            }
        }

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Pieces"))
            {
                return hit;
            }
        }

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Holes"))
            {
                return hit;
            }
        }

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                return hit;
            }
        }

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Mover"))
            {
                return hit;
            }
        }

        return new RaycastHit2D();
    }
    
    RaycastHit2D RayCheckCenter(Transform center)
         {
             RaycastHit2D[] rayCheckHits =
                 Physics2D.RaycastAll(new Vector2(center.position.x, center.position.y),
                     new Vector2(k_PositionOffset,k_PositionOffset), k_RayCastLength);
     
             foreach (RaycastHit2D hit in rayCheckHits)
             {
                 if (hit.collider.gameObject.CompareTag("Player"))
                 {
                     return hit;
                 }
             }
     
             foreach (RaycastHit2D hit in rayCheckHits)
             {
                 if (hit.collider.gameObject.CompareTag("Pieces"))
                 {
                     return hit;
                 }
             }
     
             return new RaycastHit2D();
         }
    
    bool RayCheckCenterBool(Transform center)
    {
        RaycastHit2D[] rayCheckHits =
            Physics2D.RaycastAll(new Vector2(center.position.x, center.position.y),
                new Vector2(k_PositionOffset,k_PositionOffset), k_RayCastLength);

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }

        foreach (RaycastHit2D hit in rayCheckHits)
        {
            if (hit.collider.gameObject.CompareTag("Pieces"))
            {
                return true;
            }
        }

        return false;
    }


    private IEnumerator SmoothMove(GameObject piece, Transform startPos, Vector3 endPos)
    {
        playerController.input = false;

        playerController.moverMoving = true;


        //set the variable that is how many percent the object has completed on the path
        float m_movePos = 0;
        //start a loop that loops until the object has reached its target
        while (startPos.position != endPos)
        {
            //move object
            piece.transform.position = Vector3.Lerp(startPos.position, endPos, m_movePos);
            m_movePos = m_movePos + moveSpeed / 200;
            yield return new WaitForSecondsRealtime(1 / 10 / moveSpeed);
        }

        //round the objects position so it doesn't stray away from the grid
        piece.transform.position = new Vector3(Mathf.RoundToInt(piece.transform.position.x),
            Mathf.RoundToInt(piece.transform.position.y), 0);

        playerController.input = true;

        playerController.moverMoving = false;
    }

    private void FindRotation()
    {
        switch (Convert.ToInt32(Mathf.Round(transform.rotation.eulerAngles.z)))
        {
            case 0:
                m_move.x = m_move.x - 1;
                break;
            case 90:
                m_move.y = m_move.y - 1;
                break;
            case 180:
                m_move.x = m_move.x + 1;
                break;
            case 270:
                m_move.y = m_move.y + 1;
                break;
        }
    }

    private void AddBorder()
    {
        border = Instantiate(border, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z),
            Quaternion.identity);
        border.transform.parent = transform;
    }
}