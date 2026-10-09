using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;

namespace GameServer.Services
{
    class UserService : Singleton<UserService>
    {

        public UserService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnCreateCharacter);

            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameEnterRequest>(this.OnGameEnter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameLeaveRequest>(this.OnGameLeave);

            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserDeleteCharacterRequest>(this.OnDeleteCharacter);
        }



        public void Init()
        {

        }

        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)
        {
            Log.InfoFormat("UserLoginRequest: User:{0}  Pass:{1}", request.User, request.Passward);


            sender.Session.Response.userLogin = new UserLoginResponse();
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            //message.Response.userLogin = new UserLoginResponse();


            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user == null)
            {
                sender.Session.Response.userLogin.Result = Result.Failed;
                sender.Session.Response.userLogin.Errormsg = "用户不存在";
            }
            else if (user.Password != request.Passward)
            {
                sender.Session.Response.userLogin.Result = Result.Failed;
                sender.Session.Response.userLogin.Errormsg = "密码错误";
               
            }
            else
            {
                //!!!
                sender.Session.User = user;

                sender.Session.Response.userLogin.Result = Result.Success;
                sender.Session.Response.userLogin.Errormsg = "None";
                sender.Session.Response.userLogin.Userinfo = new NUserInfo();
                sender.Session.Response.userLogin.Userinfo.Id = (int)user.ID;
                sender.Session.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                sender.Session.Response.userLogin.Userinfo.Player.Id = user.Player.ID;

                foreach (var c in user.Player.Characters)
                {
                    NCharacterInfo info = new NCharacterInfo();
                    info.Id = c.ID;
                    info.Name = c.Name;
                    info.Type = CharacterType.Player;
                    info.Class = (CharacterClass)c.Class;
                    info.Level = c.Level;
                    info.ConfigId = c.ID;
                    sender.Session.Response.userLogin.Userinfo.Player.Characters.Add(info);
                }

            }
            sender.SendResponse();
            
        }

        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)
        {
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            sender.Session.Response.userRegister = new UserRegisterResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {
                sender.Session.Response.userRegister.Result = Result.Failed;
                sender.Session.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                TPlayer player = DBService.Instance.Entities.Players.Add(new TPlayer());
                DBService.Instance.Entities.Users.Add(new TUser() { Username = request.User, Password = request.Passward, Player = player });
                DBService.Instance.Entities.SaveChanges();
                sender.Session.Response.userRegister.Result = Result.Success;
                sender.Session.Response.userRegister.Errormsg = "None";
            }

            sender.SendResponse();
        }

        void OnCreateCharacter(NetConnection<NetSession> sender, UserCreateCharacterRequest request)
        {
            Log.InfoFormat("UserCreateCharacterRequest: Name:{0}  Class:{1}", request.Name, request.Class);

            TCharacter character = new TCharacter()
            {
                Name = request.Name,
                Class = (int)request.Class,
                TID = (int)request.Class,
                Level = 1,
                MapID = 1,
                MapPosX = 14160, //初始出生位置X
                MapPosY = 8242, //初始出生位置Y
                MapPosZ = 1032,
                Gold = 10000,
                HP = 100000000,
                MP = 100000000,
                Equips = new byte[28]
            };

            //背包加入
            var bag = new TCharacterBag();
            bag.Owner = character;
            bag.Items = new byte[0];
            bag.Unlocked = 40;
            character.Bag = DBService.Instance.Entities.CharacterBags.Add(bag);

            character = DBService.Instance.Entities.Characters.Add(character);

            //添加俩个道具
            character.Items.Add(new TCharacterItem()
            {
                Owner = character,
                ItemID = 1,
                ItemCount = 20,
            });

            character.Items.Add(new TCharacterItem()
            {
                Owner = character,
                ItemID = 2,
                ItemCount = 20,
            });

            //Session!!!
            sender.Session.User.Player.Characters.Add(character);

            DBService.Instance.Entities.SaveChanges();

            sender.Session.Response.createChar = new UserCreateCharacterResponse();

            sender.Session.Response.createChar.Result = Result.Success;
            sender.Session.Response.createChar.Errormsg = "None";

            foreach (var c in sender.Session.User.Player.Characters)
            {
                NCharacterInfo info = new NCharacterInfo();
                info.Id = c.ID;
                info.Name = c.Name;
                info.Type = CharacterType.Player;
                info.Class = (CharacterClass)c.Class;
                info.Level = c.Level;
                info.ConfigId = c.TID;
                sender.Session.Response.createChar.Characters.Add(info);
            }

            sender.SendResponse();
        }

        void OnGameEnter(NetConnection<NetSession> sender, UserGameEnterRequest request)
        {
            TCharacter dbchar = sender.Session.User.Player.Characters.ElementAt(request.characterIdx);
            Log.InfoFormat("UserGameEnterRequest: characterID:{0}:{1} Map:{2}", dbchar.ID, dbchar.Name, dbchar.MapID);

            Character character = CharacterManager.Instance.AddCharacter(dbchar, sender.Session);


            SessionManager.Instance.AddSession(character.Id, sender);

            sender.Session.Response.gameEnter = new UserGameEnterResponse();

            sender.Session.Response.gameEnter.Result = Result.Success;
            sender.Session.Response.gameEnter.Errormsg = "None";

            //游戏进入时，传给客户端角色上的所有信息
            sender.Session.Response.gameEnter.Character = character.Info;

            sender.Session.Character = character;

            sender.Session.PostResponser = character;

            sender.SendResponse();

            MapManager.Instance[dbchar.MapID].CharacterEnter(sender, character);


            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            //message.Response.gameEnter = new UserGameEnterResponse();
            //message.Response.gameEnter.Result = Result.Success;
            //message.Response.gameEnter.Errormsg = "None";
            //进入成功，发送角色信息
            //message.Response.gameEnter.Character = character.Info;

            ////道具系统测试
            //int itemId = 1;
            //bool hasItem = character.ItemManager.HasItem(itemId);
            //Log.InfoFormat("HasItem:[{0}]{1}", itemId, hasItem);
            //if (hasItem)
            //{
            //    //character.ItemManager.RemoveItem(itemId, 1);
            //}
            //else
            //{
            //    character.ItemManager.AddItem(1, 200);
            //    character.ItemManager.AddItem(2, 100);
            //    character.ItemManager.AddItem(3, 30);
            //    character.ItemManager.AddItem(4, 120);
            //}
            //Models.Item item = character.ItemManager.GetItem(itemId);

            //Log.InfoFormat("Item:[{0}][{1}]",itemId,item);
            //DBService.Instance.Save();

        }

        void OnGameLeave(NetConnection<NetSession> sender, UserGameLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("UserGameLeaveRequest: characterID:{0}:{1} Map:{2}", character.Id, character.Info.Name, character.Info.mapId);

            //SessionManager.Instance.RemoveSession(character.Id);
            //BUG反复退回登录界面角色会出问题

            this.CharacterLeave(character);

            sender.Session.Response.gameLeave = new UserGameLeaveResponse();

            sender.Session.Response.gameLeave.Result = Result.Success;
            sender.Session.Response.gameLeave.Errormsg = "None";

            sender.SendResponse();

            sender.Session.Character = null;
            sender.Session.PostResponser = null;

        }

        public void CharacterLeave(Character character)
        {
            Log.InfoFormat("CharacterLeave:   characterID:{0}:{1}", character.Id, character.Info.Name);

            SessionManager.Instance.RemoveSession(character.Id);

            CharacterManager.Instance.RemoveCharacter(character.Id);

            character.Clear();
            MapManager.Instance[character.Info.mapId].CharacterLeave(character);
        }

        private void OnDeleteCharacter(NetConnection<NetSession> sender, UserDeleteCharacterRequest request)
        {
            sender.Session.Response.deleteChar = new UserDeleteCharacterResponse();

            var character = DBService.Instance.Entities.Characters.FirstOrDefault(c => c.ID == request.Id && c.Player.ID == sender.Session.User.Player.ID);
            if (character == null)
            {
                sender.Session.Response.deleteChar.Result = Result.Failed;
                sender.Session.Response.deleteChar.Errormsg = "角色不存在或者错误！";
                sender.SendResponse();
                return;
            }

            Log.InfoFormat("Deleting character [{0}] from player [{1}]", character.Name, sender.Session.User.Player.ID);


            DBService.Instance.Entities.CharacterItems.RemoveRange(DBService.Instance.Entities.CharacterItems.Where(i => i.CharacterID == character.ID));
            DBService.Instance.Entities.CharacterQuests.RemoveRange(DBService.Instance.Entities.CharacterQuests.Where(q => q.TCharacterID == character.ID));
            DBService.Instance.Entities.CharacterFriends.RemoveRange(DBService.Instance.Entities.CharacterFriends.Where(f => f.CharacterID == character.ID || f.FriendID == character.ID));
            DBService.Instance.Entities.GuildMembers.RemoveRange(DBService.Instance.Entities.GuildMembers.Where(m => m.CharacterId == character.ID));
            DBService.Instance.Entities.GuildApplies.RemoveRange(DBService.Instance.Entities.GuildApplies.Where(a => a.CharacterId == character.ID));


            DBService.Instance.Entities.Entry(character).Reference(c => c.Bag).Load();
            DBService.Instance.Entities.CharacterBags.Remove(character.Bag);


            DBService.Instance.Entities.Characters.Remove(character);


            DBService.Instance.Entities.SaveChanges();


            var remain = DBService.Instance.Entities.Characters
            .Where(c => c.Player.ID == sender.Session.User.Player.ID)
            .Select(c => new { c.ID, c.Name, c.Class , c.TID ,c.Level})
            .ToList();

            foreach (var c in remain)
            {
                var info = new NCharacterInfo
                {
                    Id = c.ID,
                    Name = c.Name,
                    Level = c.Level,
                    Type = CharacterType.Player,
                    Class = (CharacterClass)c.Class,
                    ConfigId = c.TID
                };
                sender.Session.Response.deleteChar.Characters.Add(info);
            }

            sender.Session.Response.deleteChar.Result = Result.Success;
            sender.Session.Response.deleteChar.Errormsg = $"已删除角色：{character.Name}";

            sender.SendResponse();
        }

    }
}
