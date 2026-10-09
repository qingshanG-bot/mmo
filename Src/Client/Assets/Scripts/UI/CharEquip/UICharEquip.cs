using Common.Battle;
using Managers;
using Models;
using SkillBridge.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICharEquip : UIWindow
{
    public Image icon;
    public Text title;
    public Text money;

    public GameObject itemPrefab;
    public GameObject itemEquipedPrefab;

    public Transform itemListRoot;

    public List<Transform> slots;

    public Text avatarName;
    public TMP_Text hp;
    public Slider hpBar;

    public TMP_Text mp;
    public Slider mpBar;

    public Text[] attrs;
	void Start () {
        RefreshUI();
        EquipManager.Instance.OnEquipChanged += RefreshUI;

        User.Instance.OnLevelChanged += OnLevelChanged;
        if (User.Instance.CurrentCharacter != null)
        {
            User.Instance.CurrentCharacter.Attributes.OnAttributeChanged += OnAttributeChanged;
        }
    }
    private void OnDestroy()
    {
        EquipManager.Instance.OnEquipChanged -= RefreshUI;

        User.Instance.OnLevelChanged -= OnLevelChanged;
        if (User.Instance.CurrentCharacter != null)
        {
            User.Instance.CurrentCharacter.Attributes.OnAttributeChanged -= OnAttributeChanged;
        }
    }
    private void OnLevelChanged(int level)
    {
        this.avatarName.text = User.Instance.CurrentCharacterInfo.Name + " Lv." + level;
    }

    private void OnAttributeChanged()
    {
        InitAttributes();
    }

    void RefreshUI()
    {
        ClearAllEquipList();
        InitAllEquipItems();
        ClearEquipedList();
        InitEquipedItems();
        this.icon.overrideSprite = SpriteManager.Instance.classIcons[(int)User.Instance.CurrentCharacterInfo.Class +2];
        this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();
        this.avatarName.text = User.Instance.CurrentCharacterInfo.Name + " Lv." + User.Instance.CurrentCharacterInfo.Level;

        InitAttributes();
    }

    
    void InitAllEquipItems()
    {
        foreach (var kv in ItemManager.Instance.Items)
        {
            //筛选类型
            if (kv.Value.Define.Type == ItemType.Equip && kv.Value.Define.LimitClass == User.Instance.CurrentCharacterInfo.Class)
            {
                //已经装备的不显示
                if (EquipManager.Instance.Contains(kv.Key))
                    continue;
                GameObject go = Instantiate(itemPrefab, itemListRoot);
                UIEquipItem ui = go.GetComponent<UIEquipItem>();
                ui.SetEquipItem(kv.Key, kv.Value, this, false);
            }
        }
    }

    void ClearAllEquipList()
    {
        foreach (var item in itemListRoot.GetComponentsInChildren<UIEquipItem>())
        {
            Destroy(item.gameObject);
        }
    }

    void ClearEquipedList()
    {
        foreach (var item in slots)
        {
            if (item.childCount > 0)
                Destroy(item.GetChild(0).gameObject);
        }
    }


    void InitEquipedItems()
    {
        for (int i = 0; i < (int)EquipSlot.SlotMax; i++)
        {
            var item = EquipManager.Instance.Equips[i];
            {
                if (item!=null)
                {
                    GameObject go = Instantiate(itemEquipedPrefab, slots[i]);
                    UIEquipItem ui = go.GetComponent<UIEquipItem>();
                    ui.SetEquipItem(i, item, this, true);
                }
            }
        }
    }

    public void DoEquip(Item item)
    {
        EquipManager.Instance.EquipItem(item);
    }

    public void UnEquip(Item item)
    {
        EquipManager.Instance.UnEquipItem(item);
    }

    private void InitAttributes()
    {
        var charattr = User.Instance.CurrentCharacter.Attributes;
        this.hp.text = string.Format("{0}/{1}", charattr.HP, charattr.MaxHP);
        this.mp.text = string.Format("{0}/{1}", charattr.MP, charattr.MaxMP);

        this.hpBar.maxValue = charattr.MaxHP;
        this.hpBar.value = charattr.HP;

        this.mpBar.maxValue = charattr.MaxMP;
        this.mpBar.value = charattr.MP;

        for (int i = (int)AttributeType.STR; i < (int)AttributeType.MAX; i++)
        {
            if (i == (int)AttributeType.CRI)
                this.attrs[i - 2].text = string.Format("{0:f2}%", charattr.Final.Data[i] * 100);
            else
                this.attrs[i - 2].text = ((int)charattr.Final.Data[i]).ToString();
        }
    }

}
