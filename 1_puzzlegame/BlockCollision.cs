using UnityEngine;

/// <summary>
/// �e�s�[�X�Ɋ��蓖�Ă�X�N���v�g
/// �v���C���[�̃J�[�\���Əd�Ȃ��Ă��邩��������
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
        //�X�e�[�W��̂ǂ����ɏd�Ȃ��Ă���ꍇ
        if (collision.transform.tag == "Stage" && pieceParent.catchd && !processed)
        {
            overLapping = true;
            processed = true;

            pieceParent.overLappingStageCount++;
        }

        //�v���C���[�̃J�[�\���ɏd�Ȃ��Ă���ꍇ
        if (collision.transform.tag == "Player" && !pieceParent.catchd && !corsorProcessed)
        {
            corsorOverLapping = true;
            corsorProcessed = true;

            pieceParent.overLappingCorsorCount++;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        ////�X�e�[�W�ォ�痣�ꂽ�ꍇ
        if (collision.transform.tag == "Stage" && pieceParent.catchd && overLapping)
        {
            overLapping = false;
            processed = false;

            pieceParent.overLappingStageCount--;
        }

        //�v���C���[�̃J�[�\�����痣�ꂽ�ꍇ
        if (collision.transform.tag == "Player" && !pieceParent.catchd && corsorOverLapping)
        {
            corsorOverLapping = false;
            corsorProcessed = false;

            pieceParent.overLappingCorsorCount--;
        }
    }
}
