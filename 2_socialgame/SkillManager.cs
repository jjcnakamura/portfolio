using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�ƓG�L�������X�L�����g�p����ۂɌĂяo���֐����܂Ƃ߂��N���X
/// </summary>
public class SkillManager : Singleton<SkillManager>
{
    //�L�����̃X�L��
    public Skill[,] playerCharaSkill;
    public Skill[,] enemySkill;

    [Space(10)]

    public int skillNum;

    [Space(10)]

    [SerializeField] Color damageTextColor;
    [SerializeField] Color healTextColor;
    [SerializeField] Color buffTextColor;

    //�ʏ�U��
    public void NormalAttack(bool playerOrEnemy, int userIndex, int targetIndex)
    {
        //�����L�����̍U��
        if (playerOrEnemy)
        {
            //�e�L�X�g�E�B���h�E�Ƀe�L�X�g��\��
            BattleManager.Instance.battleText.text = BattleManager.Instance.playerCharaStatus[userIndex].name + "�̍U���I";
            //SE���Đ�
            SoundManager.Instance.PlaySE_Game(BattleManager.Instance.playerCharaStatus[userIndex].normalAttackId);

            //�U�����̍U���͂���󂯂鑤�̖h��͂������ă_���[�W�����߂�
            int damage = Mathf.Max(1, BattleManager.Instance.playerCharaStatus[userIndex].atk - BattleManager.Instance.enemyStatus[targetIndex].def);
            BattleManager.Instance.enemyStatus[targetIndex].hp = Mathf.Max(0, BattleManager.Instance.enemyStatus[targetIndex].hp - damage);
            //�G�t�F�N�g�ƃ_���[�W��\��
            GameObject effect = Instantiate(EffectManager.Instance.skill[BattleManager.Instance.playerCharaStatus[userIndex].normalAttackId]);
            effect.transform.position = new Vector3(BattleManager.Instance.enemyObj[targetIndex].transform.position.x, BattleManager.Instance.enemyObj[targetIndex].transform.position.y, 0);
            BattleManager.Instance.enemyDamageText[targetIndex].gameObject.SetActive(true);
            BattleManager.Instance.enemyDamageText[targetIndex].color = damageTextColor;
            BattleManager.Instance.enemyDamageText[targetIndex].text = damage.ToString();
            BattleManager.Instance.enemyHPSlider[targetIndex].value = (float)BattleManager.Instance.enemyStatus[targetIndex].hp / (float)BattleManager.Instance.enemyStatus[targetIndex].hp_default;
            //HP���O�ɂȂ����玀�S
            if (BattleManager.Instance.enemyStatus[targetIndex].hp <= 0)
            {
                BattleManager.Instance.enemyStatus[targetIndex].dead = true;
            }
        }
        //�G�̍U��
        else
        {
            //�e�L�X�g�E�B���h�E�Ƀe�L�X�g��\��
            BattleManager.Instance.battleText.text = BattleManager.Instance.enemyStatus[userIndex].name + "�̍U���I";
            //SE���Đ�
            SoundManager.Instance.PlaySE_Game(BattleManager.Instance.enemyStatus[userIndex].normalAttackId);

            //�U�����̍U���͂���󂯂鑤�̖h��͂������ă_���[�W�����߂�
            int damage = Mathf.Max(1, (BattleManager.Instance.enemyStatus[userIndex].atk - BattleManager.Instance.playerCharaStatus[targetIndex].def));
            BattleManager.Instance.playerCharaStatus[targetIndex].hp = Mathf.Max(0, BattleManager.Instance.playerCharaStatus[targetIndex].hp - damage);
            //�G�t�F�N�g�ƃ_���[�W��\��
            GameObject effect = Instantiate(EffectManager.Instance.skill[BattleManager.Instance.enemyStatus[userIndex].normalAttackId]);
            effect.transform.position = new Vector3(BattleManager.Instance.playerCharaObj[targetIndex].transform.position.x, BattleManager.Instance.playerCharaObj[targetIndex].transform.position.y, 0);
            BattleManager.Instance.playerCharaDamageText[targetIndex].gameObject.SetActive(true);
            BattleManager.Instance.playerCharaDamageText[targetIndex].color = damageTextColor;
            BattleManager.Instance.playerCharaDamageText[targetIndex].text = damage.ToString();
            BattleManager.Instance.playerCharaHPSlider[targetIndex].value = (float)BattleManager.Instance.playerCharaStatus[targetIndex].hp / (float)BattleManager.Instance.playerCharaStatus[targetIndex].hp_default;
            //HP���O�ɂȂ����玀�S
            if (BattleManager.Instance.playerCharaStatus[targetIndex].hp <= 0)
            {
                BattleManager.Instance.playerCharaStatus[targetIndex].dead = true;
            }
        }
    }

    //�����ɉ����Ċe��X�L���̊֐����Ăяo��
    public void SkillActivation(int skillIndex, bool playerOrEnemy, int userIndex, int targetIndex, bool targetAll = false)
    {
        //�X�L���̕��ޔԍ��ɂ���ČĂяo���X�L���̊֐���ς���
        int skillType = (playerOrEnemy) ? playerCharaSkill[userIndex, skillIndex].skillType : enemySkill[userIndex, skillIndex].skillType;

        if (skillType == 0 || skillType == 2) AttackSkill(skillIndex, playerOrEnemy, userIndex, targetIndex, targetAll, (skillType == 2));
        if (skillType == 1) HealSkill(skillIndex, playerOrEnemy, userIndex, targetIndex, targetAll);
        if (skillType == 3) Buff_DebuffSkill(skillIndex, playerOrEnemy, userIndex, targetIndex, targetAll);
        if (skillType == 4) Buff_DebuffSkill(skillIndex, playerOrEnemy, userIndex, targetIndex, targetAll);
        if (skillType == 5) AbnormalitySkill(skillIndex, playerOrEnemy, userIndex, targetIndex, targetAll);
    }

    //�U���X�L��
    void AttackSkill(int skillIndex, bool playerOrEnemy, int userIndex, int targetIndex, bool targetAll, bool absorb)
    {
        //�����L�����̍U��
        if (playerOrEnemy)
        {
            //�e�L�X�g�E�B���h�E�Ƀe�L�X�g��\��
            BattleManager.Instance.battleText.text = BattleManager.Instance.playerCharaStatus[userIndex].name + "��" + playerCharaSkill[userIndex, skillIndex].name + "�I";
            //SE���Đ�
            SoundManager.Instance.PlaySE_Game(playerCharaSkill[userIndex, skillIndex].seIndex);

            //�S�̍U���̏ꍇ�̂݃��[�v����
            for (int i = (!targetAll) ? targetIndex : 0; i < BattleManager.Instance.enemyStatus.Length; i++)
            {
                //�퓬�s�\�̏ꍇ�͑Ώۂɂ��Ȃ�
                if (!BattleManager.Instance.enemyStatus[i].dead)
                {
                    //�X�L���̍U���͔{����������
                    int attackPower = (int)(BattleManager.Instance.playerCharaStatus[userIndex].atk * playerCharaSkill[userIndex, skillIndex].value);
                    //�U�����̍U���͂���󂯂鑤�̖h��͂������ă_���[�W�����߂�
                    int damage = Mathf.Max(1, attackPower - BattleManager.Instance.enemyStatus[i].def);
                    BattleManager.Instance.enemyStatus[i].hp = Mathf.Max(0, BattleManager.Instance.enemyStatus[i].hp - damage);
                    //�G�t�F�N�g�ƃ_���[�W��\��  
                    GameObject effect = Instantiate(EffectManager.Instance.skill[playerCharaSkill[userIndex, skillIndex].effectIndex]);
                    effect.transform.position = new Vector3(BattleManager.Instance.enemyObj[i].transform.position.x, BattleManager.Instance.enemyObj[i].transform.position.y, 0);
                    BattleManager.Instance.enemyDamageText[i].gameObject.SetActive(true);
                    BattleManager.Instance.enemyDamageText[i].color = damageTextColor;
                    BattleManager.Instance.enemyDamageText[i].text = damage.ToString();
                    BattleManager.Instance.enemyHPSlider[i].value = (float)BattleManager.Instance.enemyStatus[i].hp / (float)BattleManager.Instance.enemyStatus[i].hp_default;
                    //HP���O�ɂȂ����玀�S
                    if (BattleManager.Instance.enemyStatus[i].hp <= 0)
                    {
                        BattleManager.Instance.enemyStatus[i].dead = true;
                    }
                    //HP�̋z������
                    if (absorb)
                    {
                        //�ő�HP�𒴂��Ȃ��悤��HP����
                        BattleManager.Instance.playerCharaStatus[userIndex].hp = Mathf.Min(BattleManager.Instance.playerCharaStatus[userIndex].hp_default, (BattleManager.Instance.playerCharaStatus[userIndex].hp + damage));
                        //�񕜗ʂ�\��
                        BattleManager.Instance.playerCharaDamageText[userIndex].gameObject.SetActive(true);
                        BattleManager.Instance.playerCharaDamageText[userIndex].color = healTextColor;
                        BattleManager.Instance.playerCharaDamageText[userIndex].text = damage.ToString();
                        BattleManager.Instance.playerCharaHPSlider[userIndex].value = (float)BattleManager.Instance.playerCharaStatus[userIndex].hp / (float)BattleManager.Instance.playerCharaStatus[userIndex].hp_default;
                    }
                }

                if (!targetAll) break;
            }
        }
        //�G�̍U��
        else
        {
            //�S�̍U���̏ꍇ�̂݃��[�v����
            for (int i = (!targetAll) ? targetIndex : 0; i < BattleManager.Instance.playerCharaStatus.Length; i++)
            {
                //�e�L�X�g�E�B���h�E�Ƀe�L�X�g��\��
                BattleManager.Instance.battleText.text = BattleManager.Instance.enemyStatus[userIndex].name + "��" + enemySkill[userIndex, skillIndex].name + "�I";
                //SE���Đ�
                SoundManager.Instance.PlaySE_Game(enemySkill[userIndex, skillIndex].seIndex);

                //�퓬�s�\�̏ꍇ�͑Ώۂɂ��Ȃ�
                if (!BattleManager.Instance.playerCharaStatus[i].dead)
                {
                    //�X�L���̍U���͔{����������
                    int attackPower = (int)(BattleManager.Instance.enemyStatus[userIndex].atk * enemySkill[userIndex, skillIndex].value);
                    //�U�����̍U���͂���󂯂鑤�̖h��͂������ă_���[�W�����߂�
                    int damage = Mathf.Max(1, attackPower - BattleManager.Instance.playerCharaStatus[i].def);
                    BattleManager.Instance.playerCharaStatus[i].hp = Mathf.Max(0, BattleManager.Instance.playerCharaStatus[i].hp - damage);
                    //�G�t�F�N�g�ƃ_���[�W��\��
                    GameObject effect = Instantiate(EffectManager.Instance.skill[enemySkill[userIndex, skillIndex].effectIndex]);
                    effect.transform.position = new Vector3(BattleManager.Instance.playerCharaObj[i].transform.position.x, BattleManager.Instance.playerCharaObj[i].transform.position.y, 0);
                    BattleManager.Instance.playerCharaDamageText[i].gameObject.SetActive(true);
                    BattleManager.Instance.playerCharaDamageText[i].color = damageTextColor;
                    BattleManager.Instance.playerCharaDamageText[i].text = damage.ToString();
                    BattleManager.Instance.playerCharaHPSlider[i].value = (float)BattleManager.Instance.playerCharaStatus[i].hp / (float)BattleManager.Instance.playerCharaStatus[i].hp_default;
                    //HP���O�ɂȂ����玀�S
                    if (BattleManager.Instance.playerCharaStatus[i].hp <= 0)
                    {
                        BattleManager.Instance.playerCharaStatus[i].dead = true;
                    }
                    //HP�̋z������
                    if (absorb)
                    {
                        //�ő�HP�𒴂��Ȃ��悤��HP����
                        BattleManager.Instance.enemyStatus[userIndex].hp = Mathf.Min(BattleManager.Instance.enemyStatus[userIndex].hp_default, (BattleManager.Instance.enemyStatus[userIndex].hp + damage));
                        //�񕜗ʂ�\��
                        BattleManager.Instance.enemyDamageText[userIndex].gameObject.SetActive(true);
                        BattleManager.Instance.enemyDamageText[userIndex].color = healTextColor;
                        BattleManager.Instance.enemyDamageText[userIndex].text = damage.ToString();
                        BattleManager.Instance.enemyHPSlider[userIndex].value = (float)BattleManager.Instance.enemyStatus[userIndex].hp / (float)BattleManager.Instance.enemyStatus[userIndex].hp_default;
                    }
                }

                if (!targetAll) break;
            }
        }
    }
    //�񕜃X�L��
    void HealSkill(int skillIndex, bool playerOrEnemy, int userIndex, int targetIndex, bool targetAll)
    {
        //�����L�����̉�
        if (playerOrEnemy)
        {
            //�e�L�X�g�E�B���h�E�Ƀe�L�X�g��\��
            BattleManager.Instance.battleText.text = BattleManager.Instance.playerCharaStatus[userIndex].name + "��" + playerCharaSkill[userIndex, skillIndex].name + "�I";
            //SE���Đ�
            SoundManager.Instance.PlaySE_Game(playerCharaSkill[userIndex, skillIndex].seIndex);

            //�S�̉񕜂̏ꍇ�̂݃��[�v����
            for (int i = (!targetAll) ? targetIndex : 0; i < BattleManager.Instance.playerCharaStatus.Length; i++)
            {
                //�퓬�s�\�̏ꍇ�͑Ώۂɂ��Ȃ�
                if (!BattleManager.Instance.playerCharaStatus[i].dead)
                {
                    //�ő�HP�𒴂��Ȃ��悤��HP����
                    int heal = (int)playerCharaSkill[userIndex, skillIndex].value;
                    BattleManager.Instance.playerCharaStatus[i].hp = Mathf.Min(BattleManager.Instance.playerCharaStatus[i].hp_default, (BattleManager.Instance.playerCharaStatus[i].hp + heal));
                    //�G�t�F�N�g�Ɖ񕜗ʂ�\��
                    GameObject effect = Instantiate(EffectManager.Instance.skill[playerCharaSkill[userIndex, skillIndex].effectIndex]);
                    effect.transform.position = new Vector3(BattleManager.Instance.playerCharaObj[i].transform.position.x, BattleManager.Instance.playerCharaObj[i].transform.position.y, 0);
                    BattleManager.Instance.playerCharaDamageText[i].gameObject.SetActive(true);
                    BattleManager.Instance.playerCharaDamageText[i].color = healTextColor;
                    BattleManager.Instance.playerCharaDamageText[i].text = heal.ToString();
                    BattleManager.Instance.playerCharaHPSlider[i].value = (float)BattleManager.Instance.playerCharaStatus[i].hp / (float)BattleManager.Instance.playerCharaStatus[i].hp_default;
                }

                if (!targetAll) break;
            }
        }
        //�G�̉�
        else
        {
            //�e�L�X�g�E�B���h�E�Ƀe�L�X�g��\��
            BattleManager.Instance.battleText.text = BattleManager.Instance.enemyStatus[userIndex].name + "��" + enemySkill[userIndex, skillIndex].name + "�I";
            //SE���Đ�
            SoundManager.Instance.PlaySE_Game(enemySkill[userIndex, skillIndex].seIndex);

            //�S�̉񕜂̏ꍇ�̂݃��[�v����
            for (int i = (!targetAll) ? targetIndex : 0; i < BattleManager.Instance.enemyStatus.Length; i++)
            {
                //�퓬�s�\�̏ꍇ�͑Ώۂɂ��Ȃ�
                if (!BattleManager.Instance.enemyStatus[i].dead)
                {
                    //�ő�HP�𒴂��Ȃ��悤��HP����
                    int heal = (int)enemySkill[userIndex, skillIndex].value;
                    BattleManager.Instance.enemyStatus[i].hp = Mathf.Min(BattleManager.Instance.enemyStatus[i].hp_default, (BattleManager.Instance.enemyStatus[i].hp + heal));
                    //�G�t�F�N�g�Ɖ񕜗ʂ�\��
                    GameObject effect = Instantiate(EffectManager.Instance.skill[enemySkill[userIndex, skillIndex].effectIndex]);
                    effect.transform.position = new Vector3(BattleManager.Instance.enemyObj[i].transform.position.x, BattleManager.Instance.enemyObj[i].transform.position.y, 0);
                    BattleManager.Instance.enemyDamageText[i].gameObject.SetActive(true);
                    BattleManager.Instance.enemyDamageText[i].color = healTextColor;
                    BattleManager.Instance.enemyDamageText[i].text = heal.ToString();
                    BattleManager.Instance.enemyHPSlider[i].value = (float)BattleManager.Instance.enemyStatus[i].hp / (float)BattleManager.Instance.enemyStatus[i].hp_default;
                }

                if (!targetAll) break;
            }
        }
    }
    //�o�t�A�f�o�t�X�L��
    void Buff_DebuffSkill(int skillIndex, bool playerOrEnemy, int userIndex, int targetIndex, bool targetAll)
    {

    }
    //��Ԉُ�X�L��
    void AbnormalitySkill(int skillIndex, bool playerOrEnemy, int userIndex, int targetIndex, bool targetAll)
    {

    }

    //�퓬�ɏo�Ă���L�����̃X�L����ǂݍ���
    public void SkillDataRoad()
    {
        //�v���C���[�L�����̃X�L����ǂݍ���
        playerCharaSkill = new Skill[BattleManager.Instance.playerCharaStatus.Length, 2];
        for (int i = 0; i < BattleManager.Instance.playerCharaStatus.Length; i++)
        {
            playerCharaSkill[i, 0] = new Skill();
            playerCharaSkill[i, 1] = new Skill();

            int skillId1 = BattleManager.Instance.playerCharaStatus[i].skillId1;
            int skillId2 = BattleManager.Instance.playerCharaStatus[i].skillId2;

            playerCharaSkill[i, 0].id = skillId1;
            playerCharaSkill[i, 0].name = DataManager.Instance.skillData[skillId1].skillName;
            playerCharaSkill[i, 0].effectIndex = DataManager.Instance.skillData[skillId1].effectIndex;
            playerCharaSkill[i, 0].seIndex = DataManager.Instance.skillData[skillId1].seIndex;
            playerCharaSkill[i, 0].skillType = DataManager.Instance.skillData[skillId1].skillType;
            playerCharaSkill[i, 0].value = DataManager.Instance.skillData[skillId1].value;
            playerCharaSkill[i, 0].targetAll = DataManager.Instance.skillData[skillId1].targetAll;
            playerCharaSkill[i, 0].effectiveTurn = DataManager.Instance.skillData[skillId1].effectiveTurn;

            playerCharaSkill[i, 1].id = skillId1;
            playerCharaSkill[i, 1].name = DataManager.Instance.skillData[skillId2].skillName;
            playerCharaSkill[i, 1].effectIndex = DataManager.Instance.skillData[skillId2].effectIndex;
            playerCharaSkill[i, 1].seIndex = DataManager.Instance.skillData[skillId2].seIndex;
            playerCharaSkill[i, 1].skillType = DataManager.Instance.skillData[skillId2].skillType;
            playerCharaSkill[i, 1].value = DataManager.Instance.skillData[skillId2].value;
            playerCharaSkill[i, 1].targetAll = DataManager.Instance.skillData[skillId2].targetAll;
            playerCharaSkill[i, 1].effectiveTurn = DataManager.Instance.skillData[skillId2].effectiveTurn;
        }
        //�G�L�����̃X�L����ǂݍ���
        enemySkill = new Skill[BattleManager.Instance.enemyStatus.Length, 2];
        for (int i = 0; i < BattleManager.Instance.enemyStatus.Length; i++)
        {
            enemySkill[i, 0] = new Skill();
            enemySkill[i, 1] = new Skill();

            int skillId1 = BattleManager.Instance.enemyStatus[i].skillId1;
            int skillId2 = BattleManager.Instance.enemyStatus[i].skillId2;

            enemySkill[i, 0].id = skillId1;
            enemySkill[i, 0].name = DataManager.Instance.skillData[skillId1].skillName;
            enemySkill[i, 0].effectIndex = DataManager.Instance.skillData[skillId1].effectIndex;
            enemySkill[i, 0].seIndex = DataManager.Instance.skillData[skillId1].seIndex;
            enemySkill[i, 0].skillType = DataManager.Instance.skillData[skillId1].skillType;
            enemySkill[i, 0].value = DataManager.Instance.skillData[skillId1].value;
            enemySkill[i, 0].targetAll = DataManager.Instance.skillData[skillId1].targetAll;
            enemySkill[i, 0].effectiveTurn = DataManager.Instance.skillData[skillId1].effectiveTurn;

            enemySkill[i, 1].id = skillId1;
            enemySkill[i, 1].name = DataManager.Instance.skillData[skillId2].skillName;
            enemySkill[i, 1].effectIndex = DataManager.Instance.skillData[skillId2].effectIndex;
            enemySkill[i, 1].seIndex = DataManager.Instance.skillData[skillId2].seIndex;
            enemySkill[i, 1].skillType = DataManager.Instance.skillData[skillId2].skillType;
            enemySkill[i, 1].value = DataManager.Instance.skillData[skillId2].value;
            enemySkill[i, 1].targetAll = DataManager.Instance.skillData[skillId2].targetAll;
            enemySkill[i, 1].effectiveTurn = DataManager.Instance.skillData[skillId2].effectiveTurn;
        }
    }

    //�ŁA�X�e�[�^�X�ϓ��Ȃǂ̃^�[���𐔂���
    public void AbnormalityTurnCount()
    {
        for (int i = 0; i < BattleManager.Instance.playerCharaStatus.Length; i++)
        {

        }
    }
}

//�퓬���̃X�L���̃f�[�^
//skillType�͍U���A�񕜂Ȃǂ̌��ʂ̕��ށB�O���U���A�P���񕜁A�Q���X�e�[�^�X�㏸�A�R���X�e�[�^�X�ቺ�A�S����Ԉُ�
[System.Serializable]
public class Skill
{
    public int id;
    public string name;
    public int effectIndex;
    public int seIndex;

    public int skillType;
    public float value;
    public bool targetAll;
    public int effectiveTurn;
}
