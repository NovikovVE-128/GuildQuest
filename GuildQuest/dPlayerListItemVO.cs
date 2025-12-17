using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO
{
    [AmfObjectName("dPlayerListItemVO")]
    public class SettlersPlayerListItemVO
    {
        [AmfObjectName("onlineStatus")]
        public bool onlineStatus { get; set; }
        [AmfObjectName("adventureVO")]
        public object adventureVO { get; set; }
        [AmfObjectName("username")]
        public string username { get; set; }
        [AmfObjectName("avatarId")]
        public int avatarId { get; set; }
        [AmfObjectName("id")]
        public int id { get; set; }
        [AmfObjectName("playerLevel")]
        public int playerLevel { get; set; }
        [AmfObjectName("friendSince")]
        public double friendSince { get; set; }
    }
}
 