using GorillaCosmetics.Data;
using UnityEngine;

namespace GorillaCosmetics.UI
{
    public class HatButton : BaseCosmeticButton
    {
        public GorillaHat Hat { get; private set; }
        GameObject previewHat;

        protected override void Awake()
        {
            base.Awake();
            Plugin.SelectionManager.OnCosmeticsUpdated += UpdateButton;
        }

        protected override void OnDestroy()
        {
            Plugin.SelectionManager.OnCosmeticsUpdated -= UpdateButton;
            if (previewHat != null)
            {
                previewHat.SetActive(false);
                Destroy(previewHat);
            }
            base.OnDestroy();
        }

        public override void ButtonActivation()
        {
            Plugin.SelectionManager.SetHat(Plugin.SelectionManager.CurrentHat == Hat ? null : Hat);
        }

        public void SetHat(GorillaHat hat, Transform parent)
        {
            Hat = hat;
            if (previewHat != null)
            {
                previewHat.SetActive(false);
                Destroy(previewHat);
                previewHat = null;
            }
            if (Hat != null)
            {
                previewHat = Hat.GetCleanAsset();
                previewHat.transform.SetParent(parent, false);
                previewHat.transform.localPosition = Constants.PreviewHatLocalPosition;
                previewHat.transform.localRotation = Constants.PreviewHatLocalRotation;
                previewHat.transform.localScale = Constants.PreviewHatLocalScale;
            }
            UpdateButton();
        }

        void UpdateButton()
        {
            isOn = Hat != null && Plugin.SelectionManager.CurrentHat == Hat;
            UpdateColor();
        }
    }
}
