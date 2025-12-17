using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildEditValueVO")]
    public class SettlersGuildEditValueVO
    {
        [AmfObjectName("newValue")]
        public string newValue { get; set; }
        [AmfObjectName("parameters")]
        public ArrayCollection parameters { get; set; }
        [AmfObjectName("type")]
        public int type { get; set; }
    }
}
