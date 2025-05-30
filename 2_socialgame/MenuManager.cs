using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// メニュー画面で使う処理をまとめたクラス
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    //メニュー中の各Cavnas
    [SerializeField] GameObject[] canvas;
    //セーブデータ生成中の表示
    public GameObject saveDataInitializeWindow;
    //背景画像
    [SerializeField] Image[] backGroundImage;
    [SerializeField] Image backGroundCharaImage;

    [Space(10)]

    //キャラのステータス画面用変数
    [SerializeField] GameObject charaStatusViewButton;
    [SerializeField] GameObject charaStatusParent;
    [SerializeField] GameObject charaStatusViewWindow;
    [SerializeField] TextMeshProUGUI charaStatusView_CharaNameText;
    [SerializeField] Image charaStatusView_CharaImage;
    [SerializeField] GameObject charaStatusView_TotsuImage_Parent;
    [SerializeField] GameObject charaStatusView_StatusText_Parent;
    GameObject[] charaStatusView_TotsuImage;
    TextMeshProUGUI[] charaStatusView_StatusText;
    [SerializeField] GameObject battleCharaChoiceApplyWindow;
    [SerializeField] TextMeshProUGUI battleCharaChoice_CautionText;
    [SerializeField] Button battleCharaChoice_ApplyButton;
    Button[] charaIcon;
    GameObject[] charaIconMini;

    [Space(10)]

    //ステージ選択画面に関する変数
    //ステージ選択のボタンと親のオブジェクト
    [SerializeField] GameObject stageSelectWindow;
    GameObject stageSelectButton_Group;
    GameObject stageSelectButton;
    public int stageNum;              //総ステージ数 EnemyOrderData.csvの行数から得る
    public int pageNum;               //ステージ選択画面のページ数
    public int nowPage;               //現在のページの番号
    int oneWindowStageNum = 6; //一つのウィンドウにいくつのボタンを入れるか

    [Space(10)]

    //オプション画面に関する変数
    [SerializeField] GameObject optionWindowButton;
    [SerializeField] GameObject saveDataDeleteWindow;
    [SerializeField] GameObject soundSettingWindow;

    [Space(10)]

    [SerializeField] float lvMagnification = 0.25f;
    [SerializeField] float totsuMagnification = 1.1f;

    bool stageSelect, backGroundCharaChoice;
    int[] choiceCharaId; //戦闘時の編成に選ばれているキャラのIDを一時的に保存する変数、選ばれていない場合は -1 になる
    int choiceCharaId_Index;
    int choiceCharaId_Tmp;
    int choiceCharaCount;
    bool charaChoiceComplete;

    void Start()
    {
        //Cavnasの表示切り替え
        canvas[0].SetActive(false);
        canvas[1].SetActive(true);
        canvas[2].SetActive(DataManager.Instance.isBattle);
        canvas[3].SetActive(false);
        canvas[4].SetActive(false);
        canvas[5].SetActive(false);
        //背景画像読み込み
        Sprite backGroundSprite = Resources.Load<Sprite>("Textures/Background_Menu");
        for (int i = 0; i < backGroundImage.Length; i++)
        {
            backGroundImage[i].sprite = backGroundSprite;
            backGroundImage[i].color = new Color(1, 1, 1, 1);
            backGroundImage[i].transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }
        backGroundCharaImage.sprite = Resources.Load<Sprite>(DataManager.Instance.charaData[DataManager.Instance.playerData.menuBackCharaId].spritePass);
        backGroundCharaImage.color = new Color(1, 1, 1, 1);
        backGroundCharaImage.transform.localScale = new Vector3(1f, 1f, 1f);
        //BGMを再生
        SoundManager.Instance.PlayBGM(0);

        //ゲーム起動時にのみタイトル画面を表示
        if (DataManager.Instance.isStartup)
        {
            canvas[1].SetActive(false);
            canvas[0].SetActive(true);
            DataManager.Instance.isStartup = false;
        }

        DataManager.Instance.isBattle = false;

        StageSelectButtonInitialize();
        CharaStatusCanvasInitialize();  
    }

    //メニュー画面へ移行するボタン
    public void Menu()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        canvas[0].SetActive(false);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);
        canvas[4].SetActive(false);
        canvas[5].SetActive(false);

        canvas[1].SetActive(true);
    }
    //戦闘画面へ移行するボタン
    public void StageSelect(int battleId)
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        //引数がマイナスならステージセレクト画面の表示切り替え
        if (battleId < 0)
        {
            canvas[2].SetActive(!canvas[2].activeSelf);
            stageSelect = !stageSelect;
        }
        //引数が0以上なら戦闘画面へ移行
        else
        {
            DataManager.Instance.isBattle = true;
            DataManager.Instance.playerData.battleId = battleId;
            SaveManager.Instance.PlayerDataSave();
            FadeManager.Instance.LoadSceneIndex(1, 0.5f);
        }
    }
    //ステージ選択画面のページ切り替え
    public void StagePageChange(bool nextPage)
    {
        if (nextPage && nowPage >= pageNum - 1 || !nextPage && nowPage <= 0) return;

        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        //現在開いているページを非表示
        GameObject nowPageObj = stageSelectWindow.transform.GetChild(nowPage).gameObject;
        nowPageObj.SetActive(false);
        int loopNum = (nowPage < (pageNum - 1)) ? oneWindowStageNum : nowPageObj.transform.childCount;
        //ボタンのサイズをリセット
        for (int i = 0; i < loopNum; i++)
        {
            nowPageObj.transform.GetChild(i).localScale = new Vector3(1, 1, 1);
        }

        //次のページを表示
        nowPage = (nextPage) ? nowPage + 1 : nowPage - 1;
        stageSelectWindow.transform.GetChild(nowPage).gameObject.SetActive(true);
    }
    //ガチャ画面へ移行するボタン
    public void Gacha()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        canvas[1].SetActive(canvas[4].activeSelf);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);
        canvas[5].SetActive(false);

        canvas[4].SetActive(!canvas[4].activeSelf);
    }
    //オプション画面へ移行するボタン
    public void Option()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        canvas[5].SetActive(!canvas[5].activeSelf);
    }
    //メニュー画面に表示するキャラを選択するボタン
    public void BackGroundCharaChoice()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        backGroundCharaChoice = true;

        CharaStatus();
    }
    //音量設定画面を表示するボタン
    public void SoundSetting()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        if (!soundSettingWindow.activeSelf)
        {
            optionWindowButton.SetActive(false);
            soundSettingWindow.SetActive(true);
        }
        else
        {
            optionWindowButton.SetActive(true);
            soundSettingWindow.SetActive(false);

            DataManager.Instance.playerData.bgmVolume = (int)SoundManager.Instance.bgmVolSlider.value;
            DataManager.Instance.playerData.seVolume = (int)SoundManager.Instance.seVolSlider.value;

            SaveManager.Instance.PlayerDataSave();
        }
    }
    //セーブデータ消去の画面を表示するボタン
    public void SaveDataDeleteWindow()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        saveDataDeleteWindow.SetActive(!saveDataDeleteWindow.activeSelf);
    }
    //セーブデータを消去するボタン
    public void SaveDataDelete()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        SaveManager.Instance.PlayerDataInitialize();
        SaveManager.Instance.CharaDataInitialize();

        FadeManager.Instance.LoadSceneIndex(0, 0.5f);
        DataManager.Instance.DataLoad();
    }
    //ゲーム終了ボタン
    public void Exit()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        //UnityEditor上でプレイを終了する場合
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //ビルドした実行データでプレイを終了する場合
        Application.Quit();
#endif
    }

    //キャラクターのステータス画面へ移行するボタン
    public void CharaStatus()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        if (stageSelect && canvas[3].activeSelf)
        {
            //キャラ編成画面から戻る場合にウィンドウを表示
            if (!battleCharaChoiceApplyWindow.activeSelf)
            {  
                if (choiceCharaId_Tmp >= 0 && charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.activeSelf)
                {
                    charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
                    charaIcon[choiceCharaId_Tmp].interactable = true;
                }
                battleCharaChoiceApplyWindow.SetActive(true);
                //編成用のキャラを選んでいない場合は保存するボタンを押せない
                battleCharaChoice_CautionText.gameObject.SetActive(choiceCharaCount < DataManager.Instance.partyNum);
                battleCharaChoice_ApplyButton.interactable = choiceCharaCount >= DataManager.Instance.partyNum;
                return;
            }
            //キャラ編成画面から戻るのをキャンセルした場合の処理
            else if (!charaChoiceComplete)
            {
                battleCharaChoiceApplyWindow.SetActive(false);
                return;
            }
            //キャラ編成が完了した場合の処理
            else
            {
                battleCharaChoiceApplyWindow.SetActive(false);
                battleCharaChoice_CautionText.gameObject.SetActive(false);
                battleCharaChoice_ApplyButton.interactable = true;

                //ステージ選択画面でキャラ編成を表示する
                for (int i = 0; i < charaIconMini.Length; i++)
                {
                    string charaName;
                    Sprite charaSprite;
                    //キャラごとの絵と名前を読み込み
                    charaSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[DataManager.Instance.playerData.battleCharaId[i]].spritePass);
                    charaIconMini[i].transform.GetChild(0).GetComponent<Image>().sprite = charaSprite;
                    charaName = DataManager.Instance.charaData[DataManager.Instance.playerData.battleCharaId[i]].charaName;
                    charaIconMini[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = charaName;
                }

                charaChoiceComplete = false;
            }
        }

        canvas[1].SetActive(canvas[3].activeSelf);
        canvas[2].SetActive(stageSelect);
        canvas[4].SetActive(false);

        if (backGroundCharaChoice && canvas[3].activeSelf)
        {
            canvas[5].SetActive(true);
            backGroundCharaChoice = false;
        }
        else
        {
            canvas[5].SetActive(false);
        }

        canvas[3].SetActive(!canvas[3].activeSelf);
    }
    //ステータス画面でキャラをクリックした時の処理
    public void CharaStatusIconPush(int charaId)
    {
        //キャラのステータス確認画面の場合
        if (!stageSelect && charaId >= 0 && !backGroundCharaChoice)
        {
            SoundManager.Instance.PlaySE_Sys(0); //クリック音

            choiceCharaId_Tmp = charaId;
            CharaStatusView();
        }
        //ステージ選択前のキャラ選択の場合
        else if (!backGroundCharaChoice)
        {
            if (choiceCharaId_Tmp >= 0 && charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.activeSelf)
            {
                SoundManager.Instance.PlaySE_Sys(0); //クリック音

                charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
                charaIcon[choiceCharaId_Tmp].interactable = true;
            }
            if (charaId >= 0)
            {
                SoundManager.Instance.PlaySE_Sys(0); //クリック音

                choiceCharaId_Tmp = charaId;
                charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(true);
                charaIcon[choiceCharaId_Tmp].interactable = false;
            }
        }
        //メニュー画面に表示するキャラ選択の場合
        else
        {
            if (charaId >= 0)
            {
                SoundManager.Instance.PlaySE_Sys(0); //クリック音

                DataManager.Instance.playerData.menuBackCharaId = charaId; //選ばれたキャラのIDを保存
                SaveManager.Instance.PlayerDataSave();

                FadeManager.Instance.LoadSceneIndex(0, 0.5f);
            }
        }
    }
    //キャラのステータスを表示
    public void CharaStatusView()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        if (!charaStatusViewWindow.activeSelf)
        {
            charaStatusViewWindow.SetActive(true);

            CharaData charaData = DataManager.Instance.charaData[choiceCharaId_Tmp];

            charaStatusView_CharaNameText.text = charaData.charaName;
            charaStatusView_CharaImage.sprite = Resources.Load<Sprite>(charaData.spritePass);
            for (int i = 0; i < charaStatusView_TotsuImage.Length; i++)
            {
                charaStatusView_TotsuImage[i].SetActive(i + 1 == charaData.totsu);
            }
            //キャラのステータスの基礎値にレベルと凸の数値を掛けて戦闘時のステータスを決める
            int totsu = charaData.totsu;
            int lv = charaData.lv;
            charaStatusView_StatusText[0].text = lv.ToString();
            charaStatusView_StatusText[1].text = charaData.exp.ToString();
            charaStatusView_StatusText[2].text = (charaData.hp + (int)(lv * lvMagnification * charaData.hp) * (int)(totsu * totsuMagnification)).ToString();
            charaStatusView_StatusText[3].text = (charaData.atk + (int)(lv * lvMagnification * charaData.atk) * (int)(totsu * totsuMagnification)).ToString();
            charaStatusView_StatusText[4].text = (charaData.def + (int)(lv * lvMagnification * charaData.def) * (int)(totsu * totsuMagnification)).ToString();
            charaStatusView_StatusText[5].text = (charaData.spd + (int)(lv * lvMagnification * charaData.spd) * (int)(totsu * totsuMagnification)).ToString();
            charaStatusView_StatusText[6].text = DataManager.Instance.skillData[charaData.skillId1].skillName;
            charaStatusView_StatusText[7].text = DataManager.Instance.skillData[charaData.skillId2].skillName;
        }
        else
        {
            if (stageSelect)
            {
                charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
                charaIcon[choiceCharaId_Tmp].interactable = true;
            }

            charaStatusViewWindow.SetActive(false);
            choiceCharaId_Tmp = -1;
        }
    }
    //キャラを戦闘の編成に追加
    public void BattleCharaChoice()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
        charaIcon[choiceCharaId_Tmp].interactable = true;

        //編成の最後のキャラまで選び終えた後に他のキャラを選んだ場合は、編成１キャラ目の選択を解除する
        if (choiceCharaId[choiceCharaId_Index] >= 0)
        {
            GameObject preChoiceImage = charaIcon[choiceCharaId[choiceCharaId_Index]].transform.GetChild(4).gameObject;
            preChoiceImage.SetActive(false);
            charaIcon[choiceCharaId[choiceCharaId_Index]].interactable = true;
        }

        choiceCharaId[choiceCharaId_Index] = choiceCharaId_Tmp;

        GameObject choiceImage = charaIcon[choiceCharaId_Tmp].transform.GetChild(4).gameObject;
        choiceImage.SetActive(true);
        choiceImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (choiceCharaId_Index + 1).ToString();
        charaIcon[choiceCharaId_Tmp].interactable = false;

        choiceCharaId_Index = (choiceCharaId_Index + 1) % choiceCharaId.Length;
        choiceCharaCount = Mathf.Min(choiceCharaCount + 1, DataManager.Instance.partyNum);
    }
    //キャラの編成を保存するか破棄するか
    public void BattleCharaChoice_ApplyOrDiscard(bool applyOrDiscard)
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            if (choiceCharaId[i] >= 0)
            {
                GameObject choiceImage = charaIcon[choiceCharaId[i]].transform.GetChild(4).gameObject;
                choiceImage.SetActive(false);
                charaIcon[choiceCharaId[i]].interactable = true;
            }
            if (applyOrDiscard)
            {
                DataManager.Instance.playerData.battleCharaId[i] = choiceCharaId[i];
            }
            choiceCharaId[i] = -1;
        }
        choiceCharaId_Index = 0;
        choiceCharaCount = 0;
        charaChoiceComplete = true;
        CharaStatus();
    }

    //キャラのステータス画面を初期化
    void CharaStatusCanvasInitialize()
    {
        charaIcon = new Button[DataManager.Instance.charaNum];
        
        for (int i = 0; i < charaIcon.Length; i++)
        {
            if (i <= 0)
            {
                charaIcon[0] = Instantiate(Resources.Load<GameObject>("Prefabs/CharaStatusIcon")).GetComponent<Button>();
            }
            else
            {
                charaIcon[i] = Instantiate(charaIcon[0]);
            }
            charaIcon[i].transform.SetParent(charaStatusParent.transform);
            charaIcon[i].transform.position = new Vector3(0, 0, 0);
            charaIcon[i].transform.localScale = new Vector3(1, 1, 1);

            string charaName;
            Sprite charaSprite;
            //キャラごとの絵と名前を読み込み
            charaSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[i].spritePass);
            charaIcon[i].transform.GetChild(0).GetComponent<Image>().sprite = charaSprite;
            charaName = DataManager.Instance.charaData[i].charaName;
            charaIcon[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = charaName;
            
            int _i = i;
            //キャラを選択したときの処理をボタンに割り当て
            charaIcon[i].onClick.AddListener(() => CharaStatusIconPush(_i));
            GameObject window = charaIcon[i].transform.GetChild(2).gameObject;
            window.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(BattleCharaChoice);
            window.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(CharaStatusView);

            //初期状態で表示しないウィンドウ
            charaIcon[i].transform.GetChild(2).gameObject.SetActive(false);
            charaIcon[i].transform.GetChild(4).gameObject.SetActive(false);
            charaStatusViewWindow.SetActive(false);
        }

        //所持しているキャラのみをクリックできるようにする
        CharaStatusCanvasFlag();

        //戦闘時のキャラ編成の配列を-1で初期化
        choiceCharaId = new int[DataManager.Instance.partyNum];
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            choiceCharaId[i] = -1;
        }

        //ステージ選択画面でキャラ編成を表示する
        charaIconMini = new GameObject[DataManager.Instance.partyNum];
        for (int i = 0; i < charaIconMini.Length; i++)
        {
            charaIconMini[i] = Instantiate(Resources.Load<GameObject>("Prefabs/CharaIconMini"));
            charaIconMini[i].transform.SetParent(charaStatusViewButton.transform);
            charaIconMini[i].transform.position = new Vector3(0, 0, 0);
            charaIconMini[i].transform.localScale = new Vector3(1, 1, 1);

            string charaName;
            Sprite charaSprite;
            //キャラごとの絵と名前を読み込み
            charaSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[DataManager.Instance.playerData.battleCharaId[i]].spritePass);
            charaIconMini[i].transform.GetChild(0).GetComponent<Image>().sprite = charaSprite;
            charaName = DataManager.Instance.charaData[DataManager.Instance.playerData.battleCharaId[i]].charaName;
            charaIconMini[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = charaName;
        }

        //ステータス画面で表示するコンポーネントを取得
        charaStatusView_TotsuImage = new GameObject[charaStatusView_TotsuImage_Parent.transform.childCount];
        for (int i = 0; i < charaStatusView_TotsuImage.Length; i++)
        {
            charaStatusView_TotsuImage[i] = charaStatusView_TotsuImage_Parent.transform.GetChild(i).gameObject;
        }
        charaStatusView_StatusText = new TextMeshProUGUI[charaStatusView_StatusText_Parent.transform.childCount];
        for (int i = 0; i < charaStatusView_StatusText.Length; i++)
        {
            charaStatusView_StatusText[i] = charaStatusView_StatusText_Parent.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
        }

        battleCharaChoice_CautionText.text = "※まだ" + DataManager.Instance.partyNum + "体選ばれていません";

        //初期状態で表示しないウィンドウ
        battleCharaChoiceApplyWindow.SetActive(false);
    }

    //所持しているキャラのみをキャラステータス画面でアクティブ化
    public void CharaStatusCanvasFlag()
    {
        for (int i = 0; i < DataManager.Instance.charaNum; i++)
        {
            bool flag = DataManager.Instance.charaData[i].get;
            charaIcon[i].interactable = flag;
            charaIcon[i].transform.GetChild(3).gameObject.SetActive(!flag);
        }
    }

    //ステージ選択画面を初期化
    void StageSelectButtonInitialize()
    {
        stageNum = DataManager.Instance.stageNum;

        stageSelectButton_Group = Resources.Load<GameObject>("Prefabs/StageSelectButtonGroup");
        stageSelectButton = Resources.Load<GameObject>("Prefabs/StageSelectButton");

        for (int i = 0; i < stageNum; i += oneWindowStageNum)
        {
            GameObject stageSelectButton_Group_Instance = Instantiate(stageSelectButton_Group);
            stageSelectButton_Group_Instance.transform.SetParent(stageSelectWindow.transform);
            stageSelectButton_Group_Instance.transform.localPosition = new Vector3(0, 0, 0);
            stageSelectButton_Group_Instance.transform.localScale = new Vector3(1, 1, 1);

            int loopNum = (i < stageNum - oneWindowStageNum) ? oneWindowStageNum : stageNum - i;
            for (int j = 0; j < loopNum; j++)
            {
                GameObject stageSelectButton_Instance = Instantiate(stageSelectButton);
                stageSelectButton_Instance.transform.SetParent(stageSelectButton_Group_Instance.transform);
                stageSelectButton_Instance.transform.localPosition = new Vector3(0, 0, 0);
                stageSelectButton_Instance.transform.localScale = new Vector3(1, 1, 1);
                //Buttonのコンポーネントを設定
                int _i = i + j;
                Button button = stageSelectButton_Instance.GetComponent<Button>();
                button.onClick.AddListener(() => StageSelect(_i));
                //Textを設定
                TextMeshProUGUI tmp = stageSelectButton_Instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                tmp.text = "ステージ" + (i + j + 1);
            }

            pageNum++;
            stageSelectButton_Group_Instance.SetActive(false);
        }

        nowPage = 0;
        stageSelectWindow.transform.GetChild(nowPage).gameObject.SetActive(true);
    }
}
