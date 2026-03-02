using GorillaTagModMenu.Menu;
using UnityEngine;

namespace GorillaTagModMenu.Mods
{
    public class MovementMods : MonoBehaviour
    {
        private const string CAT = "Movement";

        // Tunable values
        private const float SpeedMult      = 3.0f;
        private const float JumpMult       = 3.0f;
        private const float FlySpeed       = 10f;
        private const float SuperGlideSpd  = 22f;

        // Cache
        private GorillaLocomotion.Player GT => GorillaLocomotion.Player.Instance;

        // ─────────────────────────────────────────────────────────────────────

        private void Start()
        {
            ModMenuManager.Instance.RegisterRange(new[]
            {
                /* Speed Boost */
                new ModButton("Speed Boost", $"{SpeedMult}x movement speed", CAT,
                    onEnable:  () => ApplySpeed(SpeedMult),
                    onDisable: () => ApplySpeed(1f)),

                /* High Jump */
                new ModButton("High Jump", $"{JumpMult}x jump height", CAT,
                    onEnable:  () => ApplyJump(JumpMult),
                    onDisable: () => ApplyJump(1f)),

                /* Fly */
                new ModButton("Fly", "WASD + Space/Ctrl to fly freely", CAT,
                    onEnable:  StartFly,
                    onDisable: StopFly,
                    onUpdate:  FlyUpdate),

                /* No Clip */
                new ModButton("No Clip", "Phase through walls", CAT,
                    onEnable:  () => SetCollision(false),
                    onDisable: () => SetCollision(true),
                    onUpdate:  NoClipUpdate),

                /* Super Glide */
                new ModButton("Super Glide", "Rocket forward at max speed", CAT,
                    onUpdate: SuperGlideUpdate),

                /* Freeze */
                new ModButton("Freeze", "Lock yourself in place", CAT,
                    onEnable:  FreezeOn,
                    onDisable: FreezeOff),

                /* Snap to Ground */
                new ModButton("Snap to Ground", "Teleport down to nearest floor", CAT,
                    onEnable: SnapToGround),
            });
        }

        // ── Speed / Jump ──────────────────────────────────────────────────────

        private void ApplySpeed(float mult)
        {
            if (GT == null) return;
            GT.jumpMultiplier = mult;   // jumpMultiplier also affects slide speed in GT
        }

        private void ApplyJump(float mult)
        {
            if (GT == null) return;
            GT.jumpMultiplier = mult;
        }

        // ── Fly ───────────────────────────────────────────────────────────────

        private void StartFly()
        {
            var rb = GetRb();
            if (rb != null) rb.useGravity = false;
        }

        private void StopFly()
        {
            var rb = GetRb();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.velocity   = Vector3.zero;
            }
        }

        private void FlyUpdate()
        {
            var rb  = GetRb();
            var cam = Camera.main?.transform;
            if (rb == null || cam == null) return;

            rb.useGravity = false;

            var dir = Vector3.zero;
            if (Input.GetKey(KeyCode.W))           dir += cam.forward;
            if (Input.GetKey(KeyCode.S))           dir -= cam.forward;
            if (Input.GetKey(KeyCode.A))           dir -= cam.right;
            if (Input.GetKey(KeyCode.D))           dir += cam.right;
            if (Input.GetKey(KeyCode.Space))       dir += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) dir -= Vector3.up;

            rb.velocity = dir.normalized * FlySpeed;
        }

        // ── No Clip ───────────────────────────────────────────────────────────

        private void SetCollision(bool on)
        {
            if (GT == null) return;
            foreach (var col in GT.GetComponentsInChildren<Collider>())
                col.enabled = on;
        }

        private void NoClipUpdate()
        {
            var rb = GetRb();
            if (rb != null) rb.useGravity = false;
        }

        // ── Super Glide ───────────────────────────────────────────────────────

        private void SuperGlideUpdate()
        {
            var rb  = GetRb();
            var cam = Camera.main?.transform;
            if (rb == null || cam == null) return;
            rb.velocity = cam.forward * SuperGlideSpd;
        }

        // ── Freeze ────────────────────────────────────────────────────────────

        private void FreezeOn()
        {
            var rb = GetRb();
            if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void FreezeOff()
        {
            var rb = GetRb();
            if (rb != null) rb.constraints = RigidbodyConstraints.None;
        }

        // ── Snap to Ground ────────────────────────────────────────────────────

        private void SnapToGround()
        {
            if (GT == null) return;
            if (Physics.Raycast(GT.transform.position, Vector3.down, out RaycastHit hit, 200f))
                GT.transform.position = hit.point + Vector3.up * 0.6f;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private Rigidbody GetRb() => GT?.GetComponent<Rigidbody>();
    }
}
