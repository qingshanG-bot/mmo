using Common.Utils;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildMemberItem : ListView.ListViewItem
{
    public Text nickname;
    public Text level;
    public Text @class;
    public Text title;
    public Text joinTime;
    public Text status;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

    public NGuildMemberInfo Info;

    public void SetGuildMemberInfo(NGuildMemberInfo item)
    {
        this.Info = item;
        if (this.nickname != null) this.nickname.text = this.Info.Info.Name;
        if (this.@class != null) this.@class.text = this.Info.Info.Class.ToString();
        if (this.level != null) this.level.text = this.Info.Info.Level.ToString();
        if (this.title != null) this.title.text = this.GetGuildTitleName(this.Info.Title);
        if (this.joinTime != null) this.joinTime.text = TimeUtil.GetTime( this.Info.joinTime).ToString();
        if (this.status != null) this.status.text = this.Info.Status == 1 ? "在线" : TimeUtil.GetTime(this.Info.lastTime).ToString();
    }

    string GetGuildTitleName(GuildTitle title)
    {
        switch (title)
        {
            case GuildTitle.President:
                return "会长";
            case GuildTitle.VicePresident:
                return "副会长";
            case GuildTitle.None:
            default:
                return "会员";
        }
    }
}
