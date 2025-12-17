using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildRankListItemVO")]
    public class SettlersGuildRankListItemVO
    {
        [AmfObjectName("id")]
        public int id { get; set; }
        [AmfObjectName("name")]
        public string name { get; set; }
        [AmfObjectName("position")]
        public int position { get; set; }
    }
}
