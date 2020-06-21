using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PieceController : MonoBehaviour
{
    public string pieceColor;

    public Sprite DefaultSprite;

    public bool pieceCompleted;
    
    public List<Sprite> sprAnim = new List<Sprite>();

}