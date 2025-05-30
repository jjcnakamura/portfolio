using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// キャラを引くガチャの処理
/// </summary>
public class GachaManager : Singleton<GachaManager>
{
    public int gachaTicketNum;
    [SerializeField] int totsuMax = 5;

    [Space(10)]

    //ガチャの各画面
    [SerializeField] GameObject window_Start, window_Pull, window_Result;
    //ガチャ画面で使用するコンポーネント
    [SerializeField] TextMeshProUGUI[] gachaTicketText;
    [SerializeField] Button gachaButton,gachaButton_TenPull;
    [SerializeField] TextMeshProUGUI pullCharaNameText;
    [SerializeField] Image pullCharaImage;
    //ガチャのリザルトで表示するキャラ
    [SerializeField] GameObject resultCharaNameObj;
    [SerializeField] GameObject resultCharaImageObj;

    TextMeshProUGUI[] resultCharaNameText;
    Image[] resultCharaImage;

    //入手したキャラを表示する順番を管理する変数
    int[] getCharaArray;
    int getCharaArrayNum;

    void Start()
    {
        //ウィンドウの表示切り替え
        window_Start.SetActive(true);
        window_Pull.SetActive(false);
        window_Result.SetActive(false);

        //手持ちガチャチケット数をロード
        gachaTicketNum = DataManager.Instance.playerData.gachaTicketNum;
        for (int i = 0; i < gachaTicketText.Length; i++)
        {
            gachaTicketText[i].text = "× " + gachaTicketNum;
        }
        //チケットがなければガチャを引くボタンを非アクティブに
        gachaButton.interactable = (gachaTicketNum >= 1);
        gachaButton_TenPull.interactable = (gachaTicketNum >= 10);

        //リザルト画面用のオブジェクトを格納
        resultCharaImage = new Image[resultCharaImageObj.transform.childCount];
        resultCharaNameText = new TextMeshProUGUI[resultCharaImage.Length];
        for (int i = 0; i < resultCharaImage.Length; i++)
        {
            resultCharaImage[i] = resultCharaImageObj.transform.GetChild(i).GetComponent<Image>();
            resultCharaImage[i].gameObject.SetActive(false);

            resultCharaNameText[i] = resultCharaNameObj.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            resultCharaNameText[i].gameObject.SetActive(false);
        }
    }

    //ガチャを引くボタン
    public void PullGacha(int pullValue)
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        //ガチャチケットを消費
        gachaTicketNum -= pullValue;
        DataManager.Instance.playerData.gachaTicketNum = gachaTicketNum;
        gachaTicketText[0].text = "× " + gachaTicketNum;
        //チケットがなければガチャを引くボタンを非アクティブに
        gachaButton.interactable = (gachaTicketNum >= 1);
        gachaButton_TenPull.interactable = (gachaTicketNum >= 10);

        Array.Resize(ref getCharaArray, pullValue);
        for (int i = 0; i < pullValue; i++)
        {
            //0 ～ キャラ数で抽選
            int randomNum = UnityEngine.Random.Range(0, DataManager.Instance.charaNum);

            //キャラクター入手の情報を保存
            DataManager.Instance.charaData[randomNum].get = true;
            DataManager.Instance.charaData[randomNum].totsu = Math.Min(DataManager.Instance.charaData[randomNum].totsu + 1, totsuMax);
            SaveManager.Instance.CharaDataSave();

            getCharaArray[i] = randomNum;

            //リザルトのキャラを表示
            resultCharaNameText[i].gameObject.SetActive(true);
            resultCharaNameText[i].text = DataManager.Instance.charaData[randomNum].charaName;
            resultCharaImage[i].gameObject.SetActive(true);
            resultCharaImage[i].sprite = Resources.Load<Sprite>(DataManager.Instance.charaData[randomNum].spritePass);
            resultCharaImage[i].preserveAspect = true;
        }
        //リザルトのキャラを非表示
        for (int i = pullValue; i < resultCharaImage.Length; i++)
        {
            resultCharaImage[i].gameObject.SetActive(false);
            resultCharaNameText[i].gameObject.SetActive(false);
        }

        //ガチャチケットの消費と入手したキャラをセーブ
        SaveManager.Instance.PlayerDataSave();

        //所持しているキャラのみをキャラステータス画面でアクティブ化
        MenuManager.Instance.CharaStatusCanvasFlag();

        getCharaArrayNum = 0;
        ViewGetChara(getCharaArray[getCharaArrayNum]);

        window_Start.SetActive(false);
        window_Pull.SetActive(true);
    }

    //入手したキャラクターを表示する
    void ViewGetChara(int getCharaId)
    {
        pullCharaNameText.text = DataManager.Instance.charaData[getCharaId].charaName;
        pullCharaImage.sprite = Resources.Load<Sprite>(DataManager.Instance.charaData[getCharaId].spritePass);
        pullCharaImage.preserveAspect = true;
    }

    //引いたキャラをページ送りするボタン
    public void PullCharaNext()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        getCharaArrayNum++;
        //最後のキャラまで表示したらリザルトに移る
        if (getCharaArrayNum >= getCharaArray.Length - 1)
        {
            ViewResult();
            return;
        }
        ViewGetChara(getCharaArray[getCharaArrayNum]);
    }

    //引いたキャラのリザルトを表示する
    public void ViewResult()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        window_Result.SetActive(true);
    }

    //リザルトを閉じるボタン
    public void ResultEnd()
    {
        SoundManager.Instance.PlaySE_Sys(0); //クリック音

        window_Pull.SetActive(false);
        window_Result.SetActive(false);
        window_Start.SetActive(true);

        gachaTicketText[1].text = "× " + gachaTicketNum;
    }
}
