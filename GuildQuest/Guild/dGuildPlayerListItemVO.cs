using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild 
{
    [AmfObjectName("Guild.dGuildPlayerListItemVO")]
    public class SettlersGuildPlayerListItemVO : SettlersPlayerListItemVO
    {
        [AmfObjectName("rankID")]
        public int rankID { get; set; }
        [AmfObjectName("note")]
        public string note { get; set; }
        [AmfObjectName("officerNote")]
        public string officerNote { get; set; }
        [AmfObjectName("quest")]
        public SettlersGuildQuestVO quest { get; set; }
        [AmfObjectName("onlineLast24")]
        public bool onlineLast24 { get; set; }
    }
}
