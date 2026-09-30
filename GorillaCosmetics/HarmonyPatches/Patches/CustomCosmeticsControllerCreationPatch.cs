using HarmonyLib;

namespace GorillaCosmetics.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(VRRig), "OnEnable")]
    internal class CustomCosmeticsControllerCreationPatch
    {
        static void Postfix(VRRig __instance)
        {
            if (__instance.GetComponent<CustomCosmeticsController>() == null)
                __instance.gameObject.AddComponent<CustomCosmeticsController>();
        }
    }
}
