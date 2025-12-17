using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx.AMF3;
using FluorineFx.Attributes;

namespace GuildQuest
{
    [AmfObjectName("dServerCall")]
    public class SettlersServerCall
    {
        public static int PlayerZoneId = 0;
        public static string AuthToken;
        public static int AuthUser;


        [AmfObjectName("dsoAuthToken")]
        public string dsoAuthToken { get; set; }
        [AmfObjectName("type")]
        public int type { get; set; }
        [AmfObjectName("zoneID")]
        public int zoneID { get; set; }
        [AmfObjectName("dsoAuthUser")]
        public int dsoAuthUser { get; set; }
        [AmfObjectName("data")]
        public object data { get; set; }
        [AmfObjectName("dsoAuthRandomClientID")]
        public int dsoAuthRandomClientID { get; set; }

        public SettlersServerCall(int message, object dataObject)
        {
            type = message;
            zoneID = PlayerZoneId;
            data = dataObject;
            dsoAuthUser = AuthUser;
            dsoAuthToken = AuthToken;
            int rand = new Random().Next() * 2147483646;
            if (rand < 0) rand *= -1;
            dsoAuthRandomClientID = rand;
        }
    }
}