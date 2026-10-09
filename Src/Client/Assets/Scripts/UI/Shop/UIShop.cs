using Common.Data;
using Managers;
using Models;
using SkillBridge.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIWindow {

    public TMP_Text money;
    public Text title;

    public GameObject ShopItem;

    ShopDefine shop;
    public Transform[] itemRoot;

    void Start() {

        StartCoroutine(InitItems());
    }

    //更新金币
    void OnEnable()
    {
        User.Instance.OnGoldChanged += UpdateMoney;
    }

    void OnDisable()
    {
        if (User.Instance != null)
            User.Instance.OnGoldChanged -= UpdateMoney;
    }
    private void UpdateMoney(long newGold)
    {
        this.money.text = newGold.ToString();
    }


    IEnumerator InitItems()
    {
        int count = 0;
        int page = 0;
        foreach (var kv in DataManager.Instance.ShopItems[shop.ID])
        {
            if (kv.Value.Status > 0)
            {
                GameObject go = Instantiate(ShopItem, itemRoot[page]);
                UIShopItem ui = go.GetComponent<UIShopItem>();
                ui.SetShopItem(kv.Key, kv.Value, this);
                count++;
                if (count >= 10)
                {
                    count = 0;
                    page++;
                    itemRoot[page].gameObject.SetActive(true);//分页
                }
            }
        }
        yield return null;
    }

    public void SetShop(ShopDefine shop)
    {
        this.shop = shop;
        this.title.text = shop.Name;
        this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();
    }

    //6.5 00:48:00
    private UIShopItem selectedItem;
    public void SelectShopItem(UIShopItem item)
    {
        if (selectedItem != null)
        {
            selectedItem.Selected = false;
        }
        selectedItem = item;

    }

    public void OnClickBuy()
    {
        if (this.selectedItem == null)
        {
            MessageBox.Show("请选择也要购买的道具", "购买提示");
            return;
        }

        var shopItemDef = DataManager.Instance.ShopItems[this.shop.ID][this.selectedItem.ShopItemID];
        var itemDef = DataManager.Instance.Items[shopItemDef.ItemID];

        var input = InputBox.Show(
            $"请输入购买次数\n(每次获得 {itemDef.Name} x{shopItemDef.Count}，单次价格 {shopItemDef.Price})",
            "购买道具",
            "下一步",
            "取消",
            "次数不能为空"
        );

        input.OnSubmit += (string text, out string tips) =>
        {
            tips = "";
            if (!int.TryParse(text, out int times) || times <= 0)
            {
                tips = "请输入正确的次数";
                return false;
            }

            long totalPrice = (long)shopItemDef.Price * (long)times;
            long myGold = User.Instance.CurrentCharacterInfo.Gold;
            if (myGold < totalPrice)
            {
                tips = $"金币不足，需要 {totalPrice}，当前 {myGold}";
                return false;
            }

            int totalGive = shopItemDef.Count * times;

            var msg = MessageBox.Show(
                $"确定购买 {itemDef.Name} x{totalGive}？\n购买次数：{times}\n总价：{totalPrice}",
                "确认购买",
                MessageBoxType.Confirm,
                "购买",
                "取消"
            );

            msg.OnYes = () =>
            {
                if(itemDef.Type == ItemType.Equip)
                    SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Buy_Equip);
                else if(itemDef.Type == ItemType.Ride)
                    SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Buy_Ride);
                else
                    SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Buy_Food);
                ShopManager.Instance.BuyItem(this.shop.ID, this.selectedItem.ShopItemID, times);
            };

            return true;
        };
    }



}