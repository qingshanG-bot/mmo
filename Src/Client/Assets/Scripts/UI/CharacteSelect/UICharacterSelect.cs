using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Services;
using SkillBridge.Message;
using System;
public class UICharacterSelect : MonoBehaviour {

    public GameObject panelCreate;
    public GameObject panelSelect;

    public GameObject btnCreateCancel;

    public InputField charName;

    CharacterClass charClass;

    public Transform uiCharList;
    public GameObject uiCharInfo;

    public List<GameObject> uiChars = new List<GameObject>();

    public Image[] titles;

    public Text descs;


    public Text[] names;

    private int selectCharacterIdx = -1;

    public UICharacterView characterView;

    // Use this for initialization
    void Start()
    {
        InitCharacterSelect(true);
        UserService.Instance.OnCharacterCreate += OnCharacterCreate;
        UserService.Instance.OnCharacterDelete += OnCharacterDelete;
    }



    void OnDestroy()
    {
        // 防止多次订阅导致回调被触发多次
        if (UserService.Instance != null)
        {
            UserService.Instance.OnCharacterCreate -= OnCharacterCreate;

            UserService.Instance.OnCharacterDelete -= OnCharacterDelete;
        }

    }

    public void InitCharacterCreate()
    {
        panelCreate.SetActive(true);
        panelSelect.SetActive(false);

        characterView.CurrectCharacter = 0;
        OnSelectClass(1);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClickCreate()
    {
        if (string.IsNullOrEmpty(this.charName.text))
        {
            MessageBox.Show("请输入角色名称");
            return;
        }

        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);

        UserService.Instance.SendCharacterCreate(this.charName.text, this.charClass);
    }

    /// <summary>
    /// 选择职业
    /// </summary>
    /// <param name="charClass"></param>
    public void OnSelectClass(int charClass)
    {
        this.charClass = (CharacterClass)charClass;

        characterView.CurrectCharacter = charClass - 1;

        for (int i = 0; i < 3; i++)
        {
            titles[i].gameObject.SetActive(i == charClass - 1);
            names[i].text = DataManager.Instance.Characters[i + 1].Name;
        }

        descs.text = DataManager.Instance.Characters[charClass].Description;

        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }


    void OnCharacterCreate(Result result, string message)
    {
        if (result == Result.Success)
        {
            InitCharacterSelect(true);

        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }


    public void InitCharacterSelect(bool init)
    {
        panelCreate.SetActive(false);
        panelSelect.SetActive(true);

        if (init)
        {
            foreach (var old in uiChars)
            {
                if (old != null) // 确保对象未被销毁
                {
                    Destroy(old);
                }
            }
            uiChars.Clear();

            for (int i = 0; i < User.Instance.Info.Player.Characters.Count; i++) 
            {

                GameObject go = Instantiate(uiCharInfo, this.uiCharList);
                UICharInfo chrInfo = go.GetComponent<UICharInfo>();
                chrInfo.info = User.Instance.Info.Player.Characters[i];

                Button button = go.GetComponent<Button>();
                int idx = i;
                button.onClick.AddListener(() => {
                    OnSelectCharacter(idx);
                });

                uiChars.Add(go);
                go.SetActive(true);
            }
        }
    }


    public void OnSelectCharacter(int idx)
    {
        this.selectCharacterIdx = idx;
        var cha = User.Instance.Info.Player.Characters[idx];
        Debug.LogFormat("Select Char:[{0}]{1}[{2}]", cha.Id, cha.Name, cha.Class);
        characterView.CurrectCharacter = ((int)cha.Class - 1);
        //characterView.CurrectCharacter = idx;

        for (int i = 0; i < User.Instance.Info.Player.Characters.Count; i++)
        {
            UICharInfo ci = this.uiChars[i].GetComponent<UICharInfo>();
            ci.Selected = idx == i;
        }

        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }
    public void OnClickPlay()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);

        if (selectCharacterIdx >= 0)
        {
            UserService.Instance.SendGameEnter(selectCharacterIdx);
        }
    }


    /// <summary>
    /// 新增切换
    /// </summary>
    public void OnClickNextCharacter()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);

        int next = characterView.CurrectCharacter + 1;
        if (next >= characterView.characters.Length)
            next = 0; // 循环回第一个

        characterView.CurrectCharacter = next;

        // 更新 UI 文字信息
        UpdateCharacterInfo(next + 1);
    }

    public void OnClickPrevCharacter()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);

        int prev = characterView.CurrectCharacter - 1;
        if (prev < 0)
            prev = characterView.characters.Length - 1; // 循环到最后一个

        characterView.CurrectCharacter = prev;

        UpdateCharacterInfo(prev + 1);
    }

    void UpdateCharacterInfo(int charClass)
    {
        for (int i = 0; i < titles.Length; i++)
        {
            titles[i].gameObject.SetActive(i == charClass - 1);
            names[i].text = DataManager.Instance.Characters[i + 1].Name;
        }

        descs.text = DataManager.Instance.Characters[charClass].Description;
    }

    //角色删除
    public void OnClickDeleteCharacter()
    {
        if (this.selectCharacterIdx <0) 
        {
            MessageBox.Show("请选择要删除的角色！");
            return;
        }

        var charInfo = User.Instance.Info.Player.Characters[selectCharacterIdx];
        MessageBox.Show(string.Format("确定要删除角色【{0}】吗？该操作不可恢复！！！", charInfo.Name), "删除角色", MessageBoxType.Confirm, "确认", "取消").OnYes = () =>
        {
            //SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
            UserService.Instance.SendCharacterDelete(charInfo.Id);
        };
    
    }

    private void OnCharacterDelete(Result result, string message)
    {
        if (result == Result.Success)
        {
            MessageBox.Show(message);

            
            InitCharacterSelect(true);
            selectCharacterIdx = -1;
        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }
}
