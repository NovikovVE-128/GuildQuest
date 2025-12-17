using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest.VO.Guild
{
    [AmfObjectName("Guild.dGuildPlayerPermissionVO")]
    public class SettlersGuildPlayerPermissionVO
    {
        [AmfObjectName("joinRequestAllow")]
        public int joinRequestAllow { get; set; }
        [AmfObjectName("kick")]
        public int kick { get; set; }
        [AmfObjectName("officerNote")]
        public int officerNote { get; set; }
        [AmfObjectName("invite")]
        public int invite { get; set; }
        [AmfObjectName("guildMail")]
        public int guildMail { get; set; }
        [AmfObjectName("officersChannel")]
        public int officersChannel { get; set; }
        [AmfObjectName("motd")]
        public int motd { get; set; }
        [AmfObjectName("ranksAssign")]
        public int ranksAssign { get; set; }
        [AmfObjectName("joinRequestAccept")]
        public int joinRequestAccept { get; set; }
        [AmfObjectName("ranksEdit")]
        public int ranksEdit { get; set; }
        [AmfObjectName("note")]
        public int note { get; set; }
        [AmfObjectName("banner")]
        public int banner { get; set; }
        [AmfObjectName("description")]
        public int description { get; set; }
    }
}