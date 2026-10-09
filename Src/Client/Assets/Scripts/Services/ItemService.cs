using Managers;
using Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Services
{
    class ItemService : Singleton<ItemService>, IDisposable
    {
        public ItemService()
        {
            MessageDistributer.Instance.Subscribe<ItemBuyResponse>(this.OnItemBuy);
            MessageDistributer.Instance.Subscribe<ItemEquipResponse>(this.OnItemEquip);
            MessageDistributer.Instance.Subscribe<ItemSellResponse>(this.OnItemSell);

        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<ItemBuyResponse>(this.OnItemBuy);
            MessageDistributer.Instance.Unsubscribe<ItemEquipResponse>(this.OnItemEquip);
            MessageDistributer.Instance.Unsubscribe<ItemSellResponse>(this.OnItemSell);

        }

        public void SendBuyItem(int shopId, int shopItemId, int count)
        {
            Debug.Log("SendBuyItem");

            if (count <= 0) count = 1;

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemBuy = new ItemBuyRequest();
            message.Request.itemBuy.shopId = shopId;
            message.Request.itemBuy.shopItemId = shopItemId;
            message.Request.itemBuy.Count = count;
            NetClient.Instance.SendMessage(message);
        }
        private void OnItemBuy(object sender, ItemBuyResponse message)
        {
            MessageBox.Show("购买结果：" + message.Result + "\n" + message.Errormsg, "购买完成");
        }


        Item pendingEquip = null;
        bool isEquip;
        public bool SendEquipItem(Item equip, bool isEquip)
        {
            if (pendingEquip != null)
                return false;
            Debug.Log("SendEquipItem");

            pendingEquip = equip;
            this.isEquip = isEquip;

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemEquip = new ItemEquipRequest();
            message.Request.itemEquip.Slot = (int)equip.EquipInfo.Slot;
            message.Request.itemEquip.itemId = equip.Id;
            message.Request.itemEquip.isEquip = isEquip;
            NetClient.Instance.SendMessage(message);
            return true;
        }

        private void OnItemEquip(object sender, ItemEquipResponse message)
        {
            if (message.Result == Result.Success)
            {
                if (pendingEquip != null)
                {
                    if (this.isEquip)
                        EquipManager.Instance.OnEquipItem(pendingEquip);
                    else
                        EquipManager.Instance.OnUnEquipItem(pendingEquip.EquipInfo.Slot);
                    pendingEquip = null;
                }
            }
        }

        public void SendSellItem(int itemId, int count)
        {
            Debug.Log("SendSellItem");

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemSell = new ItemSellRequest();
            message.Request.itemSell.itemId = itemId;
            message.Request.itemSell.Count = count;
            NetClient.Instance.SendMessage(message);
        }

        private void OnItemSell(object sender, ItemSellResponse message)
        {
            if (message.Result == Result.Success)
            {
                MessageBox.Show("出售成功", "出售完成");
            }
            else
            {
                MessageBox.Show("出售失败：" + message.Result + "\n" + message.Errormsg, "出售失败", MessageBoxType.Error);
            }
        }

    }
}
