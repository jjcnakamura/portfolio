using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

/// <summary>
/// �퓬��ʂ̏���
/// </summary>
public class BattleManager : Singleton<BattleManager>
{
    //�eCavnas
    [SerializeField] GameObject[] canvas;
    [SerializeField] GameObject poseButton;

    [Space(10)]

    public TextMeshProUGUI battleText; //�퓬���̃e�L�X�g
    [SerializeField] GameObject targetCancelButton; //�U���A�X�L���̑Ώۂ�I�Ԃ̂��L�����Z������{�^��

    [SerializeField] Image backgroundImage; //�퓬���̔w�i
    int bgmIndex; //BGM�̔ԍ�

    [Space(10)]

    //�|�[�Y��ʂɊւ���ϐ�
    [SerializeField] GameObject pauseWindowButton;
    [SerializeField] GameObject soundSettingWindow;

    [Space(10)]

    [SerializeField] float battleStartTime = 2.5f; //�퓬�J�n�܂ł̎���
    [SerializeField] float actionWaitTime = 1.5f;  //�L�����̍s���̊Ԋu
    [SerializeField] float resultWaitTime = 2.5f;  //�퓬�I�����烊�U���g�\���܂ł̊Ԋu

    [Space(10)]

    //�X�e�[�W�N���A��ɓ����o���l�ƃ��x���A�K�`���`�P�b�g��\������e�L�X�g
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

    int getGachaTicket; //�X�e�[�W�N���A���ɃK�`���`�P�b�g���������炦�邩

    [SerializeField] int lvUpExp = 100; //�o���l�������ƂɃ��x���A�b�v���邩
    [SerializeField] int maxExp = 2000; //�ő�l���o���l
    [SerializeField] int maxLv = 20;    //�ő僌�x��

    [Space(10)]

    //�v���C���[�L�����̃I�u�W�F�N�g�ƃX�e�[�^�X
    [SerializeField] float lvMagnification = 0.25f; //���x���ɂ��X�e�[�^�X�̋����{��
    [SerializeField] float totsuMagnification = 1.05f; //�ʂɂ��X�e�[�^�X�̋����{��
    [Space(10)]
    public GameObject[] playerCharaObj;
    public Status[] playerCharaStatus;
    public TextMeshProUGUI[] playerCharaDamageText;
    Button[] playerCharaButton;
    GameObject[] playerCharaTargetImage;
    [System.NonSerialized] public GameObject[] commandWindow;
    [System.NonSerialized] public Slider[] playerCharaHPSlider;

    [Space(10)]

    //�G�̃I�u�W�F�N�g�ƃX�e�[�^�X
    public GameObject[] enemyObj;
    public Status[] enemyStatus;
    public TextMeshProUGUI[] enemyDamageText;
    Button[] enemyButton;
    [System.NonSerialized] public GameObject[] enemyTargetImage;
    [System.NonSerialized] public Slider[] enemyHPSlider;

    [Space(10)]

    public int commandNum = 3; //�v���C���[���G���I���ł���R�}���h�̐�
    int commandCharaIndex; //���݃R�}���h��I�����Ă���L����
    int[,] action; //�I�����ꂽ�U���܂��̓X�L���A�g�p����L�����A�ΏۂɂȂ�L������ێ����Ă����z��

    void Start()
    {
        //�퓬�ɏo�Ă���L�����̃f�[�^�����[�h
        BattleCharaDataLoad();
        BattleEnemyDataLoad();
        SkillManager.Instance.SkillDataRoad();

        SoundManager.Instance.PlayBGM(bgmIndex); //BGM���Đ�

        //�퓬�J�n���ɓG�̖��O���e�L�X�g�ŕ\��
        battleText.text = "";
        string[] enemyName = new string[enemyStatus.Length];
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            string enemyNameTmp = enemyStatus[i].name;

            //�������O�̓G��(�G�̖��O)A�A(�G�̖��O)B�Ƒ�����
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
            if (i <= enemyStatus.Length - 2) battleText.text += "�A";
        }
        battleText.text += " �����ꂽ�I";

        for (int i = 0; i < enemyStatus.Length; i++)
        {
            enemyStatus[i].name = enemyName[i];
        }

        action = new int[playerCharaStatus.Length + enemyStatus.Length, 2];
        poseButton.SetActive(false); //�|�[�Y�{�^�����A�N�e�B�u������
        TargetActive(false, true, true);
        Invoke("TurnStart", battleStartTime); //�^�[�����J�n���A�L�����̃R�}���h��\������
    }

    //�^�[�����J�n���A�P�Ԗڂ̃L�����̃R�}���h��\������
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
        playerCharaObj[commandCharaIndex].transform.SetAsLastSibling(); //���݃R�}���h��I�����Ă���L��������Ԏ�O�̃��C���[�ɕ\������
        battleText.text = playerCharaStatus[commandCharaIndex].name + "�͂ǂ�����H"; //���݃R�}���h��I�����Ă���L�����̖��O���e�L�X�g�ŕ\��
        poseButton.SetActive(true); //�|�[�Y�{�^�����A�N�e�B�u������

        TargetActive(false, true, true);
        targetCancelButton.SetActive(false);
    }

    //�ʏ�U���R�}���h
    public void AttackButton()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��
        commandWindow[commandCharaIndex].SetActive(false);

        //�I�񂾃R�}���h��z��ɒǉ�
        action[commandCharaIndex, 0] = 0;
        
        TargetActive(true, false, true);
    }
    //�X�L���R�}���h
    public void SkillButton(int skillIndex)
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��
        commandWindow[commandCharaIndex].SetActive(false);

        //�I�񂾃R�}���h��z��ɒǉ�
        action[commandCharaIndex, 0] = skillIndex;

        skillIndex--;

        //�S�̂�ΏۂƂ���X�L���̏ꍇ�̓X�L���̑Ώۂ�-�P�ɂ���
        if (SkillManager.Instance.playerCharaSkill[commandCharaIndex, skillIndex].targetAll)
        {
            action[commandCharaIndex, 1] = -1;

            NextChara();
            return;
        }
        //�G�������ǂ����Ώۂɂ��邩
        if (SkillManager.Instance.playerCharaSkill[commandCharaIndex,skillIndex].skillType != 1 && SkillManager.Instance.playerCharaSkill[commandCharaIndex, skillIndex].skillType != 3)
        {
            //�G�ɑ΂���X�L��
            TargetActive(true, false, true);
        }
        else
        {
            //�����ɑ΂���X�L��
            TargetActive(true, true, false);
        }
    }
    //�U���A�X�L���̑Ώۂ�I��
    public void TargetButton(int targetIndex)
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        action[commandCharaIndex, 1] = targetIndex;

        TargetActive(false, true, true);
        targetCancelButton.SetActive(false);
        NextChara();
    }
    //�U���A�X�L���̑Ώۂ�I�Ԃ̂��L�����Z��
    public void TargetCancelButton()
    {
        TargetActive(false, true, true);
        targetCancelButton.SetActive(false);
        commandWindow[commandCharaIndex].SetActive(true);
        battleText.text = playerCharaStatus[commandCharaIndex].name + "�͂ǂ�����H"; //���݃R�}���h��I�����Ă���L�����̖��O���e�L�X�g�ŕ\��

        TargetActive(false, true, true);
    }
    //�O�̃L�����̃R�}���h��I�����Ȃ���
    public void CharaBackButton()
    {
        if (playerCharaStatus[commandCharaIndex - 1].dead) return; //�O�̃L�������퓬�s�\�̏ꍇ�͖߂�Ȃ�

        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        commandWindow[commandCharaIndex].SetActive(false); 
        commandCharaIndex = Mathf.Max(0, commandCharaIndex - 1);
        commandWindow[commandCharaIndex].SetActive(true);
        playerCharaObj[commandCharaIndex].transform.SetAsLastSibling(); //���݃R�}���h��I�����Ă���L��������Ԏ�O�̃��C���[�ɕ\������
        battleText.text = playerCharaStatus[commandCharaIndex].name + "�͂ǂ�����H"; //���݃R�}���h��I�����Ă���L�����̖��O���e�L�X�g�ŕ\��

        TargetActive(false, true, true);
    }

    //���̃L�����̃R�}���h�I���ֈڂ�
    void NextChara()
    {
        TargetActive(false, true, true);

        commandCharaIndex++;

        //����̍Ō�̃L�����̏ꍇ�͓G�̍s�����I�Ɉڂ�
        if (commandCharaIndex == playerCharaStatus.Length)
        {
            poseButton.SetActive(false); //�|�[�Y�{�^�����A�N�e�B�u������
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
                poseButton.SetActive(false); //�|�[�Y�{�^�����A�N�e�B�u������
                EnemyActionSelect();
                return;
            }
            else
            {
                break;
            }
        }
        commandWindow[commandCharaIndex].SetActive(true);
        playerCharaObj[commandCharaIndex].transform.SetAsLastSibling(); //���݃R�}���h��I�����Ă���L��������Ԏ�O�̃��C���[�ɕ\������
        battleText.text = playerCharaStatus[commandCharaIndex].name + "�͂ǂ�����H"; //���݃R�}���h��I�����Ă���L�����̖��O���e�L�X�g�ŕ\��
    }
    //�Z�̑ΏۂɂȂ�L������؂�ւ�
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
    //�G�̍s���𒊑I
    void EnemyActionSelect()
    {
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            int randomCommand = UnityEngine.Random.Range(0, commandNum);
            action[i + playerCharaStatus.Length, 0] = randomCommand;

            //�S�̂�ΏۂƂ���X�L���̏ꍇ�̓X�L���̑Ώۂ�-�P�ɂ���
            if (randomCommand == 1 && SkillManager.Instance.enemySkill[i, randomCommand - 1].targetAll ||
                randomCommand == 2 && SkillManager.Instance.enemySkill[i, randomCommand - 1].targetAll)
            {
                action[i + playerCharaStatus.Length, 1] = -1;
            }
            //�����_���ɂP�̑I��
            else
            {
                //�G�ɑ΂���X�L��
                if (randomCommand == 0 ||
                    randomCommand > 0 && SkillManager.Instance.enemySkill[i, randomCommand - 1].skillType != 1 ||
                    randomCommand > 0 && SkillManager.Instance.enemySkill[i, randomCommand - 1].skillType != 3)
                {
                    while (true)
                    {
                        int randomTarget = UnityEngine.Random.Range(0, playerCharaStatus.Length);
                        //�����_���ɑI�΂ꂽ�L���������S���Ă����ꍇ�͑I�тȂ���
                        if (!playerCharaStatus[randomTarget].dead)
                        {
                            action[i + playerCharaStatus.Length, 1] = randomTarget;
                            break;
                        }
                    }
                }
                //����(�v���C���[��)�ɑ΂���X�L��
                else
                {
                    while (true)
                    {
                        int randomTarget = UnityEngine.Random.Range(0, enemyStatus.Length);
                        //�����_���ɑI�΂ꂽ�L���������S���Ă����ꍇ�͑I�тȂ���
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
    //�S�ăL�������s����I�яI������A�L������f�������Ƀ\�[�g����
    void ActionStart()
    {
        int[] speed = new int[playerCharaStatus.Length + enemyStatus.Length];
        int[] actionOrderIndex = new int[speed.Length];

        //�����L�����ƓG�L������f�������ɕ��בւ���
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

    //�f�������ɍU���A�X�L�������s����
    IEnumerator Action(int[] actionOrderIndex)
    {
        for (int i = 0; i < actionOrderIndex.Length; i++)
        {
            int userIndex = actionOrderIndex[i];
            int actionIndex = action[userIndex, 0];
            int targetIndex = action[userIndex, 1];

            //�s���\�ȃL������ێ�
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

            //�����L�����̍s���@�s������L�������퓬�s�\�̏ꍇ�͔�΂�
            if (userIndex < playerCharaStatus.Length && aliveChara[userIndex])
            {
                if (actionIndex == 0)
                {
                    //�U���̑Ώۂ��퓬�s�\�̏ꍇ�͔�΂�
                    if (!enemyStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.NormalAttack(true, userIndex, targetIndex);
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                }
                else
                {
                    //�X�L���̑Ώۂ��퓬�s�\�̏ꍇ�͔�΂�
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
            //�G�L�����̍s���@//�s������G�L�������퓬�s�\�̏ꍇ�͔�΂�
            else if (userIndex >= playerCharaStatus.Length && aliveEnemy[userIndex - playerCharaStatus.Length])
            {
                if (actionIndex == 0)
                {
                    //�U���̑Ώۂ��퓬�s�\�̏ꍇ�͔�΂�
                    if (!playerCharaStatus[targetIndex].dead)
                    {
                        SkillManager.Instance.NormalAttack(false, userIndex - playerCharaStatus.Length, targetIndex);
                        yield return new WaitForSeconds(actionWaitTime);
                    }
                }
                else
                {
                    //�X�L���̑Ώۂ��퓬�s�\�̏ꍇ�͔�΂�
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
            //�_���[�W�̕\��������  HP���O�ɂȂ����L�����͔�A�N�e�B�u��
            if (!turnSkip)
            {
                string deadText = "";
                for (int j = 0; j < playerCharaStatus.Length; j++)
                {
                    playerCharaDamageText[j].gameObject.SetActive(false);

                    //�퓬�s�\�ɂȂ����ꍇ
                    if (aliveChara[j] && playerCharaStatus[j].dead)
                    {
                        playerCharaObj[j].SetActive(false);
                        deadText += playerCharaStatus[j].name += "�A";
                    }
                }
                //�퓬�s�\�̃L�������e�L�X�g�ŕ\��
                if (deadText != "")
                {
                    battleText.text = deadText.Substring(0, deadText.Length - 1) + "�͂��ꂽ�I";
                    yield return new WaitForSeconds(actionWaitTime);
                }
                deadText = "";
                for (int j = 0; j < enemyStatus.Length; j++)
                {
                    enemyDamageText[j].gameObject.SetActive(false);

                    //�퓬�s�\�ɂȂ����ꍇ
                    if (aliveEnemy[j] && enemyStatus[j].dead)
                    {
                        enemyObj[j].SetActive(false);
                        deadText += enemyStatus[j].name += "�A";
                    }
                }
                //�퓬�s�\�̃L�������e�L�X�g�ŕ\��
                if (deadText != "")
                {
                    battleText.text = deadText.Substring(0, deadText.Length - 1) + "��|�����I";
                    yield return new WaitForSeconds(actionWaitTime);
                }
            }
        }

        TurnEnd();

        yield break;
    }
    //���ׂẴL�������s�����I������玟�̃^�[���ɉf��
    void TurnEnd()
    {
        int deadPlayerCharaCount = 0;
        int killEnemyCount = 0;

        for (int i = 0; i < playerCharaStatus.Length; i++)
        {
            //HP���O�ɂȂ����L�������J�E���g
            if (playerCharaStatus[i].dead)
            {
                deadPlayerCharaCount++; //�|���ꂽ�����̐����J�E���g
            }
        }
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            //HP���O�ɂȂ����G���J�E���g
            if (enemyStatus[i].dead)
            {
                killEnemyCount++; //�|�����G�̐����J�E���g
            }
        }

        //���������ׂē|���ꂽ��Q�[���I�[�o�[
        if (deadPlayerCharaCount >= playerCharaStatus.Length)
        {
            GameOver();
            return;
        }
        //�S�Ă̓G��|������X�e�[�W�N���A
        if (killEnemyCount >= enemyStatus.Length)
        {
            StageClear();
            return;
        }

        //���̃^�[��
        TurnStart();
    }
    //�Q�[���I�[�o�[
    void GameOver()
    {
        canvas[1].SetActive(true);
        SoundManager.Instance.PlayBGM(2); //�Q�[���I�[�o�[����BGM���Đ�
    }
    //�X�e�[�W�N���A
    void StageClear()
    {
        battleText.text = "�퓬�ɏ��������I";
        SoundManager.Instance.PlayBGM(1); //�X�e�[�W�N���A����BGM���Đ�

        Invoke("Result", resultWaitTime);
    }
    //���U���g���
    void Result()
    {
        canvas[2].SetActive(true);

        int id = 0;
        //�o���l���l���A���x���A�b�v
        for (int i = 0; i <playerCharaStatus.Length; i++)
        {
            id = DataManager.Instance.playerData.battleCharaId[i];

            DataManager.Instance.charaData[id].exp = Mathf.Min(maxExp, DataManager.Instance.charaData[id].exp + getExp);
            DataManager.Instance.charaData[id].lv = 1 + Mathf.Min(maxLv - 1, (int)(DataManager.Instance.charaData[id].exp / lvUpExp));

            resultExpText_Now[i].text = DataManager.Instance.charaData[id].exp.ToString();
            resultLvText_Now[i].text = DataManager.Instance.charaData[id].lv.ToString();
        }
        //�K�`���`�P�b�g���l��
        DataManager.Instance.playerData.gachaTicketNum += getGachaTicket;
        getGachaTicketText.text = "�~ " + getGachaTicket;

        //�f�[�^���Z�[�u
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

        //�퓬�ɏo��L�����̊G�ƃX�e�[�^�X��ǂݍ���
        playerCharaStatus = new Status[DataManager.Instance.partyNum];
        for (int i = 0; i < playerCharaStatus.Length; i++)
        {
            playerCharaStatus[i] = new Status();

            //�L������ID�Ɩ��O���擾
            int id = DataManager.Instance.playerData.battleCharaId[i];
            playerCharaStatus[i].id = id;
            playerCharaStatus[i].name = DataManager.Instance.charaData[id].charaName;

            //�L���������e�R���|�[�l���g���擾
            playerCharaButton[i] = playerCharaObj[i].GetComponent<Button>();
            playerCharaTargetImage[i] = playerCharaObj[i].transform.GetChild(0).gameObject;
            commandWindow[i] = playerCharaObj[i].transform.GetChild(1).gameObject;
            playerCharaHPSlider[i] = playerCharaObj[i].transform.GetChild(2).GetComponent<Slider>();
            playerCharaHPSlider[i].value = 1;
            //���U���g��ʂŕ\������e�L�X�g
            resultExpText_Pre[i] = resultExpParent_Pre.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultLvText_Pre[i] = resultLvParent_Pre.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultExpText_Now[i] = resultExpParent_Now.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultLvText_Now[i] = resultLvParent_Now.transform.GetChild(i).GetComponent<TextMeshProUGUI>();

            resultExpText_Pre[i].text = DataManager.Instance.charaData[id].exp.ToString();
            resultLvText_Pre[i].text = DataManager.Instance.charaData[id].lv.ToString();

            //�L�����̃X�e�[�^�X�̊�b�l�Ƀ��x���Ɠʂ̐��l���|���Đ퓬���̃X�e�[�^�X�����߂�
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

            //�R�}���h�E�B���h�E�̃{�^���ɃX�L���������蓖��
            commandWindow[i].transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = playerCharaStatus[i].skillName1;
            commandWindow[i].transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = playerCharaStatus[i].skillName2;
            //��ԍŏ��ɍs������L�����͍s������߂��{�^�����g���Ȃ��悤�ɂ���
            if (i == 0) commandWindow[i].transform.GetChild(1).gameObject.SetActive(false);

            //�L�����̊G���I�u�W�F�N�g���蓖�Ă�
            Image charaImage = playerCharaObj[i].GetComponent<Image>();
            Sprite roadSprite = Resources.Load<Sprite>(DataManager.Instance.charaData[id].spritePass);
            charaImage.sprite = roadSprite;

            resultCharaImage[i] = resultCharaImageParent.transform.GetChild(i).GetComponent<Image>();
            resultCharaImage[i].sprite = roadSprite;
        }
    }

    void BattleEnemyDataLoad()
    {
        //CSV����G�̕Ґ��f�[�^��ǂݍ���
        TextAsset csvFile = Resources.Load("EnemyOrderData") as TextAsset;
        StringReader reader = new StringReader(csvFile.text);
        List<string[]> enemyOrderDatas = new List<string[]>();
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            enemyOrderDatas.Add(line.Split(','));
        }

        int battleId = DataManager.Instance.playerData.battleId;

        //�w�i��ǂݍ���
        backgroundImage.sprite = Resources.Load<Sprite>(enemyOrderDatas[battleId][0]);
        backgroundImage.color = new Color(float.Parse(enemyOrderDatas[battleId][1]), float.Parse(enemyOrderDatas[battleId][2]), float.Parse(enemyOrderDatas[battleId][3]));
        //BGM�̔ԍ���ǂݍ���
        bgmIndex = int.Parse(enemyOrderDatas[battleId][4]);

        //�N���A���̕�V��ǂݍ���
        getExp = int.Parse(enemyOrderDatas[battleId][5]); //�o���l
        getGachaTicket = int.Parse(enemyOrderDatas[battleId][6]); //�K�`���`�P�b�g

        enemyButton = new Button[DataManager.Instance.partyNum];
        enemyTargetImage = new GameObject[DataManager.Instance.partyNum];
        enemyHPSlider = new Slider[DataManager.Instance.partyNum];

        //�퓬�ɏo��G�̊G�ƃX�e�[�^�X��ǂݍ���
        enemyStatus = new Status[DataManager.Instance.partyNum];
        for (int i = 0; i < enemyStatus.Length; i++)
        {
            enemyStatus[i] = new Status();

            //�G��ID���擾�i�V�s�ڂ܂ł͔w�i�摜�̃p�X��N���A���̕�V�̃f�[�^�������Ă��邽�߁A�W�s�ڂ���ǂݍ��ށj
            int id = int.Parse(enemyOrderDatas[battleId][i + 7]);
            enemyStatus[i].id = id;
            enemyStatus[i].name = DataManager.Instance.enemyData[id].enemyName;

            //�G�����e�R���|�[�l���g���擾
            enemyButton[i] = enemyObj[i].GetComponent<Button>();
            enemyTargetImage[i] = enemyObj[i].transform.GetChild(0).gameObject;
            enemyHPSlider[i] = enemyObj[i].transform.GetChild(1).GetComponent<Slider>();
            enemyHPSlider[i].value = 1;

            //�G�L�����̃X�e�[�^�X�����蓖�Ă�
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

            //�G�L�����̊G���I�u�W�F�N�g���蓖�Ă�
            Image image = enemyObj[i].GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(DataManager.Instance.enemyData[id].spritePass);
        }
    }

    //�퓬���ꎞ�I����{�^��
    public void Pause()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        canvas[3].SetActive(!canvas[3].activeSelf);
    }
    //�퓬����蒼���{�^��
    public void Retry()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        FadeManager.Instance.LoadSceneIndex(1, 0.5f);
    }
    //���j���[��ʂɖ߂�{�^��
    public void BackMenu()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

        FadeManager.Instance.LoadSceneIndex(0, 0.5f);
    }
    //���ʐݒ��ʂ�\������{�^��
    public void SoundSetting()
    {
        SoundManager.Instance.PlaySE_Sys(0); //�N���b�N��

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

//�����A�G�̐퓬���̃X�e�[�^�X�p�N���X
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