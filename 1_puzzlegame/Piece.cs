using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// �s�[�X�����R���|�[�l���g�BPlayerController����Q�Ƃ���ϐ���֐�������
/// </summary>
public class Piece : MonoBehaviour
{
    [SerializeField, Label("�s�[�X�̔ԍ�")] public int pieceNum;
    [SerializeField, Label("��]�̎���ς���")] public bool offsetChange;

    //�q�I�u�W�F�N�g�̃R���|�[�l���g
    PieceCollision[] pieceCollision;
    SpriteRenderer[] blockSprite;

    //�O�Ƀs�[�X���u����Ă����ʒu��ۑ�����
    GameObject[] preFitInPos;
    [System.NonSerialized] public Vector3 prePos, preRot;

    [System.NonSerialized] public bool catchd;        //�v���C���[�����g��͂�ł��邩
    [System.NonSerialized] public bool fitIn;         //�g�ɛƂ܂��Ă��邩
    [System.NonSerialized] public int placeableCount; //�g�ɛƂ߂���u���b�N�̐�
    [System.NonSerialized] public int overLappingCorsorCount;//�v���C���[�Əd�Ȃ��Ă���u���b�N�̐�
    [System.NonSerialized] public bool overLappingCorsor;    //�v���C���[�����g�Əd�Ȃ��Ă��邩
                                  bool preOverLappingCorsor;
    [System.NonSerialized] public int overLappingStageCount; //�d�Ȃ��Ă���X�e�[�W�̐�
    [System.NonSerialized] public bool overLappingStage;     //�X�e�[�W�ɏd�Ȃ��Ă��邩
    [System.NonSerialized] public int overLappingPieceCount; //�d�Ȃ��Ă��鑼�̃s�[�X�̐�
    [System.NonSerialized] public bool overLappingPiece;     //���̃s�[�X�ɏd�Ȃ��Ă��邩
    [System.NonSerialized] public int blockNum;              //���g�����u���b�N�̃s�[�X��
    [System.NonSerialized] public Piece collisionPiece;      //�Փ˂��Ă��鑼�̃s�[�X

    bool invalidScene; //����̃V�[���ł��̃X�N���v�g�����s���Ȃ�

    void Awake()
    {
        //�^�C�g���A�X�e�[�W�I���̃V�[���̏ꍇ�͖߂�
        invalidScene = (SceneManager.GetActiveScene().buildIndex < 2);
        if (invalidScene) return;

        blockNum = transform.childCount;
        Array.Resize(ref preFitInPos, blockNum);
        Array.Resize(ref pieceCollision, blockNum);
        Array.Resize(ref blockSprite, blockNum);
        //���g�����u���b�N��z��Ɋi�[
        for (int i = 0; i < blockNum; i++)
        {
            pieceCollision[i] = transform.GetChild(i).GetChild(0).GetComponent<PieceCollision>();
            blockSprite[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
        }
        preFitInPos = null;
    }

    void Update()
    {
        if (invalidScene) return;

        //���g���v���C���[���͂�ł���ꍇ
        if (catchd)
        {
            //�g�̒��Ɏ��܂�u���b�N�̐��𐔂���
            PlayerController.Instance.placeable = (placeableCount == blockNum) ? true : false;
            overLappingStage = (overLappingStageCount > 0) ? true : false;
            overLappingPiece = (overLappingPieceCount >= blockNum ||
                                collisionPiece != null &&
                                overLappingPieceCount >= collisionPiece.blockNum) ? true : false;

            overLappingCorsorCount = 0;
            preOverLappingCorsor = false;
            overLappingCorsor = false;
        }
        //���g���v���C���[���͂�ł��Ȃ��ꍇ
        else
        {
            placeableCount = 0;
            overLappingStageCount = 0;
            overLappingPieceCount = 0;
            overLappingPiece = false;
            collisionPiece = null;

            //�����蔻��̕\����؂�ւ�
            if (!PlayerController.Instance.catchd)
            {
                overLappingCorsor = (overLappingCorsorCount > 0 && gameObject == PlayerController.Instance.overLappObject) ? true : false;
                if (preOverLappingCorsor != overLappingCorsor)BlockSpriteSetActive(overLappingCorsor);
                preOverLappingCorsor = overLappingCorsor;
            }
        }
        if (PlayerController.Instance.catchd && blockSprite[0].enabled || !GameManager.Instance.mainGame)
        {
            BlockSpriteSetActive(false);
        }
    }

    /// <summary>
    /// �s�[�X���g�ɛƂ܂��Ă��邩�̃t���O��ύX����
    /// </summary>
    public void FitInFlagChange(bool flg)
    {
        GameObject[] fitInPos = new GameObject[blockNum];
        for (int i = 0; i < blockNum; i++)
        {
            if (pieceCollision[i].fitInPos == null)
            {
                if (!flg && preFitInPos != null)
                {
                    fitInPos = preFitInPos;
                    break;
                }
                return;
            }
            fitInPos[i] = pieceCollision[i].fitInPos;
        }
        if (flg)
        {
            preFitInPos = fitInPos;
        }
        GameManager.Instance.FitInFlagChange(fitInPos, flg);
    }

    /// <summary>
    /// �s�[�X��Ƃ߂邱�Ƃ��ł��邩�𔻒肷��
    /// </summary>
    public bool FitInCheck()
    {
        GameObject[] fitInPos = new GameObject[blockNum];
        for (int i = 0; i < blockNum; i++)
        {
            if (pieceCollision[i].fitInPos == null) return true;
            fitInPos[i] = pieceCollision[i].fitInPos;
        }
        return GameManager.Instance.FitInCheck(fitInPos);
    }

    /// <summary>
    /// �s�[�X��u���Ȃ������ꍇ�ɑO�̈ʒu�ɖ߂�
    /// </summary>
    public void PosBack()
    {
        if (preFitInPos == null || !overLappingStage && !overLappingPiece)
        {
            preFitInPos = null;

            if (overLappingStage || !overLappingStage && overLappingPiece)
            {
                transform.position = prePos;
                transform.eulerAngles = preRot;
            }

            return;
        }

        fitIn = true;

        transform.position = prePos;
        transform.eulerAngles = preRot;
        
        GameManager.Instance.FitInFlagChange(preFitInPos, true);
    }

    /// <summary>
    /// �����蔻��̕\����؂�ւ�
    /// </summary>
    void BlockSpriteSetActive(bool flg)
    {
        for (int i = 0; i < blockNum; i++)
        {
            blockSprite[i].enabled = flg;
        }
    }
}
