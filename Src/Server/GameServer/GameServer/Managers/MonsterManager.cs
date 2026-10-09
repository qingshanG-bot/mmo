using Common;
using GameServer.Entities;
using GameServer.Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class MonsterManager
    {
        private Map map;

        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();

        public void Init(Map map)
        {
            this.map = map;
        }

        public Monster Create(int spawnMonID, int spawnLeavl, NVector3 position, NVector3 direction)
        {
            Monster monster = new Monster(spawnMonID, spawnLeavl, position, direction);
            EntityManager.Instance.AddEntity(this.map.ID, monster);
            //monster.Id = monster.entityId;
            monster.Info.EntityId = monster.entityId;
            monster.Info.mapId = this.map.ID;

            //id-->entityId
            Monsters[monster.entityId] = monster;

            Log.InfoFormat("MonsterCreate: Map:{0} entityId:{1} tid:{2}",
            this.map.ID, monster.entityId, spawnMonID);

            Log.InfoFormat("[Monster] map={0} entityId={1} info.EntityId={2}",
            this.map.ID, monster.entityId, monster.Info.EntityId);


            this.map.MonsterEnter(monster);
            return monster;

        }
    }
}
