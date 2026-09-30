using BepInEx;
using GorillaCosmetics.HarmonyPatches;
using GorillaCosmetics.UI;
using GorillaNetworking;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GorillaCosmetics
{
    [BepInPlugin("org.legoandmars.gorillatag.gorillacosmetics", "Gorilla Cosmetics", "3.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static IAssetLoader AssetLoader { get; private set; }
        public static ISelectionManager SelectionManager { get; private set; }
        public static ICosmeticsNetworker CosmeticsNetworker { get; private set; }

        IEnumerator Start()
        {
            GorillaCosmeticsPatches.ApplyHarmonyPatches();
            Logger.LogInfo("waiting for game cosmetics");
            while (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null ||
                CosmeticsController.instance == null || !CosmeticsV2Spawner_Dirty.isPrepared)
                yield return new WaitForSecondsRealtime(0.25f);

            AssetLoader = new AssetLoader();
            Logger.LogInfo($"loaded {AssetLoader.GetAssets<Data.GorillaHat>().Count} hats and {AssetLoader.GetAssets<Data.GorillaMaterial>().Count} materials");
            foreach (VRRig rig in VRRigCache.AllRigs)
            {
                if (rig != null && rig.GetComponent<CustomCosmeticsController>() == null)
                    rig.gameObject.AddComponent<CustomCosmeticsController>();
            }
            while (!FindObjectsByType<CosmeticWardrobe>(FindObjectsSortMode.None).Any(wardrobe => !wardrobe.UseTemporarySet))
                yield return new WaitForSecondsRealtime(0.25f);
            SelectionManager = new SelectionManager();
            CosmeticsNetworker = gameObject.AddComponent<CosmeticsNetworker>();
            Logger.LogInfo("custom wardrobe ready");
        }

        void OnDestroy()
        {
            if (CosmeticsNetworker is CosmeticsNetworker networker)
                DestroyImmediate(networker);
            if (SelectionManager is SelectionManager selection)
                selection.Dispose();
            foreach (CustomCosmeticsController controller in Resources.FindObjectsOfTypeAll<CustomCosmeticsController>())
                DestroyImmediate(controller);
            GorillaCosmeticsPatches.RemoveHarmonyPatches();
            if (AssetLoader is AssetLoader loader)
                loader.Dispose();
            CosmeticsNetworker = null;
            SelectionManager = null;
            AssetLoader = null;
        }
    }
}

