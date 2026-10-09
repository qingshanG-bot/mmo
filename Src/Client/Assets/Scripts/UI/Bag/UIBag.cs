using Common.Data;
using Managers;
using Models;
using Services;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//6.4 1:40:00
public class UIBag : UIWindow
{
    public TMP_Text money;

    public Transform[] pages;

    public GameObject bagItem;

    List<Image> slots;

    public ListView listMain;
    public UIItemInfoPanel itemInfoPanel;
    private UIIconItem selectedItem;

    public TabView tabView;

    void Start()
    {
        if (slots == null)
        {
            slots = new List<Image>();
            for (int page = 0; page < this.pages.Length; page++)
            {
                slots.AddRange(this.pages[page].GetComponentsInChildren<Image>(true));
            }
        }

        listMain.onItemSelected += OnBagItemSelected;

        tabView.OnTabSelect += OnTabChanged;

        StartCoroutine(InitBags());

    }

    private StatusService.StatusNotifyHandler itemHandler;

    void OnEnable()
    {
        if (itemHandler == null) itemHandler = OnItemStatus;

        StatusService.Instance.RegisterStatusNofity(StatusType.Item, itemHandler);

        User.Instance.OnGoldChanged += UpdateMoney;

        EquipManager.Instance.OnEquipChanged += RefreshBag;
    }

    void OnDisable()
    {
        if (StatusService.Instance != null && itemHandler != null)
            StatusService.Instance.UnregisterStatusNotify(StatusType.Item, itemHandler);

        if (User.Instance != null)
            User.Instance.OnGoldChanged -= UpdateMoney;

        if (EquipManager.Instance != null)
            EquipManager.Instance.OnEquipChanged -= RefreshBag;
    }

    private bool OnItemStatus(NStatus status)
    {
        if (!this || !this.gameObject) return true; // 保险
        RefreshBag();
        return true;
    }


    private void UpdateMoney(long newGold)
    {
        this.money.text = newGold.ToString();
    }


    // *** 修改：Tab 改变时，只负责通知 BagManager + 刷 UI
    void OnTabChanged(int index)
    {
        BagManager.Instance.SetFilterByTabIndex(index);
        RefreshBag();
    }
    IEnumerator InitBags()
    {
        RefreshBag();
        yield return null;
    }

    // *** 修改：真正的 UI 刷新只看 BagManager 提供的数据
    void RefreshBag()
    {
        if (itemInfoPanel != null) itemInfoPanel.Hide();

        Clear();
        if (listMain != null) listMain.RemoveAll();

        int slotIndex = 0;

        foreach (var tuple in BagManager.Instance.GetDisplayItems())
        {
            var bagIndex = tuple.bagIndex;
            var bagItemData = tuple.bagItem;
            var def = tuple.define;

            if (slotIndex >= slots.Count)
                break;

            GameObject go = Instantiate(bagItem, slots[slotIndex].transform);
            var ui = go.GetComponent<UIIconItem>();
            ui.SetMainIcon(def.Icon, bagItemData.Count.ToString());
            ui.BindData(bagIndex, bagItemData.ItemId, bagItemData.Count);

            ui.SetEquipped(EquipManager.Instance.Contains(bagItemData.ItemId));


            if (listMain != null) listMain.AddItem(ui);

            slotIndex++;
        }

        for (int i = BagManager.Instance.Items.Length; i < slots.Count; i++)
        {
            slots[i].color = Color.gray;
        }

        this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();

        this.selectedItem = null;

        // 刷新结束后确保面板是隐藏状态
        if (itemInfoPanel != null) itemInfoPanel.Hide();

    }

    //IEnumerator InitBags()
    //{
    //    for (int i = 0; i < BagManager.Instance.Items.Length; i++)
    //    {

    //        var item = BagManager.Instance.Items[i];
    //        if (item.ItemId > 0)
    //        {
    //            GameObject go = Instantiate(bagItem, slots[i].transform);
    //            var ui = go.GetComponent<UIIconItem>();
    //            var def = ItemManager.Instance.Items[item.ItemId].Define;
    //            ui.SetMainIcon(def.Icon, item.Count.ToString());

    //            ui.BindData(i, item.ItemId, item.Count);
    //            if (listMain != null) listMain.AddItem(ui);
    //        }
    //    }
    //    for (int i = BagManager.Instance.Items.Length; i < slots.Count; i++)
    //    {
    //        slots[i].color = Color.gray;
    //    }
    //    this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();
    //    yield return null;
    //}

    void Clear()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].transform.childCount>0)
            {
                Destroy(slots[i].transform.GetChild(0).gameObject);
            }
        }
    }
    public void OnReset()
    {
        BagManager.Instance.Reset();
        this.Clear();
        if (listMain != null) listMain.RemoveAll();
        StartCoroutine(InitBags());
       
    }

    void OnBagItemSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UIIconItem;
        if (itemInfoPanel == null)
            return;

        if (this.selectedItem == null || this.selectedItem.itemId <= 0 || this.selectedItem.count <= 0)
        {
            itemInfoPanel.Hide();
            return;
        }

        if (!ItemManager.Instance.Items.ContainsKey(this.selectedItem.itemId))
        {
            itemInfoPanel.Hide();
            return;
        }

        var def = ItemManager.Instance.Items[this.selectedItem.itemId].Define as ItemDefine;
        if (def == null)
        {
            itemInfoPanel.Hide();
            return;
        }

        itemInfoPanel.Show(def, this.selectedItem.count);
    }


    public void OnClickSell()
    {
        if (this.selectedItem == null)
        {
            MessageBox.Show("请选择要出售的道具", "出售提示");
            return;
        }

        int itemId = this.selectedItem.itemId;
        int owned = this.selectedItem.count;

        var def = ItemManager.Instance.Items[itemId].Define as ItemDefine;
        if (def.Type == ItemType.Task)
        {
            MessageBox.Show("任务道具不能出售", "出售提示");
            return;
        }

        var input = InputBox.Show(
            $"请输入出售数量（最多 {owned}）",
            "出售道具",
            "下一步",
            "取消",
            "数量不能为空"
        );

        input.OnSubmit += (string text, out string tips) =>
        {
            tips = "";
            if (!int.TryParse(text, out int sellCount) || sellCount <= 0)
            {
                tips = "请输入正确的数量";
                return false;
            }
            if (sellCount > owned)
            {
                tips = "数量超过拥有数量";
                return false;
            }

            int totalGold = def.SellPrice * sellCount;

            var msg = MessageBox.Show(
                $"确定出售 {def.Name} x{sellCount}？\n单价:{def.SellPrice} 总价:{totalGold}",
                "确认出售",
                MessageBoxType.Confirm,
                "出售",
                "取消"
            );
            msg.OnYes = () =>
            {
                SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Sell);
                ItemService.Instance.SendSellItem(itemId, sellCount);
            };
            return true;
        };
    }

    public void OnClickEmptySlot()
    {
        this.selectedItem = null;
        if (itemInfoPanel != null)
            itemInfoPanel.Hide();
    }


}
