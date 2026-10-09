using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class ItemService : Singleton<ItemService>
    {
        public ItemService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemBuyRequest>(this.OnItemBuy);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemEquipRequest>(this.OnItemEquip);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemSellRequest>(this.OnItemSell);

        }

        public void Init()
        {

        }
        private void OnItemBuy(NetConnection<NetSession> sender, ItemBuyRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnItemBuy: :character:{0}:Shop:{1} ShopItem:{2}", character.Id, request.shopId, request.shopItemId);
            var result = ShopManager.Instance.BuyItem(sender, request.shopId, request.shopItemId, request.Count);
            sender.Session.Response.itemBuy = new ItemBuyResponse();
            sender.Session.Response.itemBuy.Result = result;
            sender.SendResponse();
        }

        private void OnItemEquip(NetConnection<NetSession> sender, ItemEquipRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnItemEquip: :character:{0}:Slot:{1} Item:{2} Equip:{3}",character.Id, request.Slot, request.itemId, request.isEquip);
            var result = EquipManager.Instance.EquipItem(sender, request.Slot, request.itemId, request.isEquip);
            sender.Session.Response.itemEquip = new ItemEquipResponse();
            sender.Session.Response.itemEquip.Result = result;
            sender.SendResponse();
        }

        private void OnItemSell(NetConnection<NetSession> sender, ItemSellRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnItemSell: character:{0} Item:{1} Count:{2}", character.Id, request.itemId, request.Count);

            var result = ShopManager.Instance.SellItem(sender, request.itemId, request.Count);

            sender.Session.Response.itemSell = new ItemSellResponse();
            sender.Session.Response.itemSell.Result = result;
            sender.Session.Response.itemSell.Errormsg = result == Result.Success ? "" : "出售失败";
            sender.SendResponse();
        }

    }
}
