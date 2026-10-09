using Managers;
using Models;
using Services;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGuild : UIWindow {

    public GameObject itemPrefab;
    public ListView listMain;
    public Transform itemRoot;
    public UIGuildInfo uiInfo;
    public UIGuildMemberItem selectedItem;

    public GameObject panelAdmin;
    public GameObject panelLeader;

	void Start ()
    {
        GuildService.Instance.OnGuildUpdate += UpdateUI;
        this.listMain.onItemSelected += this.OnGuildMemberSelected;
        this.UpdateUI();
	}
    private void OnDestroy()
    {
        GuildService.Instance.OnGuildUpdate -= UpdateUI;
    }

    void UpdateUI()
    {
        this.uiInfo.Info = GuildManager.Instance.guildInfo;
        this.selectedItem = null;

        ClearList();
        InitItems();

        this.panelAdmin.SetActive(GuildManager.Instance.myMemberInfo.Title > GuildTitle.None);
        this.panelLeader.SetActive(GuildManager.Instance.myMemberInfo.Title ==GuildTitle.President);
    }

    public void OnGuildMemberSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UIGuildMemberItem;

    }


    void InitItems()
    {
        foreach (var item in GuildManager.Instance.GetSortedMembers())
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIGuildMemberItem ui = go.GetComponent<UIGuildMemberItem>();
            ui.SetGuildMemberInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    void ClearList()
    {
        this.listMain.RemoveAll();
    }

    public void OnClickAppliesList()
    {
        UIManager.Instance.Show<UIGuildApplyList>();
    }

    public void OnClickLeave()
    {
        MessageBox.Show("确定要离开公会吗？", "退出公会", MessageBoxType.Confirm, "确定离开", "取消").OnYes = () =>
        {
            GuildService.Instance.SendGuildLeaveRequest();
        };
    }

    public void OnClickAddFriend()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要添加好友的成员");
            return;
        }

        MessageBox.Show(string.Format("要添加【{0}】为好友吗？", this.selectedItem.Info.Info.Name), "好友申请", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            FriendService.Instance.SendFriendAddRequest(this.selectedItem.Info.Info.Id,this.selectedItem.Info.Info.Name);
        };
    }

    public void OnClickKickout()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要移除的成员");
            return;
        }

        MessageBox.Show(string.Format("要将【{0}】移出公会吗？", this.selectedItem.Info.Info.Name), "移出公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
               {
                   GuildService.Instance.SendGuildAdminCommand(GuildAdminCommand.Kickout, this.selectedItem.Info.Info.Id);
               };
    }

    public void OnClickPromote()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要晋升的成员");
            return;
        }
        if (selectedItem.Info.Title != GuildTitle.None)
        {
            MessageBox.Show("对方身份尊贵，已经无法再晋升啦！");
            return;
        }
        MessageBox.Show(string.Format("要晋升【{0}】为公会副会长吗？", this.selectedItem.Info.Info.Name), "晋升", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
               {
                   GuildService.Instance.SendGuildAdminCommand(GuildAdminCommand.Promote, this.selectedItem.Info.Info.Id);
               };
    }

    public void OnClickDepose()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要罢免的成员");
            return;
        }
        if (selectedItem.Info.Title == GuildTitle.None)
        {
            MessageBox.Show("对方暂时还没有职位呢");
            return;
        }
        if (selectedItem.Info.Title == GuildTitle.President)
        {
            MessageBox.Show("他可是会长大人！你想反了吗！！！");
            return;
        }
        MessageBox.Show(string.Format("要罢免【{0}】的公会职务吗？",this.selectedItem.Info.Info.Name), "罢免职务", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
              {
                  GuildService.Instance.SendGuildAdminCommand(GuildAdminCommand.Depost, this.selectedItem.Info.Info.Id);
              };
    }

    public void OnClickTransfer()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要转让的成员");
            return;
        }
        MessageBox.Show(string.Format("要把会长转让给【{0}】吗？", this.selectedItem.Info.Info.Name), "转让会长", MessageBoxType.Confirm, "确认", "取消").OnYes = () =>
               {
                   GuildService.Instance.SendGuildAdminCommand(GuildAdminCommand.Transfer, this.selectedItem.Info.Info.Id);
               };
    }

    public void OnClickSetNotice()
    {
        if (GuildManager.Instance.guildInfo == null || GuildManager.Instance.myMemberInfo == null)
        {
            MessageBox.Show("你还没有公会");
            return;
        }

        // 只有会长/副会长可修改（服务端也会再校验）
        var title = GuildManager.Instance.myMemberInfo.Title;
        if (title != GuildTitle.President && title != GuildTitle.VicePresident)
        {
            MessageBox.Show("只有会长或副会长才能修改公会宣言");
            return;
        }

        string current = GuildManager.Instance.guildInfo.Notice ?? "";
        var box = InputBox.Show(
            message: "请输入新的公会宣言（3-50个字符）",
            title: "修改公会宣言",
            btnOK: "确定",
            btnCancel: "取消",
            emptyTips: "公会宣言不能为空"
        );

        box.OnSubmit += (string inputText, out string tips) =>
        {
            tips = "";
            string notice = (inputText ?? "").Trim();

            if (notice.Length < 3 || notice.Length > 50)
            {
                tips = "公会宣言长度需要 3-50 个字符";
                return false;
            }
            if (notice == current.Trim())
            {
                tips = "宣言没有变化";
                return false;
            }

            GuildService.Instance.SendGuildSetNoticeRequest(notice);
            return true; // true = 关闭输入框
        };
    }
}
