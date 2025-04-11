using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Serilog;

namespace PartySortPlus.Utility
{
    internal static unsafe class PartyList
    {
        public unsafe static Span<HudPartyMember> GetPartyMembers()
        {
            return AgentHUD.Instance()->PartyMembers;
        }

        public unsafe static void PrintPartyMemberDetails()
        {
            var partyList = AgentHUD.Instance()->PartyMembers;

            if (partyList.Length == 0)
            {
                Log.Information("No party members found.");
                return;
            }

            foreach (ref var member in partyList)
            {
                var name = member.Name.ToString();
                var index = member.Index;
                var battleChara = member.Object;

                if (battleChara != null)
                {
                    Log.Information($"Party Member Index: {index}, Name: {name}, ClassJob: {battleChara->ClassJob.ToString()}");
                }
            }
        }
    }
}
