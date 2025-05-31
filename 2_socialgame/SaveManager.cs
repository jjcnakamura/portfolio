using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// データをセーブする処理
/// </summary>
public class SaveManager : Singleton<SaveManager>
{
    //CSVファイルのパス
    string playerCsvPath = "Assets/Resources/PlayerSaveData.csv";
    string charaCsvPath = "Assets/Resources/CharaSaveData.csv";

    void Awake()
    {
        //シーンを遷移してもSaveManagerは残る
        if (gameObject.transform.parent != null) gameObject.transform.parent = null;
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// プレイヤーのデータをCSVファイルに書き込む
    /// </summary>
    public void PlayerDataSave()
    {
        List<int[]> writeDataList = new List<int[]>();
        int[] writeData = new int[1];

        //所持ガチャチケット数
        writeData[0] = DataManager.Instance.playerData.gachaTicketNum;
        writeDataList.Add(writeData);
        //戦闘用キャラの編成
        Array.Resize(ref writeData, DataManager.Instance.partyNum);
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            writeData[i] = DataManager.Instance.playerData.battleCharaId[i];
        }
        writeDataList.Add(writeData);
        Array.Resize(ref writeData, 1);
        //メニュー画面に表示するキャラのID
        writeData[0] = DataManager.Instance.playerData.menuBackCharaId;
        writeDataList.Add(writeData);
        //BGMとSEの音量
        Array.Resize(ref writeData, 2);
        writeData[0] = DataManager.Instance.playerData.bgmVolume;
        writeData[1] = DataManager.Instance.playerData.seVolume;
        writeDataList.Add(writeData);

        StreamWriter writer = new StreamWriter(playerCsvPath, false);
        writer.WriteLine(DataManager.Instance.playerData.userId);   //ユーザーidをCSV１行目に
        writer.WriteLine(DataManager.Instance.playerData.userName); //ユーザー名をCSV２行目に
        for (int i = 0; i < DataManager.Instance.playerDataNum - 2; i++)
        {
            string line = string.Join(",", writeDataList[i]); //配列をカンマで区切った文字列に変換する
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// プレイヤーのセーブデータの初期化
    /// </summary>
    public void PlayerDataInitialize()
    {
        List<int[]> writeDataList = new List<int[]>();
        int[] writeData = new int[1];

        //所持ガチャチケット数
        writeData[0] = 10;
        writeDataList.Add(writeData);
        //戦闘用キャラの編成
        Array.Resize(ref writeData, DataManager.Instance.partyNum);
        for (int i = 0; i < DataManager.Instance.partyNum; i++)
        {
            writeData[i] = i;
        }
        writeDataList.Add(writeData);
        Array.Resize(ref writeData, 1);
        //メニュー画面に表示するキャラのID
        writeData[0] = 0;
        writeDataList.Add(writeData);
        //BGMとSEの音量
        Array.Resize(ref writeData, 2);
        writeData[0] = 8;
        writeData[1] = 8;
        writeDataList.Add(writeData);

        StreamWriter writer = new StreamWriter(playerCsvPath, false);
        writer.WriteLine(0);      //ユーザーidをCSV１行目に
        writer.WriteLine("User"); //ユーザー名をCSV２行目に
        for (int i = 0; i < DataManager.Instance.playerDataNum - 2; i++)
        {
            string line = string.Join(",", writeDataList[i]); //配列をカンマで区切った文字列に変換する
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// キャラのデータをCSVファイルに書き込む
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
            string line = string.Join(",", writeDataList[i]); //配列をカンマで区切った文字列に変換する
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// キャラのセーブデータの初期化
    /// </summary>
    public void CharaDataInitialize()
    {
        List<int[]> writeDataList = new List<int[]>();

        //初期からいるキャラは最初から入手状態にする
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
            string line = string.Join(",", writeDataList[i]); //配列をカンマで区切った文字列に変換する
            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }

    //ビルドした実行データの場合はファイルのパスを変更
    public void FilePassChange()
    {
        playerCsvPath = DataManager.Instance.saveDataDirectoryPass + DataManager.Instance.playerSaveDataPass;
        charaCsvPath = DataManager.Instance.saveDataDirectoryPass + DataManager.Instance.charaSaveDataPass;
    }
}
