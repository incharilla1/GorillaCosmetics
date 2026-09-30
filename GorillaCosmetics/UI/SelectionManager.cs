using GorillaCosmetics.Data;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaCosmetics.UI
{
    public class SelectionManager : ISelectionManager, IDisposable
    {
        const string HatPlayerPrefKey = "GorillaCosmetics.Hat";
        const string MaterialPlayerPrefKey = "GorillaCosmetics.Material";

        public Action OnCosmeticsUpdated { get; set; }
        public GorillaHat CurrentHat { get; private set; }
        public GorillaMaterial CurrentMaterial { get; private set; }
        public bool Enabled { get; private set; }
        public CosmeticWardrobe Wardrobe { get; }

        readonly CustomCosmeticsController controller;
        readonly List<GorillaHat> hats;
        readonly List<GorillaMaterial> materials;
        readonly CosmeticWardrobe.CosmeticWardrobeSelection[] displays;
        readonly CosmeticWardrobe.CosmeticWardrobeCategory[] categories;
        readonly HeadModel selfDoll;
        readonly List<BaseCosmeticButton> buttons = new List<BaseCosmeticButton>();
        readonly Dictionary<GorillaPressableButton, bool> disabledButtons = new Dictionary<GorillaPressableButton, bool>();
        readonly List<GameObject> labels = new List<GameObject>();
        GameObject toggle;
        GameObject previewHat;
        GameObject previewOrb;
        ISelectionManager.SelectionView view;
        int page;
        int PageSize => displays.Length;
        int PageCount => Mathf.CeilToInt((view == ISelectionManager.SelectionView.Hat ? hats.Count : materials.Count) / (float)PageSize);

        public SelectionManager()
        {
            controller = GorillaTagger.Instance.offlineVRRig.GetComponent<CustomCosmeticsController>();
            hats = Plugin.AssetLoader.GetAssets<GorillaHat>();
            materials = Plugin.AssetLoader.GetAssets<GorillaMaterial>();
            Wardrobe = UnityEngine.Object.FindObjectsByType<CosmeticWardrobe>(FindObjectsSortMode.None)
                .Where(wardrobe => !wardrobe.UseTemporarySet)
                .OrderBy(wardrobe => (wardrobe.transform.position - controller.transform.position).sqrMagnitude).First();
            displays = Wardrobe.cosmeticCollectionDisplays;
            categories = Wardrobe.cosmeticCategoryButtons;
            selfDoll = Wardrobe.currentEquippedDisplay;
            string hatName = PlayerPrefs.GetString(HatPlayerPrefKey, PlayerPrefs.GetString("hatCosmetic"));
            string materialName = PlayerPrefs.GetString(MaterialPlayerPrefKey, PlayerPrefs.GetString("materialCosmetic"));
            CurrentHat = Plugin.AssetLoader.GetAsset<GorillaHat>(PlayerPrefs.HasKey(HatPlayerPrefKey) ? hatName : hatName.StartsWith("MOD_", StringComparison.Ordinal) ? hatName.Substring(4) : null);
            CurrentMaterial = Plugin.AssetLoader.GetAsset<GorillaMaterial>(PlayerPrefs.HasKey(MaterialPlayerPrefKey) ? materialName : materialName.StartsWith("MOD_", StringComparison.Ordinal) ? materialName.Substring(4) : null);
            if (CurrentHat != null)
                SetHat(CurrentHat);
            controller.SetMaterial(CurrentMaterial);
            CreateEnableButton();
        }

        void CreateEnableButton()
        {
            CosmeticCategoryButton template = categories.First(category => category.category == CosmeticsController.CosmeticCategory.Hat).button;
            toggle = UnityEngine.Object.Instantiate(template.gameObject, template.transform.parent);
            toggle.name = "GorillaCosmetics";
            toggle.transform.localPosition += Constants.EnableButtonLocalPositionOffset;
            CosmeticCategoryButton button = toggle.GetComponent<CosmeticCategoryButton>();
            foreach (Component text in new Component[] { template.myText, template.myTmpText, template.myTmpText2 })
            {
                if (text == null)
                    continue;
                if (text.transform.IsChildOf(template.transform))
                    continue;
                Component label = UnityEngine.Object.Instantiate(text, text.transform.parent);
                label.transform.position += toggle.transform.position - template.transform.position;
                labels.Add(label.gameObject);
                if (text == template.myText)
                    button.myText = (Text)label;
                else if (text == template.myTmpText)
                    button.myTmpText = (TMP_Text)label;
                else
                    button.myTmpText2 = (TMP_Text)label;
            }
            foreach (SpriteRenderer icon in toggle.GetComponentsInChildren<SpriteRenderer>(true))
                icon.enabled = false;
            toggle.AddComponent<ToggleEnableButton>();
        }

        public void Enable()
        {
            if (Enabled)
                return;
            Enabled = true;
            GorillaPressableButton next = Wardrobe.nextSelection;
            GorillaPressableButton previous = Wardrobe.prevSelection;
            IEnumerable<GorillaPressableButton> originals = categories.Select(category => category.button).Cast<GorillaPressableButton>()
                .Concat(displays.Select(display => display.selectButton))
                .Concat(Wardrobe.uniqueCosmeticButtons)
                .Concat(new[] { next, previous,
                    Wardrobe.previousOutfit,
                    Wardrobe.nextOutfit });
            foreach (GorillaPressableButton button in originals.Distinct())
            {
                disabledButtons.Add(button, button.enabled);
                button.enabled = false;
            }
            PageButton nextPage = next.gameObject.AddComponent<PageButton>();
            nextPage.Forward = true;
            buttons.Add(nextPage);
            buttons.Add(previous.gameObject.AddComponent<PageButton>());
            foreach (CosmeticWardrobe.CosmeticWardrobeCategory category in categories)
            {
                if (category.category != CosmeticsController.CosmeticCategory.Hat && category.category != CosmeticsController.CosmeticCategory.Face)
                    continue;
                ViewButton button = category.button.gameObject.AddComponent<ViewButton>();
                button.SetView(category.category == CosmeticsController.CosmeticCategory.Hat ? ISelectionManager.SelectionView.Hat : ISelectionManager.SelectionView.Material);
                buttons.Add(button);
            }
            SetView(ISelectionManager.SelectionView.Hat);
        }

        public void Disable()
        {
            if (!Enabled)
                return;
            Enabled = false;
            foreach (BaseCosmeticButton button in buttons)
                UnityEngine.Object.DestroyImmediate(button);
            buttons.Clear();
            foreach (KeyValuePair<GorillaPressableButton, bool> button in disabledButtons)
            {
                if (button.Key != null)
                    button.Key.enabled = button.Value;
            }
            disabledButtons.Clear();
            if (previewHat != null)
            {
                previewHat.SetActive(false);
                UnityEngine.Object.Destroy(previewHat);
                previewHat = null;
            }
            if (previewOrb != null)
            {
                previewOrb.SetActive(false);
                UnityEngine.Object.Destroy(previewOrb);
                previewOrb = null;
            }
            if (Wardrobe != null)
                Wardrobe.HandleCosmeticsUpdated();
        }

        public void SetView(ISelectionManager.SelectionView view)
        {
            if (!Enabled)
                return;
            this.view = view;
            page = 0;
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                if (!(buttons[i] is HatButton) && !(buttons[i] is MaterialButton))
                    continue;
                UnityEngine.Object.DestroyImmediate(buttons[i]);
                buttons.RemoveAt(i);
            }
            foreach (CosmeticWardrobe.CosmeticWardrobeSelection display in displays)
            {
                display.displayHead.SetCosmeticActive("NOTHING");
                if (view == ISelectionManager.SelectionView.Hat)
                    buttons.Add(display.selectButton.gameObject.AddComponent<HatButton>());
                else
                    buttons.Add(display.selectButton.gameObject.AddComponent<MaterialButton>());
            }
            UpdateView();
        }

        public void NextPage()
        {
            if (!Enabled)
                return;
            if (page < PageCount - 1)
                page++;
            UpdateView();
        }

        public void PreviousPage()
        {
            if (!Enabled)
                return;
            if (page > 0)
                page--;
            UpdateView();
        }

        void UpdateView()
        {
            for (int i = 0; i < displays.Length; i++)
            {
                int index = page * PageSize + i;
                if (view == ISelectionManager.SelectionView.Hat)
                    displays[i].selectButton.GetComponent<HatButton>().SetHat(index < hats.Count ? hats[index] : null, displays[i].displayHead.transform);
                else
                    displays[i].selectButton.GetComponent<MaterialButton>().SetMaterial(index < materials.Count ? materials[index] : null, displays[i].displayHead.transform);
            }
            UpdateHeadModel();
        }

        void UpdateHeadModel()
        {
            if (!Enabled)
                return;
            if (previewHat != null)
            {
                previewHat.SetActive(false);
                UnityEngine.Object.Destroy(previewHat);
                previewHat = null;
            }
            if (CurrentHat != null)
            {
                previewHat = CurrentHat.GetCleanAsset();
                previewHat.transform.SetParent(selfDoll.transform, false);
                previewHat.transform.localPosition = Constants.PreviewHatLocalPosition;
                previewHat.transform.localRotation = Constants.PreviewHatLocalRotation;
                previewHat.transform.localScale = Constants.PreviewHatLocalScale;
            }
            if (previewOrb != null)
            {
                previewOrb.SetActive(false);
                UnityEngine.Object.Destroy(previewOrb);
                previewOrb = null;
            }
            if (CurrentMaterial != null)
            {
                previewOrb = CurrentMaterial.GetPreviewOrb(selfDoll.transform);
                previewOrb.transform.localPosition += Constants.PreviewOrbHeadModelLocalPositionOffset;
            }
        }

        public void SetHat(GorillaHat hat)
        {
            if (hat == null)
            {
                ResetHat();
                return;
            }
            CosmeticsController cosmetics = CosmeticsController.instance;
            cosmetics.currentWornSet.items[(int)CosmeticsController.CosmeticSlots.Hat] = cosmetics.nullItem;
            cosmetics.tryOnSet.items[(int)CosmeticsController.CosmeticSlots.Hat] = cosmetics.nullItem;
            cosmetics.tempUnlockedSet.items[(int)CosmeticsController.CosmeticSlots.Hat] = cosmetics.nullItem;
            PlayerPrefs.SetString(CosmeticsController.CosmeticSet.SlotPlayerPreferenceName(CosmeticsController.CosmeticSlots.Hat), cosmetics.nullItem.itemName);
            PlayerPrefs.SetString(HatPlayerPrefKey, hat.Descriptor.Name);
            PlayerPrefs.Save();
            CurrentHat = hat;
            cosmetics.UpdateWornCosmetics(true);
            controller.SetHat(hat);
            UpdateHeadModel();
            OnCosmeticsUpdated?.Invoke();
        }

        public void ResetHat()
        {
            PlayerPrefs.DeleteKey(HatPlayerPrefKey);
            PlayerPrefs.DeleteKey("hatCosmetic");
            PlayerPrefs.Save();
            CurrentHat = null;
            controller.ResetHat();
            UpdateHeadModel();
            OnCosmeticsUpdated?.Invoke();
        }

        public void SetMaterial(GorillaMaterial material)
        {
            if (material == null)
            {
                ResetMaterial();
                return;
            }
            PlayerPrefs.SetString(MaterialPlayerPrefKey, material.Descriptor.Name);
            PlayerPrefs.Save();
            CurrentMaterial = material;
            controller.SetMaterial(material);
            UpdateHeadModel();
            OnCosmeticsUpdated?.Invoke();
        }

        public void ResetMaterial()
        {
            PlayerPrefs.DeleteKey(MaterialPlayerPrefKey);
            PlayerPrefs.DeleteKey("materialCosmetic");
            PlayerPrefs.Save();
            CurrentMaterial = null;
            controller.ResetMaterial();
            UpdateHeadModel();
            OnCosmeticsUpdated?.Invoke();
        }

        public void Dispose()
        {
            Disable();
            if (toggle != null)
            {
                toggle.SetActive(false);
                UnityEngine.Object.DestroyImmediate(toggle);
            }
            foreach (GameObject label in labels)
                UnityEngine.Object.Destroy(label);
        }
    }
}
