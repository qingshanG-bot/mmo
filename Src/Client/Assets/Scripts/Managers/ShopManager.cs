using Common.Data;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managers
{
    class ShopManager:Singleton<ShopManager>
    {
        public void Init()
        {
            NpcManager.Instance.RegisterNpcEvent(NpcFunction.InvokeShop, OnOpenShop);
        }

        private bool OnOpenShop(NpcDefine npc)
        {
            this.ShowShop(npc.Param);
            return true;
        }

        public void ShowShop(int shopId)
        {
            ShopDefine shop;
            if (DataManager.Instance.Shops.TryGetValue(shopId,out shop))
            {
                if(shopId == 1)
                    SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Shop_Open);
                if (shopId == 2)
                    SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_EquipShopOpen);
                UIShop uiShop = UIManager.Instance.Show<UIShop>();
                if (uiShop != null)
                {
                    uiShop.SetShop(shop);
                }
            }

        }

        public bool BuyItem(int shopId, int shopItemId,int count)
        {
            ItemService.Instance.SendBuyItem(shopId, shopItemId,count);
            return true;
        }
    }
}
