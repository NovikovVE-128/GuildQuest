using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest
{
    [AmfObjectName("dServerResponse")]
    public class SettlersServerResponse
    {
        [AmfObjectName("type")]
        public int type { get; set; }
        [AmfObjectName("data")]
        public SettlersServerActionResult data { get; set; }

    }
}
