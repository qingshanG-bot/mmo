using Entities;
using Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINameBar : MonoBehaviour {

    public Text avaverName;
    public Image avaverIcon;


    public Character character;

    public UIBuffIcons buffIcons;

    // Use this for initialization
    void Start () {
		if(this.character!=null)
        {
            buffIcons.SetOwner(this.character);
        }
	}
	
	// Update is called once per frame
	void Update () {
        this.UpdateInfo();

        //固定住信息框的位置
        //this.transform.forward = Camera.main.transform.forward;
	}

    void UpdateInfo()
    {
        if (this.character != null)
        {
            this.avaverIcon.overrideSprite = SpriteManager.Instance.classIcons[(int)User.Instance.CurrentCharacterInfo.Class + 5];
            string name = this.character.Name + " Lv." + this.character.Info.Level;
            //性能优化
            if(name != this.avaverName.text)
            {
                this.avaverName.text = name;
            }
        }
    }
}
