using UnityEngine;

/// <summary>
/// �e�s�[�X�Ɋ��蓖�Ă�X�N���v�g
/// �X�e�[�W��̃s�[�X��Ƃ߂�ꏊ�Əd�Ȃ��Ă��邩��������
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
            //�s�[�X��Ƃ߂�ꏊ�ɏd�Ȃ��Ă���ꍇ
            if (collision.transform.tag == "FitInPos" && !processed)
            {
                overLapping = true;
                processed = true;

                pieceParent.placeableCount++;

                fitInPos = collision.gameObject;
            }

            //���̃s�[�X�ɏd�Ȃ��Ă���ꍇ
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
            //�s�[�X��Ƃ߂�ꏊ���痣�ꂽ�ꍇ
            if (collision.transform.tag == "FitInPos" && overLapping)
            {
                overLapping = false;
                processed = false;

                pieceParent.placeableCount--;

                fitInPos = null;
            }

            //���̃s�[�X���痣�ꂽ�ꍇ
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
