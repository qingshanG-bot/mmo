using Common.Data;
using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//6.4  1.10
namespace Managers
{
    class BagManager :Singleton<BagManager>
    {

        public int Unlocked;
        public BagItem[] Items;

        NBagInfo Info;

        public enum BagFilter
        {
            All,
            Normal,
            Equip,
            Material,
            Task,
            Ride
        }

        // *** 修改：当前过滤状态，默认 All
        public BagFilter CurrentFilter { get; private set; } = BagFilter.All;



        unsafe public void Init(NBagInfo info)
        {
            this.Info = info;
            this.Unlocked = info.Unlocked;
            Items = new BagItem[this.Unlocked];
            if (info.Items != null && info.Items.Length >= this.Unlocked)
            {
                Analyze(info.Items);
            }
            else
            {
                Info.Items = new byte[sizeof(BagItem) * this.Unlocked];
                Reset();
            }
        }

        public void Reset()
        {
            int i = 0;
            foreach (var kv in ItemManager.Instance.Items)
            {
                if (kv.Value.Count <= kv.Value.Define.StackLimit)
                {
                    this.Items[i].ItemId = (ushort)kv.Key;
                    this.Items[i].Count = (ushort)kv.Value.Count;
                }
                else
                {
                    int count = kv.Value.Count;
                    while(count > kv.Value.Define.StackLimit)
                    {
                        this.Items[i].ItemId = (ushort)kv.Key;
                        this.Items[i].Count = (ushort)kv.Value.Define.StackLimit;
                        i++;
                        count -= kv.Value.Define.StackLimit;
                    }
                    this.Items[i].ItemId = (ushort)kv.Key;
                    this.Items[i].Count = (ushort)count;
                }
                i++;
            }
        }

        unsafe void Analyze(byte[] data)
        {
            fixed (byte* pt = data)
            {
                for (int i = 0; i < this.Unlocked; i++)
                {
                    BagItem* item = (BagItem*) (pt + i * sizeof(BagItem));
                    Items[i] = *item;
                }
            }
        }

        unsafe public NBagInfo GetBagInfo()
        {
            fixed (byte* pt = Info.Items)
            {
                for (int i = 0; i < this.Unlocked; i++)
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));
                    *item = Items[i];
                }
            }
            return this.Info;
        }

        public void AddItem(int itemId, int count)
        {
            ushort addCount = (ushort)count;
            for (int i = 0; i < Items.Length; i++)
            {
                if (this.Items[i].ItemId == itemId)
                {
                    ushort canAdd = (ushort)(DataManager.Instance.Items[itemId].StackLimit - this.Items[i].Count);
                    if (canAdd > addCount)
                    {
                        this.Items[i].Count += addCount;
                        addCount = 0;
                        break;
                    }
                    else
                    {
                        this.Items[i].Count += canAdd;
                        addCount -= canAdd;
                    }
                }
            }
            if (addCount > 0) 
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (this.Items[i].ItemId==0)
                    {
                        this.Items[i].ItemId = (ushort)itemId;
                        this.Items[i].Count = addCount;
                        break;
                    }
                }
            }
        }

        internal void RemoveItem(int itemId, int count)
        {
            if (count <= 0) return;

            int remain = count;

            for (int i = 0; i < Items.Length; i++)
            {
                if (remain <= 0) break;

                if (this.Items[i].ItemId != itemId)
                    continue;

                int slotCount = this.Items[i].Count;
                if (slotCount <= 0)
                {
                    this.Items[i].ItemId = 0;
                    this.Items[i].Count = 0;
                    continue;
                }

                if (slotCount > remain)
                {
                    this.Items[i].Count = (ushort)(slotCount - remain);
                    remain = 0;
                    break;
                }
                else
                {
                    remain -= slotCount;
                    this.Items[i].ItemId = 0;
                    this.Items[i].Count = 0;
                }
            }

            if (remain > 0)
            {
                UnityEngine.Debug.LogWarning($"BagManager.RemoveItem not enough items: itemId={itemId} remove={count} remain={remain}");
            }
        }


        // ================= 以下是本次新增逻辑 =================

        // *** 修改：UI 告诉我当前 Tab 下标，我负责换成过滤枚举
        public void SetFilterByTabIndex(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0:
                    CurrentFilter = BagFilter.All;      
                    break;
                case 1:
                    CurrentFilter = BagFilter.Normal;
                    break;
                case 2:
                    CurrentFilter = BagFilter.Equip;
                    break;
                case 3:
                    CurrentFilter = BagFilter.Material;
                    break;
                case 4:
                    CurrentFilter = BagFilter.Ride;
                    break;

                default:
                    CurrentFilter = BagFilter.All;
                    break;
            }
        }

        /// <summary>
        /// *** 修改：给 UIBag 用的“当前要显示的格子”
        /// bagIndex：背包格子索引
        /// bagItem ：协议里存的道具数据
        /// define  ：配置数据
        /// </summary>
        public IEnumerable<(int bagIndex, BagItem bagItem, ItemDefine define)> GetDisplayItems()
        {
            for (int i = 0; i < Items.Length; i++)
            {
                var bi = Items[i];
                if (bi.ItemId <= 0)
                    continue;

                var def = ItemManager.Instance.GetItemDefine(bi.ItemId);
                if (def == null)
                    continue;

                if (!PassFilter(def))
                    continue;

                yield return (i, bi, def);
            }
        }

        // *** 过滤规则，全写这里，UI 不参与逻辑判断
        bool PassFilter(ItemDefine def)
        {
            switch (CurrentFilter)
            {
                case BagFilter.All:
                    return true;

                case BagFilter.Normal:
                    return def.Type == ItemType.Normal;

                case BagFilter.Equip:
                    return def.Type == ItemType.Equip;

                case BagFilter.Material:
                    return def.Type == ItemType.Material;

                case BagFilter.Task:
                    return def.Type == ItemType.Task;

                case BagFilter.Ride:
                    return def.Type == ItemType.Ride;

                default:
                    return true;
            }
        }

    }
}
