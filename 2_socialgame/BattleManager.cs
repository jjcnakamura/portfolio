using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

/// <summary>
/// 戦闘画面の処理
/// </summary>
public class BattleManager : Singleton<BattleManager>
{
    //各Cavnas
    [SerializeField] GameObject[] canvas;
    [SerializeField] GameObject poseButton;

    [Space(10)]

    public TextMeshProUGUI battleText; //戦闘中のテキスト
    [SerializeField] GameObject targetCancelButton; //攻撃、スキルの対象を選ぶのをキャンセルするボタン

    [SerializeField] Image backgroundImage; //戦闘中の背景
    int bgmIndex; //BGMの番号

    [Space(10)]

    //ポーズ画面に関する変数
    [SerializeField] GameObject pauseWindowButton;
    [SerializeField] GameObject soundSettingWindow;

    [Space(10)]

    [SerializeField] float battleStartTime = 2.5f; //戦闘開始までの時間
    [SerializeField] float actionWaitTime = 1.5f;  //キャラの行動の間隔
    [SerializeField] float resultWaitTime = 2.5f;  //戦闘終了からリザルト表示までの間隔

    [Space(10)]

    //ステージクリア後に得た経験値とレベル、ガチャチケットを表示するテキスト
    int getExp;
    [SerializeField] GameObject resultCharaImageParent;
    [SerializeField] GameObject resultExpParent_Pre;
    [SerializeField] GameObject resultLvParent_Pre;
    [SerializeField] GameObject resultExpParent_Now;
    [SerializeField] GameObject resultLvParent_Now;
    [SerializeField] TextMeshProUGUI getGachaTicketText;
    Image[] resultCharaImage;
    TextMeshProUGUI[] resultExpText_Pre;
    TextMeshProUGUI[] resultLvText_Pre;
    TextMeshProUGUI[] resultExpText_Now;
    TextMeshProUGUI[] resultLvText_Now;

    [Space(10)]

    int getGachaTicket; //ステージクリア時にガチャチケットをいくつもらえるか

    [SerializeField] int lvUpExp = 100; //経験値いくつごとにレベルアップするか
    [SerializeField] int maxExp = 2000; //最大獲得経験値
    [SerializeField] int maxLv = 20;    //最大レベル

    [Space(10)]

    //プレイヤーキャラのオブジェクトとステータス
    [SerializeField] float lvMagnification = 0.25f; //レベルによるステータスの強化倍率
    [SerializeField] float totsuMagnification = 1.05f; //凸によるステータスの強化倍率
    [Space(10)]
    public GameObject[] playerCharaObj;
    public Status[] playerCharaStatus;
    public TextMeshProUGUI[] playerCharaDamageText;
    Button[] playerCharaButton;
    GameObject[] playerCharaTargetImage;
    [System.NonSerialized] public GameObject[] commandWindow;
    [System.NonSerialized] public Slider[] playerCharaHPSlider;

    [Space(10)]

    //敵のオブジェクトとステータス
    public GameObject[] enemyObj;
    public Status[] enemyStatus;
    public TextMeshProUGUI[] enemyDamageText;
    Button[] enemyButton;
    [System.NonSerialized] public GameObject[] enemyTargetImage;
    [System.NonSerialized] public Slider[] enemyHPSlider;

    [Space(10)]

    public int commandNum = 3; //プレイヤーか敵が選択できるコマンドの数
    int commandCharaIndex; //現在コマンドを選択しているキャラ
    int[,] action; //選択された攻撃またはスキル、使用するキャラ、対象になるキャラを保持しておく配列

    void Start()
    {
        //戦闘に出ているキャラのデータをロード
        BattleCharaDataLoad();
        BattleEnemyDataLoad();
        SkillManager.Instance.SkillDataRoad();

        SoundManager.Instance.PlayBGM(bgmIndex); //BGMを再生

        //戦闘開始時に敵の名前をテキストで表示
        battleText.text = "";
        string[] enemyName = new string[enemyStatus.Length];
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            string enemyNameTmp = enemyStatus[i].name;

            //同じ名前の敵は(敵の名前)A、(敵の名前)Bと続ける
            int sameEnemy = 0;
            for (int j = 0; j < enemyStatus.Length; j++)
            {
                if (i != j && enemyStatus[i].name == enemyStatus[j].name)
                sameEnemy++;
            }
            if (sameEnemy > 0)
            {
                if (sameEnemy == 1)
                {
                    if (i == 0) enemyNameTmp += "A";
                    else        enemyNameTmp += "B";
                }
                else
                {
                    if      (i == 0) enemyNameTmp += "A";
                    else if (i == 1) enemyNameTmp += "B";
                    else             enemyNameTmp += "C";
                }
            }

            enemyName[i] = enemyNameTmp;

            battleText.text += enemyNameTmp;
            if (i <= enemyStatus.Length - 2) battleText.text += "、";
        }
        battleText.text += " が現れた！";

        for (int i = 0; i < enemyStatus.Length; i++)
        {
            enemyStatus[i].name = enemyName[i];
        }

        action = new int[playerCharaStatus.Length + enemyStatus.Length, 2];
        poseButton.SetActive(false); //ポーズボタンを非アクティブ化する
        TargetActive(false, true, true);
        Invoke("TurnStart", battleStartTime); //ターンを開始し、キャラのコマンドを表示する
    }

    //ターンを開始し、１番目のキャラのコマンドを表示する
    void TurnStart()
    {
        commandCharaIndex = 0;
        while (true)
        {
            if (playerCharaStatus[commandCharaIndex].dead)
            {
                commandCharaIndex++;
            }
            else
            {
                break;
            }
        }
        commandWindow[commandCharaIndex].SetActive(true);
        playerCharaObj[commandCharaIndex].transform.SetAsLastSibling(); //現在コマンドを選択しているキャラを一番手前のレイヤーに表示する
        battleText.text = playerCharaStatus[commandCharaIndex].name + "はどうする？"; //現在コマンドを選択しているキャラの名前をテキストで表示
        poseButton.SetActive(true); //ポーズボタンをアクティブ化する

        TargetActive(false, true, true);
        targetCancelButton.SetActive(false);
    }

    //通常攻撃コマンド
    public void AttackButton()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音
        commandWindow[commandCharaIndex].SetActive(false);

        //選んだコマンドを配列に追加
        action[commandCharaIndex, 0] = 0;
        
        TargetActive(true, false, true);
    }
    //スキルコマンド
    public void SkillButton(int skillIndex)
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音
        commandWindow[commandCharaIndex].SetActive(false);

        //選んだコマンドを配列に追加
        action[commandCharaIndex, 0] = skillIndex;

        skillIndex--;

        //全体を対象とするスキルの場合はスキルの対象を-１にする
        if (SkillManager.Instance.playerCharaSkill[commandCharaIndex, skillIndex].targetAll)
        {
            action[commandCharaIndex, 1] = -1;

            NextChara();
            return;
        }
        //敵か味方どちらを対象にするか
        if (SkillManager.Instance.playerCharaSkill[commandCharaIndex,skillIndex].skillType != 1 && SkillManager.Instance.playerCharaSkill[commandCharaIndex, skillIndex].skillType != 3)
        {
            //敵に対するスキル
            TargetActive(true, false, true);
        }
        else
        {
            //味方に対するスキル
            TargetActive(true, true, false);
        }
    }
    //攻撃、スキルの対象を選ぶ
    public void TargetButton(int targetIndex)
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        action[commandCharaIndex, 1] = targetIndex;

        TargetActive(false, true, true);
        targetCancelButton.SetActive(false);
        NextChara();
    }
    //攻撃、スキルの対象を選ぶのをキャンセル
    public void TargetCancelButton()
    {
        TargetActive(false, true, true);
        targetCancelButton.SetActive(false);
        commandWindow[commandCharaIndex].SetActive(true);
        battleText.text = playerCharaStatus[commandCharaIndex].name + "はどうする？"; //現在コマンドを選択しているキャラの名前をテキストで表示

        TargetActive(false, true, true);
    }
    //前のキャラのコマンドを選択しなおす
    public void CharaBackButton()
    {
        if (playerCharaStatus[commandCharaIndex - 1].dead) return; //前のキャラが戦闘不能の場合は戻らない

        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        commandWindow[commandCharaIndex].SetActive(false); 
        commandCharaIndex = Mathf.Max(0, commandCharaIndex - 1);
        commandWindow[commandCharaIndex].SetActive(true);
        playerCharaObj[commandCharaIndex].transform.SetAsLastSibling(); //現在コマンドを選択しているキャラを一番手前のレイヤーに表示する
        battleText.text = playerCharaStatus[commandCharaIndex].name + "はどうする？"; //現在コマンドを選択しているキャラの名前をテキストで表示

        TargetActive(false, true, true);
    }

    //次のキャラのコマンド選択へ移る
    void NextChara()
    {
        TargetActive(false, true, true);

        commandCharaIndex++;

        //隊列の最後のキャラの場合は敵の行動抽選に移る
        if (commandCharaIndex == playerCharaStatus.Length)
        {
            poseButton.SetActive(false); //ポーズボタンを非アクティブ化する
            EnemyActionSelect();
            return;
        }
        while (true)
        {
            if (commandCharaIndex < playerCharaStatus.Length && playerCharaStatus[commandCharaIndex].dead)
            {
                commandCharaIndex++;
            }
            else if (commandCharaIndex >= playerCharaStatus.Length)
            {
                poseButton.SetActive(false); //ポーズボタンを非アクティブ化する
                EnemyActionSelect();
                return;
            }
            else
            {
                break;
            }
        }
        commandWindow[commandCharaIndex].SetActive(true);
        playerCharaObj[commandCharaIndex].transform.SetAsLastSibling(); //現在コマンドを選択しているキャラを一番手前のレイヤーに表示する
        battleText.text = playerCharaStatus[commandCharaIndex].name + "はどうする？"; //現在コマンドを選択しているキャラの名前をテキストで表示
    }
    //技の対象になるキャラを切り替え
    void TargetActive(bool active, bool playerChara, bool enemy, int skipIndex = -1)
    {
        if (playerChara)
        {
            for (int i = 0; i < playerCharaButton.Length; i++)
            {
                if (playerCharaStatus[i].dead)
                {
                    playerCharaButton[i].interactable = false;
                }
                else if (i != skipIndex)
                {
                    playerCharaButton[i].interactable = active;
                    playerCharaTargetImage[i].SetActive(active);
                }
            }
        }
        if (enemy)
        {
            for (int i = 0; i < enemyButton.Length; i++)
            {
                if (enemyStatus[i].dead)
                {
                    enemyButton[i].interactable = false;
                }
                else if (i != skipIndex)
                {
                    enemyButton[i].interactable = active;
                    enemyTargetImage[i].SetActive(active);
                }
            }
        }

        targetCancelButton.SetActive(true);
    }
    //敵の行動を抽選
    void EnemyActionSelect()
    {
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            int randomCommand = UnityEngine.Random.Range(0, commandNum);
            action[i + playerCharaStatus.Length, 0] = randomCommand;

            //全体を対象とするスキルの場合はスキルの対象を-１にする
            if (randomCommand == 1 && SkillManager.Instance.enemySkill[i, randomCommand - 1].targetAll ||
                randomCommand == 2 && SkillManager.Instance.enemySkill[i, randomCommand - 1].targetAll)
            {
                action[i + playerCharaStatus.Length, 1] = -1;
            }
            //ランダムに１体選ぶ
            else
            {
                //敵に対するスキル
                if (randomCommand == 0 ||
                    randomCommand > 0 && SkillManager.Instance.enemySkill[i, randomCommand - 1].skillType != 1 ||
                    randomCommand > 0 && SkillManager.Instance.enemySkill[i, randomCommand - 1].skillType != 3)
                {
                    while (true)
                    {
                        int randomTarget = UnityEngine.Random.Range(0, playerCharaStatus.Length);
                        //ランダムに選ばれたキャラが死亡していた場合は選びなおす
                        if (!playerCharaStatus[randomTarget].dead)
                        {
                            action[i + playerCharaStatus.Length, 1] = randomTarget;
                            break;
                        }
                    }
                }
                //味方(プレイヤー側)に対するスキル
                else
                {
                    while (true)
                    {
                        int randomTarget = UnityEngine.Random.Range(0, enemyStatus.Length);
                        //ランダムに選ばれたキャラが死亡していた場合は選びなおす
                        if (!enemyStatus[randomTarget].dead)
                        {
                            action[i + enemyStatus.Length, 1] = randomTarget;
                            break;
                        }
                    }
                }
            }
        }

        ActionStart();
    }
    //全てキャラが行動を選び終えたら、キャラを素早さ順にソートする
    void ActionStart()
    {
        int[] speed = new int[playerCharaStatus.Length + enemyStatus.Length];
        int[] actionOrderIndex = new int[speed.Length];

        //味方キャラと敵キャラを素早さ順に並べ替える
        for (int i = 0; i < speed.Length; i++)
        {
            speed[i] = (i < playerCharaStatus.Length) ? playerCharaStatus[i].spd : enemyStatus[i - playerCharaStatus.Length].spd;
            actionOrderIndex[i] = i;

            for (int j = i; j > 0; j--)
            {
                if (speed[j] > speed[j - 1])
                {
                    int tmpSpeed = speed[j - 1];
                    speed[j - 1] = speed[j];
                    speed[j] = tmpSpeed;

                    int tmpIndex = actionOrderIndex[j - 1];
                    actionOrderIndex[j - 1] = actionOrderIndex[j];
                    actionOrderIndex[j] = tmpIndex;
                }
            }
        }

        StartCoroutine(Action(actionOrderIndex));
    }

    //素早さ順に攻撃、スキルを実行する
    IEnumerator Action(int[] actionOrderIndex)
    {
        for (int i = 0; i < actionOrderIndex.Length; i++)
        {
            int userIndex = actionOrderIndex[i];
            int actionIndex = action[userIndex, 0];
            int targetIndex = action[userIndex, 1];

            //行動可能なキャラを保持
            bool[] aliveChara = new bool[playerCharaStatus.Length];
            bool turnSkip = false;
            for (int j = 0; j < playerCharaStatus.Length; j++)
            {
                aliveChara[j] = !playerCharaStatus[j].dead;
            }
            bool[] aliveEnemy = new bool[enemyStatus.Length];
            for (int j = 0; j < enemyStatus.Length; j++)
            {
                aliveEnemy[j] = !enemyStatus[j].dead;
            }

            //味方キャラの行動　行動するキャラが戦闘不能の場合は飛ばす
            if (userIndex < playerCharaStatus.Length && aliveChara[userIndex])
            {
                if (actionIndex == 0)
                {
                    //攻撃の対象が戦闘不能の場合は飛ばす
                    if (!enemyStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.NormalAttack(true, userIndex, targetIndex);
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                }
                else
                {
                    //スキルの対象が戦闘不能の場合は飛ばす
                    if (targetIndex < 0 ||
                        targetIndex >= 0 && SkillManager.Instance.playerCharaSkill[userIndex, actionIndex - 1].skillType == 1 && !playerCharaStatus[targetIndex].dead ||
                        targetIndex >= 0 && SkillManager.Instance.playerCharaSkill[userIndex, actionIndex - 1].skillType == 3 && !playerCharaStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.SkillActivation(actionIndex - 1, true, userIndex, targetIndex, (action[userIndex, 1] < 0));
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                    else if (targetIndex < 0 || targetIndex >= 0 && !enemyStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.SkillActivation(actionIndex - 1, true, userIndex, targetIndex, (action[userIndex, 1] < 0));
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                }
            }
            //敵キャラの行動　//行動する敵キャラが戦闘不能の場合は飛ばす
            else if (userIndex >= playerCharaStatus.Length && aliveEnemy[userIndex - playerCharaStatus.Length])
            {
                if (actionIndex == 0)
                {
                    //攻撃の対象が戦闘不能の場合は飛ばす
                    if (!playerCharaStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.NormalAttack(false, userIndex - playerCharaStatus.Length, targetIndex);
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                }
                else
                {
                    //スキルの対象が戦闘不能の場合は飛ばす
                    if (targetIndex < 0 ||
                        targetIndex >= 0 && SkillManager.Instance.enemySkill[userIndex - playerCharaStatus.Length, actionIndex - 1].skillType == 1 && !enemyStatus[targetIndex].dead ||
                        targetIndex >= 0 && SkillManager.Instance.enemySkill[userIndex - playerCharaStatus.Length, actionIndex - 1].skillType == 3 && !enemyStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.SkillActivation(actionIndex - 1, false, userIndex - playerCharaStatus.Length, targetIndex, (action[userIndex, 1] < 0));
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                    else if (targetIndex < 0 || targetIndex >= 0 && !playerCharaStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.SkillActivation(actionIndex - 1, false, userIndex - playerCharaStatus.Length, targetIndex, (action[userIndex, 1] < 0));
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                }
            }
            else
            {
                turnSkip = true;
            }
            //ダメージの表示を消す  HPが０になったキャラは非アクティブ化
            if (!turnSkip)
            {
                string deadText = "";
                for (int j = 0; j < playerCharaStatus.Length; j++)
                {
                    playerCharaDamageText[j].gameObject.SetActive(false);

                    //戦闘不能になった場合
                    if (aliveChara[j] && playerCharaStatus[j].dead)
                    {
                        playerCharaObj[j].SetActive(false);
                        deadText += playerCharaStatus[j].name += "、";
                    }
                }
                //戦闘不能のキャラをテキストで表示
                if (deadText != "")
                {
                    battleText.text = deadText.Substring(0, deadText.Length - 1) + "はやられた！";
                    yield return new WaitForSeconds(actionWaitTime);
                }
                deadText = "";
                for (int j = 0; j < enemyStatus.Length; j++)
                {
                    enemyDamageText[j].gameObject.SetActive(false);

                    //戦闘不能になった場合
                    if (aliveEnemy[j] && enemyStatus[j].dead)
                    {
                        enemyObj[j].SetActive(false);
                        deadText += enemyStatus[j].name += "、";
                    }
                }
                //戦闘不能のキャラをテキストで表示
                if (deadText != "")
                {
                    battleText.text = deadText.Substring(0, deadText.Length - 1) + "を倒した！";
                    yield return new WaitForSeconds(actionWaitTime);
                }
            }
        }

        TurnEnd();

        yield break;
    }
    //すべてのキャラが行動し終わったら次のターンに映る
    void TurnEnd()
    {
        int deadPlayerCharaCount = 0;
        int killEnemyCount = 0;

        for (int i = 0; i < playerCharaStatus.Length; i++)
        {
            //HPが０になったキャラをカウント
            if (playerCharaStatus[i].dead)
            {
                deadPlayerCharaCount++; //倒された味方の数をカウント
            }
        }
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            //HPが０になった敵をカウント
            if (enemyStatus[i].dead)
            {
                killEnemyCount++; //倒した敵の数をカウント
            }
        }

        //味方がすべて倒されたらゲームオーバー
        if (deadPlayerCharaCount >= playerCharaStatus.Length)
        {
            GameOver();
            return;
        }
        //全ての敵を倒したらステージクリア
        if (killEnemyCount >= enemyStatus.Length)
        {
            StageClear();
            return;
        }

        //次のターン
        TurnStart();
    }
    //ゲームオーバー
    void GameOver()
    {
        canvas[1].SetActive(true);
        SoundManager.Instance.PlayBGM(2); //ゲームオーバー時のBGMを再生
    }
    //ステージクリア
    void StageClear()
    {
        battleText.text = "戦闘に勝利した！";
        SoundManager.Instance.PlayBGM(1); //ステージクリア時のBGMを再生

        Invoke("Result", resultWaitTime);
    }
    //リザルト画面
    void Result()
    {
        canvas[2].SetActive(true);

        int id = 0;
        //経験値を獲得、レベルアップ
        for (int i = 0; i <playerCharaStatus.Length; i++)
        {
            id = DataManager.Instance.playerData.battleCharaId[i];

            DataManager.Instance.charaData[id].exp = Mathf.Min(maxExp, DataManager.Instance.charaData[id].exp + getExp);
            DataManager.Instance.charaData[id].lv = 1 + Mathf.Min(maxLv - 1, (int)(DataManager.Instance.charaData[id].exp / lvUpExp));

            resultExpText_Now[i].text = DataManager.Instance.charaData[id].exp.ToString();
            resultLvText_Now[i].text = DataManager.Instance.charaData[id].lv.ToString();
        }
        //ガチャチケットを獲得
        DataManager.Instance.playerData.gachaTicketNum += getGachaTicket;
        getGachaTicketText.text = "× " + getGachaTicket;

        //データをセーブ
        SaveManager.Instance.PlayerDataSave();
        SaveManager.Instance.CharaDataSave();
    }

    void BattleCharaDataLoad()
    {
        playerCharaButton = new Button[DataManager.Instance.partyNum];
        playerCharaTargetImage = new GameObject[DataManager.Instance.partyNum];
        commandWindow = new GameObject[DataManager.Instance.partyNum];
        playerCharaHPSlider = new Slider[DataManager.Instance.partyNum];

        resultCharaImage = new Image[DataManager.Instance.partyNum];
        resultExpText_Pre = new TextMeshProUGUI[DataManager.Instance.partyNum];
        resultLvText_Pre = new TextMeshProUGUI[DataManager.Instance.partyNum];
        resultExpText_Now = new TextMeshProUGUI[DataManager.Instance.partyNum];
        resultLvText_Now = new TextMeshProUGUI[DataManager.Instance.partyNum];

        //戦闘に出るキャラの絵とステータスを読み込み
        playerCharaStatus = new Status[DataManager.Instance.partyNum];
        for (int i = 0; i < playerCharaStatus.Length; i++)
        {
            playerCharaStatus[i] = new Status();

            //キャラのIDと名前を取得
            int id = DataManager.Instance.playerData.battleCharaId[i];
            playerCharaStatus[i].id = id;
            playerCharaStatus[i].name = DataManager.Instance.charaData[id].charaName;

            //キャラが持つ各コンポーネントを取得
            playerCharaButton[i] = playerCharaObj[i].GetComponent<Button>();
            playerCharaTargetImage[i] = playerCharaObj[i].transform.GetChild(0).gameObject;
            commandWindow[i] = playerCharaObj[i].transform.GetChild(1).gameObject;
            playerCharaHPSlider[i] = playerCharaObj[i].transform.GetChild(2).GetComponent<Slider>();
            playerCharaHPSlider[i].value = 1;
            //リザルト画面で表示するテキスト
            resultExpText_Pre[i] = resultExpParent_Pre.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultLvText_Pre[i] = resultLvParent_Pre.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultExpText_Now[i] = resultExpParent_Now.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultLvText_Now[i] = resultLvParent_Now.transform.GetChild(i).GetComponent<TextMeshProUGUI>();

            resultExpText_Pre[i].text = DataManager.Instance.charaData[id].exp.ToString();
            resultLvText_Pre[i].text = DataManager.Instance.charaData[id].lv.ToString();

            //キャラのステータスの基礎値にレベルと凸の数値を掛けて戦闘時のステータスを決める
            int totsu = DataManager.Instance.charaData[id].totsu;
            int lv = DataManager.Instance.charaData[id].lv;
            playerCharaStatus[i].hp_default = (DataManager.Instance.charaData[id].hp + (int)(lv * lvMagnification * DataManager.Instance.charaData[id].hp)) * (int)(totsu * totsuMagnification);
            playerCharaStatus[i].hp = playerCharaStatus[i].hp_default;
            playerCharaStatus[i].atk_default = (DataManager.Instance.charaData[id].atk + (int)(lv * lvMagnification * DataManager.Instance.charaData[id].atk)) * (int)(totsu * totsuMagnification);
            playerCharaStatus[i].atk = playerCharaStatus[i].atk_default;
            playerCharaStatus[i].def_default = (DataManager.Instance.charaData[id].def + (int)(lv * lvMagnification * DataManager.Instance.charaData[id].def)) * (int)(totsu * totsuMagnification);
            playerCharaStatus[i].def = playerCharaStatus[i].def_default;
            playerCharaStatus[i].spd_default = (DataManager.Instance.charaData[id].spd + (int)(lv * lvMagnification * DataManager.Instance.charaData[id].spd)) * (int)(totsu * totsuMagnification);
            playerCharaStatus[i].spd = playerCharaStatus[i].spd_default;
            playerCharaStatus[i].skillId1 = DataManager.Instance.charaData[id].skillId1;
            playerCharaStatus[i].skillId2 = DataManager.Instance.charaData[id].skillId2;
            playerCharaStatus[i].normalAttackId = DataManager.Instance.charaData[id].normalAttackId;
            playerCharaStatus[i].skillName1 = DataManager.Instance.skillData[playerCharaStatus[i].skillId1].skillName;
            playerCharaStatus[i].skillName2 = DataManager.Instance.skillData[playerCharaStatus[i].skillId2].skillName;

            //コマンドウィンドウのボタンにスキル名を割り当て
            commandWindow[i].transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = playerCharaStatus[i].skillName1;
            commandWindow[i].transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = playerCharaStatus[i].skillName2;
            //一番最初に行動するキャラは行動順を戻すボタンを使えないようにする
            if (i == 0) commandWindow[i].transform.GetChild(1).gameObject.SetActive(false);

            //キャラの絵をオブジェクト割り当てる
            Image charaImage = playerCharaObj[i].GetComponent<Image>();
            Sprite roadSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[id].spritePass);
            charaImage.sprite = roadSprite;

            resultCharaImage[i] = resultCharaImageParent.transform.GetChild(i).GetComponent<Image>();
            resultCharaImage[i].sprite = roadSprite;
        }
    }

    void BattleEnemyDataLoad()
    {
        //CSVから敵の編成データを読み込み
        TextAsset csvFile = Resources.Load("EnemyOrderData") as TextAsset;
        StringReader reader = new StringReader(csvFile.text);
        List<string[]> enemyOrderDatas = new List<string[]>();
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            enemyOrderDatas.Add(line.Split(','));
        }

        int battleId = DataManager.Instance.playerData.battleId;

        //背景を読み込み
        backgroundImage.sprite = Resources.Load<Sprite>(enemyOrderDatas[battleId][0]);
        backgroundImage.color = new Color(float.Parse(enemyOrderDatas[battleId][1]), float.Parse(enemyOrderDatas[battleId][2]), float.Parse(enemyOrderDatas[battleId][3]));
        //BGMの番号を読み込み
        bgmIndex = int.Parse(enemyOrderDatas[battleId][4]);

        //クリア時の報酬を読み込み
        getExp = int.Parse(enemyOrderDatas[battleId][5]); //経験値
        getGachaTicket = int.Parse(enemyOrderDatas[battleId][6]); //ガチャチケット

        enemyButton = new Button[DataManager.Instance.partyNum];
        enemyTargetImage = new GameObject[DataManager.Instance.partyNum];
        enemyHPSlider = new Slider[DataManager.Instance.partyNum];

        //戦闘に出る敵の絵とステータスを読み込み
        enemyStatus = new Status[DataManager.Instance.partyNum];
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            enemyStatus[i] = new Status();

            //敵のIDを取得（７行目までは背景画像のパスやクリア時の報酬のデータが入っているため、８行目から読み込む）
            int id = int.Parse(enemyOrderDatas[battleId][i + 7]);
            enemyStatus[i].id = id;
            enemyStatus[i].name = DataManager.Instance.enemyData[id].enemyName;

            //敵が持つ各コンポーネントを取得
            enemyButton[i] = enemyObj[i].GetComponent<Button>();
            enemyTargetImage[i] = enemyObj[i].transform.GetChild(0).gameObject;
            enemyHPSlider[i] = enemyObj[i].transform.GetChild(1).GetComponent<Slider>();
            enemyHPSlider[i].value = 1;

            //敵キャラのステータスを割り当てる
            enemyStatus[i].hp_default = DataManager.Instance.enemyData[id].hp;
            enemyStatus[i].hp = enemyStatus[i].hp_default;
            enemyStatus[i].atk_default = DataManager.Instance.enemyData[id].atk;
            enemyStatus[i].atk = enemyStatus[i].atk_default;
            enemyStatus[i].def_default = DataManager.Instance.enemyData[id].def;
            enemyStatus[i].def = enemyStatus[i].def_default;
            enemyStatus[i].spd_default = DataManager.Instance.enemyData[id].spd;
            enemyStatus[i].spd = enemyStatus[i].spd_default;
            enemyStatus[i].skillId1 = DataManager.Instance.enemyData[id].skillId1;
            enemyStatus[i].skillId2 = DataManager.Instance.enemyData[id].skillId2;
            enemyStatus[i].normalAttackId = DataManager.Instance.enemyData[id].normalAttackId;
            enemyStatus[i].skillName1 = DataManager.Instance.skillData[enemyStatus[i].skillId1].skillName;
            enemyStatus[i].skillName2 = DataManager.Instance.skillData[enemyStatus[i].skillId2].skillName;

            //敵キャラの絵をオブジェクト割り当てる
            Image image = enemyObj[i].GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(DataManager.Instance.enemyData[id].spritePass);
        }
    }

    //戦闘を一時的するボタン
    public void Pause()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        canvas[3].SetActive(!canvas[3].activeSelf);
    }
    //戦闘をやり直すボタン
    public void Retry()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        FadeManager.Instance.LoadSceneIndex(1, 0.5f);
    }
    //メニュー画面に戻るボタン
    public void BackMenu()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        FadeManager.Instance.LoadSceneIndex(0, 0.5f);
    }
    //音量設定画面を表示するボタン
    public void SoundSetting()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        if (!soundSettingWindow.activeSelf)
        {
            pauseWindowButton.SetActive(false);
            soundSettingWindow.SetActive(true);
        }
        else
        {
            pauseWindowButton.SetActive(true);
            soundSettingWindow.SetActive(false);

            DataManager.Instance.playerData.bgmVolume = (int)SoundManager.Instance.bgmVolSlider.value;
            DataManager.Instance.playerData.seVolume = (int)SoundManager.Instance.seVolSlider.value;
        }
    }
}

//味方、敵の戦闘時のステータス用クラス
[System.Serializable]
public class Status
{
    public int id;
    public string name;

    public int hp;
    public int atk;
    public int def;
    public int spd;

    public int hp_default;
    public int atk_default;
    public int def_default;
    public int spd_default;

    public int skillId1;
    public string skillName1;
    public int skillId2;
    public string skillName2;

    public int normalAttackId;

    public bool dead;
    public int regeneTurn;
    public int buff_DebuffTurn;
    public int poisonTurn;
    public int sleepTurn;
}