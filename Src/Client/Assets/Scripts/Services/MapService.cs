using System;
using Network;
using UnityEngine;

using Common.Data;
using SkillBridge.Message;
using Models;
using Managers;
using Entities;


namespace Services
{
    class MapService : Singleton<MapService>, IDisposable
    {

        public int CurrentMapId = 0;

        private MapCharacterEnterResponse pendingEnterResponse;

        public MapService()
        {
            MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Subscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);
            MessageDistributer.Instance.Subscribe<MapEntitySyncResponse>(this.OnMapEntitySync);

            SceneManager.Instance.OnSceneLoaded += this.OnSceneLoaded;
        }

     

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Unsubscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);

            MessageDistributer.Instance.Unsubscribe<MapEntitySyncResponse>(this.OnMapEntitySync);

            SceneManager.Instance.OnSceneLoaded -= this.OnSceneLoaded;
        }

        public void Init()
        {

        }

        //private void OnMapCharacterEnter(object sender, MapCharacterEnterResponse response)
        //{
        //    Debug.LogFormat("OnMapCharacterEnter:Map:{0} Count:{1}", response.mapId, response.Characters.Count);

        //    if (CurrentMapId != response.mapId)
        //    {
        //        this.EnterMap(response.mapId);
        //        this.CurrentMapId = response.mapId;
        //    }

        //    foreach (var cha in response.Characters)
        //    {
        //        if (User.Instance.CurrentCharacterInfo == null || (User.Instance.CurrentCharacterInfo.Id == cha.Id))
        //        {//当前角色切换地图
        //            User.Instance.CurrentCharacterInfo = cha;
        //            if (User.Instance.CurrentCharacter == null)
        //                User.Instance.CurrentCharacter = new Character(cha);
        //            else
        //                User.Instance.CurrentCharacter.UpdateInfo(cha);

        //            User.Instance.CharacterInited();

        //            CharacterManager.Instance.AddCharacter(User.Instance.CurrentCharacter);
        //            continue;
        //        }
        //        CharacterManager.Instance.AddCharacter(new Character(cha));


        //    }

        //}
        private void OnMapCharacterEnter(object sender, MapCharacterEnterResponse response)
        {
            Debug.LogFormat("OnMapCharacterEnter:Map:{0} Count:{1}", response.mapId, response.Characters.Count);

            if (CurrentMapId != response.mapId)
            {
                pendingEnterResponse = response;
                EnterMap(response.mapId);
                return;
            }

            ProcessEnterResponse(response);
        }

        private void OnSceneLoaded(string sceneName)
        {
            if (pendingEnterResponse == null)
                return;

            if (!DataManager.Instance.Maps.ContainsKey(pendingEnterResponse.mapId))
            {
                Debug.LogErrorFormat("OnSceneLoaded: Map {0} not existed", pendingEnterResponse.mapId);
                pendingEnterResponse = null;
                return;
            }

            var map = DataManager.Instance.Maps[pendingEnterResponse.mapId];
            if (map.Resource != sceneName)
                return;

            CurrentMapId = pendingEnterResponse.mapId;

            var response = pendingEnterResponse;
            pendingEnterResponse = null;

            ProcessEnterResponse(response);
        }

        private void ProcessEnterResponse(MapCharacterEnterResponse response)
        {
            foreach (var cha in response.Characters)
            {
                if (IsCurrentPlayer(cha))
                {
                    ProcessCurrentCharacterEnter(cha);
                }
                else
                {
                    ProcessOtherCharacterEnter(cha);
                }
            }
        }

        private bool IsCurrentPlayer(NCharacterInfo cha)
        {
            return User.Instance.CurrentCharacterInfo == null
                || User.Instance.CurrentCharacterInfo.Id == cha.Id;
        }

        private void ProcessCurrentCharacterEnter(NCharacterInfo cha)
        {
            User.Instance.CurrentCharacterInfo = cha;

            if (User.Instance.CurrentCharacter == null)
                User.Instance.CurrentCharacter = new Character(cha);
            else
                User.Instance.CurrentCharacter.UpdateInfo(cha);

            User.Instance.CharacterInited();
            CharacterManager.Instance.AddCharacter(User.Instance.CurrentCharacter);
        }

        private void ProcessOtherCharacterEnter(NCharacterInfo cha)
        {
            CharacterManager.Instance.AddCharacter(new Character(cha));
        }

        private void OnMapCharacterLeave(object sender, MapCharacterLeaveResponse response)
        {
            Debug.LogFormat("OnMapCharacterLeave:CharID:{0}",response.entityId);
            if (response.entityId != User.Instance.CurrentCharacterInfo.EntityId)
            {
                CharacterManager.Instance.RemoveCharacter(response.entityId);
            }
            else
            {
                BattleManager.Instance.ClearTarget();   // 当前玩家离开地图时清目标
                CharacterManager.Instance.Clear();
            }
        }

        

        private void EnterMap(int mapId)
        {
            BattleManager.Instance.ClearTarget();

            if (DataManager.Instance.Maps.ContainsKey(mapId))
            {
                MapDefine map = DataManager.Instance.Maps[mapId];
                User.Instance.CurrentMapData = map;//获取当前地图，方便小地图
                SceneManager.Instance.LoadScene(map.Resource);

                SoundManager.Instance.PlayMusic(map.Music);
            }
            else
                Debug.LogErrorFormat("EnterMap: Map {0} not existed", mapId);
        }

        public void SendMapEntitySync(EntityEvent entityEvent,NEntity entity,int param)
        {
            //Debug.LogFormat("MapEntityUpdateRequest:ID:{0} POS:{1} DIR:{2} SPD{3}",entity.Id,entity.Position.String(),entity.Direction.String(),entity.Speed);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.mapEntitySync = new MapEntitySyncRequest();
            message.Request.mapEntitySync.entitySync = new NEntitySync()
            {
                Id = entity.Id,
                Event = entityEvent,
                Entity = entity,
                Param = param,
                
            };
            NetClient.Instance.SendMessage(message);
        }
        private void OnMapEntitySync(object sender, MapEntitySyncResponse response)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("MapEntityUpdateResponse: Entitys:{0}", response.entitySyncs.Count);
            sb.AppendLine();
            foreach (var entity in response.entitySyncs)
            {
                Managers.EntityManager.Instance.OnEntitySync(entity);
                sb.AppendFormat("    [{0}]evt:{1} entity:{2}", entity.Id, entity.Event, entity.Entity.String());
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        internal void SendMapTeleport(int teleporterID)
        {
            Debug.LogFormat("MapTeleportRequest :teleporterID:{0}",teleporterID);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.mapTeleport = new MapTeleportRequest();
            message.Request.mapTeleport.teleporterId = teleporterID;
            NetClient.Instance.SendMessage(message);
        }
    }
}
