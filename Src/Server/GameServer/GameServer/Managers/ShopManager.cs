using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Data;
using GameServer.Services;
using Network;
using SkillBridge.Message;

namespace GameServer.Managers
{
    class ShopManager:Singleton<ShopManager>
    {
        public Result BuyItem(NetConnection<NetSession> sender, int shopId, int shopItemId, int count)
        {
            if (count <= 0) count = 1;
            if (count > 999) return Result.Failed;

            if (!DataManager.Instance.Shops.ContainsKey(shopId))
                return Result.Failed;

            ShopItemDefine shopItem;
            if (DataManager.Instance.ShopItems[shopId].TryGetValue(shopItemId,out shopItem))
            {
                long totalPrice = (long)shopItem.Price * (long)count;
                int totalItemCount = shopItem.Count * count;

                Log.InfoFormat("BuyItem: :character:{0}:Item:{1} Count:{2} Price:{3}",sender.Session.Character.Id, shopItem.ItemID, shopItem.Count, shopItem.Price);
                if (sender.Session.Character.Gold >= totalPrice)
                {
                    sender.Session.Character.ItemManager.AddItem(shopItem.ItemID, totalItemCount);
                    sender.Session.Character.Gold -= totalPrice;

                    DBService.Instance.Save();
                    return Result.Success;
                }
            }
            return Result.Failed;
        }

        public Result SellItem(NetConnection<NetSession> sender, int itemId, int count)
        {
            if (count <= 0) return Result.Failed;

            var character = sender.Session.Character;

            // 1) 校验物品定义存在
            ItemDefine def;
            if (!DataManager.Instance.Items.TryGetValue(itemId, out def))
                return Result.Failed;

            if (def.Type == ItemType.Task)
                return Result.Failed;

            // 3) 校验数量足够
            var item = character.ItemManager.GetItem(itemId);
            if (item == null || item.Count < count)
                return Result.Failed;

            // 4) 计算售价
            if (def.SellPrice <= 0)
                return Result.Failed;

            int gainGold = def.SellPrice * count;

            // 5) 扣道具（内部会发 ITEM Delete 状态）
            if (!character.ItemManager.RemoveItem(itemId, count))
                return Result.Failed;

            // 6) 加金币 + 发 MONEY Add 状态（delta）
            character.Gold += gainGold;

            DBService.Instance.Save();

            Log.InfoFormat("SellItem: character:{0} Item:{1} x{2} gainGold:{3}", character.Id, itemId, count, gainGold);
            return Result.Success;
        }

    }
}
