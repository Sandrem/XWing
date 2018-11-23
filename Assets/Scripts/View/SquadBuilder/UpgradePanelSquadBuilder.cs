﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SquadBuilderNS;
using Upgrade;
using Mods;
using RuleSets;

public class UpgradePanelSquadBuilder : MonoBehaviour {

    private string UpgradeName;
    private GenericUpgrade Upgrade;
    private UpgradeSlot Slot;
    private Action<UpgradeSlot, GenericUpgrade> OnClick;
    private bool ShowFromModInfo;
    private bool Compact;

    public void Initialize(string upgradeName, UpgradeSlot slot, GenericUpgrade upgrade = null, Action<UpgradeSlot, GenericUpgrade> onClick = null, bool showFromModInfo = false, bool compact = false)
    {
        UpgradeName = upgradeName;
        Upgrade = upgrade;
        OnClick = onClick;
        Slot = slot;
        ShowFromModInfo = showFromModInfo;
        Compact = compact;

        this.gameObject.GetComponent<RectTransform>().sizeDelta = (compact) ? Edition.Instance.UpgradeCardCompactSize : Edition.Instance.UpgradeCardSize;
    }

    void Start()
    {
        this.gameObject.SetActive(false);

        if (IsSlotImage())
        {
            SetSlotImage();
        }
        else
        {
            LoadImage();
            if (ShowFromModInfo) SetFromModeName();
        }

        SetOnClickHandler();
    }

    private bool IsSlotImage()
    {
        return UpgradeName.StartsWith("Slot:");
    }

    private void SetSlotImage()
    {
        string slotTypeName = UpgradeName.Substring(5, UpgradeName.Length - 5);
        string editionName = (Edition.Instance is FirstEdition) ? "FirstEdition" : "SecondEdition";
        Sprite sprite = (Sprite)Resources.Load("Sprites/SquadBuiler/UpgradeSlots/" + editionName + "/" + slotTypeName, typeof(Sprite));
        this.gameObject.transform.Find("UpgradeImage").GetComponent<Image>().sprite = sprite;

        this.gameObject.SetActive(true);
    }

    private void LoadImage()
    {
        Global.Instance.StartCoroutine(LoadTooltipImage(this.gameObject, Upgrade.ImageUrl));
    }

    private IEnumerator LoadTooltipImage(GameObject thisGameObject, string url)
    {
        WWW www = ImageManager.GetImage(url);
        yield return www;

        if (www.error == null)
        {
            if (thisGameObject != null)
            {
                SetImageFromWeb(thisGameObject.transform.Find("UpgradeImage").gameObject, www);
            }
        }
        else
        {
            ShowTextVersionOfCard();
        }
    }

    private void SetImageFromWeb(GameObject targetObject, WWW www)
    {
        Texture2D newTexture = new Texture2D(www.texture.height, www.texture.width);
        www.LoadImageIntoTexture(newTexture);
        TextureScale.Bilinear(newTexture, (int)Edition.Instance.UpgradeCardSize.x, (int)Edition.Instance.UpgradeCardSize.y);

        Sprite newSprite = null;
        if (!Compact)
        {
            newSprite = Sprite.Create(
                newTexture,
                new Rect(0, 0, newTexture.width, newTexture.height),
                Vector2.zero
            );
        }
        else
        {
            newSprite = Sprite.Create(
                newTexture,
                new Rect(
                    (!Upgrade.UpgradeInfo.HasType(UpgradeType.Configuration)) ? Edition.Instance.UpgradeCardCompactOffset.x : Edition.Instance.UpgradeCardCompactOffset.x - 155,
                    Edition.Instance.UpgradeCardCompactOffset.y,
                    Edition.Instance.UpgradeCardCompactSize.x,
                    Edition.Instance.UpgradeCardCompactSize.y
                ),
                Vector2.zero
            );
        }

        Image image = targetObject.transform.GetComponent<Image>();
        image.sprite = newSprite;

        this.gameObject.SetActive(true);
    }

    private void ShowTextVersionOfCard()
    {
        this.transform.Find("UpgradeInfo").GetComponent<Text>().text = UpgradeName;
        if (Edition.Instance is FirstEdition) this.transform.Find("CostInfo").GetComponent<Text>().text = Upgrade.UpgradeInfo.Cost.ToString();

        this.gameObject.SetActive(true);
    }

    private void SetFromModeName()
    {
        Text infoText = this.transform.Find("FromModInfo").GetComponent<Text>();

        if (Edition.Instance is FirstEdition)
        {
            if (Upgrade.FromMod != null)
            {
                Mod mod = (Mod)Activator.CreateInstance(Upgrade.FromMod);
                this.transform.Find("FromModInfo").GetComponent<Text>().text = mod.Name;
            }
        }
        else if (Edition.Instance is SecondEdition)
        {
            infoText.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(418, 0);

            infoText.alignment = TextAnchor.MiddleRight;
            infoText.fontSize = 50;
            infoText.text = Upgrade.UpgradeInfo.Cost.ToString();
        }
    }

    private void SetOnClickHandler()
    {
        if (OnClick != null)
        {
            EventTrigger trigger = this.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(delegate { OnClick(Slot, Upgrade); });
            trigger.triggers.Add(entry);

            ScrollRect mainScroll = GameObject.Find("UI/Panels/SelectUpgradePanel/Panel/Scroll View").GetComponent<ScrollRect>();
            FixScrollRect fixScrollRect = this.gameObject.AddComponent<FixScrollRect>();
            fixScrollRect.MainScroll = mainScroll;
        }
    }
}
