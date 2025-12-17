using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO
{
    [AmfObjectName("dIntegerVO")]
    public class dIntegerVO
    {
        [AmfObjectName("value")]
        public int value { get; set; }
    }
}
 