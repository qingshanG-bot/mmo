using Common;
using GameServer.Entities;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class EquipManager:Singleton<EquipManager>
    {
        public Result EquipItem(NetConnection<NetSession> sender, int slot, int itemId, bool isEquip)
        {
            Character character = sender.Session.Character;
            if (!character.ItemManager.Items.ContainsKey(itemId))
                return Result.Failed;

            character.Data.Equips =  UpdateEquip(character.Data.Equips, slot, itemId, isEquip);

            DBService.Instance.Save();
            return Result.Success;
        }

        unsafe byte[] UpdateEquip(byte[] equipDate, int slot,int itemId,bool isEquip)
        {
            byte[] EquipData = new byte[28];
            fixed (byte* pt = equipDate)
            {
                int* slotid = (int*)(pt + slot * sizeof(int));

                if (isEquip)
                    *slotid = itemId;
                else
                    *slotid = 0;
            }

            Array.Copy(equipDate, EquipData, 28);
            return EquipData;
        }
    }
}
