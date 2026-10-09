using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;

namespace GameServer.Services
{
    class GuildService : Singleton<GuildService>
    {
        public GuildService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildCreateRequest>(this.OnGuildCreate);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildListRequest>(this.OnGuildList);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildLeaveRequest>(this.OnGuildLeave);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildAdminRequest>(this.OnGuildAdmin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildSetNoticeRequest>(this.OnGuildSetNotice);

        }



        public void Init()
        {
            GuildManager.Instance.Init();
        }


        private void OnGuildCreate(NetConnection<NetSession> sender, GuildCreateRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildCreate:GuildName:{0} character:[{1}]{2}", request.GuildName, character.Id, character.Name);
            if (sender.Session.Character.Gold >= 5000)
            {
                sender.Session.Character.Gold -= 5000;
                DBService.Instance.Save();

                sender.Session.Response.guildCreate = new GuildCreateResponse();
                if (character.Guild != null)
                {
                    sender.Session.Response.guildCreate.Result = Result.Failed;
                    sender.Session.Response.guildCreate.Errormsg = "已经有公会了";
                    sender.SendResponse();
                    return;
                }
                if (GuildManager.Instance.CheckNameExisted(request.GuildName))
                {
                    sender.Session.Response.guildCreate.Result = Result.Failed;
                    sender.Session.Response.guildCreate.Errormsg = "公会名称已存在";
                    sender.SendResponse();
                    return;
                }
                GuildManager.Instance.CreateGuild(request.GuildName, request.GuildNotice, character);
                sender.Session.Response.guildCreate.guildInfo = character.Guild.GuildInfo(character);
                sender.Session.Response.guildCreate.Result = Result.Success;
            }
            else
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "金币不够，请再赚取一些金币吧！";
            }

            sender.SendResponse();
        }

        private void OnGuildList(NetConnection<NetSession> sender, GuildListRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildList:character:[{0}]{1}", character.Id, character.Name);

            sender.Session.Response.guildList = new GuildListResponse();
            sender.Session.Response.guildList.Guilds.AddRange(GuildManager.Instance.GetGuildsInfo());
            sender.Session.Response.guildList.Result = Result.Success;
            sender.SendResponse();
        }

        /// <summary>
        /// 收到加入公会请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnGuildJoinRequest(NetConnection<NetSession> sender, GuildJoinRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinRequest : GuildId:{0} characterId:[{1}]{2}", request.Apply.GuildId, request.Apply.characterId, request.Apply.Name);
            var guild = GuildManager.Instance.GetGuild(request.Apply.GuildId);
            if (guild == null)
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "公会不存在";
                sender.SendResponse();
                return;
            }
            request.Apply.characterId = character.Data.ID;
            request.Apply.Name = character.Data.Name;
            request.Apply.Class = character.Data.Class;
            request.Apply.Level = character.Data.Level;

            if (guild.JoinApply(request.Apply))
            {
                var leader = SessionManager.Instance.GetSession(guild.Data.LeaderID);
                if (leader != null)
                {//如果会长在线
                    leader.Session.Response.guildJoinReq = request;
                    leader.SendResponse();
                }
            }
            else
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "请勿重复申请";
                sender.SendResponse();
            }
        }

        /// <summary>
        /// 收到加入公会响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnGuildJoinResponse(NetConnection<NetSession> sender, GuildJoinResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinResponse: : GuildId:{0} characterId:[{1}]{2}", response.Apply.GuildId, response.Apply.characterId, response.Apply.Name);

            var guild = GuildManager.Instance.GetGuild(response.Apply.GuildId);
            if (response.Result == Result.Success)
            {
                guild.JoinApprove(response.Apply);
            }

            var requester = SessionManager.Instance.GetSession(response.Apply.characterId);
            if (requester != null)
            {
                requester.Session.Character.Guild = guild;

                requester.Session.Response.guildJoinRes = response;
                requester.Session.Response.guildJoinRes.Result = Result.Success;
                requester.Session.Response.guildJoinRes.Errormsg = "加入公会成功";
                requester.SendResponse();
            }
        }

        private void OnGuildLeave(NetConnection<NetSession> sender, GuildLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildLeave: : character:{0}", character.Id);

            sender.Session.Response.guildLeave = new GuildLeaveResponse();
            character.Guild.Leave(character);
            sender.Session.Response.guildLeave.Result = Result.Success;

            DBService.Instance.Save();

            sender.SendResponse();
        }

        private void OnGuildAdmin(NetConnection<NetSession> sender, GuildAdminRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildAdminCommand");
            sender.Session.Response.guildAdmin = new GuildAdminResponse();
            if (character.Guild == null)
            {
                sender.Session.Response.guildAdmin.Result = Result.Failed;
                sender.Session.Response.guildAdmin.Errormsg = "你还暂时没有自己的公会呢";
                sender.SendResponse();
                return;
            }
            character.Guild.ExecuteAdmin(request.Command, request.Target, character.Id);

            var target = SessionManager.Instance.GetSession(request.Target);
            if (target!=null)
            {
                target.Session.Response.guildAdmin = new GuildAdminResponse();
                target.Session.Response.guildAdmin.Result = Result.Success;
                target.Session.Response.guildAdmin.Command = request;
                target.SendResponse();
            }

            sender.Session.Response.guildAdmin.Result = Result.Success;
            sender.Session.Response.guildAdmin.Command = request;
        }

        private void OnGuildSetNotice(NetConnection<NetSession> sender, GuildSetNoticeRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildSetNotice: character:[{0}]{1}", character.Id, character.Name);

            sender.Session.Response.guildSetNotice = new GuildSetNoticeResponse();

            if (character.Guild == null)
            {
                sender.Session.Response.guildSetNotice.Result = Result.Failed;
                sender.Session.Response.guildSetNotice.Errormsg = "你还没有公会";
                sender.SendResponse();
                return;
            }

            string err;
            if (character.Guild.SetNotice(character, request.Notice, out err))
            {
                sender.Session.Response.guildSetNotice.Result = Result.Success;
                sender.Session.Response.guildSetNotice.Errormsg = "";
            }
            else
            {
                sender.Session.Response.guildSetNotice.Result = Result.Failed;
                sender.Session.Response.guildSetNotice.Errormsg = err;
            }

            sender.SendResponse();
        }

    }
}
