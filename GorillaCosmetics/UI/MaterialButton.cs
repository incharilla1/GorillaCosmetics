using GorillaCosmetics.Data;
using UnityEngine;

namespace GorillaCosmetics.UI
{
    public class MaterialButton : BaseCosmeticButton
    {
        public GorillaMaterial Material { get; private set; }
        GameObject previewOrb;

        protected override void Awake()
        {
            base.Awake();
            Plugin.SelectionManager.OnCosmeticsUpdated += UpdateButton;
        }

        protected override void OnDestroy()
        {
            Plugin.SelectionManager.OnCosmeticsUpdated -= UpdateButton;
            if (previewOrb != null)
            {
                previewOrb.SetActive(false);
                Destroy(previewOrb);
            }
            base.OnDestroy();
        }

        public override void ButtonActivation()
        {
            Plugin.SelectionManager.SetMaterial(Plugin.SelectionManager.CurrentMaterial == Material ? null : Material);
        }

        public void SetMaterial(GorillaMaterial material, Transform parent)
        {
            Material = material;
            if (previewOrb != null)
            {
                previewOrb.SetActive(false);
                Destroy(previewOrb);
                previewOrb = null;
            }
            if (Material != null)
                previewOrb = Material.GetPreviewOrb(parent);
            UpdateButton();
        }

        void UpdateButton()
        {
            isOn = Material != null && Plugin.SelectionManager.CurrentMaterial == Material;
            UpdateColor();
        }
    }
}
