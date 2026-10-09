using Models;
using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    class ChatManager : Singleton<ChatManager>
    {
        public Action OnChat { get; internal set; }

        public LocalChannel displayChannel;
        public LocalChannel sendChannel;

        public int PrivateID = 0;
        public string PrivateName = "";

       
        public enum LocalChannel
        {
            All = 0,
            Local = 1,
            World = 2,
            Team = 3,
            Guild = 4,
            Private = 5,
        }

        //过滤器
        private ChatChannel[] ChannelFilter = new ChatChannel[6]
        {
            ChatChannel.Local | ChatChannel.World | ChatChannel.Guild | ChatChannel.Team | ChatChannel.Private| ChatChannel.System,
            ChatChannel.Local,
            ChatChannel.World,
            ChatChannel.Team,
            ChatChannel.Guild,
            ChatChannel.Private
        };

        public List<ChatMessage>[] Messages = new List<ChatMessage>[6]
            {
                new List<ChatMessage>(),
                new List<ChatMessage>(),
                new List<ChatMessage>(),
                new List<ChatMessage>(),
                new List<ChatMessage>(),
                new List<ChatMessage>(),
            };

        public void Init()
        {
            foreach (var messages in this.Messages)
            {
                messages.Clear();
            }
        }

        //私聊功能：
        internal void StartPrivateChat(int targetId, string targetName)
        {
            this.PrivateID = targetId;
            this.PrivateName = targetName;

            this.sendChannel = LocalChannel.Private;
            if (this.OnChat != null)
                this.OnChat();
        }

        //发送聊天消息
        public void SendChat(String content, int toId = 0, string toName = "")
        {
            ChatService.Instance.SendChat(this.SendChannel, content, toId, toName);
        }

        //确认聊天频道
        public ChatChannel SendChannel
        {
            get
            {
                switch (sendChannel)
                {
                    case LocalChannel.Local:
                        return ChatChannel.Local;

                    case LocalChannel.World:
                        return ChatChannel.World;

                    case LocalChannel.Team:
                        return ChatChannel.Team;

                    case LocalChannel.Guild:
                        return ChatChannel.Guild;

                    case LocalChannel.Private:
                        return ChatChannel.Private;
                }
                return ChatChannel.Local;
            }
        }

        //设置发送频道
        public bool SetSendChannel(LocalChannel channel)
        {

            if (channel == LocalChannel.Private)
            {
                if (User.Instance.CurrentCharacterInfo.Friends == null)
                {
                    this.AddSystemMessage("你还没添加任何好友");
                    return false;
                }
            }
            if (channel == LocalChannel.Team)
            {
                if (User.Instance.TeamInfo == null)
                {
                    this.AddSystemMessage("你没有加入任何队伍");
                    return false;
                }
            }

            if (channel==LocalChannel.Guild)
            {
                if (User.Instance.CurrentCharacterInfo.Guild==null)
                {
                    this.AddSystemMessage("你没有加入任何公会！");
                    return false;
                }
            }

            this.sendChannel = channel;
            Debug.LogFormat("Set channel:{0}", this.sendChannel);
            return true;
        }

        //添加消息到聊天列表：
        public void AddMessages(ChatChannel channel, List<ChatMessage> messages)
        {
            //不在游戏里（CurrentCharacter 为空）时，不处理玩家频道消息，避免 UI 刷新触发 NRE
            if (User.Instance.CurrentCharacterInfo == null)
                return;

            for (int ch = 0; ch < 6; ch++)
            {
                if ((this.ChannelFilter[ch] & channel) == channel)
                {
                    //messages.Add(messages);
                    this.Messages[ch].AddRange(messages);
                }
            }
            if (this.OnChat != null)
                this.OnChat();
        }

        //添加系统消息：
        public void AddSystemMessage(string message, string from = "")
        {

            this.Messages[(int)LocalChannel.All].Add(new ChatMessage()
            {
                Channel = ChatChannel.System,
                Message = message,
                FromName = from
            });

            if (this.OnChat != null)
                this.OnChat();
        }

        //获取当前频道的消息：
        public string GetCurrentMessages()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var message in this.Messages[(int)displayChannel])
            {
                sb.AppendLine(FormatMessage(message));
            }
            return sb.ToString();
        }

        //格式化消息：
        public string FormatMessage(ChatMessage message)
        {
            switch (message.Channel)
            {
                case ChatChannel.Local:
                    return string.Format("<color=#E6E6E6>[本地]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.World:
                    return string.Format("<color=#00FFFF>[世界]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.System:
                    return string.Format("<color=#FFD700>[系统]{0}</color>", message.Message);
                case ChatChannel.Private:
                    return string.Format("<color=#FF66CC>[私聊]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.Team:
                    return string.Format("<color=#32CD32>[队伍]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.Guild:
                    return string.Format("<color=#1E90FF>[公会]{0}{1}</color>", FormatFromPlayer(message), message.Message);
            }
            return "";
        }

        // 格式化玩家名：
        private string FormatFromPlayer(ChatMessage message)
        {
            if (message.FromId == User.Instance.CurrentCharacterInfo.Id)
            {
                return "<link=\"\"><#00FFE0><u>[我]</u></color></link>";
            }
            else
                return string.Format("<link=\"{0}:{1}\"><#00FFE0><u>[{1}]</u></color></link>", message.FromId, message.FromName);
        }
    }
}
