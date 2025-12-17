using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildUpdateVO")]
    public class SettlersGuildUpdateVO
    {
        [AmfObjectName("guild")]
        public SettlersGuildVO guild { get; set; }
    }
}
