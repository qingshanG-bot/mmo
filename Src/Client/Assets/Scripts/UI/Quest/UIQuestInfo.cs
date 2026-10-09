using Common.Data;
using Managers;
using Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestInfo : MonoBehaviour {

    public Text npcName;
    public Text title;
    public Text[] targets;
    public Text description;

    public TMP_Text rewardMoney;
    public TMP_Text rewardExp;

    public Button navButton;
    private int npc;
    public Text overview;
    public UIIconItem rewardItemPrefab;  
    public Transform rewardItemRoot; 


    void Start () {
		
	}

    public void SetQuestInfo(Quest quest)
    {
        if (quest.Info == null)
        {
            this.npc = quest.Define.AcceptNPC;
        }
        else if (quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
        {
            this.npc = quest.Define.SubmitNPC;
        }

        NpcDefine npcDefine = NpcManager.Instance.GetNpcDefine(npc);
        if (npcDefine != null && this.npcName != null)
        {
            this.npcName.text = npcDefine.Name;
        }

        this.title.text = string.Format("[{0}]{1}", quest.Define.Type, quest.Define.Name);
        if (this.overview != null) this.overview.text = quest.Define.Overview;

        if (this.description != null)
        {
            if (quest.Info == null)
            {
                this.description.text = quest.Define.Dialog;
            }
            else
            {
                if (quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
                {
                    this.description.text = quest.Define.DialogFinish;
                }
            }
        }

        foreach (Transform child in rewardItemRoot)
        {
            Destroy(child.gameObject);
        }

        List<int> rewardItemIDs = new List<int>();
        List<int> rewardItemCounts = new List<int>();
        // 获取奖励物品ID和数量
        if (quest.Define.RewardItem1 > 0)
        {
            rewardItemIDs.Add(quest.Define.RewardItem1);
            rewardItemCounts.Add(quest.Define.RewardItem1Count);
        }
        if (quest.Define.RewardItem2 > 0)
        {
            rewardItemIDs.Add(quest.Define.RewardItem2);
            rewardItemCounts.Add(quest.Define.RewardItem2Count);
        }
        if (quest.Define.RewardItem3 > 0)
        {
            rewardItemIDs.Add(quest.Define.RewardItem3);
            rewardItemCounts.Add(quest.Define.RewardItem3Count);
        }

        for (int i = 0; i < rewardItemIDs.Count; i++)
        {
            ItemDefine itemDefine = ItemManager.Instance.GetItemDefine(rewardItemIDs[i]);

            if (itemDefine != null)
            {
                UIIconItem rewardItem = Instantiate(rewardItemPrefab, rewardItemRoot);

                rewardItem.SetMainIcon(itemDefine.Icon, rewardItemCounts[i].ToString());
            }

        }

        this.rewardMoney.text = quest.Define.RewardGold.ToString();
        this.rewardExp.text = quest.Define.RewardExp.ToString();
        

        if (this.navButton != null)
        {
            this.navButton.gameObject.SetActive(this.npc > 0);
        }

        foreach (var fitter in this.GetComponentsInChildren<ContentSizeFitter>())
        {
            fitter.SetLayoutVertical();
        }
    }

    public void OnClickAbandon()
    {

    }

    public void OnClikNav()
    {
        Vector3 pos = NpcManager.Instance.GetNpcPosition(this.npc);
        User.Instance.CurrentCharacterObject.StartNav(pos);
        UIManager.Instance.Close<UIQuestSystem>();
    }

    
}
