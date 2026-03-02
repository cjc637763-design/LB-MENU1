using GorillaTagModMenu.Menu;
using UnityEngine;

namespace GorillaTagModMenu.Mods
{
    public class VisualMods : MonoBehaviour
    {
        private const string CAT = "Visual";

        private float _hue;

        // Default gorilla fur colour
        private static readonly Color DefaultColour = new Color(0.44f, 0.26f, 0.08f);

        private GorillaLocomotion.Player GT => GorillaLocomotion.Player.Instance;

        private void Start()
        {
            ModMenuManager.Instance.RegisterRange(new[]
            {
                /* Rainbow */
                new ModButton("Rainbow", "Cycle through all colours", CAT,
                    onDisable: () => SetColour(DefaultColour),
                    onUpdate:  RainbowUpdate),

                /* Colour: Cyan */
                new ModButton("Cyan Gorilla", "Set colour to bright cyan", CAT,
                    onEnable:  () => SetColour(Color.cyan),
                    onDisable: () => SetColour(DefaultColour)),

                /* Colour: Red */
                new ModButton("Red Gorilla", "Set colour to red", CAT,
                    onEnable:  () => SetColour(Color.red),
                    onDisable: () => SetColour(DefaultColour)),

                /* Big Head */
                new ModButton("Big Head", "Inflate head to 3x size", CAT,
                    onEnable:  () => ScaleHead(3f),
                    onDisable: () => ScaleHead(1f)),

                /* Tiny Gorilla */
                new ModButton("Tiny Gorilla", "Shrink your whole body", CAT,
                    onEnable:  () => ScaleBody(0.3f),
                    onDisable: () => ScaleBody(1f)),

                /* Giant Gorilla */
                new ModButton("Giant Gorilla", "Grow your whole body", CAT,
                    onEnable:  () => ScaleBody(3f),
                    onDisable: () => ScaleBody(1f)),

                /* ESP */
                new ModButton("Player ESP", "See players through walls", CAT,
                    onEnable:  () => SetESP(true),
                    onDisable: () => SetESP(false)),
            });
        }

        // ── Rainbow ───────────────────────────────────────────────────────────

        private void RainbowUpdate()
        {
            _hue = (_hue + Time.deltaTime * 0.4f) % 1f;
            SetColour(Color.HSVToRGB(_hue, 1f, 1f));
        }

        // ── Colour ────────────────────────────────────────────────────────────

        private void SetColour(Color c)
        {
            if (GT == null) return;
            foreach (var r in GT.GetComponentsInChildren<Renderer>())
                if (r.material != null)
                    r.material.color = c;
        }

        // ── Head / Body scale ─────────────────────────────────────────────────

        private void ScaleHead(float s)
        {
            if (GT == null) return;
            // Walk the transform hierarchy to find the head bone
            var head = FindDeep(GT.transform, "head");
            if (head != null) head.localScale = Vector3.one * s;
        }

        private void ScaleBody(float s)
        {
            if (GT == null) return;
            GT.transform.localScale = Vector3.one * s;
        }

        // ── ESP ───────────────────────────────────────────────────────────────

        private void SetESP(bool on)
        {
            var others = FindObjectsOfType<GorillaLocomotion.Player>();
            foreach (var p in others)
            {
                if (p == GT) continue;
                foreach (var r in p.GetComponentsInChildren<Renderer>())
                {
                    if (r.material == null) continue;
                    r.material.SetInt("_ZTest",
                        on ? (int)UnityEngine.Rendering.CompareFunction.Always
                           : (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                }
            }
        }

        // ── Util ──────────────────────────────────────────────────────────────

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name.ToLower().Contains(name.ToLower())) return child;
                var found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
