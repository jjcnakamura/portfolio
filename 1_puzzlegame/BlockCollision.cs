using UnityEngine;

/// <summary>
/// 各ピースに割り当てるスクリプト
/// プレイヤーのカーソルと重なっているか判定を取る
/// </summary>
public class BlockCollision : MonoBehaviour
{
    Piece pieceParent;
    bool overLapping, processed;

    bool corsorOverLapping, corsorProcessed;

    void Awake()
    {
        pieceParent = transform.parent.GetComponent<Piece>();
    }

    void Update()
    {
        if (!pieceParent.catchd)
        {
            overLapping = false;
            processed = false;
        }
        else
        {
            corsorOverLapping = false;
            corsorProcessed = false;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        //ステージ上のどこかに重なっている場合
        if (collision.transform.tag == "Stage" && pieceParent.catchd && !processed)
        {
            overLapping = true;
            processed = true;

            pieceParent.overLappingStageCount++;
        }

        //プレイヤーのカーソルに重なっている場合
        if (collision.transform.tag == "Player" && !pieceParent.catchd && !corsorProcessed)
        {
            corsorOverLapping = true;
            corsorProcessed = true;

            pieceParent.overLappingCorsorCount++;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        ////ステージ上から離れた場合
        if (collision.transform.tag == "Stage" && pieceParent.catchd && overLapping)
        {
            overLapping = false;
            processed = false;

            pieceParent.overLappingStageCount--;
        }

        //プレイヤーのカーソルから離れた場合
        if (collision.transform.tag == "Player" && !pieceParent.catchd && corsorOverLapping)
        {
            corsorOverLapping = false;
            corsorProcessed = false;

            pieceParent.overLappingCorsorCount--;
        }
    }
}
