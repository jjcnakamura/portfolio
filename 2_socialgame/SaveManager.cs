using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// �f�[�^���Z�[�u���鏈��
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    //CSV�t�@�C���̃p�X
    string playerCsvPath = "Assets/Resources/PlayerSaveData.csv";
    string charaCsvPath = "Assets/Resources/CharaSaveData.csv";

    void Awake()
    {
        //�V�[����J�ڂ��Ă�SaveManager�͎c��
        if (gameObject.transform.parent != null) gameObject.transform.parent = null;
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// �v���C���[�̃f�[�^��CSV�t�@�C���ɏ�������
    /// </summary>
    public void PlayerDataSave()
    {
        List<int[]> writeDataList = new List<int[]>();
        int[] writeData = new int[1];

        //�����K�`���`�P�b�g��
        writeData[0] = DataManager.Instance.playerData.gachaTicketNum;
        writeDataList.Add(writeData);
        //�퓬�p�L�����̕Ґ�
        Array.Resize(ref writeData, DataManager.Instance.partyNum);
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            writeData[i] = DataManager.Instance.playerData.battleCharaId[i];
        }
        writeDataList.Add(writeData);
        Array.Resize(ref writeData, 1);
        //���j���[��ʂɕ\������L������ID
        writeData[0] = DataManager.Instance.playerData.menuBackCharaId;
        writeDataList.Add(writeData);
        //BGM��SE�̉���
        Array.Resize(ref writeData, 2);
        writeData[0] = DataManager.Instance.playerData.bgmVolume;
        writeData[1] = DataManager.Instance.playerData.seVolume;
        writeDataList.Add(writeData);

        StreamWriter writer = new StreamWriter(playerCsvPath, false);
        writer.WriteLine(DataManager.Instance.playerData.userId);   //���[�U�[id��CSV�P�s�ڂ�
        writer.WriteLine(DataManager.Instance.playerData.userName); //���[�U�[����CSV�Q�s�ڂ�
        for (int i = 0; i < DataManager.Instance.playerDataNum - 2; i++)
        {
            string line = string.Join(",", writeDataList[i]); //�z����J���}�ŋ�؂���������ɕϊ�����
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// �v���C���[�̃Z�[�u�f�[�^�̏�����
    /// </summary>
    public void PlayerDataInitialize()
    {
        List<int[]> writeDataList = new List<int[]>();
        int[] writeData = new int[1];

        //�����K�`���`�P�b�g��
        writeData[0] = 10;
        writeDataList.Add(writeData);
        //�퓬�p�L�����̕Ґ�
        Array.Resize(ref writeData, DataManager.Instance.partyNum);
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            writeData[i] = i;
        }
        writeDataList.Add(writeData);
        Array.Resize(ref writeData, 1);
        //���j���[��ʂɕ\������L������ID
        writeData[0] = 0;
        writeDataList.Add(writeData);
        //BGM��SE�̉���
        Array.Resize(ref writeData, 2);
        writeData[0] = 8;
        writeData[1] = 8;
        writeDataList.Add(writeData);

        StreamWriter writer = new StreamWriter(playerCsvPath, false);
        writer.WriteLine(0);      //���[�U�[id��CSV�P�s�ڂ�
        writer.WriteLine("User"); //���[�U�[����CSV�Q�s�ڂ�
        for (int i = 0; i < DataManager.Instance.playerDataNum - 2; i++)
        {
            string line = string.Join(",", writeDataList[i]); //�z����J���}�ŋ�؂���������ɕϊ�����
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// �L�����̃f�[�^��CSV�t�@�C���ɏ�������
    /// </summary>
    public void CharaDataSave()
    {
        List<int[]> writeDataList = new List<int[]>();

        for (int i = 0; i < DataManager.Instance.charaNum; i++)
        {
            int[] writeData = new int[3];

            writeData[0] = DataManager.Instance.charaData[i].totsu;
            writeData[1] = DataManager.Instance.charaData[i].lv;
            writeData[2] = DataManager.Instance.charaData[i].exp;

            writeDataList.Add(writeData);
        }

        StreamWriter writer = new StreamWriter(charaCsvPath, false);
        for (int i = 0; i < DataManager.Instance.charaNum; i++)
        {
            string line = string.Join(",", writeDataList[i]); //�z����J���}�ŋ�؂���������ɕϊ�����
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// �L�����̃Z�[�u�f�[�^�̏�����
    /// </summary>
    public void CharaDataInitialize()
    {
        List<int[]> writeDataList = new List<int[]>();

        //�������炢��L�����͍ŏ���������Ԃɂ���
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            int[] writeData = new int[3];

            writeData[0] = 1;
            writeData[1] = 1;
            writeData[2] = 0;

            writeDataList.Add(writeData);
        }
        for (int i = DataManager.Instance.partyNum; i < DataManager.Instance.charaNum; i++)
        {
            int[] writeData = new int[3];

            writeData[0] = 0;
            writeData[1] = 1;
            writeData[2] = 0;

            writeDataList.Add(writeData);
        }

        StreamWriter writer = new StreamWriter(charaCsvPath, false);
        for (int i = 0; i < DataManager.Instance.charaNum; i++)
        {
            string line = string.Join(",", writeDataList[i]); //�z����J���}�ŋ�؂���������ɕϊ�����
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    //�r���h�������s�f�[�^�̏ꍇ�̓t�@�C���̃p�X��ύX
    public void FilePassChange()
    {
        playerCsvPath = DataManager.Instance.saveDataDirectoryPass + DataManager.Instance.playerSaveDataPass;
        charaCsvPath = DataManager.Instance.saveDataDirectoryPass + DataManager.Instance.charaSaveDataPass;
    }
}
