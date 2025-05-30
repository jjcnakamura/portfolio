using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// ピースが持つコンポーネント。PlayerControllerから参照する変数や関数を持つ
/// </summary>
public class Piece : MonoBehaviour
{
    [SerializeField, Label("ピースの番号")] public int pieceNum;
    [SerializeField, Label("回転の軸を変える")] public bool offsetChange;

    //子オブジェクトのコンポーネント
    PieceCollision[] pieceCollision;
    SpriteRenderer[] blockSprite;

    //前にピースが置かれていた位置を保存する
    GameObject[] preFitInPos;
    [System.NonSerialized] public Vector3 prePos, preRot;

    [System.NonSerialized] public bool catchd;        //プレイヤーが自身を掴んでいるか
    [System.NonSerialized] public bool fitIn;         //枠に嵌まっているか
    [System.NonSerialized] public int placeableCount; //枠に嵌められるブロックの数
    [System.NonSerialized] public int overLappingCorsorCount;//プレイヤーと重なっているブロックの数
    [System.NonSerialized] public bool overLappingCorsor;    //プレイヤーが自身と重なっているか
                                  bool preOverLappingCorsor;
    [System.NonSerialized] public int overLappingStageCount; //重なっているステージの数
    [System.NonSerialized] public bool overLappingStage;     //ステージに重なっているか
    [System.NonSerialized] public int overLappingPieceCount; //重なっている他のピースの数
    [System.NonSerialized] public bool overLappingPiece;     //他のピースに重なっているか
    [System.NonSerialized] public int blockNum;              //自身が何ブロックのピースか
    [System.NonSerialized] public Piece collisionPiece;      //衝突している他のピース

    bool invalidScene; //特定のシーンでこのスクリプトを実行しない

    void Awake()
    {
        //タイトル、ステージ選択のシーンの場合は戻る
        invalidScene = (SceneManager.GetActiveScene().buildIndex < 2);
        if (invalidScene) return;

        blockNum = transform.childCount;
        Array.Resize(ref preFitInPos, blockNum);
        Array.Resize(ref pieceCollision, blockNum);
        Array.Resize(ref blockSprite, blockNum);
        //自身が持つブロックを配列に格納
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

        //自身をプレイヤーが掴んでいる場合
        if (catchd)
        {
            //枠の中に収まるブロックの数を数える
            PlayerController.Instance.placeable = (placeableCount == blockNum) ? true : false;
            overLappingStage = (overLappingStageCount > 0) ? true : false;
            overLappingPiece = (overLappingPieceCount >= blockNum ||
                                collisionPiece != null &&
                                overLappingPieceCount >= collisionPiece.blockNum) ? true : false;

            overLappingCorsorCount = 0;
            preOverLappingCorsor = false;
            overLappingCorsor = false;
        }
        //自身をプレイヤーが掴んでいない場合
        else
        {
            placeableCount = 0;
            overLappingStageCount = 0;
            overLappingPieceCount = 0;
            overLappingPiece = false;
            collisionPiece = null;

            //当たり判定の表示を切り替え
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
    /// ピースが枠に嵌まっているかのフラグを変更する
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
    /// ピースを嵌めることができるかを判定する
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
    /// ピースを置けなかった場合に前の位置に戻す
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
    /// 当たり判定の表示を切り替え
    /// </summary>
    void BlockSpriteSetActive(bool flg)
    {
        for (int i = 0; i < blockNum; i++)
        {
            blockSprite[i].enabled = flg;
        }
    }
}
