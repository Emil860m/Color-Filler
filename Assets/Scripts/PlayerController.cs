using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private int gridSize = 1;

    private Vector2 m_move;

    public float moveSpeed;

    private const float k_RayCastLength = 0.1f;
    private const float k_PositionOffset = 0.5f;

    public List<GameObject> piecesInFront = new List<GameObject>();
    private Vector3 m_targetPosition;

    public bool input;
    public bool delayInput;

    private SoundManager m_soundManager;

    public bool moverMoving;

    public List<Sprite> sprAnim = new List<Sprite>();

    //queuing
    public List<Vector2> moveQueue = new List<Vector2>();

    //undoing
    public List<Vector2> prevMoves = new List<Vector2>();
    public List<Vector2> prevPos = new List<Vector2>();
    public int remainingUndoes;
    public List<int> movedPieces = new List<int>();
    private GameObject[] holes;
    public Text retryCounter;


    public bool onlyRetry;
    public Sprite defaultSprite;

    public GameObject undoHelpText;

    private void Awake()
    {
        remainingUndoes = 3;
        holes = GameObject.FindGameObjectsWithTag("Holes");


        m_soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();

        piecesInFront.Add(gameObject);
    }
    
    private void Start()
    {
        undoHelpText = GameObject.FindWithTag("undoHelpText");
    }

    // Update is called once per frame
    private void Update()
    {
        if (!delayInput) return;
        if (moverMoving) return;

        if (input && moveQueue.Count > 0 || input && UsableInput())
        {
            input = false;
            //if there is a move in the queue
            if (moveQueue.Count > 0)
            {
                m_move = moveQueue[0];
                moveQueue.RemoveAt(0);

                //save move for undo
                SaveMove();
            }
            //else if player presses anything
            else if (UsableInput())
            {
                //undo move
                if (Input.GetKeyDown(KeyCode.U))
                {
                    if (remainingUndoes > 0)
                    {

                        Vector2 prevMove = new Vector2(0, 0);
                        for (int i = prevMoves.Count - 1; i >= 0; i--)
                        {
                            if (prevMoves[i] != new Vector2(0, 0))
                            {
                                prevMove = prevMoves[i];
                                prevMoves[i] = new Vector2(0, 0);
                                break;
                            }
                        }

                        if (prevMove != new Vector2(0, 0))
                        {
                            m_soundManager.PlayUndoSound();
                            //undoing move
                            Debug.Log("Undoing move");
                            remainingUndoes--;
                            retryCounter.text = "Undos: " + remainingUndoes;

                            //remove move queue
                            moveQueue.RemoveRange(0, moveQueue.Count);

                            // --- XGROUND ---
                            GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
                            List<GameObject> xGrounds = new List<GameObject>();
                            foreach (var ground in grounds)
                            {
                                if (ground.transform.name.StartsWith("Broken"))
                                {
                                    xGrounds.Add(ground);
                                }
                            }

                            Debug.Log("xGrounds count: " + xGrounds.Count);

                            foreach (var xGround in xGrounds)
                            {
                                //Debug.Log("Checking for: " + xGround.transform.position + " at Pos: " + (prevPos + prevMove));

                                if (xGround.transform.position.x == transform.position.x - prevMove.x &&
                                    xGround.transform.position.y == transform.position.y - prevMove.y)
                                {
                                    xGround.GetComponent<SpriteRenderer>().enabled = true;
                                    xGround.GetComponent<BoxCollider2D>().enabled = true;
                                    xGround.GetComponent<BrokenBlockController>().blockEdge
                                        .GetComponent<SpriteRenderer>().enabled = true;
                                    xGround.GetComponent<BrokenBlockController>().enabled = true;
                                }
                                else if (xGround.transform.position.x == transform.position.x &&
                                         xGround.transform.position.y == transform.position.y)
                                {
                                    xGround.GetComponent<BrokenBlockController>().skipDisable = true;
                                }
                            }

                            //--- PIECE ---
                            if (movedPieces.Last() != 0)
                            {
                                int amountToMove = movedPieces.Last();
                                //get pieces to array
                                Vector2 prevPos = new Vector2(transform.position.x, transform.position.y);
                                List<GameObject> piecesToMove = new List<GameObject>();
                                List<Vector2> posToMove = new List<Vector2>();

                                GameObject[] pieces = GameObject.FindGameObjectsWithTag("Pieces");
                                GameObject[] movers = GameObject.FindGameObjectsWithTag("Mover");

                                Vector2 moverPos = new Vector2();
                                moverRestart:
                                for (int i = 0; i < pieces.Length; i++)
                                {
                                    if (amountToMove <= 0) break;

                                    Debug.Log("Checking piece (" + pieces[i].name + "): " +
                                              pieces[i].transform.position + " at Pos: " +
                                              (prevPos + prevMove));

                                    if (pieces[i].transform.position.x == prevPos.x + prevMove.x &&
                                        pieces[i].transform.position.y == prevPos.y + prevMove.y)
                                    {
                                        if (pieces[i].GetComponent<PieceController>().pieceCompleted)
                                        {
                                            bool found = false;
                                            for (int o = i + 1; o < pieces.Length; o++)
                                            {
                                                if (pieces[o].transform.position.x == prevPos.x + prevMove.x &&
                                                    pieces[o].transform.position.y == prevPos.y + prevMove.y)
                                                {
                                                    Debug.Log("Piece added by prioritasation");
                                                    piecesToMove.Add(pieces[o]);

                                                    if (moverPos != new Vector2(0, 0))
                                                    {
                                                        posToMove.Add(moverPos);
                                                        moverPos = new Vector2();
                                                    }
                                                    else
                                                    {
                                                        posToMove.Add(prevPos);
                                                    }

                                                    prevPos += prevMove;
                                                    i = -1;
                                                    amountToMove--;

                                                    found = true;
                                                }
                                            }

                                            if (!found)
                                            {
                                                Debug.Log("Piece added");
                                                piecesToMove.Add(pieces[i]);
                                                
                                                if (moverPos != new Vector2(0, 0))
                                                {
                                                    posToMove.Add(moverPos);
                                                    moverPos = new Vector2();
                                                }
                                                else
                                                {
                                                    posToMove.Add(prevPos);
                                                }

                                                prevPos += prevMove;
                                                i = -1;
                                                amountToMove--;
                                            }
                                        }
                                        else
                                        {
                                            Debug.Log("Piece added");
                                            piecesToMove.Add(pieces[i]);
                                            
                                            if (moverPos != new Vector2(0, 0))
                                            {
                                                posToMove.Add(moverPos);
                                                moverPos = new Vector2();
                                            }
                                            else
                                            {
                                                posToMove.Add(prevPos);
                                            }

                                            prevPos += prevMove;
                                            i = -1;
                                            amountToMove--;
                                        }
                                    }
                                }
                                foreach (var mover in movers)
                                {
                                    if (mover.transform.position.x == prevPos.x + prevMove.x &&
                                        mover.transform.position.y == prevPos.y + prevMove.y)
                                    {
                                        Debug.Log("mover found");

                                        moverPos = prevPos;

                                        prevPos += prevMove;
                                        prevMove = mover.GetComponent<MoverController>().m_move;
                                        goto moverRestart;
                                    }
                                }

                                for (int i = 0; i < piecesToMove.Count; i++)
                                {
                                    if (piecesToMove[i].GetComponent<PieceController>().pieceCompleted)
                                    {
                                        foreach (var hole in holes)
                                        {                                            
                                            if (hole.transform.position.x == piecesToMove[i].transform.position.x &&
                                                hole.transform.position.y == piecesToMove[i].transform.position.y)
                                            {
                                                hole.GetComponent<HoleController>().completed = false;
                                                hole.GetComponent<HoleController>().filled = false;
                                                //StartCoroutine(hole.GetComponent<HoleController>().disFilled());
                                                hole.tag = "Holes";
                                            }
                                        }


                                        piecesToMove[i].GetComponent<PieceController>().pieceCompleted = false;
                                        piecesToMove[i].GetComponent<BoxCollider2D>().enabled = true;
                                        piecesToMove[i].GetComponent<SpriteRenderer>().sprite =
                                            piecesToMove[i].GetComponent<PieceController>().DefaultSprite;
                                        piecesToMove[i].GetComponent<SpriteRenderer>().sortingOrder =
                                            piecesToMove[i].GetComponent<SpriteRenderer>().sortingOrder + 1;
                                    }

                                    piecesToMove[i].transform.position = posToMove[i];
                                }
                            }


                            //--- PLAYER ---
                            if (onlyRetry)
                            {
                                GetComponent<SpriteRenderer>().sprite = defaultSprite;

                                foreach (var hole in holes)
                                {
                                    if (hole.transform.position == transform.position)
                                    {
                                        hole.GetComponent<HoleController>().completed = false;
                                        hole.GetComponent<HoleController>().filled = false;
                                        hole.tag = "Holes";
                                    }
                                }

                                GetComponent<BoxCollider2D>().enabled = true;
                                
                                GetComponent<SpriteRenderer>().sortingOrder =
                                    GetComponent<SpriteRenderer>().sortingOrder + 1;
                            }
                            transform.position = prevPos.Last();

                            
                            
                            prevPos.RemoveAt(prevPos.Count - 1);
                            movedPieces.RemoveAt(movedPieces.Count - 1);

                        }
                        else
                        {
                            Debug.Log("No moves in prevMoves");
                            m_soundManager.PlayMoveFailSound();
                        }
                        
                        if (onlyRetry)
                        {
                            onlyRetry = false;
                        }
                    }
                    else
                    {
                        m_soundManager.PlayMoveFailSound();
                    }
                }
                else if (!onlyRetry)
                {
                    //get players input, and calculate the position the player will move to
                    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        m_move.x = m_move.x + gridSize;
                    }
                    else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        m_move.x = m_move.x - gridSize;
                    }
                    else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        m_move.y = m_move.y + gridSize;
                    }
                    else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        m_move.y = m_move.y - gridSize;
                    }

                    //save move for undo
                    SaveMove();
                }
            }

            if (m_move != new Vector2(0, 0))
            {
                nextSpace:
                //if there is an object in front            
                if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move))
                {
                    //if that object is a Piece
                    if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                        .CompareTag("Pieces"))
                    {
                        //add Piece object to the end of the list
                        piecesInFront.Add(RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider
                            .gameObject);
                        //goto nextSpace, check next block
                        goto nextSpace;
                    }

                    if (RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                            .CompareTag("Holes") ||
                        RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move).collider.gameObject
                            .CompareTag("Ground") || RayCheck(piecesInFront[piecesInFront.Count - 1].transform, m_move)
                            .collider.gameObject.CompareTag("Mover"))
                    {
                        m_soundManager.PlayMoveSound();
                        Debug.Log("moving");
                        movedPieces.Add(piecesInFront.Count - 1);
                        prevPos.Add(transform.position);


                        //move all objects in array
                        for (int i = piecesInFront.Count - 1; i >= 0; i--)
                        {
                            //move object in front of player in the direction that the player is moving in
                            StartCoroutine(SmoothMove(piecesInFront[i], piecesInFront[i].transform,
                                new Vector3(piecesInFront[i].transform.position.x + m_move.x,
                                    piecesInFront[i].transform.position.y + m_move.y, 0)));
                        }
                    }
                }
                //if there isn't anything infront, fail.
                else
                {
                    m_soundManager.PlayMoveFailSound();

                    input = true;
                }


                //reset position the player will move to
                m_move = new Vector2(0, 0);

                piecesInFront.RemoveRange(1, piecesInFront.Count - 1);
            }
            else
            {
                input = true;
            }
        }
        // --- QUEUE ---
        else if (UsableInput() && moveQueue.Count <= 3)
        {
            //get player input
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                m_move.x = m_move.x + gridSize;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                m_move.x = m_move.x - gridSize;
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                m_move.y = m_move.y + gridSize;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_move.y = m_move.y - gridSize;
            }

            //add input to the queue
            if (m_move != new Vector2(0, 0))
            {
                moveQueue.Add(m_move);
                m_move = new Vector2(0, 0);
            }
        }
    }

    private void SaveMove()
    {
        prevMoves.Add(m_move);
    }

    bool UsableInput()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.U))
        {
            return true;
        }

        return false;
    }


    RaycastHit2D RayCheck(Transform center, Vector2 direction)
    {
        RaycastHit2D[] rayCheckHits =
            Physics2D.RaycastAll(new Vector2(center.position.x + m_move.x, center.position.y + m_move.y),
                new Vector2(direction.x * k_PositionOffset, direction.y * k_PositionOffset), k_RayCastLength);

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
                moverMoving = true;
                return hit;
            }
        }

        return new RaycastHit2D();
    }

    IEnumerator SmoothMove(GameObject piece, Transform startPos, Vector3 endPos)
    {
        //disable input
        input = false;
        //set the variable that is how many percent the object has completed on the path
        float m_movePos = 0;
        //start a loop that loops until the object has reached its target
        while (startPos.position != endPos)
        {
            //move object
            piece.transform.position = Vector3.Lerp(startPos.position, endPos, m_movePos);
            m_movePos = m_movePos + (moveSpeed / 200);
            yield return new WaitForSecondsRealtime((1 / 10) / moveSpeed);
        }

        //round the objects position so it doesn't stray away from the grid
        piece.transform.position = new Vector3(Mathf.RoundToInt(piece.transform.position.x),
            Mathf.RoundToInt(piece.transform.position.y), 0);

        if (!moverMoving)
        {
            //enable input
            input = true;
        }
    }
}