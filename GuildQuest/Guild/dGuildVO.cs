using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildVO")]
    public class SettlersGuildVO
    {
        [AmfObjectName("motd")]
        public string motd { get; set; }
        [AmfObjectName("bannerID")]
        public int bannerID { get; set; }
        [AmfObjectName("cacheTimestamp")]
        public double cacheTimestamp { get; set; }
        [AmfObjectName("name")]
        public string name { get; set; }
        [AmfObjectName("log")]
        public ArrayCollection log { get; set; }
        [AmfObjectName("id")]
        public int id { get; set; }
        [AmfObjectName("size")]
        public int size { get; set; }
        [AmfObjectName("maxSize")]
        public int maxSize { get; set; }
        [AmfObjectName("playerPermissions")]
        public SettlersGuildPlayerPermissionVO playerPermissions { get; set; }
        [AmfObjectName("foundTime")]
        public double foundTime { get; set; }
        [AmfObjectName("tag")]
        public string tag { get; set; }
        [AmfObjectName("description")]
        public string description { get; set; }
        [AmfObjectName("ranks")]
        public ArrayCollection ranks { get; set; }
        [AmfObjectName("members")]
        public ArrayCollection members { get; set; }
    }
}