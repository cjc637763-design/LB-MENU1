using HarmonyLib;
using UnityEngine;

namespace GorillaTagModMenu.Patches
{
    /// <summary>
    /// All Harmony patches are collected here.
    /// They are applied automatically via _harmony.PatchAll() in Plugin.Awake().
    ///
    /// To find the exact method signatures for your game version, open
    ///   Gorilla Tag_Data\Managed\Assembly-CSharp.dll
    /// in dnSpy or ILSpy and search for the class/method you want to patch.
    /// </summary>
    public static class HarmonyPatches
    {
        // Flags toggled by mod buttons
        public static bool AntiTag   { get; set; }
        public static bool AlwaysIt  { get; set; }
        public static float SpeedMod { get; set; } = 1f;

        // ─────────────────────────────────────────────────────────────────────
        // ANTI-TAG
        // GorillaTagManager.StartTaggedPlayer is called when you are tagged.
        // Prefix returning false skips the original method entirely.
        // ─────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(GorillaTagManager), "StartTaggedPlayer")]
        [HarmonyPrefix]
        private static bool Prefix_StartTaggedPlayer(GorillaTagManager __instance,
                                                      Photon.Realtime.Player player)
        {
            if (!AntiTag) return true; // run original

            if (player == Photon.Pun.PhotonNetwork.LocalPlayer)
            {
                Plugin.Log.LogInfo("[AntiTag] Blocked incoming tag.");
                return false;           // skip original — you don't get tagged
            }

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ALWAYS IT
        // Postfix: after the game decides who is "It", force it to stay as us.
        // ─────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(GorillaTagManager), "Update")]
        [HarmonyPostfix]
        private static void Postfix_TagManagerUpdate(GorillaTagManager __instance)
        {
            if (!AlwaysIt) return;

            // currentIt is the field that holds who is "it" — confirm in dnSpy
            // __instance.currentIt = Photon.Pun.PhotonNetwork.LocalPlayer;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SPEED MULTIPLIER
        // Postfix on Player.LateUpdate to scale the rigidbody velocity.
        // This runs every frame; SpeedMod of 1 = no change.
        // ─────────────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(GorillaLocomotion.Player), "LateUpdate")]
        [HarmonyPostfix]
        private static void Postfix_PlayerLateUpdate(GorillaLocomotion.Player __instance)
        {
            if (SpeedMod <= 1f) return;

            var rb = __instance.GetComponent<Rigidbody>();
            if (rb == null) return;

            // Only boost horizontal velocity to avoid fighting gravity
            Vector3 vel = rb.velocity;
            Vector3 flat = new Vector3(vel.x, 0, vel.z);
            if (flat.magnitude < 0.1f) return;
            flat = flat.normalized * (flat.magnitude * SpeedMod);
            rb.velocity = new Vector3(flat.x, vel.y, flat.z);
        }
    }
}
