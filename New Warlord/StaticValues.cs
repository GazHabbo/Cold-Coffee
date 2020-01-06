using System;
using System.Collections.Generic;
using System.Text;

namespace Warlord
{
    class StaticValues
    {
        internal static string DisconnectMessage;

        public StaticValues(FrmMain mn, MySQL_Manager MySQL)
        {
            DisconnectMessage = MySQL.RunMySQLQuery("SELECT `text` FROM `static` WHERE `value` = 'dcmessage'", "text");
        }
    }
}
