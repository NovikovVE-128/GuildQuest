using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildHeadersListVO")]
    public class SettlersGuildHeadersListVO
    {
        [AmfObjectName("list")]
        public ArrayCollection list { get; set; }
        [AmfObjectName("page")]
        public int page { get; set; }
        [AmfObjectName("maxPages")]
        public int maxPages { get; set; }
    }

}
