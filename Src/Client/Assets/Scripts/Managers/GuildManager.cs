using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managers
{
    class GuildManager:Singleton<GuildManager>
    {
        public NGuildInfo guildInfo;

        public NGuildMemberInfo myMemberInfo;

        public bool HasGuild
        {
            get { return this.guildInfo != null; }
        }

        public void Init(NGuildInfo guild)
        {
            this.guildInfo = guild;
            if (guild == null)
            {
                myMemberInfo = null;
                return;
            }
            foreach (var mem in guild.Members)
            {
                if (mem.characterId == User.Instance.CurrentCharacterInfo.Id)
                {
                    myMemberInfo = mem;
                    break;
                }
            }
        }

        public void ShowGuild()
        {
            if (this.HasGuild)
                UIManager.Instance.Show<UIGuild>();
            else
            {
                var win = UIManager.Instance.Show<UIGuildPopNoGuild>();
                win.OnClose += PopNoGuild_OnClose;
            }
        }

        private void PopNoGuild_OnClose(UIWindow sender, UIWindow.WindowResult result)
        {
            if (result == UIWindow.WindowResult.Yes)
            {//创建
                UIManager.Instance.Show<UIGuildPopCreate>();
            }
            else if (result == UIWindow.WindowResult.No)
            {//加入
                UIManager.Instance.Show<UIGuildList>();
            }
        }


        public List<NGuildMemberInfo> GetSortedMembers()
        {
            if (this.guildInfo == null || this.guildInfo.Members == null)
                return new List<NGuildMemberInfo>();

            List<NGuildMemberInfo> result = new List<NGuildMemberInfo>(this.guildInfo.Members);

            result.Sort(CompareGuildMember);

            return result;
        }

        private int CompareGuildMember(NGuildMemberInfo a, NGuildMemberInfo b)
        {
            // 1. 按身份排序：会长 > 副会长 > 会员
            int titleCompare = GetTitlePriority(b.Title).CompareTo(GetTitlePriority(a.Title));
            if (titleCompare != 0)
                return titleCompare;

            // 2. 在线优先：在线在前
            int onlineCompare = b.Status.CompareTo(a.Status);
            if (onlineCompare != 0)
                return onlineCompare;

            // 3. 等级高优先：等级高在前
            int levelCompare = b.Info.Level.CompareTo(a.Info.Level);
            if (levelCompare != 0)
                return levelCompare;

            // 4. 加入时间早优先：时间小的在前
            int joinTimeCompare = a.joinTime.CompareTo(b.joinTime);
            if (joinTimeCompare != 0)
                return joinTimeCompare;

            return a.characterId.CompareTo(b.characterId);
        }

        private int GetTitlePriority(GuildTitle title)
        {
            switch (title)
            {
                case GuildTitle.President:
                    return 3;   // 会长
                case GuildTitle.VicePresident:
                    return 2;   // 副会长
                case GuildTitle.None:
                default:
                    return 1;   // 会员
            }
        }
    }
}
