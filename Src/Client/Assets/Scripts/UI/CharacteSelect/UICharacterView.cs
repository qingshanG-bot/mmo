using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UICharacterView : MonoBehaviour {

    public GameObject[] characters;


    private int currentCharacter = 0;

    public int CurrectCharacter
    {
        get
        {
            return currentCharacter;
        }
        set
        {
            currentCharacter = value;
            //this.UpdateCharacter();
            StartCoroutine(SwitchCharacterRoutine());
        }
    }


    //void UpdateCharacter()
    //{
    //    if (characters == null || characters.Length == 0) return;

    //    for (int i=0;i<characters.Length; i++)
    //    {
    //        characters[i].SetActive(i == this.currentCharacter);
    //    }
    //}

    IEnumerator SwitchCharacterRoutine()
    {
        // 隐藏所有角色
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
                characters[i].SetActive(false);
        }

        yield return new WaitForSeconds(0.05f); // 防止闪烁

        // 显示当前角色
        GameObject newChar = characters[currentCharacter];
        if (newChar == null) yield break;

        newChar.transform.rotation = Quaternion.Euler(0, 0, 0); ;
        newChar.SetActive(true);

        Animator anim = newChar.GetComponent<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger("UIEnter");
            
            anim.SetTrigger("UIEnter");
        }
    }

    void Start()
    {
        // 初始化显示第一个角色
        if (characters == null || characters.Length == 0) return;

        for (int i = 0; i < characters.Length; i++)
            if (characters[i] != null) characters[i].SetActive(false);

        characters[currentCharacter].SetActive(true);

        // 播放一次 UIEnter 动画
        var anim = characters[currentCharacter].GetComponent<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger("UIEnter");
            anim.SetTrigger("UIEnter");
        }
    }


}
