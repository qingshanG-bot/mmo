using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIconItem : ListView.ListViewItem
{

    public Image mainImage;
    public Image secondImage;

    public Text mainText;

    public Image background;     
    public Sprite normalBg;
    public Sprite selectedBg;

    public int index;              // 背包下标
    public int itemId;             // 道具ID
    public int count;


    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

    public void SetMainIcon(string iconName, string text)
    {
        this.mainImage.overrideSprite = Resloader.Load<Sprite>(iconName);
        this.mainText.text = text;
    }

    public void BindData(int index, int itemId, int count)
    {
        this.index = index;
        this.itemId = itemId;
        this.count = count;
    }

    public GameObject equippedTag; 

    public void SetEquipped(bool equipped)
    {
        if (equippedTag == null) return;
        equippedTag.gameObject.SetActive(equipped);
    }
}
