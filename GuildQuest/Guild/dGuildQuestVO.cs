using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildQuestVO")]
    public class SettlersGuildQuestVO
    {
        [AmfObjectName("status")]
        public int status { get; set; }
        [AmfObjectName("uniqueID")]
        public int uniqueID { get; set; }
        [AmfObjectName("questname")]
        public string questname { get; set; }
    }
}
