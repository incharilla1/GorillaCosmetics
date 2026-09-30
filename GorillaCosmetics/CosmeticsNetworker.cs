using GorillaCosmetics.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace GorillaCosmetics
{
    public class CosmeticsNetworker : MonoBehaviourPunCallbacks, ICosmeticsNetworker
    {
        const string CustomHatKey = "GorillaCosmetics::CustomHat";
        const string CustomMaterialKey = "GorillaCosmetics::Material";

        void Start()
        {
            Plugin.SelectionManager.OnCosmeticsUpdated += UpdatePlayerCosmetics;
            UpdatePlayerCosmetics();
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isOfflineVRRig && rig.Creator is PunNetPlayer player && player.PlayerRef != null)
                    OnPlayerPropertiesUpdate(player.PlayerRef, player.PlayerRef.CustomProperties);
            }
        }

        void OnDestroy()
        {
            if (Plugin.SelectionManager != null)
                Plugin.SelectionManager.OnCosmeticsUpdated -= UpdatePlayerCosmetics;
            if (PhotonNetwork.LocalPlayer != null)
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { [CustomHatKey] = null, [CustomMaterialKey] = null });
        }

        public override void OnJoinedRoom()
        {
            UpdatePlayerCosmetics();
            foreach (Player player in PhotonNetwork.PlayerListOthers)
                OnPlayerPropertiesUpdate(player, player.CustomProperties);
        }

        public override void OnLeftRoom()
        {
            UpdatePlayerCosmetics();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            UpdatePlayerCosmetics();
            foreach (Player player in PhotonNetwork.PlayerListOthers)
                OnPlayerPropertiesUpdate(player, player.CustomProperties);
        }

        void UpdatePlayerCosmetics()
        {
            CustomCosmeticsController controller = GorillaTagger.Instance.offlineVRRig.GetComponent<CustomCosmeticsController>();
            controller.SetHat(Plugin.SelectionManager.CurrentHat);
            controller.SetMaterial(Plugin.SelectionManager.CurrentMaterial);
            if (PhotonNetwork.LocalPlayer != null)
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                {
                    [CustomHatKey] = controller.CurrentHat?.Descriptor.Name,
                    [CustomMaterialKey] = controller.CurrentMaterial?.Descriptor.Name
                });
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (targetPlayer == null || targetPlayer.IsLocal || Plugin.AssetLoader == null ||
                (!changedProps.ContainsKey(CustomHatKey) && !changedProps.ContainsKey(CustomMaterialKey)))
                return;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!(rig.Creator is PunNetPlayer player) || player.PlayerRef != targetPlayer)
                    continue;
                CustomCosmeticsController controller = rig.GetComponent<CustomCosmeticsController>();
                if (controller == null)
                    controller = rig.gameObject.AddComponent<CustomCosmeticsController>();
                if (changedProps.ContainsKey(CustomHatKey))
                    controller.SetHat(Plugin.AssetLoader.GetAsset<GorillaHat>(changedProps[CustomHatKey] as string));
                if (changedProps.ContainsKey(CustomMaterialKey))
                    controller.SetMaterial(Plugin.AssetLoader.GetAsset<GorillaMaterial>(changedProps[CustomMaterialKey] as string));
                break;
            }
        }
    }
}
