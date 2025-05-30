using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���j���[��ʂŎg���������܂Ƃ߂��N���X
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    //���j���[���̊eCavnas
    [SerializeField] GameObject[] canvas;
    //�Z�[�u�f�[�^�������̕\��
    public GameObject saveDataInitializeWindow;
    //�w�i�摜
    [SerializeField] Image[] backGroundImage;
    [SerializeField] Image backGroundCharaImage;

    [Space(10)]

    //�L�����̃X�e�[�^�X��ʗp�ϐ�
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

    //�X�e�[�W�I����ʂɊւ���ϐ�
    //�X�e�[�W�I���̃{�^���Ɛe�̃I�u�W�F�N�g
    [SerializeField] GameObject stageSelectWindow;
    GameObject stageSelectButton_Group;
    GameObject stageSelectButton;
    public int stageNum;              //���X�e�[�W�� EnemyOrderData.csv�̍s�����瓾��
    public int pageNum;               //�X�e�[�W�I����ʂ̃y�[�W��
    public int nowPage;               //���݂̃y�[�W�̔ԍ�
    int oneWindowStageNum = 6; //��̃E�B���h�E�ɂ����̃{�^�������邩

    [Space(10)]

    //�I�v�V������ʂɊւ���ϐ�
    [SerializeField] GameObject optionWindowButton;
    [SerializeField] GameObject saveDataDeleteWindow;
    [SerializeField] GameObject soundSettingWindow;

    [Space(10)]

    [SerializeField] float lvMagnification = 0.25f;
    [SerializeField] float totsuMagnification = 1.1f;

    bool stageSelect, backGroundCharaChoice;
    int[] choiceCharaId; //�퓬���̕Ґ��ɑI�΂�Ă���L������ID���ꎞ�I�ɕۑ�����ϐ��A�I�΂�Ă��Ȃ��ꍇ�� -1 �ɂȂ�
    int choiceCharaId_Index;
    int choiceCharaId_Tmp;
    int choiceCharaCount;
    bool charaChoiceComplete;

    void Start()
    {
        //Cavnas�̕\���؂�ւ�
        canvas[0].SetActive(false);
        canvas[1].SetActive(true);
        canvas[2].SetActive(DataManager.Instance.isBattle);
        canvas[3].SetActive(false);
        canvas[4].SetActive(false);
        canvas[5].SetActive(false);
        //�w�i�摜�ǂݍ���
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
        //BGM���Đ�
        SoundManager.Instance.PlayBGM(0);

        //�Q�[���N�����ɂ̂݃^�C�g����ʂ�\��
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

    //���j���[��ʂֈڍs����{�^��
    public void Menu()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        canvas[0].SetActive(false);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);
        canvas[4].SetActive(false);
        canvas[5].SetActive(false);

        canvas[1].SetActive(true);
    }
    //�퓬��ʂֈڍs����{�^��
    public void StageSelect(int battleId)
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        //�������}�C�i�X�Ȃ�X�e�[�W�Z���N�g��ʂ̕\���؂�ւ�
        if (battleId < 0)
        {
            canvas[2].SetActive(!canvas[2].activeSelf);
            stageSelect = !stageSelect;
        }
        //������0�ȏ�Ȃ�퓬��ʂֈڍs
        else
        {
            DataManager.Instance.isBattle = true;
            DataManager.Instance.playerData.battleId = battleId;
            SaveManager.Instance.PlayerDataSave();
            FadeManager.Instance.LoadSceneIndex(1, 0.5f);
        }
    }
    //�X�e�[�W�I����ʂ̃y�[�W�؂�ւ�
    public void StagePageChange(bool nextPage)
    {
        if (nextPage && nowPage >= pageNum - 1 || !nextPage && nowPage <= 0) return;

        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        //���݊J���Ă���y�[�W���\��
        GameObject nowPageObj = stageSelectWindow.transform.GetChild(nowPage).gameObject;
        nowPageObj.SetActive(false);
        int loopNum = (nowPage < (pageNum - 1)) ? oneWindowStageNum : nowPageObj.transform.childCount;
        //�{�^���̃T�C�Y�����Z�b�g
        for (int i = 0; i < loopNum; i++)
        {
            nowPageObj.transform.GetChild(i).localScale = new Vector3(1, 1, 1);
        }

        //���̃y�[�W��\��
        nowPage = (nextPage) ? nowPage + 1 : nowPage - 1;
        stageSelectWindow.transform.GetChild(nowPage).gameObject.SetActive(true);
    }
    //�K�`����ʂֈڍs����{�^��
    public void Gacha()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        canvas[1].SetActive(canvas[4].activeSelf);
        canvas[2].SetActive(false);
        canvas[3].SetActive(false);
        canvas[5].SetActive(false);

        canvas[4].SetActive(!canvas[4].activeSelf);
    }
    //�I�v�V������ʂֈڍs����{�^��
    public void Option()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        canvas[5].SetActive(!canvas[5].activeSelf);
    }
    //���j���[��ʂɕ\������L������I������{�^��
    public void BackGroundCharaChoice()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        backGroundCharaChoice = true;

        CharaStatus();
    }
    //���ʐݒ��ʂ�\������{�^��
    public void SoundSetting()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

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
    //�Z�[�u�f�[�^�����̉�ʂ�\������{�^��
    public void SaveDataDeleteWindow()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        saveDataDeleteWindow.SetActive(!saveDataDeleteWindow.activeSelf);
    }
    //�Z�[�u�f�[�^����������{�^��
    public void SaveDataDelete()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        SaveManager.Instance.PlayerDataInitialize();
        SaveManager.Instance.CharaDataInitialize();

        FadeManager.Instance.LoadSceneIndex(0, 0.5f);
        DataManager.Instance.DataLoad();
    }
    //�Q�[���I���{�^��
    public void Exit()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        //UnityEditor��Ńv���C���I������ꍇ
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //�r���h�������s�f�[�^�Ńv���C���I������ꍇ
        Application.Quit();
#endif
    }

    //�L�����N�^�[�̃X�e�[�^�X��ʂֈڍs����{�^��
    public void CharaStatus()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        if (stageSelect && canvas[3].activeSelf)
        {
            //�L�����Ґ���ʂ���߂�ꍇ�ɃE�B���h�E��\��
            if (!battleCharaChoiceApplyWindow.activeSelf)
            {  
                if (choiceCharaId_Tmp >= 0 && charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.activeSelf)
                {
                    charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
                    charaIcon[choiceCharaId_Tmp].interactable = true;
                }
                battleCharaChoiceApplyWindow.SetActive(true);
                //�Ґ��p�̃L������I��ł��Ȃ��ꍇ�͕ۑ�����{�^���������Ȃ�
                battleCharaChoice_CautionText.gameObject.SetActive(choiceCharaCount < DataManager.Instance.partyNum);
                battleCharaChoice_ApplyButton.interactable = choiceCharaCount >= DataManager.Instance.partyNum;
                return;
            }
            //�L�����Ґ���ʂ���߂�̂��L�����Z�������ꍇ�̏���
            else if (!charaChoiceComplete)
            {
                battleCharaChoiceApplyWindow.SetActive(false);
                return;
            }
            //�L�����Ґ������������ꍇ�̏���
            else
            {
                battleCharaChoiceApplyWindow.SetActive(false);
                battleCharaChoice_CautionText.gameObject.SetActive(false);
                battleCharaChoice_ApplyButton.interactable = true;

                //�X�e�[�W�I����ʂŃL�����Ґ���\������
                for (int i = 0; i < charaIconMini.Length; i++)
                {
                    string charaName;
                    Sprite charaSprite;
                    //�L�������Ƃ̊G�Ɩ��O��ǂݍ���
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
    //�X�e�[�^�X��ʂŃL�������N���b�N�������̏���
    public void CharaStatusIconPush(int charaId)
    {
        //�L�����̃X�e�[�^�X�m�F��ʂ̏ꍇ
        if (!stageSelect && charaId >= 0 && !backGroundCharaChoice)
        {
            SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

            choiceCharaId_Tmp = charaId;
            CharaStatusView();
        }
        //�X�e�[�W�I��O�̃L�����I���̏ꍇ
        else if (!backGroundCharaChoice)
        {
            if (choiceCharaId_Tmp >= 0 && charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.activeSelf)
            {
                SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

                charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
                charaIcon[choiceCharaId_Tmp].interactable = true;
            }
            if (charaId >= 0)
            {
                SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

                choiceCharaId_Tmp = charaId;
                charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(true);
                charaIcon[choiceCharaId_Tmp].interactable = false;
            }
        }
        //���j���[��ʂɕ\������L�����I���̏ꍇ
        else
        {
            if (charaId >= 0)
            {
                SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

                DataManager.Instance.playerData.menuBackCharaId = charaId; //�I�΂ꂽ�L������ID��ۑ�
                SaveManager.Instance.PlayerDataSave();

                FadeManager.Instance.LoadSceneIndex(0, 0.5f);
            }
        }
    }
    //�L�����̃X�e�[�^�X��\��
    public void CharaStatusView()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

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
            //�L�����̃X�e�[�^�X�̊�b�l�Ƀ��x���Ɠʂ̐��l���|���Đ퓬���̃X�e�[�^�X�����߂�
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
    //�L������퓬�̕Ґ��ɒǉ�
    public void BattleCharaChoice()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        charaIcon[choiceCharaId_Tmp].transform.GetChild(2).gameObject.SetActive(false);
        charaIcon[choiceCharaId_Tmp].interactable = true;

        //�Ґ��̍Ō�̃L�����܂őI�яI������ɑ��̃L������I�񂾏ꍇ�́A�Ґ��P�L�����ڂ̑I������������
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
    //�L�����̕Ґ���ۑ����邩�j�����邩
    public void BattleCharaChoice_ApplyOrDiscard(bool applyOrDiscard)
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

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

    //�L�����̃X�e�[�^�X��ʂ�������
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
            //�L�������Ƃ̊G�Ɩ��O��ǂݍ���
            charaSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[i].spritePass);
            charaIcon[i].transform.GetChild(0).GetComponent<Image>().sprite = charaSprite;
            charaName = DataManager.Instance.charaData[i].charaName;
            charaIcon[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = charaName;
            
            int _i = i;
            //�L������I�������Ƃ��̏������{�^���Ɋ��蓖��
            charaIcon[i].onClick.AddListener(() => CharaStatusIconPush(_i));
            GameObject window = charaIcon[i].transform.GetChild(2).gameObject;
            window.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(BattleCharaChoice);
            window.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(CharaStatusView);

            //������Ԃŕ\�����Ȃ��E�B���h�E
            charaIcon[i].transform.GetChild(2).gameObject.SetActive(false);
            charaIcon[i].transform.GetChild(4).gameObject.SetActive(false);
            charaStatusViewWindow.SetActive(false);
        }

        //�������Ă���L�����݂̂��N���b�N�ł���悤�ɂ���
        CharaStatusCanvasFlag();

        //�퓬���̃L�����Ґ��̔z���-1�ŏ�����
        choiceCharaId = new int[DataManager.Instance.partyNum];
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            choiceCharaId[i] = -1;
        }

        //�X�e�[�W�I����ʂŃL�����Ґ���\������
        charaIconMini = new GameObject[DataManager.Instance.partyNum];
        for (int i = 0; i < charaIconMini.Length; i++)
        {
            charaIconMini[i] = Instantiate(Resources.Load<GameObject>("Prefabs/CharaIconMini"));
            charaIconMini[i].transform.SetParent(charaStatusViewButton.transform);
            charaIconMini[i].transform.position = new Vector3(0, 0, 0);
            charaIconMini[i].transform.localScale = new Vector3(1, 1, 1);

            string charaName;
            Sprite charaSprite;
            //�L�������Ƃ̊G�Ɩ��O��ǂݍ���
            charaSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[DataManager.Instance.playerData.battleCharaId[i]].spritePass);
            charaIconMini[i].transform.GetChild(0).GetComponent<Image>().sprite = charaSprite;
            charaName = DataManager.Instance.charaData[DataManager.Instance.playerData.battleCharaId[i]].charaName;
            charaIconMini[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = charaName;
        }

        //�X�e�[�^�X��ʂŕ\������R���|�[�l���g���擾
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

        battleCharaChoice_CautionText.text = "���܂�" + DataManager.Instance.partyNum + "�̑I�΂�Ă��܂���";

        //������Ԃŕ\�����Ȃ��E�B���h�E
        battleCharaChoiceApplyWindow.SetActive(false);
    }

    //�������Ă���L�����݂̂��L�����X�e�[�^�X��ʂŃA�N�e�B�u��
    public void CharaStatusCanvasFlag()
    {
        for (int i = 0; i < DataManager.Instance.charaNum; i++)
        {
            bool flag = DataManager.Instance.charaData[i].get;
            charaIcon[i].interactable = flag;
            charaIcon[i].transform.GetChild(3).gameObject.SetActive(!flag);
        }
    }

    //�X�e�[�W�I����ʂ�������
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
                //Button�̃R���|�[�l���g��ݒ�
                int _i = i + j;
                Button button = stageSelectButton_Instance.GetComponent<Button>();
                button.onClick.AddListener(() => StageSelect(_i));
                //Text��ݒ�
                TextMeshProUGUI tmp = stageSelectButton_Instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                tmp.text = "�X�e�[�W" + (i + j + 1);
            }

            pageNum++;
            stageSelectButton_Group_Instance.SetActive(false);
        }

        nowPage = 0;
        stageSelectWindow.transform.GetChild(nowPage).gameObject.SetActive(true);
    }
}
