using GorillaCosmetics.UI;
using GorillaNetworking;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace GorillaCosmetics.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(VRRig), "LocalUpdateCosmeticsWithTryon")]
    internal class WardrobeButtonPressPatch
    {
        static void Postfix(VRRig __instance, CosmeticsController.CosmeticSet newSet, CosmeticsController.CosmeticSet newTryOnSet)
        {
            if (__instance.isOfflineVRRig && Plugin.SelectionManager?.CurrentHat != null &&
                (!newSet.items[(int)CosmeticsController.CosmeticSlots.Hat].isNullItem ||
                !newTryOnSet.items[(int)CosmeticsController.CosmeticSlots.Hat].isNullItem))
                Plugin.SelectionManager.ResetHat();
        }
    }

    [HarmonyPatch]
    internal class WardrobeDisplayPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CosmeticWardrobe), "UpdateCosmeticDisplays");
            yield return AccessTools.Method(typeof(CosmeticWardrobe), "UpdateCategoryButtons");
            yield return AccessTools.Method(typeof(CosmeticWardrobe), "UpdateOutfitButtons");
        }

        static bool Prefix(CosmeticWardrobe __instance)
        {
            return !(Plugin.SelectionManager is SelectionManager selection) || !selection.Enabled || selection.Wardrobe != __instance;
        }
    }
}
