using HarmonyLib;

namespace GorillaCosmetics.HarmonyPatches
{
    public static class GorillaCosmeticsPatches
    {
        static readonly Harmony instance = new Harmony("org.legoandmars.gorillatag.gorillacosmetics");
        public static bool IsPatched { get; private set; }

        internal static void ApplyHarmonyPatches()
        {
            if (IsPatched)
                return;
            try
            {
                instance.PatchAll(typeof(Plugin).Assembly);
                IsPatched = true;
            }
            catch
            {
                instance.UnpatchSelf();
                throw;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            instance.UnpatchSelf();
            IsPatched = false;
        }
    }
}
