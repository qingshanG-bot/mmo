using Common.Battle;
using Entities;
using Managers;
using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMain : MonoSingleton <UIMain>
{
    public Image avatarIcon;
    public Text avatarName;
    public Text avatarLevel;

    public TMP_Text hp;
    public Slider hpBar;
    public TMP_Text mp;
    public Slider mpBar;


    public UITeam TeamWindow;

    public GameObject ChatWindow;

    public UICreatureInfo targetUI;

    public UISkillSlots skillSlots;

    private Attributes currentAttributes;

    protected override void  OnStart ()
    {
        if (targetUI != null)
            targetUI.gameObject.SetActive(false);

        BattleManager.Instance.OnTargetChanged += OnTargetChanged;

        User.Instance.OnCharacterInit += OnCharacterInit;

        // 防止事件注册前角色已经初始化
        TryRefreshAll();
    }

    private void OnCharacterInit()
    {
        BindAttributeEvents();
        TryRefreshAll();
    }

    private void TryRefreshAll()
    {
        if (!IsCharacterReady())
            return;

        RefreshAvatar();
        RefreshAttributes();

        if (skillSlots != null)
            skillSlots.UpdateSkills();
    }

    private bool IsCharacterReady()
    {
        return User.Instance != null
            && User.Instance.CurrentCharacterInfo != null
            && User.Instance.CurrentCharacter != null
            && User.Instance.CurrentCharacter.Attributes != null;
    }
    private void RefreshAvatar()
    {
        var info = User.Instance.CurrentCharacterInfo;

        avatarName.text = $"{info.Name}[{info.Id}]";
        avatarLevel.text = info.Level.ToString();

        int iconIndex = (int)info.Class + 5;

        if (SpriteManager.Instance != null &&
            SpriteManager.Instance.classIcons != null &&
            iconIndex >= 0 &&
            iconIndex < SpriteManager.Instance.classIcons.Length)
        {
            avatarIcon.overrideSprite = SpriteManager.Instance.classIcons[iconIndex];
        }
    }

    private void RefreshAttributes()
    {
        var attr = User.Instance.CurrentCharacter.Attributes;

        hp.text = $"{attr.HP}/{attr.MaxHP}";
        mp.text = $"{attr.MP}/{attr.MaxMP}";

        hpBar.maxValue = attr.MaxHP;
        hpBar.value = attr.HP;

        mpBar.maxValue = attr.MaxMP;
        mpBar.value = attr.MP;
    }

    private void BindAttributeEvents()
    {
        if (currentAttributes != null)
            currentAttributes.OnAttributeChanged -= RefreshAttributes;

        if (User.Instance != null &&
            User.Instance.CurrentCharacter != null &&
            User.Instance.CurrentCharacter.Attributes != null)
        {
            currentAttributes = User.Instance.CurrentCharacter.Attributes;
            currentAttributes.OnAttributeChanged += RefreshAttributes;
        }
    }

    public void OnClickTest()
    {
       UITest test = UIManager.Instance.Show<UITest>();
       test.SetTitle("这是一个测试UI");
       test.OnClose += Test_OnClose;
    }

    private void Test_OnClose(UIWindow sender, UIWindow.WindowResult result)
    {
        MessageBox.Show("点击了对话框的：" + result, "对话框响应结果", MessageBoxType.Information);
    }

    public void OnClickBag()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_BagOpen);
        UIManager.Instance.Show<UIBag>();
    }

    public void OnClickCharEquip()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Equip_Open);
        UIManager.Instance.Show<UICharEquip>();
    }

    public void OnClickQuest()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_QuestOpen);
        UIManager.Instance.Show<UIQuestSystem>();
    }

    public void OnClickFriend()
    {
        UIManager.Instance.Show<UIFriends>();
    }

    public void ShowTeamUI(bool show)
    {
        TeamWindow.ShowTeam(show);
    }

    public void OnClickGuild()
    {
        GuildManager.Instance.ShowGuild();
    }

    public void OnClickRide()
    {
        UIManager.Instance.Show<UIRide>();
    }

    public void OnClickSetting()
    {
        UIManager.Instance.Show<UISetting>();
    }

    public void OnClickSkill()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Skill_Open);
        UIManager.Instance.Show<UISkill>();
    }

    public void OnClikChat()
    {
        if (UIManager.Instance.IsOpen<UIChat>())
            UIManager.Instance.Close<UIChat>();
        else
            UIManager.Instance.Show<UIChat>();
    }


    private void OnTargetChanged(Creature target)
    {
        if (target != null)
        {
            if (!targetUI.isActiveAndEnabled) targetUI.gameObject.SetActive(true);
            targetUI.Target = target;
        }
        else
        {
            targetUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                EntityController controller = hit.collider.GetComponentInParent<EntityController>();
                if (controller == null)
                {
                    BattleManager.Instance.ClearTarget();
                }
            }
            else
            {
                BattleManager.Instance.ClearTarget();
            }
        }
    }


}
