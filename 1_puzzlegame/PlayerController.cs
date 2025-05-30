using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�̑���
/// </summary>
public class PlayerController : Singleton<PlayerController>
{
    //�͂ރI�u�W�F�N�g�Ɋւ���ϐ�
    [System.NonSerialized] public bool overLapping;          //�s�[�X�ɏd�Ȃ��Ă��邩
    [System.NonSerialized] public GameObject overLappObject; //�J�[�\���ɏd�Ȃ��Ă���I�u�W�F�N�g
    GameObject catchObject; //�͂�ł���I�u�W�F�N�g
    Piece piece;            //�s�[�X�̃R���|�[�l���g
    SpriteRenderer pieceSprite; //�s�[�X�̃X�v���C�g
    public bool catchd { get; private set; }      //�s�[�X������ł��邩
    [System.NonSerialized] public bool placeable; //�s�[�X��ݒu�\��
    bool pauseCatchd; //�s�[�X��͂�ł��鎞�Ƀ|�[�Y������
    bool inoperable;  //����s�\���

    //SE�Ɋւ���ϐ�
    float animalSETimer = 1f;
    float animalSEInterval = 1f;
    bool animalSEWait;

    //�J�[�\���Ɋւ���ϐ�
    GameObject[] corsor;

    void Awake()
    {
        //�J�[�\����GameObject��z��Ɋi�[
        corsor = new GameObject[2];
        for (int i = 0; i < transform.childCount; i++)
        {
            corsor[i] = transform.GetChild(i).gameObject;
            corsor[i].SetActive(false);
        }
        corsor[0].SetActive(true);
    }

    void Update()
    {
        if (!GameManager.Instance.mainGame) return; //���C���Q�[�����ȊO�͈ȉ��̏��������Ȃ�
        PausePiecesPlacement();
        if (GameManager.Instance.pause) return;     //�|�[�Y���͈ȉ��̏��������Ȃ�
        AnimalSEInterval();
        if (inoperable) return; //����s�\��ԂȂ�ȉ��̏��������Ȃ�
        PiecesCatch();
        PiecesRotate();
        PiecesPlacement();
    }

    void PiecesCatch(bool rotate = false)
    {
        //�s�[�X��͂�ł��Ȃ��č��N���b�N����������
        if (overLapping && Input.GetButtonDown("Fire1") && !catchd && !rotate || rotate)
        {
            if (overLappObject == null) return;

            //�s�[�X�̃R���|�[�l���g���擾
            catchObject = overLappObject;
            piece = catchObject.GetComponent<Piece>();
            piece.prePos = catchObject.transform.position;
            piece.preRot = catchObject.transform.eulerAngles;

            //�s�[�X�̃X�v���C�g���擾���A���̃s�[�X���O�̃��C���[�ɂ���
            pieceSprite = catchObject.GetComponent<SpriteRenderer>();
            pieceSprite.sortingOrder++;

            if (!rotate)
            {
                SoundManager.Instance.PlaySE_Game(0);
                AnimalSE(piece.pieceNum); //�����̐���炷

                //�J�[�\���؂�ւ�
                if (corsor[0] != null && corsor[1] != null)
                {
                    corsor[0].SetActive(false);
                    corsor[1].SetActive(true);
                }

                catchObject.transform.SetParent(transform); //�s�[�X���J�[�\���̎q�I�u�W�F�N�g�ɂ���

                catchd = true;
            }
            piece.catchd = true;
            if (piece.fitIn)
            {
                piece.FitInFlagChange(false);
            }
            if (!rotate) piece.fitIn = false;
        }
    }

    /// <summary>
    /// �͂񂾃s�[�X����]�����鏈��
    /// </summary>
    void PiecesRotate()
    {
        //�s�[�X�ɃJ�[�\�����d�Ȃ��Ă��ĉE�N���b�N����������
        if (Input.GetButtonDown("Fire2"))
        {
            //�s�[�X��͂�ł���ꍇ
            if (catchd && catchObject != null)
            {
                SoundManager.Instance.PlaySE_Game(2);

                catchObject.transform.eulerAngles -= new Vector3(0, 0, 90);
            }
            //�s�[�X��͂�ł��Ȃ��ꍇ
            else if (overLapping && overLappObject != null)
            {
                SoundManager.Instance.PlaySE_Game(2);

                inoperable = true; //����s�\�ɂ���

                PiecesCatch(true); //Rotation��ς���O�Ƀs�[�X��͂ޏ���������

                catchObject.transform.eulerAngles -= new Vector3(0, 0, 90);
                if (piece.fitIn)
                {
                    //�s�[�X���g�ɛƂ�悤�Ɉʒu�����炷
                    if (piece.offsetChange)
                    {
                        float curAngle = 360 - catchObject.transform.eulerAngles.z;
                        float x = 0.45f;
                        float y = -0.45f;
                        if (curAngle <= 90)
                        {
                            x = 0.45f;
                            y = -0.45f;
                        }
                        else if (curAngle <= 180)
                        {
                            x = -0.45f;
                            y = -0.45f;
                        }
                        else if (curAngle <= 270)
                        {
                            x = -0.45f;
                            y = 0.45f;
                        }
                        else if (curAngle <= 360)
                        {
                            x = 0.45f;
                            y = 0.45f;
                        }
                        catchObject.transform.position += new Vector3(x, y, 0);
                    }
                }

                piece.fitIn = false;

                Invoke("RotateApply", 0.1f); //Rotation��ς�����Ƀs�[�X��u������������
            }    
        }
    }
    void RotateApply()
    {
        catchd = true;
        PiecesPlacement(true);
    }

    /// <summary>
    /// �͂񂾃s�[�X��u�������@��]��������Ăяo���ꍇ�͈�����true������
    /// </summary>
    void PiecesPlacement(bool rotate = false)
    {
        //�s�[�X��͂�ł��č��N���b�N��b������
        if (Input.GetButtonUp("Fire1") && catchd && !rotate || pauseCatchd || rotate)
        {
            //�s�[�X��͂�ł��Ȃ��ꍇ�͖߂�
            if (!catchd || catchObject == null) return;

            if (!rotate)
            {
                SoundManager.Instance.PlaySE_Game(1);

                //�J�[�\���؂�ւ�
                if (corsor[0] != null && corsor[1] != null)
                {
                    corsor[1].SetActive(false);
                    corsor[0].SetActive(true);
                }

                catchObject.transform.SetParent(null); //�s�[�X�̎q�I�u�W�F�N�g��������

                overLapping = false;
            }
            else
            {
                inoperable = false; //����s�\��Ԃ�����
            }

            pieceSprite.sortingOrder--; //�s�[�X�̃��C���[��߂�

            catchd = false;
            piece.catchd = false;
            pauseCatchd = false;

            //�s�[�X��Ƃ߂鏈��
            if (placeable && !piece.FitInCheck())
            {
                if (!rotate) AnimalSE(piece.pieceNum); //�����̐���炷

                placeable = false;
                piece.fitIn = true;

                //�s�[�X���g�̈ʒu�ɍ����悤�Ɉړ�
                GameObject block = catchObject.transform.GetChild(catchObject.transform.childCount - 1).gameObject;
                block.transform.SetParent(null);
                catchObject.transform.SetParent(block.transform.GetChild(0));
                block.transform.position = block.transform.GetChild(0).GetComponent<PieceCollision>().fitInPos.transform.position;
                catchObject.transform.SetParent(null);
                block.transform.SetParent(catchObject.transform);

                piece.FitInFlagChange(true);
            }
            //�s�[�X�̌`���g�ɍ���Ȃ���
            else
            {
                piece.PosBack();

                placeable = false;
            }
        }
    }

    /// <summary>
    /// �s�[�X��͂�ł��鎞�Ƀ|�[�Y�������ꍇ�̏���
    /// </summary>
    void PausePiecesPlacement()
    {
        if (Input.GetButtonUp("Fire1") && GameManager.Instance.pause)
        {
            if (!catchd || catchObject == null) return;
            pauseCatchd = true;
        }

        if (catchObject != null && catchObject.transform.parent != null && GameManager.Instance.pause)
        {
            catchObject.transform.SetParent(null);
        }
        else if (catchd && !pauseCatchd && catchObject != null && catchObject.transform.parent == null && !GameManager.Instance.pause)
        {
            catchObject.transform.SetParent(transform);
            catchObject.transform.position = transform.position;
        }
    }

    /// <summary>
    /// �����̐���炷����
    /// </summary>
    void AnimalSE(int num)
    {
        if (!animalSEWait)
        {
            SoundManager.Instance.PlaySE_Animal(num);
            animalSEWait = true;
        }
    }
    void AnimalSEInterval()
    {
        if (animalSEWait)
        {
            animalSETimer -= Time.deltaTime;
            if (animalSETimer <= 0)
            {
                animalSETimer = animalSEInterval;
                animalSEWait = false;
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        //�s�[�X�ɏd�Ȃ����ꍇ
        if (collision.transform.tag == "Block" && !overLapping && !catchd)
        {
            overLapping = true;
            overLappObject = collision.transform.parent.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        //�s�[�X���痣�ꂽ�ꍇ
        if (collision.transform.tag == "Block" && !catchd)
        {
            overLapping = false;
        }
    }
}
