using Models;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestItem : ListView.ListViewItem {

    public Text title;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    public TMP_Text statusText;

    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

    public Quest quest;
    // Use this for initialization
    void Start () {
		
	}

    bool isEquiped = false;

    public void SetQuestInfo(Quest item)
    {
        this.quest = item;
        if (this.title != null) this.title.text = this.quest.Define.Name;

        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (statusText == null)
            return;

        if (quest == null || quest.Info == null)
        {
            statusText.text = ""; 
            return;
        }

        switch (quest.Info.Status)
        {
            case QuestStatus.InProgress:
                statusText.text = "未完成";
                statusText.color = Color.red;
                break;

            case QuestStatus.Complated:
                statusText.text = "可提交";
                statusText.color = Color.yellow;
                break;

            case QuestStatus.Finished:
                statusText.text = "已完成";
                statusText.color = Color.green;
                break;

            default:
                statusText.text = "";
                break;
        }
    }

}
