using GorillaCosmetics.Data;
using UnityEngine;

namespace GorillaCosmetics
{
    public class CustomCosmeticsController : MonoBehaviour, ICustomCosmeticsController
    {
        public GorillaHat CurrentHat { get; private set; }
        public GorillaMaterial CurrentMaterial { get; private set; }
        GameObject currentHatObject;
        Material material;
        VRRig rig;

        void Awake()
        {
            rig = GetComponent<VRRig>();
        }

        void OnEnable()
        {
            if (Plugin.SelectionManager == null)
                return;
            if (rig.isOfflineVRRig)
            {
                SetHat(Plugin.SelectionManager.CurrentHat);
                SetMaterial(Plugin.SelectionManager.CurrentMaterial);
            }
            else if (rig.Creator is PunNetPlayer player && player.PlayerRef != null)
                Plugin.CosmeticsNetworker?.OnPlayerPropertiesUpdate(player.PlayerRef, player.PlayerRef.CustomProperties);
        }

        void OnDisable()
        {
            ResetHat();
            ResetMaterial();
        }

        void OnDestroy()
        {
            ResetHat();
            ResetMaterial();
        }

        public void SetHat(GorillaHat hat)
        {
            if (hat == null || (hat.Descriptor.DisablePublicLobbies && NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.SessionIsPrivate))
            {
                ResetHat();
                return;
            }
            if (CurrentHat == hat)
                return;
            ResetHat();
            CurrentHat = hat;
            currentHatObject = hat.GetAsset();
            currentHatObject.transform.SetParent(rig.headMesh.transform, false);
            currentHatObject.transform.localScale = Vector3.one * 0.25f;
            currentHatObject.transform.localPosition = new Vector3(0, 0.365f, 0.04f);
            currentHatObject.transform.localRotation = Quaternion.Euler(0, 90, 10);
            foreach (Transform child in currentHatObject.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = rig.bodyRenderer.GetBody(GorillaBodyType.Default).gameObject.layer;
        }

        public void ResetHat()
        {
            if (currentHatObject != null)
            {
                currentHatObject.SetActive(false);
                Destroy(currentHatObject);
                currentHatObject = null;
            }
            CurrentHat = null;
        }

        public void SetMaterial(GorillaMaterial cosmetic)
        {
            if (cosmetic == null || (cosmetic.Descriptor.DisablePublicLobbies && NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.SessionIsPrivate))
            {
                ResetMaterial();
                return;
            }
            if (CurrentMaterial != cosmetic)
            {
                ResetMaterial();
                CurrentMaterial = cosmetic;
                material = cosmetic.GetMaterial();
                SetColor(rig.playerColor.r, rig.playerColor.g, rig.playerColor.b);
            }
            ApplyMaterial();
        }

        public void ResetMaterial()
        {
            CurrentMaterial = null;
            if (material == null)
                return;
            Material oldMaterial = material;
            material = null;
            if (rig.bodyRenderer != null && rig.bodyRenderer.ActiveBody != null)
                rig.bodyRenderer.SetMaterialIndex(rig.setMatIndex);
            Destroy(oldMaterial);
        }

        public void SetColor(float red, float green, float blue)
        {
            if (CurrentMaterial == null || !CurrentMaterial.Descriptor.CustomColors)
                return;
            Color color = new Color(Mathf.Clamp01(red), Mathf.Clamp01(green), Mathf.Clamp01(blue));
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        public void ApplyMaterial()
        {
            if (material != null && rig.setMatIndex == 0 && rig.CurrentModeSkin == null && rig.TemporaryEffectSkin == null && rig.bodyRenderer.ActiveBody != null &&
                (rig.bodyRenderer.bodyType == GorillaBodyType.Default || rig.bodyRenderer.bodyType == GorillaBodyType.NoHead))
                rig.bodyRenderer.ActiveBody.sharedMaterial = material;
        }
    }
}
