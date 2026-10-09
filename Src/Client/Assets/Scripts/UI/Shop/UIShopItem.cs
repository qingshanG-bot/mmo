using Common.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour, ISelectHandler,IPointerClickHandler
{
    public Image icon;
    public Text title;
    public Text price;
    public Text limitClass;
    public Text count;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    private bool selected;

    private float lastClickTime = 0f;  // 记录上一次点击的时间
    private float doubleClickThreshold = 0.3f;  // 双击的时间阈值，单位是秒

    public bool Selected
    {
        get { return selected; }
        set
        {
            selected = value;
            this.background.overrideSprite = selected ? selectedBg : normalBg;
        }
    }

    public int ShopItemID { get; set; }
    private UIShop shop;

    private ItemDefine item;
    private ShopItemDefine ShopItem { get; set; }

    public void SetShopItem(int id, ShopItemDefine shopItem, UIShop owner)
    {
        this.shop = owner;
        this.ShopItemID = id;
        this.ShopItem = shopItem;
        this.item = DataManager.Instance.Items[this.ShopItem.ItemID];
        
        this.title.text = this.item.Name;
        this.count.text = "x"+ ShopItem.Count.ToString();
        this.price.text = ShopItem.Price.ToString();
        this.limitClass.text = this.item.LimitClass.ToString();
        this.icon.overrideSprite = Resloader.Load<Sprite>(item.Icon);

    }

    //6.5 0:44:00

    public void OnSelect(BaseEventData eventData)
    {
        this.Selected = true;
        this.shop.SelectShopItem(this);
    }

    //增加双击购买！

    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.time;
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            this.OnDoubleClick();
        }
        else
            lastClickTime = currentTime;
    }

    private void OnDoubleClick()
    {
        Debug.Log("检测到双击，开始购买");
        this.shop.OnClickBuy();
    }
}
