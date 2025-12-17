using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest
{
    [AmfObjectName("dServerActionResult")]
    public class SettlersServerActionResult
    {
        [AmfObjectName("errorCode")]
        public int errorCode { get; set; }
        [AmfObjectName("clientTime")]
        public double clientTime { get; set; }
        [AmfObjectName("data")]
        public object data { get; set; }
    }
}
