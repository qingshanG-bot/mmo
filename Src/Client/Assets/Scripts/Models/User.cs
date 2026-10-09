using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Data;
using Entities;
using SkillBridge.Message;
using UnityEngine;

namespace Models
{
    class User : Singleton<User>
    {
        NUserInfo userInfo;


        public NUserInfo Info
        {
            get { return userInfo; }
        }


        public void SetupUserInfo(SkillBridge.Message.NUserInfo info)
        {
            this.userInfo = info;
        }


        public MapDefine CurrentMapData { get; set; }

        public NCharacterInfo CurrentCharacterInfo { get; set; }

        public Character CurrentCharacter { get; set; }

        public PlayerInputController CurrentCharacterObject { get; set; }

        public NTeamInfo TeamInfo { get; set; }

        public event Action<long> OnGoldChanged;
        public event Action<long> OnExpChanged;
        public event Action<int> OnLevelChanged;


        public void AddGold(int gold)
        {
            var newGold = this.CurrentCharacterInfo.Gold += gold;
            if (OnGoldChanged != null)
                OnGoldChanged(newGold);
            
        }

        public void AddExp(long exp)
        {
            if (this.CurrentCharacterInfo == null)
                return;

            var newExp = this.CurrentCharacterInfo.Exp += exp;
            if (OnExpChanged != null)
                OnExpChanged(newExp);
        }

        public void SetLevel(int level)
        {
            if (this.CurrentCharacterInfo == null)
                return;

            this.CurrentCharacterInfo.Level = level;

            if (this.CurrentCharacter != null)
            {
                this.CurrentCharacter.UpdateLevel(level);
            }

            OnLevelChanged?.Invoke(level);
        }

        public int CurrentRide = 0;
        public void Ride(int id)
        {
            if (CurrentRide != id)
            {
                CurrentRide = id;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, CurrentRide);
            }

            else
            {
                CurrentRide = 0;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, 0);
            }
        }

        public delegate void CharacterInitHandle();
        public event CharacterInitHandle OnCharacterInit;

        public void CharacterInited()
        {
            if (OnCharacterInit!=null)
            {
                OnCharacterInit();
            }
        }
    }
}
