using Common.Data;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Battle
{
    public class Attributes
    {
        public event Action OnAttributeChanged;

        AttributeData Initial = new AttributeData();
        AttributeData Growth = new AttributeData();
        AttributeData Equip = new AttributeData();
        public AttributeData Basic = new AttributeData();
        public AttributeData Buff = new AttributeData();

        public AttributeData Final = new AttributeData();

        int Level;

        public NAttributeDynamic DynamicAttr;

        //public float HP
        //{
        //    get { return DynamicAttr.Hp; }
        //    set { DynamicAttr.Hp = (int)Math.Min(MaxHP, value); }
        //}
        public float HP
        {
            get
            {
                return DynamicAttr != null ? DynamicAttr.Hp : 0;
            }
            set
            {
                if (DynamicAttr == null)
                    DynamicAttr = new NAttributeDynamic();

                int newValue = (int)Math.Max(0, Math.Min(MaxHP, value));
                if (DynamicAttr.Hp != newValue)
                {
                    DynamicAttr.Hp = newValue;
                    RaiseAttributeChanged();
                }
            }
        }

        //public float MP
        //{
        //    get { return DynamicAttr.Mp; }
        //    set { DynamicAttr.Mp = (int)Math.Min(MaxMP, value); }
        //}
        public float MP
        {
            get
            {
                return DynamicAttr != null ? DynamicAttr.Mp : 0;
            }
            set
            {
                if (DynamicAttr == null)
                    DynamicAttr = new NAttributeDynamic();

                int newValue = (int)Math.Max(0, Math.Min(MaxMP, value));
                if (DynamicAttr.Mp != newValue)
                {
                    DynamicAttr.Mp = newValue;
                    RaiseAttributeChanged();
                }
            }
        }

        public float MaxHP { get { return this.Final.MaxHP; } }

        public float MaxMP { get { return this.Final.MaxMP; } }

        public float STR { get { return this.Final.STR; } }

        public float INT { get { return this.Final.INT; } }

        public float DEX { get { return this.Final.DEX; } }

        public float AD { get { return this.Final.AD; } }

        public float AP { get { return this.Final.AP; } }

        public float DEF { get { return this.Final.DEF; } }

        public float MDEF { get { return this.Final.MDEF; } }

        public float SPD { get { return this.Final.SPD; } }

        public float CRI { get { return this.Final.CRI; } }


        /// <summary>
        /// 属性初始化
        /// </summary>
        /// <param name="define"></param>
        /// <param name="level"></param>
        /// <param name="equips"></param>
        /// <param name="dynamicAttr"></param>
        public void Init(CharacterDefine define, int level, List<EquipDefine> equips, NAttributeDynamic dynamicAttr)
        {
            this.DynamicAttr = dynamicAttr;

            this.LoadInitAttribute(this.Initial, define);
            this.LoadGrowthAttribute(this.Growth, define);
            this.LoadEquipAttributes(this.Equip, equips);

            this.Level = level;
            this.InitBasicAttributes();
            this.InitSecondaryAttributes();

            this.InitFinalAttributes();

            if (this.DynamicAttr == null)
            {
                this.DynamicAttr = new NAttributeDynamic();
                //this.HP = this.MaxHP;
                //this.MP = this.MaxMP;
                
            }
            //else
            //{
            //    //this.HP = DynamicAttr.Hp;
            //    //this.MP = DynamicAttr.Mp;
            //    //this.DynamicAttr.Hp = (int)Math.Max(0, Math.Min(this.MaxHP, this.DynamicAttr.Hp));
            //    //this.DynamicAttr.Mp = (int)Math.Max(0, Math.Min(this.MaxMP, this.DynamicAttr.Mp));
            //}
            this.DynamicAttr.Hp = (int)this.MaxHP;
            this.DynamicAttr.Mp = (int)this.MaxMP;

            RaiseAttributeChanged();
        }

        /// <summary>
        /// 初始属性
        /// </summary>
        /// <param name="initial"></param>
        /// <param name="define"></param>
        private void LoadInitAttribute(AttributeData initial, CharacterDefine define)
        {
            initial.MaxHP = define.MaxHP;
            initial.MaxMP = define.MaxMP;

            initial.STR = define.STR;
            initial.INT = define.INT;
            initial.DEX = define.DEX;
            initial.AD = define.AD;
            initial.AP = define.AP;
            initial.DEF = define.DEF;
            initial.MDEF = define.MDEF;
            initial.SPD = define.SPD;
            initial.CRI = define.CRI;
        }

        /// <summary>
        /// 成长属性
        /// </summary>
        /// <param name="growth"></param>
        /// <param name="define"></param>
        private void LoadGrowthAttribute(AttributeData growth, CharacterDefine define)
        {
            growth.STR = define.GrowthSTR;
            growth.INT = define.GrowthINT;
            growth.DEX = define.GrowthDEX;
        }

        /// <summary>
        /// 装备属性
        /// </summary>
        /// <param name="equip"></param>
        /// <param name="equips"></param>
        private void LoadEquipAttributes(AttributeData equip, List<EquipDefine> equips)
        {
            equip.Reset();
            if (equips == null) return;

            foreach (var define in equips)
            {
                equip.MaxHP += define.MaxHP;
                equip.MaxMP += define.MaxMP;

                equip.STR += define.STR;
                equip.INT += define.INT;
                equip.DEX += define.DEX;
                equip.AD += define.AD;
                equip.AP += define.AP;
                equip.DEF += define.DEF;
                equip.MDEF += define.MDEF;
                equip.SPD += define.SPD;
                equip.CRI += define.CRI;
            }
        }


        private void InitBasicAttributes()
        {
            for (int i = (int)AttributeType.MaxHP  ; i < (int)AttributeType.MAX; i++)
            {
                this.Basic.Data[i] = this.Initial.Data[i];
            }

            for (int i = (int)AttributeType.STR; i <=(int)AttributeType.DEX; i++)
            {
                this.Basic.Data[i] = this.Initial.Data[i] + this.Growth.Data[i] *(this.Level -1);

                this.Basic.Data[i] += this.Equip.Data[i];
            }
        }

        private void InitSecondaryAttributes()
        {
            this.Basic.MaxHP = this.Basic.STR * 10 + this.Initial.MaxHP + this.Equip.MaxHP;
            this.Basic.MaxMP = this.Basic.INT * 10 + this.Initial.MaxMP + this.Equip.MaxMP;

            this.Basic.AD = this.Basic.STR * 5 + this.Initial.AD + this.Equip.AD;
            this.Basic.AP = this.Basic.INT * 5 + this.Initial.AP + Equip.AP;
            this.Basic.DEF = this.Basic.STR * 2 + this.Basic.DEX * 1 + this.Initial.DEF + this.Equip.DEF;
            this.Basic.MDEF = this.Basic.INT * 2 + this.Basic.DEX * 1 + this.Initial.MDEF + this.Equip.MDEF;

            this.Basic.SPD = this.Basic.DEX * 0.2f + this.Initial.SPD + this.Equip.SPD;
            this.Basic.CRI = this.Basic.DEX * 0.0002f + this.Initial.CRI + this.Equip.CRI;
        }

        public void InitFinalAttributes()
        {
            for (int i = (int)AttributeType.MaxHP;  i < (int)AttributeType.MAX;  i++)
            {
                this.Final.Data[i] = this.Basic.Data[i] + this.Buff.Data[i];
            }

            if (DynamicAttr != null)
            {
                DynamicAttr.Hp = (int)Math.Max(0, Math.Min(this.MaxHP, DynamicAttr.Hp));
                DynamicAttr.Mp = (int)Math.Max(0, Math.Min(this.MaxMP, DynamicAttr.Mp));
            }

            RaiseAttributeChanged();
        }

        private void RaiseAttributeChanged()
        {
            OnAttributeChanged?.Invoke();
        }
    }
}
