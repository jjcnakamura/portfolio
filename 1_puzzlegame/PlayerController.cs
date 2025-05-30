using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの操作
/// </summary>
public class PlayerController : Singleton<PlayerController>
{
    //掴むオブジェクトに関する変数
    [System.NonSerialized] public bool overLapping;          //ピースに重なっているか
    [System.NonSerialized] public GameObject overLappObject; //カーソルに重なっているオブジェクト
    GameObject catchObject; //掴んでいるオブジェクト
    Piece piece;            //ピースのコンポーネント
    SpriteRenderer pieceSprite; //ピースのスプライト
    public bool catchd { get; private set; }      //ピースをつかんでいるか
    [System.NonSerialized] public bool placeable; //ピースを設置可能か
    bool pauseCatchd; //ピースを掴んでいる時にポーズをした
    bool inoperable;  //操作不可能状態

    //SEに関する変数
    float animalSETimer = 1f;
    float animalSEInterval = 1f;
    bool animalSEWait;

    //カーソルに関する変数
    GameObject[] corsor;

    void Awake()
    {
        //カーソルのGameObjectを配列に格納
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
        if (!GameManager.Instance.mainGame) return; //メインゲーム中以外は以下の処理をしない
        PausePiecesPlacement();
        if (GameManager.Instance.pause) return;     //ポーズ中は以下の処理をしない
        AnimalSEInterval();
        if (inoperable) return; //操作不可能状態なら以下の処理をしない
        PiecesCatch();
        PiecesRotate();
        PiecesPlacement();
    }

    void PiecesCatch(bool rotate = false)
    {
        //ピースを掴んでいなくて左クリックを押した時
        if (overLapping && Input.GetButtonDown("Fire1") && !catchd && !rotate || rotate)
        {
            if (overLappObject == null) return;

            //ピースのコンポーネントを取得
            catchObject = overLappObject;
            piece = catchObject.GetComponent<Piece>();
            piece.prePos = catchObject.transform.position;
            piece.preRot = catchObject.transform.eulerAngles;

            //ピースのスプライトを取得し、他のピースより前のレイヤーにする
            pieceSprite = catchObject.GetComponent<SpriteRenderer>();
            pieceSprite.sortingOrder++;

            if (!rotate)
            {
                SoundManager.Instance.PlaySE_Game(0);
                AnimalSE(piece.pieceNum); //動物の声を鳴らす

                //カーソル切り替え
                if (corsor[0] != null && corsor[1] != null)
                {
                    corsor[0].SetActive(false);
                    corsor[1].SetActive(true);
                }

                catchObject.transform.SetParent(transform); //ピースをカーソルの子オブジェクトにする

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
    /// 掴んだピースを回転させる処理
    /// </summary>
    void PiecesRotate()
    {
        //ピースにカーソルが重なっていて右クリックを押した時
        if (Input.GetButtonDown("Fire2"))
        {
            //ピースを掴んでいる場合
            if (catchd && catchObject != null)
            {
                SoundManager.Instance.PlaySE_Game(2);

                catchObject.transform.eulerAngles -= new Vector3(0, 0, 90);
            }
            //ピースを掴んでいない場合
            else if (overLapping && overLappObject != null)
            {
                SoundManager.Instance.PlaySE_Game(2);

                inoperable = true; //操作不可能にする

                PiecesCatch(true); //Rotationを変える前にピースを掴む処理をする

                catchObject.transform.eulerAngles -= new Vector3(0, 0, 90);
                if (piece.fitIn)
                {
                    //ピースが枠に嵌るように位置をずらす
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

                Invoke("RotateApply", 0.1f); //Rotationを変えた後にピースを置く処理をする
            }    
        }
    }
    void RotateApply()
    {
        catchd = true;
        PiecesPlacement(true);
    }

    /// <summary>
    /// 掴んだピースを置く処理　回転処理から呼び出す場合は引数にtrueを入れる
    /// </summary>
    void PiecesPlacement(bool rotate = false)
    {
        //ピースを掴んでいて左クリックを話した時
        if (Input.GetButtonUp("Fire1") && catchd && !rotate || pauseCatchd || rotate)
        {
            //ピースを掴んでいない場合は戻る
            if (!catchd || catchObject == null) return;

            if (!rotate)
            {
                SoundManager.Instance.PlaySE_Game(1);

                //カーソル切り替え
                if (corsor[0] != null && corsor[1] != null)
                {
                    corsor[1].SetActive(false);
                    corsor[0].SetActive(true);
                }

                catchObject.transform.SetParent(null); //ピースの子オブジェクト化を解除

                overLapping = false;
            }
            else
            {
                inoperable = false; //操作不可能状態を解除
            }

            pieceSprite.sortingOrder--; //ピースのレイヤーを戻す

            catchd = false;
            piece.catchd = false;
            pauseCatchd = false;

            //ピースを嵌める処理
            if (placeable && !piece.FitInCheck())
            {
                if (!rotate) AnimalSE(piece.pieceNum); //動物の声を鳴らす

                placeable = false;
                piece.fitIn = true;

                //ピースが枠の位置に合うように移動
                GameObject block = catchObject.transform.GetChild(catchObject.transform.childCount - 1).gameObject;
                block.transform.SetParent(null);
                catchObject.transform.SetParent(block.transform.GetChild(0));
                block.transform.position = block.transform.GetChild(0).GetComponent<PieceCollision>().fitInPos.transform.position;
                catchObject.transform.SetParent(null);
                block.transform.SetParent(catchObject.transform);

                piece.FitInFlagChange(true);
            }
            //ピースの形が枠に合わない時
            else
            {
                piece.PosBack();

                placeable = false;
            }
        }
    }

    /// <summary>
    /// ピースを掴んでいる時にポーズをした場合の処理
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
    /// 動物の声を鳴らす処理
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
        //ピースに重なった場合
        if (collision.transform.tag == "Block" && !overLapping && !catchd)
        {
            overLapping = true;
            overLappObject = collision.transform.parent.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        //ピースから離れた場合
        if (collision.transform.tag == "Block" && !catchd)
        {
            overLapping = false;
        }
    }
}
