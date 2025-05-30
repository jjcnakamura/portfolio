using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ゲームの進行に関わる処理のまとめ
/// </summary>
public class GameManager : Singleton<GameManager>
{
    //エフェクトに関する変数
    [SerializeField, Label("クリア時のエフェクト")] GameObject clearEffect;
    [SerializeField, Label("クリア画面表示までの間隔")] float clearInterval = 2f;

    [Space(10)]

    //ピースを嵌める場所に関する変数
    [SerializeField] GameObject stageParent;
    GameObject[] fitInPos;
    bool[] fitIn;
    int fitInPosCount, fitInCount;

    //時間に関する変数
    float timer;

    //セーブに関する変数
    string stageClearFlagDetaName = "StageClearFlag_";
    string stageClearTimeDetaName = "StageClearTime_";

    //UIに関する変数
    [SerializeField, Label("メインゲームCanvas")] GameObject mainCanvas;
    [SerializeField, Label("何ステージ目かを表すText")] TextMeshProUGUI StageNumberText;
    [SerializeField, Label("ポーズボタン")] GameObject pauseButton;
    [SerializeField, Label("クリアCanvas")] GameObject clearCanvas;
    [SerializeField, Label("クリア時間のText")] TextMeshProUGUI clearTimeText;

    //操作方法のUIに関する変数
    [SerializeField, Label("メインゲームの操作方法")] GameObject operation1; //変数名を変えるとSerializeFieldが外れるため古い変数名になっている
    GameObject[] operations;
    Image[] operationImage1, operationImage2, operationImage3;
    TextMeshProUGUI[] operationText;
    GimmickScale[] operationTextGimmickScale;
    Color operationUiColor = new Color(1, 0, 0, 1);
    Color operationUiColorTranslucent = new Color(1, 0, 0, 0.5f);
    float[] operationTimer;
    float operationChangeTime = 0.1f;

    public bool mainGame, gameClear, pause;

    void Awake()
    {
        //タイトル、ステージ選択のシーン以外の場合
        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            PlayerPrefs.SetInt("DifficultyChoice", 1);

            if (clearEffect == null)
            {
                //クリア時のエフェクトをロード
                clearEffect = Resources.Load<GameObject>("ClearParticle");
            }
        }

        //操作説明のコンポーネントを取得
        if (operation1 != null)
        {
            int operationCount = operation1.transform.childCount;
            operations = new GameObject[operationCount];
            operationImage1 = new Image[operationCount];
            operationImage2 = new Image[operationCount];
            operationImage3 = new Image[operationCount];
            operationText = new TextMeshProUGUI[operationCount];
            operationTextGimmickScale = new GimmickScale[operationCount];
            operationTimer = new float[operationCount];
            for (int i = 0; operationCount > i; i++)
            {
                operations[i] = operation1.transform.GetChild(i).gameObject;
                operations[i].SetActive(true);
                operationText[i] = operations[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                operationTextGimmickScale[i] = operationText[i].GetComponent<GimmickScale>();
                operationImage1[i] = operations[i].transform.GetChild(1).GetComponent<Image>();
                operationImage2[i] = operations[i].transform.GetChild(2).GetComponent<Image>();
                operationImage3[i] = operations[i].transform.GetChild(3).GetComponent<Image>();
                operationTimer[i] = operationChangeTime;
            }
        }

        StageDetaSet();
    }

    void Start()
    {
        mainGame = true;
        clearCanvas.SetActive(false);

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        //5ステージごとに違うBGMを流す
        int bgmNum = (sceneIndex - 2) / 5 + 1;
        SoundManager.Instance.PlayBGM(bgmNum);

        //ステージセレクトのページを保存
        int stageSelectPage = 0;
        if      (sceneIndex <= 11) stageSelectPage = 0;
        else if (sceneIndex <= 21) stageSelectPage = 0;
        else if (sceneIndex <= 31) stageSelectPage = 1;
        else if (sceneIndex <= 36) stageSelectPage = 1;
        else if (sceneIndex <= 46) stageSelectPage = 2;
        else if (sceneIndex <= 51) stageSelectPage = 2;
        PlayerPrefs.SetInt("StageSelectPage", stageSelectPage);
    }

    void Update()
    {
        UIUpdate();
        if (!mainGame) return; //メインゲーム中以外は以下の処理をしない
        PauseCheck();
        if (pause) return; //ポーズ中は以下の処理をしない
        GameClearChack();
    }

    void FixedUpdate()
    {
        //クリアまでタイムを増やしていく
        if (!mainGame || pause) return;

        timer += Time.fixedDeltaTime;
    }

    /// <summary>
    /// float型の時間の引数を受け取り、分と秒に分けて返す。返り値はint型の配列で、要素番号0が分、要素番号1が秒
    /// </summary>
    public int[] TimeCount(float time)
    {
        int minutes = 0;
        int seconds = 0;
        for (float i = time; i >= 60; i -= 60)
        {
            minutes++;
            time -= 60;
        }
        for (float i = time; i >= 1; i -= 1)
        {
            seconds++;
            time -= 1;
        }
        int[] returnTime = { minutes, seconds };
        return returnTime;
    }

    void PauseCheck()
    {
        //ポーズ中かどうか判定する
        pause = (PauseScript.Instance.pause) ? true : false;
        if (pauseButton != null) pauseButton.SetActive(!pause);
    }

    /// <summary>
    /// ゲームの状態ごとにUIを切り替える
    /// </summary>
    void UIUpdate()
    {
        //メインゲーム中
        if (mainGame && !pause)
        {
            //ステージ数の表示をシーン番号をもとに変える
            StageNumberText.text = "ステージ" + (SceneManager.GetActiveScene().buildIndex -1);

            //操作説明の表示切り替え
            if (operation1 != null)
            {
                if (PlayerController.Instance.catchd)
                {
                    operationTextGimmickScale[0].Stop();
                    operations[0].SetActive(false);

                    operationImage1[1].color = operationUiColor;
                    operationImage2[1].color = operationUiColor;
                    operationImage3[1].color = operationUiColor;
                    operationText[1].color = operationUiColor;
                    operationTextGimmickScale[1].Resume();
                    operations[1].SetActive(true);
                    operationTimer[1] = 0;

                    operationImage1[2].color = operationUiColor;
                    operationImage2[2].color = operationUiColor;
                    operationImage3[2].color = operationUiColor;
                    operationText[2].color = operationUiColor;
                    operationTextGimmickScale[2].Resume();
                    operationTimer[2] = 0;
                }
                else if (PlayerController.Instance.overLapping)
                {
                    operationImage1[0].color = operationUiColor;
                    operationImage2[0].color = operationUiColor;
                    operationImage3[0].color = operationUiColor;
                    operationText[0].color = operationUiColor;
                    operationTextGimmickScale[0].Resume();
                    operations[0].SetActive(true);
                    operationTimer[0] = 0;

                    operationTextGimmickScale[1].Stop();
                    operations[1].SetActive(false);

                    operationImage1[2].color = operationUiColor;
                    operationImage2[2].color = operationUiColor;
                    operationImage3[2].color = operationUiColor;
                    operationText[2].color = operationUiColor;
                    operationTextGimmickScale[2].Resume();
                    operationTimer[2] = 0;
                }
                else
                {
                    if (operationTimer[0] < operationChangeTime)
                    {
                        operationTimer[0] += Time.deltaTime;
                    }
                    else
                    {
                        operationImage1[0].color = operationUiColorTranslucent;
                        operationImage2[0].color = operationUiColorTranslucent;
                        operationImage3[0].color = operationUiColorTranslucent;
                        operationText[0].color = operationUiColorTranslucent;
                        operationTextGimmickScale[0].Stop();
                    }
                    operations[0].SetActive(true);

                    operationTextGimmickScale[1].Stop();
                    operations[1].SetActive(false);

                    if (operationTimer[2] < operationChangeTime)
                    {
                        operationTimer[2] += Time.deltaTime;
                    }
                    else
                    {
                        operationImage1[2].color = operationUiColorTranslucent;
                        operationImage2[2].color = operationUiColorTranslucent;
                        operationImage3[2].color = operationUiColorTranslucent;
                        operationText[2].color = operationUiColorTranslucent;
                        operationTextGimmickScale[2].Stop();
                    }
                }
            }
        }
        //ゲームクリア時
        else if (gameClear && mainCanvas.activeSelf)
        {
            mainCanvas.SetActive(false);
        }
    }

    void GameClearChack()
    {
        //全てのマスにピースが嵌まっている場合
        if (!gameClear && fitInCount >= fitInPosCount)
        {
            //フラグを変更
            mainGame = false;
            gameClear = true;

            //BGMを消し、クリア時のSEを再生
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySE_Game(3);

            //エフェクトを生成
            Instantiate(clearEffect);

            //クリア画面を表示
            Invoke("GameClear", clearInterval);
        }
    }

    void GameClear()
    {
        //クリアCanvasを表示
        clearCanvas.SetActive(true);
        //Debug.Log("ステージクリア");

        //クリア時間を表示
        int[] clearTime = TimeCount(timer);
        if (clearTimeText != null) clearTimeText.text += clearTime[0].ToString() + "分" + clearTime[1].ToString() + "秒";
        //Debug.Log("クリア時間..." + clearTime[0] + "分" + clearTime[1] + "秒");

        //クリアデータをセーブ
        int sceneNum = SceneManager.GetActiveScene().buildIndex - 1;
        stageClearFlagDetaName += sceneNum;
        stageClearTimeDetaName += sceneNum;
        Debug.Log(stageClearFlagDetaName + " = " + "1");
        Debug.Log(stageClearTimeDetaName + " = " + timer);
        PlayerPrefs.SetInt(stageClearFlagDetaName, 1);
        //過去のクリアタイムより短いタイムだった場合、または、過去のクリアタイムがない場合にセーブする
        if (timer < PlayerPrefs.GetFloat(stageClearTimeDetaName) || PlayerPrefs.GetFloat(stageClearTimeDetaName) == 0)
        PlayerPrefs.SetFloat(stageClearTimeDetaName, timer);
    }

    /// <summary>
    /// ステージ上の各ブロックを配列に格納する
    /// </summary>
    void StageDetaSet()
    {
        int stageBlockNum = stageParent.transform.childCount;

        fitInPos = new GameObject[stageBlockNum];
        fitIn = new bool[stageBlockNum];

        fitInPosCount = stageBlockNum;

        for (int i = 0; i < fitInPosCount; i++)
        {
            fitInPos[i] = stageParent.transform.GetChild(i).GetChild(0).gameObject;
        }
    }

    /// <summary>
    /// ピースが枠に嵌まっているかのフラグを変更する
    /// </summary>
    public void FitInFlagChange(GameObject[] collisionFitInPos, bool flg)
    {
        int cnt = collisionFitInPos.Length;
        for (int i = 0; i < fitInPos.Length; i++)
        {
            for (int j = 0; j < collisionFitInPos.Length; j++)
            {
                if (collisionFitInPos[j] == fitInPos[i])
                {
                    fitIn[i] = flg;
                    if (flg) fitInCount++;
                    else     fitInCount--;
                    cnt--;
                    if (cnt == 0) return;
                }
            }
        }
    }

    /// <summary>
    /// ピースを嵌めることができるかを判定する
    /// </summary>
    public bool FitInCheck(GameObject[] collisionFitInPos)
    {
        int cnt = collisionFitInPos.Length;
        for (int i = 0; i < fitInPos.Length; i++)
        {
            for (int j = 0; j < collisionFitInPos.Length; j++)
            {
                if (collisionFitInPos[j] == fitInPos[i])
                {
                    if (fitIn[i]) return true;
                    cnt--;
                    if (cnt == 0) return false;
                } 
            }
        }
        return false;
    }
}
