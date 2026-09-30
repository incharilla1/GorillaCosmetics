using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace GorillaCosmetics.HarmonyPatches.Patches
{
    [HarmonyPatch]
    internal class ChangeMaterialPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GorillaBodyRenderer), "SetMaterialIndex");
            yield return AccessTools.Method(typeof(GorillaBodyRenderer), "ResetBodyMaterial");
            yield return AccessTools.Method(typeof(GorillaBodyRenderer), "SetSkinMaterials");
            yield return AccessTools.Method(typeof(GorillaBodyRenderer), "SetBodyType");
        }

        static void Postfix(GorillaBodyRenderer __instance)
        {
            __instance.rig.GetComponent<CustomCosmeticsController>()?.ApplyMaterial();
        }
    }
}
