using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// �f�[�^�����[�h���鏈���ƁA�V�[�����܂����ŗ��p����Q�[�����̕ϐ����܂Ƃ߂��N���X
/// </summary>
public class DataManager : Singleton<DataManager>
{
    [System.NonSerialized] public bool isStartup = true; //�Q�[���N������
    [System.NonSerialized] public bool isBattle;         //�퓬��ʂ�

    //CSV����f�[�^��ǂݍ��ޕϐ�
    TextAsset csvFile;
    List<string[]> csvDatas = new List<string[]>();

    //�v���C���[�̃f�[�^���i�[����ϐ�
    public PlayerData playerData;
    [System.NonSerialized] public int playerDataNum = 6; //�v���C���[�̐ݒ荀�ڂ̐�
    [System.NonSerialized] public int partyNum = 3; //�o�g�����ɕҐ��o����L�����̐�
    [System.NonSerialized] public int stageNum;     //���X�e�[�W��
    public string stageDataPass = "EnemyOrderData";
    public string playerDataPass = "PlayerData";
    public string playerSaveDataPass = "PlayerSaveData";

    //�L�����N�^�[�̃f�[�^���i�[����ϐ�
    public CharaData[] charaData;
    [System.NonSerialized] public int charaNum;
    public string charaDataPass = "CharaData";
    public string charaSaveDataPass = "CharaSaveData";
    //�G�̃f�[�^���i�[����ϐ�
    public EnemyData[] enemyData;
    public string enemyDataPass = "EnemyData";

    //�X�L���̃f�[�^���i�[����ϐ�
    public SkillData[] skillData;
    public int skillNum;
    public string skillDataPass = "SkillData";

    public string saveDataDirectoryPass = "ProductionByCourse_Data/";

    StringReader reader;

    bool saveDataInitialize; //�Z�[�u�f�[�^�̏��������������s����ꍇ�ɗ��Ă�

    void Awake()
    {
        //�V�[����J�ڂ��Ă�DataManager�͎c��
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

    //�f�[�^�̃��[�h���J�n����@���̃N���X������Ăяo����
    public void DataLoad()
    {
        /*
        //�r���h�������s�f�[�^�̏ꍇ�̓t�@�C���̃p�X��ύX
#if !UNITY_EDITOR
        playerSaveDataPass = "PlayerSaveData.csv";
        charaSaveDataPass = "CharaSaveData.csv";
        SaveManager.Instance.FilePassChange();
#endif
        */

        //�f�[�^�����[�h
        GameDataLoad();

        /*
        //�Z�[�u�f�[�^���Ȃ��ꍇ�͐V���ɐ���
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
            //�Z�[�u�f�[�^�����[�h
            SaveDataLoad();
        }
        */

        //�Z�[�u�f�[�^�����[�h
        SaveDataLoad();
    }

    //�Q�[���̃f�[�^�����[�h
    void GameDataLoad()
    {
        //�L�����̃f�[�^��CSV����ǂݏo��
        csvFile = Resources.Load(charaDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        charaNum = csvDatas.Count;
        charaData = new CharaData[charaNum];
        //�L�����̃f�[�^��z��Ɋi�[
        for (int i = 0; true; i++)
        {
            if (i >= charaNum) break;
            charaData[i] = new CharaData();
            //�X�e�[�^�X
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
        //���X�g�̃f�[�^���N���A
        csvDatas.Clear();

        //�G�̃f�[�^��CSV����ǂݏo��
        csvFile = Resources.Load(enemyDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        int enemyNum = csvDatas.Count;
        enemyData = new EnemyData[enemyNum];
        //�G�̃f�[�^��z��Ɋi�[
        for (int i = 0; true; i++)
        {
            if (i >= enemyNum) break;
            enemyData[i] = new EnemyData();
            //�X�e�[�^�X
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
        //���X�g�̃f�[�^���N���A
        csvDatas.Clear();
        //�L�����̃f�[�^��CSV����ǂݏo��
        csvFile = Resources.Load(charaDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        charaNum = csvDatas.Count;
        charaData = new CharaData[charaNum];
        //�L�����̃f�[�^��z��Ɋi�[
        for (int i = 0; true; i++)
        {
            if (i >= charaNum) break;
            charaData[i] = new CharaData();
            //�X�e�[�^�X
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
        //���X�g�̃f�[�^���N���A
        csvDatas.Clear();

        //�X�L���̃f�[�^��CSV����ǂݏo��
        csvFile = Resources.Load(skillDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        skillNum = csvDatas.Count;
        skillData = new SkillData[skillNum];
        //�X�L���̃f�[�^��z��Ɋi�[
        for (int i = 0; true; i++)
        {
            if (i >= skillNum) break;
            skillData[i] = new SkillData();
            //�X�e�[�^�X
            skillData[i].id = i;
            skillData[i].skillName = csvDatas[i][0];
            skillData[i].effectIndex = int.Parse(csvDatas[i][1]);
            skillData[i].seIndex = int.Parse(csvDatas[i][2]);
            skillData[i].skillType = int.Parse(csvDatas[i][3]);
            skillData[i].value = float.Parse(csvDatas[i][4]);
            skillData[i].targetAll = ((int.Parse(csvDatas[i][5]) >= 1));
            skillData[i].effectiveTurn = int.Parse(csvDatas[i][6]);
        }
        //���X�g�̃f�[�^���N���A
        csvDatas.Clear();

        //�X�e�[�W����CSV�̍s������J�E���g
        csvFile = Resources.Load(stageDataPass) as TextAsset;
        reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            reader.ReadLine();
            stageNum++;
        }
    }

    //�Z�[�u�f�[�^�����[�h
    void SaveDataLoad()
    {
        /*
        //�v���C���[�̃Z�[�u�f�[�^��CSV����ǂݏo��
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
        //�f�[�^��z��Ɋi�[
        playerData.userId = int.Parse(csvDatas[0][0]);         //���[�U�[ID
        playerData.userName = csvDatas[1][0];                  //���[�U�[��
        playerData.gachaTicketNum = int.Parse(csvDatas[2][0]); //�K�`���`�P�b�g��
        for (int i = 0; i < partyNum; i++)
        {
            playerData.battleCharaId[i] = int.Parse(csvDatas[3][i]); //�퓬�ɏo��L������ID
        }
        playerData.menuBackCharaId = int.Parse(csvDatas[4][0]); //���j���[��ʂɕ\������L������ID
        playerData.bgmVolume = int.Parse(csvDatas[5][0]);       //BGM�̉���
        playerData.seVolume = int.Parse(csvDatas[5][1]);        //SE�̉���
        //���X�g�̃f�[�^���N���A
        csvDatas.Clear();

        /*
        //�L�����̃Z�[�u�f�[�^��CSV����ǂݏo��
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
        //�L�����̃p�����[�^�̃Z�[�u�f�[�^��z��Ɋi�[
        for (int i = 0; true; i++)
        {
            if (i >= charaNum) break;
            charaData[i].get = (int.Parse(csvDatas[i][0]) >= 1); //����ς݂̃L������
            charaData[i].lv = int.Parse(csvDatas[i][1]);         //���x��
            charaData[i].exp = int.Parse(csvDatas[i][2]);        //����o���l
            charaData[i].totsu = int.Parse(csvDatas[i][0]);  //��(�����L��������������Ă��邩)
        }
        //���X�g�̃f�[�^���N���A
        csvDatas.Clear();
    }
}

//�v���C���[�̃f�[�^
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

//�L�����N�^�[�̃f�[�^
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

//�G�̃f�[�^
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

//�퓬���̃X�L���̃f�[�^
//skillType�͍U���A�񕜂Ȃǂ̌��ʂ̕��ށB�O���U���A�P���񕜁A�Q���X�e�[�^�X�ϓ��A�R����Ԉُ�
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
