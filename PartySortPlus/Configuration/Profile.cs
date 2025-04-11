using System;
using System.Collections.Generic;

namespace PartySortPlus.Configuration
{
    public class Profile
    {
        [NonSerialized] internal string GUID = Guid.NewGuid().ToString();
        public string Name = "";
        public List<ApplyRule> Rules = [];
        public List<Preset> Presets = [];
        public Preset? SelectedPreset = null;
        public bool isEditingPresetName = false;
    }
}
