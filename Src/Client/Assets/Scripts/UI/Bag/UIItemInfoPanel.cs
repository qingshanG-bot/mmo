using Common.Data;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInfoPanel : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public Text priceText;
    public Text sellPriceText;
    public Text descText;

    // 记录当前展示的物品，后续 Sell/Use 直接用
    private ItemDefine currentDef;
    private int currentCount;

    void Awake()
    {
        Hide(); // 初始隐藏
    }

    public void Show(ItemDefine def, int count)
    {
        if (def == null)
        {
            Hide();
            return;
        }

        this.currentDef = def;
        this.currentCount = count;

        if (icon) icon.overrideSprite = string.IsNullOrEmpty(def.Icon) ? null : Resloader.Load<Sprite>(def.Icon);

        if (nameText) nameText.text = def.Name;
        if (priceText) priceText.text = def.Price.ToString();
        if (sellPriceText) sellPriceText.text = def.SellPrice.ToString();
        if (descText) descText.text = def.Description ?? "";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.currentDef = null;
        this.currentCount = 0;

        if (icon) icon.overrideSprite = null;
        if (nameText) nameText.text = "";
        if (priceText) priceText.text = "";
        if (sellPriceText) sellPriceText.text = "";
        if (descText) descText.text = "";

        gameObject.SetActive(false);
    }

    public void OnClickSell()
    {
    }

    public void OnClickUse()
    {
    }
}
