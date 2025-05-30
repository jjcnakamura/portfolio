using UnityEngine;

/// <summary>
/// 各ピースに割り当てるスクリプト
/// ステージ上のピースを嵌める場所と重なっているか判定を取る
/// </summary>
public class PieceCollision : MonoBehaviour
{
    [System.NonSerialized] public GameObject fitInPos;
    Piece pieceParent;
    bool overLapping, processed;

    GameObject myBlock;
    bool blockOverLapping, blockProcessed;

    void Awake()
    {
        pieceParent = transform.parent.parent.GetComponent<Piece>();
    }

    void Update()
    {
        if (!pieceParent.catchd)
        {
            overLapping = false;
            processed = false;

            fitInPos = null;

            blockOverLapping = false;
            blockProcessed = false;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (pieceParent.catchd)
        {
            //ピースを嵌める場所に重なっている場合
            if (collision.transform.tag == "FitInPos" && !processed)
            {
                overLapping = true;
                processed = true;

                pieceParent.placeableCount++;

                fitInPos = collision.gameObject;
            }

            //他のピースに重なっている場合
            if (collision.transform.tag == "Block")
            {
                if (myBlock == null)
                {
                    myBlock = collision.gameObject;
                }
                if (collision.gameObject != myBlock && !blockProcessed)
                {
                    blockOverLapping = true;
                    blockProcessed = true;

                    pieceParent.overLappingPieceCount++;

                    pieceParent.collisionPiece = collision.transform.parent.GetComponent<Piece>();
                } 
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (pieceParent.catchd)
        {
            //ピースを嵌める場所から離れた場合
            if (collision.transform.tag == "FitInPos" && overLapping)
            {
                overLapping = false;
                processed = false;

                pieceParent.placeableCount--;

                fitInPos = null;
            }

            //他のピースから離れた場合
            if (collision.transform.tag == "Block")
            {
                if (collision.gameObject != myBlock && blockOverLapping)
                {
                    blockOverLapping = false;
                    blockProcessed = false;

                    pieceParent.overLappingPieceCount--;
                }
            }
        }     
    }
}
