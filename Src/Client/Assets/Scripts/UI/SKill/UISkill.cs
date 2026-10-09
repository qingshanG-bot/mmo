using Managers;
using Models;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkill : UIWindow
{
    public Text description;
    public GameObject itemPrefab;
    public ListView listMain;
    

    void Start()
    {
        RefreshUI();
        this.listMain.onItemSelected += this.OnItemSelected;
    }

    private void OnDestroy()
    {

    }
    private UISkillItem selectedItem;
    public void OnItemSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UISkillItem;
        this.description.text = this.selectedItem.item.Define. Description;
    }
    // Update is called once per frame
    void RefreshUI()
    {
        ClearItems();
        InitItems();
    }

    public void InitItems()
    {
        //var SKills = DataManager.Instance.SKills[(int)User.Instance.CurrentCharacterInfo.Class];
        var Skills = User.Instance.CurrentCharacter.SkillMgr.Skills;
        foreach (var skill in Skills)
        {
            if (skill.Define.Type == Common.Battle.SkillType.Skill)
            {
                GameObject go = Instantiate(itemPrefab, this.listMain.transform);
                UISkillItem ui = go.GetComponent<UISkillItem>();
                ui.SetItem(skill, this, false);
                this.listMain.AddItem(ui);
            }
        }
    }

    public void ClearItems()
    {
        this.listMain.RemoveAll();
    }

    //待改升级技能
    //public void DoRide()
    //{
    //    if (this.selectedItem == null)
    //    {
    //        MessageBox.Show("请选择你的坐骑", "提示");
    //        return;
    //    }
    //    User.Instance.Ride(this.selectedItem.item.Id);
    //}
}
