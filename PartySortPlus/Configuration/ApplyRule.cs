using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace PartySortPlus.Configuration
{
    [Serializable]
    public class ApplyRule
    {
        [NonSerialized] internal string GUID = Guid.NewGuid().ToString();
        public bool Enabled = true;

        public List<uint> Territories = [];
        public List<Job> Jobs = [];
        public List<Job> PartyJobs = [];
        public List<string> SelectedPresets = [];

        public NotConditions Not = new();

        [Serializable]
        public class NotConditions
        {
            public List<uint> Territories = [];
            public List<Job> Jobs = [];
            public List<Job> PartyJobs = [];
        }
    }
}
