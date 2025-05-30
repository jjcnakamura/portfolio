using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// データをロードする処理と、シーンをまたいで利用するゲーム中の変数をまとめたクラス
/// </summary>
public class DataManager : Singleton<DataManager>
{
    [System.NonSerialized] public bool isStartup = true; //ゲーム起動時か
    [System.NonSerialized] public bool isBattle;         //戦闘画面か

    //CSVからデータを読み込む変数
    TextAsset csvFile;
    List<string[]> csvDatas = new List<string[]>();

    //プレイヤーのデータを格納する変数
    public PlayerData playerData;
    [System.NonSerialized] public int playerDataNum = 6; //プレイヤーの設定項目の数
    [System.NonSerialized] public int partyNum = 3; //バトル時に編成出来るキャラの数
    [System.NonSerialized] public int stageNum;     //総ステージ数
    public string stageDataPass = "EnemyOrderData";
    public string playerDataPass = "PlayerData";
    public string playerSaveDataPass = "PlayerSaveData";

    //キャラクターのデータを格納する変数
    public CharaData[] charaData;
    [System.NonSerialized] public int charaNum;
    public string charaDataPass = "CharaData";
    public string charaSaveDataPass = "CharaSaveData";
    //敵のデータを格納する変数
    public EnemyData[] enemyData;
    public string enemyDataPass = "EnemyData";

    //スキルのデータを格納する変数
    public SkillData[] skillData;
    public int skillNum;
    public string skillDataPass = "SkillData";

    public string saveDataDirectoryPass = "ProductionByCourse_Data/";

    StringReader reader;

    bool saveDataInitialize; //セーブデータの初期化処理を実行する場合に立てる

    void Awake()
    {
        //シーンを遷移してもDataManagerは残る
        if (gameObject.transform.parent != null) gameObject.transform.parent = null;
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        DataLoad();
    }

    void Update()
    {
        if (saveDataInitialize && File.Exists(saveDataDirectoryPass + playerSaveDataPass) && File.Exists(saveDataDirectoryPass + charaSaveDataPass))
        {
            SaveManager.Instance.PlayerDataInitialize();
            SaveManager.Instance.CharaDataInitialize();
            SaveDataLoad();

            saveDataInitialize = false;
            isStartup = true;
            FadeManager.Instance.LoadSceneIndex(0, 0f); 
        }
    }

    //データのロードを開始する　他のクラスからも呼び出せる
    public void DataLoad()
    {
        /*
        //ビルドした実行データの場合はファイルのパスを変更
#if !UNITY_EDITOR
        playerSaveDataPass = "PlayerSaveData.csv";
        charaSaveDataPass = "CharaSaveData.csv";
        SaveManager.Instance.FilePassChange();
#endif
        */

        //データをロード
        GameDataLoad();

        /*
        //セーブデータがない場合は新たに生成
        if (Directory.Exists(saveDataDirectoryPass) && !File.Exists(saveDataDirectoryPass + playerSaveDataPass) ||
            Directory.Exists(saveDataDirectoryPass) && !File.Exists(saveDataDirectoryPass + charaSaveDataPass))
        {
            File.Create(saveDataDirectoryPass + playerSaveDataPass);
            File.Create(saveDataDirectoryPass + charaSaveDataPass);

            MenuManager.Instance.saveDataInitializeWindow.SetActive(true);

            saveDataInitialize = true;
        }
        else
        {
            //セーブデータをロード
            SaveDataLoad();
        }
        */

        //セーブデータをロード
        SaveDataLoad();
    }

    //ゲームのデータをロード
    void GameDataLoad()
    {
        //キャラのデータをCSVから読み出し
        csvFile = Resources.Load(charaDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        charaNum = csvDatas.Count;
        charaData = new CharaData[charaNum];
        //キャラのデータを配列に格納
        for (int i = 0; true; i++)
        {
            if (i >= charaNum) break;
            charaData[i] = new CharaData();
            //ステータス
            charaData[i].id = i;
            charaData[i].charaName = csvDatas[i][0];
            charaData[i].spritePass = csvDatas[i][1];
            charaData[i].hp = int.Parse(csvDatas[i][2]);
            charaData[i].atk = int.Parse(csvDatas[i][3]);
            charaData[i].def = int.Parse(csvDatas[i][4]);
            charaData[i].spd = int.Parse(csvDatas[i][5]);
            charaData[i].skillId1 = int.Parse(csvDatas[i][6]);
            charaData[i].skillId2 = int.Parse(csvDatas[i][7]);
        }
        //リストのデータをクリア
        csvDatas.Clear();

        //敵のデータをCSVから読み出し
        csvFile = Resources.Load(enemyDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        int enemyNum = csvDatas.Count;
        enemyData = new EnemyData[enemyNum];
        //敵のデータを配列に格納
        for (int i = 0; true; i++)
        {
            if (i >= enemyNum) break;
            enemyData[i] = new EnemyData();
            //ステータス
            enemyData[i].id = i;
            enemyData[i].enemyName = csvDatas[i][0];
            enemyData[i].spritePass = csvDatas[i][1];
            enemyData[i].hp = int.Parse(csvDatas[i][2]);
            enemyData[i].atk = int.Parse(csvDatas[i][3]);
            enemyData[i].def = int.Parse(csvDatas[i][4]);
            enemyData[i].spd = int.Parse(csvDatas[i][5]);
            enemyData[i].skillId1 = int.Parse(csvDatas[i][6]);
            enemyData[i].skillId2 = int.Parse(csvDatas[i][7]);
            enemyData[i].normalAttackId = int.Parse(csvDatas[i][8]);
        }
        //リストのデータをクリア
        csvDatas.Clear();
        //キャラのデータをCSVから読み出し
        csvFile = Resources.Load(charaDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        charaNum = csvDatas.Count;
        charaData = new CharaData[charaNum];
        //キャラのデータを配列に格納
        for (int i = 0; true; i++)
        {
            if (i >= charaNum) break;
            charaData[i] = new CharaData();
            //ステータス
            charaData[i].id = i;
            charaData[i].charaName = csvDatas[i][0];
            charaData[i].spritePass = csvDatas[i][1];
            charaData[i].hp = int.Parse(csvDatas[i][2]);
            charaData[i].atk = int.Parse(csvDatas[i][3]);
            charaData[i].def = int.Parse(csvDatas[i][4]);
            charaData[i].spd = int.Parse(csvDatas[i][5]);
            charaData[i].skillId1 = int.Parse(csvDatas[i][6]);
            charaData[i].skillId2 = int.Parse(csvDatas[i][7]);
            charaData[i].normalAttackId = int.Parse(csvDatas[i][8]);
        }
        //リストのデータをクリア
        csvDatas.Clear();

        //スキルのデータをCSVから読み出し
        csvFile = Resources.Load(skillDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        skillNum = csvDatas.Count;
        skillData = new SkillData[skillNum];
        //スキルのデータを配列に格納
        for (int i = 0; true; i++)
        {
            if (i >= skillNum) break;
            skillData[i] = new SkillData();
            //ステータス
            skillData[i].id = i;
            skillData[i].skillName = csvDatas[i][0];
            skillData[i].effectIndex = int.Parse(csvDatas[i][1]);
            skillData[i].seIndex = int.Parse(csvDatas[i][2]);
            skillData[i].skillType = int.Parse(csvDatas[i][3]);
            skillData[i].value = float.Parse(csvDatas[i][4]);
            skillData[i].targetAll = ((int.Parse(csvDatas[i][5]) >= 1));
            skillData[i].effectiveTurn = int.Parse(csvDatas[i][6]);
        }
        //リストのデータをクリア
        csvDatas.Clear();

        //ステージ数をCSVの行数からカウント
        csvFile = Resources.Load(stageDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            reader.ReadLine();
            stageNum++;
        }
    }

    //セーブデータをロード
    void SaveDataLoad()
    {
        /*
        //プレイヤーのセーブデータをCSVから読み出し
#if UNITY_EDITOR
        csvFile = Resources.Load(playerSaveDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
#else
        reader = new StringReader(File.ReadAllText(saveDataDirectoryPass + playerSaveDataPass));
#endif
        */
        csvFile = Resources.Load(playerSaveDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        //データを配列に格納
        playerData.userId = int.Parse(csvDatas[0][0]);         //ユーザーID
        playerData.userName = csvDatas[1][0];                  //ユーザー名
        playerData.gachaTicketNum = int.Parse(csvDatas[2][0]); //ガチャチケット数
        for (int i = 0; i < partyNum; i++)
        {
            playerData.battleCharaId[i] = int.Parse(csvDatas[3][i]); //戦闘に出るキャラのID
        }
        playerData.menuBackCharaId = int.Parse(csvDatas[4][0]); //メニュー画面に表示するキャラのID
        playerData.bgmVolume = int.Parse(csvDatas[5][0]);       //BGMの音量
        playerData.seVolume = int.Parse(csvDatas[5][1]);        //SEの音量
        //リストのデータをクリア
        csvDatas.Clear();

        /*
        //キャラのセーブデータをCSVから読み出し
#if UNITY_EDITOR
        csvFile = Resources.Load(charaSaveDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
#else
        reader = new StringReader(File.ReadAllText(saveDataDirectoryPass + charaSaveDataPass));
#endif
        */
        csvFile = Resources.Load(charaSaveDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        //キャラのパラメータのセーブデータを配列に格納
        for (int i = 0; true; i++)
        {
            if (i >= charaNum) break;
            charaData[i].get = (int.Parse(csvDatas[i][0]) >= 1); //入手済みのキャラか
            charaData[i].lv = int.Parse(csvDatas[i][1]);         //レベル
            charaData[i].exp = int.Parse(csvDatas[i][2]);        //入手経験値
            charaData[i].totsu = int.Parse(csvDatas[i][0]);  //凸(同じキャラを何回引いているか)
        }
        //リストのデータをクリア
        csvDatas.Clear();
    }
}

//プレイヤーのデータ
[System.Serializable]
public class PlayerData
{
    public int userId;
    public string userName;
    public int gachaTicketNum;
    public int[] battleCharaId = new int[3];
    public int battleId;
    public int menuBackCharaId;
    public int bgmVolume;
    public int seVolume;
}

//キャラクターのデータ
[System.Serializable]
public class CharaData
{
    public int id;
    public string charaName;
    public string spritePass;

    public bool get;
    public int totsu;
    public int lv;
    public int exp;

    public int hp;
    public int atk;
    public int def;
    public int spd;

    public int skillId1;
    public int skillId2;

    public int normalAttackId;
}

//敵のデータ
[System.Serializable]
public class EnemyData
{
    public int id;
    public string enemyName;
    public string spritePass;

    public int hp;
    public int atk;
    public int def;
    public int spd;

    public int skillId1;
    public int skillId2;

    public int normalAttackId;
}

//戦闘中のスキルのデータ
//skillTypeは攻撃、回復などの効果の分類。０が攻撃、１が回復、２がステータス変動、３が状態異常
[System.Serializable]
public class SkillData
{
    public int id;
    public string skillName;
    public int effectIndex;
    public int seIndex;

    public int skillType;
    public float value;
    public bool targetAll;
    public int effectiveTurn;
}
