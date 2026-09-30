using HarmonyLib;

namespace GorillaCosmetics.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(VRRig), "InitializeNoobMaterialLocal")]
    internal class ColorPatch
    {
        static void Postfix(VRRig __instance, float red, float green, float blue)
        {
            __instance.GetComponent<CustomCosmeticsController>()?.SetColor(red, green, blue);
        }
    }
}
