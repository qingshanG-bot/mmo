using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    class Chat
    {
        //Character Owner;

        // 本地频道按地图分别记录读取位置
        private Dictionary<int, int> localIdxs = new Dictionary<int, int>();

        public int worldIdx;
        public int systemIdx;
        public int teamIdx;
        public int guildIdx;

        public Chat()
        {
        }

        public void PostProcess(Character owner, NetMessageResponse message)
        {
            if (message.Chat == null)
            {
                message.Chat = new ChatResponse();
                message.Chat.Result = Result.Success;
            }
            //this.localIdx = ChatManager.Instance.GetLocalMessages(this.Owner.Info.mapId, this.localIdx, message.Chat.localMessages);
            //this.worldIdx = ChatManager.Instance.GetWorldMessages(this.worldIdx, message.Chat.worldMessages);
            //this.systemIdx = ChatManager.Instance.GetSystemMessages(this.systemIdx, message.Chat.systemMessages);
            //if (this.Owner.Team != null)
            //{
            //    this.teamIdx = ChatManager.Instance.GetTeamMessages(this.Owner.Team.Id, this.teamIdx, message.Chat.teamMessages);
            //}
            //if (this.Owner.Guild != null)
            //{
            //    this.guildIdx = ChatManager.Instance.GetGuildMessages(this.Owner.Guild.Id, this.guildIdx, message.Chat.guildMessages);
            //}

            int mapId = owner.Info.mapId;
            int localIdx = 0;
            this.localIdxs.TryGetValue(mapId, out localIdx);
            localIdx = ChatManager.Instance.GetLocalMessages(mapId, localIdx, message.Chat.localMessages);
            this.localIdxs[mapId] = localIdx;

            this.worldIdx = ChatManager.Instance.GetWorldMessages(this.worldIdx, message.Chat.worldMessages);
            this.systemIdx = ChatManager.Instance.GetSystemMessages(this.systemIdx, message.Chat.systemMessages);

            if (owner.Team != null)
            {
                this.teamIdx = ChatManager.Instance.GetTeamMessages(owner.Team.Id, this.teamIdx, message.Chat.teamMessages);
            }

            if (owner.Guild != null)
            {
                this.guildIdx = ChatManager.Instance.GetGuildMessages(owner.Guild.Id, this.guildIdx, message.Chat.guildMessages);
            }
        }

        public void ResetTeam()
        {
            this.teamIdx = 0;
        }

        public void ResetGuild()
        {
            this.guildIdx = 0;
        }
    }
}
