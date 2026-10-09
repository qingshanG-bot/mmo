using Managers;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoSingleton<RewardManager>
{
    public RectTransform root;
    public UIReward toastPrefab;

    private readonly List<UIReward> Rewards = new List<UIReward>();

    public void ShowItem(int itemId, int count)
    {
        var def = ItemManager.Instance.GetItemDefine(itemId);
        if (def == null)
        {
            Debug.LogWarning($"RewardManager: 没有物品定义 {itemId}");
            return;
        }

        var icon = Resloader.Load<Sprite>(def.Icon);
        Show(icon, def.Name, count);
    }

    public void Show(Sprite icon, string name, int count)
    {
        UIReward reward = Instantiate(toastPrefab, root, false);
        reward.gameObject.SetActive(true);
        reward.Setup(icon, name, count);

        Rewards.Add(reward);

        reward.transform.SetAsFirstSibling();

        // 播放动画，结束后从列表移除并销毁对象
        reward.PlayAndAutoRecycle(_ =>
        {
            Rewards.Remove(reward);
            if (reward)
                Destroy(reward.gameObject);
        });
    }

    void OnDisable()
    {
        for (int i = 0; i < Rewards.Count; i++)
        {
            if (Rewards[i])
                Destroy(Rewards[i].gameObject);
        }
        Rewards.Clear();
    }
}
