using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaTagModMenu.Menu
{
    public class ModMenuManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static ModMenuManager Instance { get; private set; }

        // ── Settings ──────────────────────────────────────────────────────────
        private const KeyCode ToggleKey      = KeyCode.Insert;
        private const int     ButtonsPerPage = 7;

        // ── State ─────────────────────────────────────────────────────────────
        private bool   _open;
        private int    _page;
        private int    _catIndex;   // 0 = All
        private Rect   _win         = new Rect(16, 16, 300, 480);
        private Vector2 _scroll;

        private readonly List<ModButton> _buttons    = new List<ModButton>();
        private readonly List<string>    _categories = new List<string>();

        // ── Styles (built once) ───────────────────────────────────────────────
        private GUIStyle _styleTitle;
        private GUIStyle _styleBtnName;
        private GUIStyle _styleBtnDesc;
        private GUIStyle _styleFooter;
        private bool     _stylesBuilt;

        // ── Textures ──────────────────────────────────────────────────────────
        private Texture2D _texDark;
        private Texture2D _texGreen;
        private Texture2D _texRed;
        private Texture2D _texGrey;
        private Texture2D _texHeader;

        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
            BuildTextures();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void Register(ModButton btn)
        {
            _buttons.Add(btn);
            if (!_categories.Contains(btn.Category))
                _categories.Add(btn.Category);
        }

        public void RegisterRange(IEnumerable<ModButton> btns)
        {
            foreach (var b in btns) Register(b);
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
                _open = !_open;

            foreach (var b in _buttons)
                b.RunUpdate();
        }

        private void OnGUI()
        {
            if (!_open) return;

            if (!_stylesBuilt) BuildStyles();

            // Dim the background
            GUI.color = new Color(0, 0, 0, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texDark);
            GUI.color = Color.white;

            _win = GUILayout.Window(9999, _win, DrawWindow, "", GUIStyle.none);
        }

        // ── Window ────────────────────────────────────────────────────────────

        private void DrawWindow(int id)
        {
            // Background
            GUI.DrawTexture(new Rect(0, 0, _win.width, _win.height), _texDark);

            GUILayout.Space(4);

            // ── Title bar ──
            GUI.DrawTexture(new Rect(0, 28, _win.width, 2), _texGreen);
            GUILayout.Label("  🦍  GORILLA TAG MOD MENU", _styleTitle);
            GUI.DrawTexture(new Rect(0, 54, _win.width, 2), _texGreen);

            GUILayout.Space(6);

            // ── Category tabs ──
            var allCats = new[] { "All" }.Concat(_categories).ToArray();
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            for (int i = 0; i < allCats.Length; i++)
            {
                bool active = i == _catIndex;
                GUI.backgroundColor = active ? new Color(0.2f, 0.75f, 0.35f) : new Color(0.18f, 0.18f, 0.20f);
                if (GUILayout.Button(allCats[i], GUILayout.Height(22)))
                {
                    _catIndex = i;
                    _page     = 0;
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(4);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // ── Button list ──
            var visible   = GetFiltered();
            int pageCount = Mathf.Max(1, Mathf.CeilToInt(visible.Count / (float)ButtonsPerPage));
            _page = Mathf.Clamp(_page, 0, pageCount - 1);

            _scroll = GUILayout.BeginScrollView(_scroll,
                false, false,
                GUIStyle.none, GUIStyle.none,
                GUILayout.Height(ButtonsPerPage * 52));

            int start = _page * ButtonsPerPage;
            int end   = Mathf.Min(start + ButtonsPerPage, visible.Count);
            for (int i = start; i < end; i++)
                DrawButton(visible[i]);

            GUILayout.EndScrollView();

            GUILayout.Space(4);

            // ── Pagination ──
            if (pageCount > 1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUI.enabled = _page > 0;
                if (GUILayout.Button("◀", GUILayout.Width(36), GUILayout.Height(22))) _page--;
                GUI.enabled = true;
                GUILayout.Label($"Page {_page + 1} / {pageCount}", _styleFooter);
                GUI.enabled = _page < pageCount - 1;
                if (GUILayout.Button("▶", GUILayout.Width(36), GUILayout.Height(22))) _page++;
                GUI.enabled = true;
                GUILayout.Space(8);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            // ── Footer ──
            int activeCount = _buttons.Count(b => b.IsEnabled);
            GUILayout.Label($"  Active: {activeCount} / {_buttons.Count}   |   [INSERT] to close", _styleFooter);
            GUILayout.Space(6);

            GUI.DragWindow();
        }

        private void DrawButton(ModButton btn)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);

            // Row background
            GUI.DrawTexture(
                new Rect(6, GUILayoutUtility.GetLastRect().y, _win.width - 12, 48),
                btn.IsEnabled ? _texGreen : _texGrey);

            // Status dot
            GUI.color = btn.IsEnabled ? Color.green : new Color(0.5f, 0.5f, 0.5f);
            GUILayout.Label(btn.IsEnabled ? "●" : "○",
                new GUIStyle(_styleBtnName) { fontSize = 18 },
                GUILayout.Width(22), GUILayout.Height(48));
            GUI.color = Color.white;

            // Name + description
            GUILayout.BeginVertical(GUILayout.Height(48));
            GUILayout.Space(6);
            GUILayout.Label(btn.Name,        _styleBtnName);
            GUILayout.Label(btn.Description, _styleBtnDesc);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Toggle
            GUI.backgroundColor = btn.IsEnabled ? new Color(0.85f, 0.15f, 0.15f) : new Color(0.15f, 0.65f, 0.25f);
            if (GUILayout.Button(btn.IsEnabled ? "OFF" : " ON",
                    GUILayout.Width(46), GUILayout.Height(48)))
                btn.Toggle();

            GUI.backgroundColor = Color.white;
            GUILayout.Space(6);
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private List<ModButton> GetFiltered()
        {
            if (_catIndex == 0) return _buttons;
            string cat = _categories[_catIndex - 1];
            return _buttons.Where(b => b.Category == cat).ToList();
        }

        // ── Asset building ────────────────────────────────────────────────────

        private void BuildTextures()
        {
            _texDark   = MakeTex(new Color(0.08f, 0.08f, 0.10f, 0.97f));
            _texGreen  = MakeTex(new Color(0.10f, 0.40f, 0.18f, 0.55f));
            _texRed    = MakeTex(new Color(0.45f, 0.10f, 0.10f, 0.55f));
            _texGrey   = MakeTex(new Color(0.15f, 0.15f, 0.18f, 0.70f));
            _texHeader = MakeTex(new Color(0.12f, 0.12f, 0.15f, 1.00f));
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void BuildStyles()
        {
            _styleTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = new Color(0.3f, 0.9f, 0.5f) }
            };

            _styleBtnName = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Color.white }
            };

            _styleBtnDesc = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                normal   = { textColor = new Color(0.75f, 0.75f, 0.75f) }
            };

            _styleFooter = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 9,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            };

            _stylesBuilt = true;
        }
    }
}
