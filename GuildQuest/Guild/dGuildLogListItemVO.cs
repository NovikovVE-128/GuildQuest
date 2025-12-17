using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildLogListItemVO")]
    public class SettlersGuildLogListItemVO
    {
        [AmfObjectName("timestamp")]
        public double timestamp { get; set; }
        [AmfObjectName("identifier")]
        public int identifier { get; set; }
        [AmfObjectName("parameters")]
        public ArrayCollection parameters { get; set; }
    }

}
