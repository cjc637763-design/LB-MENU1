using System;

namespace GorillaTagModMenu.Menu
{
    public class ModButton
    {
        public string Name        { get; }
        public string Description { get; }
        public string Category    { get; }
        public bool   IsEnabled   { get; private set; }

        private readonly Action _onEnable;
        private readonly Action _onDisable;
        private readonly Action _onUpdate;

        public ModButton(string name, string description, string category,
                         Action onEnable = null, Action onDisable = null, Action onUpdate = null)
        {
            Name        = name;
            Description = description;
            Category    = category;
            _onEnable   = onEnable;
            _onDisable  = onDisable;
            _onUpdate   = onUpdate;
        }

        public void Toggle()
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled) _onEnable?.Invoke();
            else           _onDisable?.Invoke();
        }

        public void RunUpdate()
        {
            if (IsEnabled) _onUpdate?.Invoke();
        }
    }
}
