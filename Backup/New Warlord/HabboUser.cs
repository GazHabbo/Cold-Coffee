using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Warlord
{
    public class HabboUser
    {
        internal Socket MySocket;
        internal int MyID;
        internal AsyncCallback pfnWorkerCallBack;
        internal byte[] dataBuffer = new byte[1000];
        internal RC4 Rc4;
        internal HabboEncoding HabboEncodinG = new HabboEncoding();
        private bool EncryptionOn;
        internal MySQL_Manager MySQL;
        internal FrmMain Main;
        internal RoomManager MyRoom;
        internal string Username;
        internal int TradingWithID;
        internal bool IsWalking;
        internal int EnterRoomID;
        internal string Figure;
        internal string Mission;
        internal int HandPage;
        internal string Sex;
        internal string Badge;
        internal int CFH_Total;
        internal bool Seated;
        internal int Level;
        internal int HabboID;
        internal int Credits;
        internal int[] CallerID = new int[2000];
        internal string[] CFH_Message = new string[2000];
        internal int[] CFH_RoomID = new int[2000];
        internal bool IsInTrade;
        internal int MyRoomID;
        internal bool IsRoomOwner;
        internal string Badges;
        internal int BadgeCount;
        internal string RoomType;
        internal int BadgeStatus;
        internal int GotHC;
        internal int HC_PrepaidMonths;
        internal int HC_PassedMonths;
        internal string MyConsoleRoomName;
        internal string HC_DateBought;
        internal string ConsoleMission;
        internal int ConsMsgCount = 0;

        // Inside a Room Delcarations
        internal int MyX;
        internal int MyY;
        internal int TmpX;
        internal int TmpY;
        internal double MyElevation;
        internal int MyHeadDirection;
        internal int MyBodyDirection;
        internal int MyInnerRoomID;
        internal string StoredHeightmap;
        internal string ShockwaveID;
        internal int GoalX = -1;
        internal int GoalY;
        internal bool SentDg;
        internal bool GotRights;
        internal System.Collections.Specialized.NameValueCollection Statuss = new System.Collections.Specialized.NameValueCollection();
        // End Of Inside a Room Declarations
        internal bool SentBFData = false;
        

        internal HabboUser(Socket mS, int SID, FrmMain MainRef, MySQL_Manager MSRef)
        {
            MySocket = mS;
            MyID = SID;
            Main = MainRef;
            MySQL = MSRef;
            Initiate();
        }

        private void Initiate()
        {
            createMessage("@@");
            WaitForData();
        }

        #region Socket Events
        private void WaitForData()
        {
            if (pfnWorkerCallBack == null)
            {
                if (MySocket.Connected == true)
                {
                    pfnWorkerCallBack = new AsyncCallback(DataRecieved);
                }
            }

            if (MySocket.Connected == true)
            {
                MySocket.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, null);

            }

        }

        private void DataRecieved(IAsyncResult syncc)
        {
            try
            {
                int iRx = 0;
                iRx = MySocket.EndReceive(syncc);
                if (iRx == 0)
                {
                    DropConnection();
                }

                char[] chars = new char[iRx];
                System.Text.Decoder d = System.Text.Encoding.Default.GetDecoder();
                int charLen = d.GetChars(dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);
                if (szData == Convert.ToChar(10).ToString() + Convert.ToChar(13).ToString())
                {
                    DropConnection();
                }

                if (szData.Length == 0)
                {
                    DropConnection();
                }
                if (EncryptionOn == true) { szData = Rc4.deciphper(szData); }
                SeperatePackets(szData);




                WaitForData();
            }
            catch
            {
                GotError();
                WaitForData();
            }
        }


        private void SeperatePackets(string ThePackets)
        {
            string Temp;
            int LengthHeader;
            int Count;
            Temp = ThePackets;
            Count = 0;
        Redo:
            Count++;
            //if (Count > 7)
            //{ 
            //    goto TooMany;
            //}
            LengthHeader = HabboEncodinG.decodeB64(Temp.Substring(1, 2));
            if (Temp.Length <= LengthHeader + 3)
            {
                //RunPacketDelegate delRP2 = new RunPacketDelegate(RunPacket);
               // IAsyncResult tag2 = delRP2.BeginInvoke(Temp, null, null);
                RunPacket(Temp);
            }
            else
            {
                //RunPacketDelegate delRP2 = new RunPacketDelegate(RunPacket);
                //IAsyncResult tag2 = delRP2.BeginInvoke(Temp.Substring(0, LengthHeader + 3), null, null);
                RunPacket(Temp.Substring(0, LengthHeader + 3));
                Temp = Temp.Substring(LengthHeader + 3);
                goto Redo;
            }

        }
        #endregion

        #region Packet Control
        #region Dull Stuff
        internal void DropConnection()
        {
            createMessage("BK" + StaticValues.DisconnectMessage);
            MySocket.Close();
            if (MyRoom != null)
            {
                MyRoom.RemoveUser(MyID);
            }
            MyRoom = null;
            SocketManager.activeSockets.Remove(MyID);
            Main._Debug("Client Disconnected");
            Main._RemoveNode(MyID);
        }

        internal void GotError()
        {
            createMessage("Dk" + DateTime.Now);
        }
        internal delegate void RunPacketDelegate(string ThePacket);
        internal delegate void WalkLoopDelegate();
        #endregion
        private void RunPacket(string ThePacket)
        {
                            string RoomStatus;
                            string RoomDescription;
                            int OwnerID;
                            int ShowOwner;
                            string RoomOwner;
                            string RoomModel;
                            string CatNam;
                            string RoomName;
                            string RoomCName;
                            string RoomCCT;
                            string Tab = "	";
                            int CurIn;
                            int MaxIn;
                            int RoomID;
                            int AllRights;
            #region More Dull Stuff
            Main._Debug(ThePacket);
            string PacketHeader = ThePacket.Substring(3, 2);

            string PacketBuilder;
            int AmmountOfFrs;
            switch (PacketHeader)
            {
            #endregion

                #region PreLogin Stuff
                case "CN":
                    createMessage("DUH");
                    break;

                case "CJ":
                    if (MySQL.RunCountMySQLQuery("SELECT `ip` FROM `bans` WHERE `ip` = '" + MySocket.RemoteEndPoint.ToString().Split(":".ToCharArray())[0] + "'", "ip") > 0)
                    {
                        createMessage("@c" + MySQL.RunMySQLQuery("SELECT `reason` FROM `bans` WHERE `ip` = '" + MySocket.RemoteEndPoint.ToString().Split(":".ToCharArray())[0] + "'", "reason"));
                        DropConnection();
                        break;
                    }
                    createMessage("@Aouqz1evvqy3rng07y4s9wqmgiq426ja2qstv0p9bog89usro0sgairq3hlafmr0nwtuzevyrg749qgq2j2svpbg9sosar3lfrnt");
                    EncryptionOn = true;
                    Rc4 = new RC4("ouqz1evvqy3rng07y4s9wqmgiq426ja2qstv0p9bog89usro0sgairq3hlafmr0nwtuzevyrg749qgq2j2svpbg9sosar3lfrnt");
                    break;

                case "@F": // @@X@F@T44284384838485842007
                    ShockwaveID = ThePacket.Substring(7);
                    if (MySQL.RunCountMySQLQuery("SELECT `shockwave` FROM `bans` WHERE `shockwave` = '" + ShockwaveID + "'", "shockwave") > 0)
                    {
                        createMessage("@c" + MySQL.RunMySQLQuery("SELECT `reason` FROM `bans` WHERE `shockwave` = '" + ShockwaveID + "'", "reason"));
                        DropConnection();
                        break;
                    }
                    break;
                case "Bu":
                    createMessage("DAQBHIIIKHJHPAHQAdd-MM-yyyyRAHSAHPBhttp://hotel-caDBI");
                    break;

                case "CL": // Login Ticket Crap..
                    string LoginTicket;
                    string tmpName;
                    LoginTicket = ThePacket.Substring(7);
                    if (LoginTicket == "#")
                    {
                        DropConnection();
                        break;
                    }
                    tmpName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "habboname");
                    if (tmpName == "")
                    {
                        DropConnection();
                        break;
                    }
                    foreach (HabboUser AlreadyOnUsr in SocketManager.activeSockets.Values)
                    {
                            if (AlreadyOnUsr.Username == tmpName)
                            {
                                AlreadyOnUsr.DropConnection();
                            }
                    }

                    tmpName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "habboname");
                    Username = tmpName;
                    Figure = MySQL.RunMySQLQuery("SELECT `figure` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "figure");
                    Sex = tmpName = MySQL.RunMySQLQuery("SELECT `sex` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "sex");
                    Mission = MySQL.RunMySQLQuery("SELECT `mission` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "mission");
                    Badge = MySQL.RunMySQLQuery("SELECT `curbadge` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "curbadge");
                    ConsoleMission = MySQL.RunMySQLQuery("SELECT `conmission` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "conmission");
                    HabboID = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "id"));
                    GotHC = int.Parse(MySQL.RunMySQLQuery("SELECT `hc` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "hc"));
                    Level = int.Parse(MySQL.RunMySQLQuery("SELECT `level` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "level"));

                    if (Level == 0) // User Is Banned!
                    {
                        string DateGiven = MySQL.RunMySQLQuery("SELECT `datemade` FROM `bans` where `userid` = '" + HabboID + "'", "datemade");
                        int LengthOfBan = int.Parse(MySQL.RunMySQLQuery("SELECT `length` FROM `bans` where `userid` = '" + HabboID + "'", "length"));
                        int DaysLeftOfBan;
                        DateTime TheDateItEnds = DateTime.Parse(DateGiven);
                        TheDateItEnds = TheDateItEnds.Add(System.TimeSpan.FromHours(LengthOfBan));
                        DaysLeftOfBan = int.Parse((DateStuff.DateAndTime.DateDiff(DateStuff.DateInterval.Hour, DateTime.Parse(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString()), TheDateItEnds).ToString()));
                        if (DaysLeftOfBan <= 0)
                        {
                            // Remove Ban!
                            MySQL.RunMySQLQuery("DELETE FROM `bans` WHERE `userid` = '" + HabboID + "'", "");
                            if (GotHC == 1)
                            {
                                HC_PassedMonths = int.Parse(MySQL.RunMySQLQuery("SELECT `passedmonths` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "passedmonths"));
                                if (HC_PrepaidMonths >= 12)
                                {
                                    Level = 3;
                                }
                                else
                                {
                                    Level = 2;
                                }
                            }
                            else
                            {
                                Level = 1;
                            }
                            MySQL.RunMySQLQuery("UPDATE `users` SET `level` = '" + Level + "' WHERE `id` = '" + HabboID + "'", "");

                        }
                        else
                        {
                            createMessage("@c" + MySQL.RunMySQLQuery("SELECT `reason` FROM `bans` WHERE `userid` = '" + HabboID + "'", "reason"));
                            return;
                        }

                    }
                    BadgeStatus = int.Parse(MySQL.RunMySQLQuery("SELECT `badgestat` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "badgestat"));
                    Credits = int.Parse(MySQL.RunMySQLQuery("SELECT `credits` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "credits"));
                    Badges = MySQL.RunMySQLQuery("SELECT `name` FROM `badges` WHERE `ownerid` = '" + HabboID + "'", "name");

                    MyConsoleRoomName = "On hotel view";
                    NoStatus();
                    if (GotHC == 1)
                    {
                        HC_PassedMonths = int.Parse(MySQL.RunMySQLQuery("SELECT `passedmonths` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "passedmonths"));
                        HC_PrepaidMonths = int.Parse(MySQL.RunMySQLQuery("SELECT `prepaidmonths` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "prepaidmonths"));
                        HC_DateBought = MySQL.RunMySQLQuery("SELECT `datebought` FROM `users` WHERE `ssoticket` = '" + LoginTicket + "'", "datebought");
                    }

                    Badges.Replace(Convert.ToChar(5), Convert.ToChar(2));
                    //MySQL.RunMySQLQuery("UPDATE `users` SET `ssoticket` = '#' WHERE `id` = '" + HabboID + "'", "");

                    string Fuses;
                    Fuses = MySQL.RunMySQLQuery("SELECT `code` FROM `fuse` WHERE `level` <= '" + Level + "'", "code");
                    Fuses = Fuses.Replace(Convert.ToChar(5).ToString(), "");
                    createMessage("@B" + Fuses);
                    createMessage("DbIH");
                    createMessage("@C");
                    MySQL.RunMySQLQuery("UPDATE `users` SET `shockwave` = '" + ShockwaveID + "' WHERE `id` = '" + HabboID + "'", "");
                    MySQL.RunMySQLQuery("UPDATE `users` SET `ip` = '" + MySocket.RemoteEndPoint.ToString().Split(":".ToCharArray())[0] + "' WHERE `id` = '" + HabboID + "'", "");
                    break;
                #endregion
                #region Loading of Habbo details
                case "@G": // User Details

                    createMessage("@E" + HabboID + "" + Username + "" + Figure + "" + Sex + "" + Mission + "Hch=s01/53,51,44HH");
                    Main._RemoveNode(MyID);
                    if (Level < 7)
                    {
                        Main.NewNode("Users",Username, MyID );
                    }
                    else
                    {
                        Main.NewNode( "Staff",Username, MyID);
                    }
                    break;

                case "@H": // Get Credits
                    createMessage("@F" + Credits + ".0");
                    break;

                case "B]": // Get Available Badges
                    BadgeCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `badges` WHERE `ownerid` = '" + HabboID + "'", "name");
                    createMessage("Ce" + HabboEncodinG.encodeVL64(BadgeCount).ToString() + Badges.Replace(Convert.ToChar(5), Convert.ToChar(2)) + Convert.ToChar(2) + HabboEncodinG.encodeVL64(BadgeNumber(Badge)).ToString() + HabboEncodinG.encodeVL64(BadgeStatus));
                    break;
                case "@Z": // Get HC Info
                    int DaysLeft;
                    if (HC_DateBought == "" || HC_DateBought == null)
                    {
                        DaysLeft = 31;
                    }
                    else
                    {
                        DaysLeft = 31 - (31 - DayDiff(HC_DateBought));
                    }
                    if (DaysLeft >= 31)
                    {
                        if (HC_PrepaidMonths >= 1)
                        {

                            MySQL.RunMySQLQuery("UPDATE `users` SET `prepaidmonths` = '" + (HC_PrepaidMonths - 1) + "' WHERE `habboname` = '" + Username + "'", "");
                            HC_PrepaidMonths--;
                            MySQL.RunMySQLQuery("UPDATE `users` SET `datebought` = '" + DateTime.Now.ToShortDateString() + "' WHERE `habboname` = '" + Username + "'", "");
                            HC_DateBought = DateTime.Now.ToShortDateString();
                            HC_PassedMonths++;
                            MySQL.RunMySQLQuery("UPDATE `users` SET `passedmonths` = '" + HC_PassedMonths + "' WHERE `habboname` = '" + Username + "'", "");
                            if (HC_PassedMonths >= 12)
                            {
                                // Give Gold HC
                                if (Level < 3)
                                {
                                    Level = 3;
                                    MySQL.RunMySQLQuery("UPDATE `users` SET `level` = '3' WHERE `habboname` = '" + Username + "'", "");
                                }
                                MySQL.RunMySQLQuery("INSERT INTO `badges` (`name`, `ownerid`) VALUES ('HC2', '" + HabboID + "')", "");
                            }
                        }
                        else
                        {
                            // Remove HC
                            if (GotHC == 1)
                            {
                                GotHC = 0;
                                HC_PassedMonths++;
                                MySQL.RunMySQLQuery("UPDATE `users` SET `passedmonths` = '" + HC_PassedMonths + "' WHERE `habboname` = '" + Username + "'", "");
                                MySQL.RunMySQLQuery("UPDATE `users` SET `datebought` = '' WHERE `habboname` = '" + Username + "'", "");
                                MySQL.RunMySQLQuery("DELETE FROM `badges` WHERE `ownerid` = '" + HabboID + "' AND `badge` = 'HC1' OR `badge` = 'HC2')", "");
                                HC_DateBought = "";
                            }

                        }
                    }
                    createMessage("@Gclub_habbo" + Convert.ToChar(2) + HabboEncodinG.encodeVL64(31 - DaysLeft) + HabboEncodinG.encodeVL64(HC_PassedMonths) + HabboEncodinG.encodeVL64(HC_PrepaidMonths) + "I");
                    int CatCount;
                    string TmpBuilder = "";
                    CatCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '1' AND `ispubcat` <= '" + Level + "'", "name");

                    for (int y = 0; y < CatCount; y++)
                    {
                        TmpBuilder += HabboEncodinG.encodeVL64(int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `iscat` = '1' AND `ispubcat` <= '" + Level + "'", "id", y))) + MySQL.SpecificMySQLQueryOutput("SELECT `name` FROM `private` WHERE `iscat` = '1' AND `ispubcat` <= '" + Level + "'", "name", y) + "";
                    }
                    createMessage("C]" + (HabboEncodinG.encodeVL64(CatCount + 1)) + "HNo Floor." + TmpBuilder);
                    break;

                case "@L": // Set All Console Data
                    string SearchName;
                    string SearchFigure;
                    string SearchLastLogin;
                    string SearchUsaname;
                    string SearchMission;
                    string SearchRoom = "";


                    // Build Friends List
                    int AmOfFriends;
                    int OthersID = 0;
                    int FriendOn = 0;
                    SearchRoom = "";

                    AmOfFriends = MySQL.RunCountMySQLQuery("SELECT `userid` FROM `friendslist` WHERE `userid` = '" + HabboID + "'", "userid");
                    if (AmOfFriends > 0)
                    {
                        PacketBuilder = "@LXVBPrXVB" + HabboEncodinG.encodeVL64(AmOfFriends);
                        for (int y = 0; y < AmOfFriends; y++)
                        {
                            OthersID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `friendid` FROM `friendslist` WHERE `userid` = '" + HabboID + "'", "friendid", y));
                            for (int x = 0; x < FrmMain.MaxConnections; x++)
                            {
                                if (Warlord.SocketManager.activeSockets.ContainsKey(x) == true)
                                {
                                    if (Warlord.SocketManager.GetInstance(x).HabboID == OthersID)
                                    {
                                        FriendOn = 1;
                                        SearchRoom = Warlord.SocketManager.GetInstance(x).MyConsoleRoomName;
                                    }
                                }
                            }
                            SearchName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OthersID + "'", "habboname");
                            SearchFigure = MySQL.RunMySQLQuery("SELECT `figure` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "figure");
                            SearchLastLogin = MySQL.RunMySQLQuery("SELECT `lastlogin` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "lastlogin");
                            SearchUsaname = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "habboname");
                            SearchMission = MySQL.RunMySQLQuery("SELECT `conmission` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "conmission");
                            if (FriendOn == 1)
                            {
                                PacketBuilder = PacketBuilder + HabboEncodinG.encodeVL64(OthersID) + SearchName + "I" + SearchMission + "I" + SearchRoom + "" + SearchLastLogin + "" + SearchFigure + "";
                            }
                            else
                            {
                                PacketBuilder = PacketBuilder + HabboEncodinG.encodeVL64(OthersID) + SearchName + "I" + SearchMission + "H" + "" + SearchLastLogin + "" + SearchFigure + "";
                            }
                            createMessage(PacketBuilder);

                        }
                    }
                    else
                    {
                        createMessage("@L");
                    }
                    createMessage("BS" + ConsoleMission + Convert.ToChar(2).ToString());

                    // Grab Friend Requests;
                    AmmountOfFrs = MySQL.RunCountMySQLQuery("SELECT `userid` FROM `friendrequests` WHERE `userid` = '" + HabboID + "'", "userid");

                    int RequesterID;
                    string RequesterName;
                    for (int x = 0; x < AmmountOfFrs; x++)
                    {
                        RequesterID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `requesterid` FROM `friendrequests` WHERE `userid` = '" + HabboID + "'", "requesterid", x));
                        RequesterName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + RequesterID + "'", "habboname");
                        createMessage("BD" + HabboEncodinG.encodeVL64(x) + RequesterName + Convert.ToChar(2));
                    }

                    // Grab Messages
                    int AmConsoleMsgs = 0;
                    string ConsMsg;
                    int ConsMsgID = 0;
                    int SenderID = 0;
                    int msgID = 0;
                    string ConsMsgSent;
                    PacketBuilder = "";
                    AmConsoleMsgs = MySQL.RunCountMySQLQuery("SELECT `froid` FROM `ConsoleMessages` WHERE `toid` = '" + HabboID + "'", "froid");

                    for (int x = 0; x < AmConsoleMsgs; x++)
                    {
                        SenderID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `froid` FROM `ConsoleMessages` WHERE `toid` = '" + HabboID + "'", "froid", x));
                        ConsMsg = MySQL.SpecificMySQLQueryOutput("SELECT `message` FROM `ConsoleMessages` WHERE `toid` = '" + HabboID + "'", "message", x);
                        msgID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `ConsoleMessages` WHERE `toid` = '" + HabboID + "'", "id", x));
                        ConsMsgSent = MySQL.SpecificMySQLQueryOutput("SELECT `sent` FROM `ConsoleMessages` WHERE `toid` = '" + HabboID + "'", "sent", x);
                        PacketBuilder += HabboEncodinG.encodeVL64(msgID) + HabboEncodinG.encodeVL64(SenderID) + ConsMsgSent + "" + ConsMsg + "";
                    }
                    if (AmConsoleMsgs > 0)
                    {
                        createMessage("BF" + HabboEncodinG.encodeVL64(AmConsoleMsgs) + PacketBuilder);
                    }

                    break;
                #endregion
                #region Mod Tool
                case "CH":
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    int TempInt;
                    if (Level < 5) { break; }
                    string ModActionType;
                    ModActionType = ThePacket.Substring(5, 2);
                    switch (ModActionType)
                    {

                        case "HJ": // Ban User (@@\CHHJ@FReason@EExtra@DNameJHH)
                            int BanMsgLength;
                            string BanReason;
                            BanMsgLength = HabboEncodinG.decodeB64(ThePacket.Substring(7, 2));
                            BanReason = ThePacket.Substring(9, BanMsgLength);
                            int ExtraLength;
                            string ExtraText;
                            ExtraLength = HabboEncodinG.decodeB64(ThePacket.Substring(9 + BanMsgLength, 2));
                            ExtraText = ThePacket.Substring(11 + BanMsgLength, ExtraLength);
                            int BanNameLength;
                            string BanName;
                            BanNameLength = HabboEncodinG.decodeB64(ThePacket.Substring(11 + BanMsgLength + ExtraLength, 2));
                            BanName = ThePacket.Substring(13 + BanMsgLength + ExtraLength, BanNameLength);

                            int IPBan = HabboEncodinG.decodeVL64(ThePacket.Substring(ThePacket.Length - 1, 1));
                            int ShockwaveBan = HabboEncodinG.decodeVL64(ThePacket.Substring(ThePacket.Length -2,1));
                        // Check User Exists
                            TempInt = MySQL.RunCountMySQLQuery("SELECT `username` FROM `users` WHERE `username` = '" + BanName.ToLower() + "'", "username");

                            if (TempInt == 0) { createMessage("BKUser does not exist"); break; } // Username not found

                            int banuserid;
                            banuserid = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `users` WHERE `username` = '" + BanName.ToLower() + "'", "id"));

                            if (MySQL.RunCountMySQLQuery("SELECT `ip` FROM `bans` WHERE `userid` = '" + banuserid + "'", "ip") > 0)
                                break;

                            int BanLength;
                            BanLength = HabboEncodinG.decodeVL64(ThePacket.Substring(13 + BanMsgLength + ExtraLength + BanNameLength).ToCharArray());
                            MySQL.RunMySQLQuery("INSERT INTO `bans` (`datemade`, `length`, `userid`, `reason`, `givenbyid`) VALUES ('" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "', '" + BanLength + "', '" + banuserid + "', '" + BanReason + "', '" + HabboID + "')", "");
                            MySQL.RunMySQLQuery("UPDATE `users` SET `level` = '0' WHERE `id` = '" + banuserid + "'", "");
                            SendToUser(BanName, "@c" + BanReason);

                            if (IPBan == 1)
                            {
                                string HabbosIP = MySQL.RunMySQLQuery("SELECT `ip` FROM `users` WHERE `username` = '" + BanName.ToLower() + "'","ip");
                                if (HabbosIP != "")
                                {
                                    MySQL.RunMySQLQuery("UPDATE `bans` SET `ip` = '" + HabbosIP + "' WHERE `userid` = '" + banuserid + "'", "");
                                }
                            }

                            if (ShockwaveBan == 1)
                            {
                                string HabbosShockwave = MySQL.RunMySQLQuery("SELECT `shockwave` FROM `users` WHERE `username` = '" + BanName.ToLower() + "'", "shockwave");
                                if (HabbosShockwave != "")
                                {
                                    MySQL.RunMySQLQuery("UPDATE `bans` SET `shockwave` = '" + HabbosShockwave + "' WHERE `userid` = '" + banuserid + "'", "");
                                }
                            }
                            //DropUser(BanName);
                            break;

                        case "HH": // alert user (@@ZCHHH@GMessage@EExtra@DName)
                            BanMsgLength = HabboEncodinG.decodeB64(ThePacket.Substring(7, 2));
                            BanReason = ThePacket.Substring(9, BanMsgLength);
                            ExtraLength = HabboEncodinG.decodeB64(ThePacket.Substring(9 + BanMsgLength, 2));
                            ExtraText = ThePacket.Substring(11 + BanMsgLength, ExtraLength);
                            BanNameLength = HabboEncodinG.decodeB64(ThePacket.Substring(11 + BanMsgLength + ExtraLength, 2));
                            BanName = ThePacket.Substring(13 + BanMsgLength + ExtraLength, BanNameLength);
                            SendToUser(BanName, "B!" + BanReason);
                            break;
                        case "HI": // Kick User
                            BanMsgLength = HabboEncodinG.decodeB64(ThePacket.Substring(7, 2));
                            BanReason = ThePacket.Substring(9, BanMsgLength);
                            ExtraLength = HabboEncodinG.decodeB64(ThePacket.Substring(9 + BanMsgLength, 2));
                            ExtraText = ThePacket.Substring(11 + BanMsgLength, ExtraLength);
                            BanNameLength = HabboEncodinG.decodeB64(ThePacket.Substring(11 + BanMsgLength + ExtraLength, 2));
                            BanName = ThePacket.Substring(13 + BanMsgLength + ExtraLength, BanNameLength);
                            SendToUser(BanName, "@amod_warn/" + BanReason);
                            foreach (int s in SocketManager.activeSockets.Keys)
                            {
                                HabboUser N = SocketManager.GetInstance(s);
                                if (N.Username.ToLower() == BanName.ToLower())
                                {
                                    MyRoom.RemoveUser(N.MyID);
                                    break;
                                }
                            }
                            break;
                        case "IH": // Room Alert (@@TCHIH@GMessage@EExtra)
                            BanMsgLength = HabboEncodinG.decodeB64(ThePacket.Substring(7, 2));
                            BanReason = ThePacket.Substring(9, BanMsgLength);
                            ExtraLength = HabboEncodinG.decodeB64(ThePacket.Substring(9 + BanMsgLength, 2));
                            ExtraText = ThePacket.Substring(11 + BanMsgLength, ExtraLength);
                            MyRoom.createMessage("B!" + BanReason + Chr(2));
                            break;
                        case "II": // Room Kick 
                            BanMsgLength = HabboEncodinG.decodeB64(ThePacket.Substring(7, 2));
                            BanReason = ThePacket.Substring(9, BanMsgLength);
                            ExtraLength = HabboEncodinG.decodeB64(ThePacket.Substring(9 + BanMsgLength, 2));
                            ExtraText = ThePacket.Substring(11 + BanMsgLength, ExtraLength);
                            MyRoom.createMessage("@amod_warn/" + BanReason);
                            foreach (int s in SocketManager.activeSockets.Keys)
                            {
                                HabboUser N = SocketManager.GetInstance(s);
                                if (N.Level < 4)
                                    MyRoom.RemoveUser(N.MyID);
                            }
                            break;
                    }
                    break;
                #endregion
                #region Call for Help

                case "Cm": // Opening "write cfh"
                    createMessage("D" + Chr(127) + "H");
                    break;

                case "AV": // Sending a cfh
                        ThePacket = ThePacket.Replace(Chr(13), "");
                        ThePacket = ThePacket.Replace(Chr(1), "");
                        ThePacket = ThePacket.Replace(Chr(2), "");
                        ThePacket = ThePacket.Replace(Chr(5), "");
                        ThePacket = ThePacket.Replace(Chr(10), "");
                        int TheLength = ThePacket.Length - 8;
                        string PubName;
                        if (MyRoom.RoomsID < 100)
                        {
                            PubName = MySQL.RunMySQLQuery("SELECT `Name` FROM `public` WHERE `id` = '" + MyRoom.RoomsID + "'", "Name");
                        }
                        else
                        {
                            PubName = "Private Room";
                        }
                        string TheMessage = ThePacket.Substring(7, TheLength);
                        
                         // <-- Thats why, its adding it to MY CallerID Array, not yours. :P

                        foreach (int s in SocketManager.activeSockets.Keys)
                        {
                            HabboUser TheHabbo = SocketManager.GetInstance(s);
                            if (TheHabbo.Level >= 4)
                            {
                                SocketManager.GetInstance(s).CFH_RoomID[CFH_Total] = MyRoom.RoomsID;
                                SocketManager.GetInstance(s).CallerID[CFH_Total] = HabboID;
                                SocketManager.GetInstance(s).CFH_Message[CFH_Total] = ThePacket.Substring(7, TheLength);
                                SocketManager.GetInstance(s).createMessage("BT" + HabboEncodinG.encodeVL64(SocketManager.GetInstance(s).CFH_Total) + Chr(2) + "IOn " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + Chr(2) + Username + Chr(2) + TheMessage + Chr(2) + "J" + Chr(2) + PubName + Chr(2) + "I");
                                SocketManager.GetInstance(s).CFH_Total++;
                            }
                        }
                        createMessage("EAH" + Chr(2)); // Confirm sent screen.
                    break;

                case "@p": // Pick up CFH
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    int CFH_ID = HabboEncodinG.decodeVL64(ThePacket.Substring(7));
                    string TheSentMessage = CFH_Message[CFH_ID];
                    foreach (int s in SocketManager.activeSockets.Keys)
                    {
                        HabboUser TheHabbo = SocketManager.GetInstance(s);
                        if (TheHabbo.Level >= 4)
                        {
                            SocketManager.GetInstance(s).createMessage("BT" + HabboEncodinG.encodeVL64(CFH_ID) + Chr(2) + "IPicked up by " + Username + " at " + DateTime.Now.ToShortTimeString() + Chr(2) + Username + Chr(2) + TheSentMessage + Chr(2) + "J" + Chr(2) + "Picked up" + Chr(2) + "I");
                        }
                        else
                        {
                            //SocketManager.GetInstance(GetTheirID).HabboID = GetTheirID;
                            //SocketManager.GetInstance(s).createMessage("DR" + TheMessage + Chr(2));
                        }
                    }
                    break;

                case "CG": // CFH pick up and reply
                    // 7 for id
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    // Haha, ya :P
                    int TheirID = HabboEncodinG.decodeVL64(ThePacket.Substring(7)); // VL64 can be any length. If you Put in IHello! it'll get 1 still. It gets First VL64 it gets. so u can do this. VL64 can be like FgOPRfk4%^ in length.
                    TheLength = ThePacket.Length - 9 - HabboEncodinG.encodeVL64(TheirID).Length;
                    int GetTheirID = CallerID[TheirID]; // Get their socket ID from the hold thingay
                    TheSentMessage = CFH_Message[TheirID];
                    TheMessage = ThePacket.Substring(9 + HabboEncodinG.encodeVL64(TheirID).Length, TheLength);
                    foreach (int s in SocketManager.activeSockets.Keys)
                    {
                        HabboUser TheHabbo = SocketManager.GetInstance(s);
                        if (TheHabbo.Level >= 4 && GetTheirID != TheHabbo.HabboID)
                        {
                            SocketManager.GetInstance(s).createMessage("BT" + HabboEncodinG.encodeVL64(TheirID) + Chr(2) + "IPicked up by " + Username + " at " + DateTime.Now.ToShortTimeString() + Chr(2) + Username + Chr(2) + TheSentMessage + Chr(2) + "J" + Chr(2) + "Picked up" + Chr(2) + "I");
                        }
                        else if (GetTheirID == TheHabbo.HabboID && TheHabbo.Level >= 4)
                        {
                            SocketManager.GetInstance(s).createMessage("BT" + HabboEncodinG.encodeVL64(TheirID) + Chr(2) + "IPicked up by " + Username + " at " + DateTime.Now.ToShortTimeString() + Chr(2) + Username + Chr(2) + TheSentMessage + Chr(2) + "J" + Chr(2) + "Picked up" + Chr(2) + "I");
                            SocketManager.GetInstance(s).createMessage("DR" + TheMessage + Chr(2));
                        }
                        else
                        {
                           //SocketManager.GetInstance(GetTheirID).HabboID = GetTheirID;
                           SocketManager.GetInstance(s).createMessage("DR" + TheMessage + Chr(2));
                        }
                    }
                    break;

                #endregion
                #region Navigator Stuff

                case "Bv": // Get Interests
                    GoalX = -1;
                    if (EnterRoomID < 100)
                    {
                        createMessage("DB0");
                        break;
                    }
                    int ROwnerID = 0;
                    ROwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `ownerid` FROM `private` WHERE `id` = '" + EnterRoomID + "'", "ownerid"));
                    if (ROwnerID == HabboID || Level > 6)
                    {
                        createMessage("DB0");
                        break;
                    }
                    //createMessage("DB0");
                    createMessage("@S");
                    break;
                case "AP": // Get Item/Drink
                    // Uh.. i forgot string names
                    //@@CAP1
                    try
                    {
                        int DrinkID = int.Parse(ThePacket.Substring(5));
                        RemoveContainStatus("dance");
                        AddStatus("carryd " + DrinkID);
                        MyRoom.BumpCords(MyX, MyY);
                    }
                    catch
                    {
                        // He's tryna exploitz me!!!
                    }
                    break;
                case "A~": // Get Room Add?
                    createMessage("CP0");
                    break;
                case "@|": // Get Heightmap
                    if (RoomType == "private")
                    {
                        int Floor = 0;
                        int Walls = 0;
                        try
                        {
                            Floor = int.Parse(MySQL.RunMySQLQuery("SELECT `floor` FROM `private` WHERE `id` = '" + MyRoomID + "'", "floor"));
                            Walls = int.Parse(MySQL.RunMySQLQuery("SELECT `wallpaper` FROM `private` WHERE `id` = '" + MyRoomID + "'", "wallpaper"));
                        }
                        catch { 
                        }
                        createMessage("@nwallpaper/" + Walls);
                        createMessage("@nfloor/" + Floor);
                    }
                    string WholeHeightmap;
                    WholeHeightmap = MySQL.RunMySQLQuery("SELECT `heightmap` FROM `" + RoomType + "` WHERE `id` = '" + MyRoomID + "'", "heightmap").Replace(Environment.NewLine, Convert.ToChar(13).ToString());
                    createMessage("@_" + WholeHeightmap);
                    //StoredHeightmap = WholeHeightmap;
                    //createMessage("@^");
                    string TheFurniture;
                    int FurniCount;
                    FurniCount = MySQL.RunCountMySQLQuery("SELECT `id` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "id");
                    TheFurniture = "";
                    for (int y = 0; y < FurniCount; y++)
                    {
                        long TmpFurniID;
                        string TmpFurniSprite;
                        int TmpFurniX;
                        int TmpFurniY;
                        int TmpFurniWidth;
                        int TmpFurniLength;
                        int TmpFurniRotation;
                        string TmpFurniColour;
                        string TmpFurniVariable;
                        double TmpFurniElevation;
                        TmpFurniID =  long.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "id", y));
                        TmpFurniSprite = MySQL.SpecificMySQLQueryOutput("SELECT `sprite` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "sprite", y);
                        TmpFurniColour = MySQL.SpecificMySQLQueryOutput("SELECT `colour` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "colour", y);
                        TmpFurniVariable = MySQL.SpecificMySQLQueryOutput("SELECT `variable` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "variable", y);
                        TmpFurniX = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `x` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "x", y));
                        TmpFurniY = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `y` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "y", y));
                        TmpFurniWidth = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `width` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "width", y));
                        TmpFurniLength = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `length` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "length", y));
                        TmpFurniRotation = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `rotation` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "rotation", y));
                        TmpFurniElevation = double.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `elevation` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "'", "elevation", y));
                        if (RoomType == "private")
                        {
                            TheFurniture += TmpFurniID + Chr(2) + TmpFurniSprite + Chr(2) + HabboEncodinG.encodeVL64(TmpFurniX) + HabboEncodinG.encodeVL64(TmpFurniY) + HabboEncodinG.encodeVL64(TmpFurniWidth) + HabboEncodinG.encodeVL64(TmpFurniLength) + HabboEncodinG.encodeVL64(TmpFurniRotation) + TmpFurniElevation.ToString() + Chr(2) + TmpFurniColour + Chr(2) + Chr(2) + "H" + TmpFurniVariable + Chr(2);
                        }
                        else
                        {
                            TheFurniture += TmpFurniID + " " + TmpFurniSprite + " " + TmpFurniX + " " + TmpFurniY + " " + TmpFurniElevation + " " + TmpFurniRotation + Chr(13);
                        }
                    }
                    //TheFurniture = MySQL.RunMySQLQuery("SELECT `furniture` FROM `" + RoomType + "` WHERE `id` = '" + MyRoomID + "'", "furniture").Replace(Environment.NewLine, Convert.ToChar(13).ToString());
                    //SetupHeightmap(StoredHeightmap);
                    //SetupFurniture(TheFurniture);
                    if (RoomType == "public")
                    {
                        createMessage("@^" + TheFurniture);
                        createMessage("@`H");
                    }
                    else
                    {
                        createMessage("@`" + HabboEncodinG.encodeVL64(FurniCount) + TheFurniture);
                        createMessage("@^");
                    }
                    string DefaultLocation;
                    DefaultLocation = MySQL.RunMySQLQuery("SELECT `door` FROM `" + RoomType + "` WHERE `id` = '" + MyRoomID + "'", "door");
                    MyX = int.Parse(DefaultLocation.Split(" ".ToCharArray())[0]);
                    MyY = int.Parse(DefaultLocation.Split(" ".ToCharArray())[1]);
                    //MyConsoleRoomName = FrmMain.RunMySQLQuery("SELECT `Name` FROM `" + RoomType + "` WHERE `id` = '" + MyRoomID + "'", "Name");
                    MyElevation = double.Parse(DefaultLocation.Split(" ".ToCharArray())[2]);
                    createMessage(MyRoom.BuildRoomUsers());
                    //createMessage("@_" + MySQL.RunMySQLQuery("SELECT `heightmap` FROM `" + RoomType + "` WHERE `id` = '" + MyRoomID + "'", "heightmap"));
                    break;
                case "@~": // Get Objects
                    //createMessage("@^");
                    createMessage("@m");
                    break;

                case "@}": // Get Users
                    break;

                case "@u": // Leave Room
                    MyRoom.RemoveUser(MyID);
                    MyRoom = null;
                    break;
                case "@B": // Room Directory
                    NoStatus();
                    int TempInt2;
                    string RoomTypeLol;
                    RoomTypeLol = ThePacket.Substring(5, 1);
                    if (MyRoomID > 0)
                    {
                        MyRoom.RemoveUserNoKick(MyID);
                    }

                    if (RoomTypeLol == "@")
                    {
                        RoomTypeLol = "private";
                        RoomType = "private";
                    }
                    else
                    {
                        RoomTypeLol = "public";
                        RoomType = "public";
                    }
                        MyRoomID = int.Parse(HabboEncodinG.decodeVL64(ThePacket.Substring(6, ThePacket.Length - 7).ToCharArray()).ToString());
                        int MxInTmp;
                        MxInTmp = int.Parse(MySQL.RunMySQLQuery("SELECT `maxin` FROM `" + RoomTypeLol + "` WHERE `id` = '" + MyRoomID + "'", "maxin"));
                        TempInt2 = int.Parse(MySQL.RunMySQLQuery("SELECT `curin` FROM `" + RoomTypeLol + "` WHERE `id` = '" + MyRoomID + "'", "curin"));
                        if (MxInTmp <= TempInt2)
                        {
                            createMessage("BKThe Room Is Full!");
                            createMessage("@R");
                            MyRoomID = 0;
                            break;
                        }
                        //SendData("Bf" + FrmMain.RunMySQLQuery("SELECT `bfdata` FROM `static` WHERE `id` = '" + MyRoomID + "'", "BFData"));
                        createMessage("AE" + MySQL.RunMySQLQuery("SELECT `aedata` FROM `" + RoomTypeLol + "` WHERE `id` = '" + MyRoomID + "'", "AEData"));
                        createMessage("Bf" + MySQL.RunMySQLQuery("SELECT `bfdata` FROM `" + RoomTypeLol + "` WHERE `id` = '" + MyRoomID + "'", "BFData"));
                        IsRoomOwner = false;
                        GotRights = false;
                        NoStatus();
                        if (RoomType == "private")
                        {
                            ROwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `ownerid` FROM `private` WHERE `id` = '" + MyRoomID + "'","ownerid"));
                            if (ROwnerID == HabboID || Level > 6)
                            {
                                IsRoomOwner = true;
                                GotRights = true;
                                createMessage("@j");
                                createMessage("@o");
                                AddStatus("flatctrl useradmin");
                            }
                            int GtRights = MySQL.RunCountMySQLQuery("SELECT `roomid` FROM `rights` WHERE `roomid` = '" + MyRoomID + "' AND `habboid` = '" + HabboID + "'", "roomid");
                            if (GtRights > 0)
                            {
                                GotRights = true;
                                createMessage("@j");
                                AddStatus("flatctrl useradmin");
                            }
                        }
                        //MyRoom = new RoomManager(MyRoomID, MyID, MySQL, RoomType);
                    if (RoomManager.GetInstance(MyRoomID) == null)
                        {
                            MyRoom = new RoomManager(MyRoomID, MyID, MySQL,RoomType);
                        }
                        else
                        {

                            MyRoom = RoomManager.GetInstance(MyRoomID);
                            MyRoom.UsersInRoom2.Add(MyID.ToString(), MyID.ToString());
                            MyRoom.UsersInRoom.Add(MyID, SocketManager.GetInstance(MyID));
                            //MyRoom.NewUser(MyID);
                        }

                        if (SentDg == false)
                        {
                            //SentDg = true;
                            createMessage("Bfhttp://hotel-ca/client.priv.Floor1a.0");
                        }
                    break;

                case "AK": // Walking :(
                    
                    TmpX = HabboEncodinG.decodeB64(ThePacket.Substring(5, 2));
                    TmpY = HabboEncodinG.decodeB64(ThePacket.Substring(7, 2));
                    GoalX = TmpX;
                    GoalY = TmpY;
                    if (MyRoom.MyHeightmap[GoalX, GoalY] == -1)
                        break;
                    Main._Debug(MyRoom.UsersOnCords[GoalX, GoalY].ToString());
                    if (MyRoom.UsersOnCords[GoalX, GoalY] == -1)
                        break;
                    //Main._Debug(MyRoom.UsersOnCords[GoalX, GoalY].ToString());
                    if (IsWalking == true)
                        break;
                    WalkLoopDelegate delRP2 = new WalkLoopDelegate(WalkLoop);
                    IAsyncResult tag2 = delRP2.BeginInvoke(null, null);
                    
                    
                    break;
                case "BV": //Navigate
                    int ShowFull;
                    int Public;
                    ShowFull = HabboEncodinG.decodeVL64(ThePacket.Substring(5, 1).ToCharArray());

                        Public = HabboEncodinG.decodeVL64(ThePacket.Substring(6).ToCharArray());
                    int RoomCount;
                    switch (Public)
                    {
                        case 3: // Public Rooms
                            StringBuilder PacketBuilder1 = new StringBuilder(100);
                            PacketBuilder1.Append("C\\" + HabboEncodinG.encodeVL64(ShowFull) + "KHCodename Warlord[LD[aJI");
                            RoomCount = MySQL.RunCountMySQLQuery("SELECT * FROM `public`", "Name");
                            int x = 0;
                            while (x < RoomCount)
                            {
                                try
                                {
                                    RoomName = MySQL.SpecificMySQLQueryOutput("SELECT * FROM `public`", "Name", x);
                                }
                                catch
                                {
                                    RoomName = "undefined";
                                }
                                try { RoomCName = MySQL.SpecificMySQLQueryOutput("SELECT * FROM `public`", "CName", x); }
                                catch { RoomCName = "undefined"; }
                                try { RoomCCT = MySQL.SpecificMySQLQueryOutput("SELECT * FROM `public`", "CCT", x); }
                                catch { RoomCCT = "undefined"; }
                                try { CurIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT * FROM `public`", "CurIn", x)); }
                                catch { CurIn = 0; }
                                try { MaxIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT * FROM `public`", "MaxIn", x)); }
                                catch { MaxIn = 0; }
                                try { RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT * FROM `public`", "id", x)); }
                                catch { RoomID = -1; }
                                if (ShowFull == 1)
                                {
                                    if (CurIn >= MaxIn) { goto SkipRoom; }
                                }
                                PacketBuilder1.Append(HabboEncodinG.encodeVL64(RoomID) + "I" + RoomName + "" + HabboEncodinG.encodeVL64(CurIn) + HabboEncodinG.encodeVL64(MaxIn) + "K" + RoomCName + "" + HabboEncodinG.encodeVL64(RoomID) + "H" + RoomCCT + "II");
                            SkipRoom:
                                x++;
                            }
                            createMessage(PacketBuilder1.ToString());

                            break;
                        case 4: // Private Rooms
                            StringBuilder PacketBuilder2 = new StringBuilder(100);
                            PacketBuilder2.Append("C\\" + HabboEncodinG.encodeVL64(ShowFull) + "PAHCodename WarlordRzaCCAI");
                            RoomCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '1' AND `incat` = '0'","name");
                            for (int y = 0; y < RoomCount; y++)
                            {
                                string CatagoryName;
                                 CatagoryName = MySQL.SpecificMySQLQueryOutput("SELECT `name` FROM `private` WHERE `iscat` = '1' AND `incat` = '0'","name",y);
                                 RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `iscat` = '1' AND `incat` = '0'", "id", y));
                                 PacketBuilder2.Append("" + HabboEncodinG.encodeVL64(RoomID) + "J" + CatagoryName + "HX^EPAH");
                            }

                            createMessage(PacketBuilder2.ToString());
                            break;

                        default:
                            int CatagoryID = Public;

                            StringBuilder PacketBuilder3 = new StringBuilder(200);
                            
                            RoomCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "name");
                            CatNam = MySQL.RunMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '1' AND `id` = '" + CatagoryID + "'", "name");
                            //if (RoomCount == 0) { createMessage("@z"); break; }
                            if (RoomCount > 20) { RoomCount = 20; }
                            PacketBuilder3.Append("C\\" + HabboEncodinG.encodeVL64(ShowFull) + HabboEncodinG.encodeVL64(CatagoryID) + "J" + CatNam + "" + HabboEncodinG.encodeVL64(RoomCount) + "XmEPA" + HabboEncodinG.encodeVL64(RoomCount));
                            for (int y = 0; y < RoomCount; y++)
                            {
                                RoomName = MySQL.SpecificMySQLQueryOutput("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "name", y);
                                RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "id", y));
                                RoomStatus = MySQL.SpecificMySQLQueryOutput("SELECT `status` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "status", y);
                                OwnerID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `ownerid` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "ownerid", y));
                                ShowOwner = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `showname` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "showname", y));
                                if (ShowOwner == 0)
                                {
                                    RoomOwner = "";
                                }
                                else
                                {
                                    RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                                }
                                RoomDescription= MySQL.SpecificMySQLQueryOutput("SELECT `description` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "description", y);
                                CurIn = int.Parse (MySQL.SpecificMySQLQueryOutput("SELECT `curin` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "curin", y));
                                MaxIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `maxin` FROM `private` WHERE `iscat` = '0' AND `incat` = '" + CatagoryID + "'", "maxin", y));
                                PacketBuilder3.Append(HabboEncodinG.encodeVL64(RoomID) + "" + RoomName + "" + RoomOwner +  "" + RoomStatus + "" + HabboEncodinG.encodeVL64(CurIn) + HabboEncodinG.encodeVL64(MaxIn) + RoomDescription + "");
                            }
                            createMessage(PacketBuilder3.ToString());
                            break;
                    }
                    break;
                case "AX": // Stop Carrying
                    string CarryWhat = ThePacket.Substring(5);
                    switch (CarryWhat)
                    {
                        case "CarryItem":
                            //NoStatus();
                            break;
                        default:
                            RemoveContainStatus(CarryWhat.ToLower());
                            if (MyRoom != null)
                                MyRoom.BumpCords(MyX, MyY);
                            break;
                    }
                    break;

                case("A^"): // Wave
                    MyHabboWave();
                    break;
                case "@P": // Own Rooms
                    int RoomID2;
                    string RoomName2;
                    string RoomStatus2;
                    string Description;
                    int MaxIn2;
                    int CurIn2;
                    StringBuilder PacketBuilder4 = new StringBuilder(100);
                    RoomCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "name");
                    PacketBuilder4.Append("@P");
                    if (RoomCount == 0) { createMessage("@y" + Username); break; }

                    for (int y = 0; y < RoomCount; y++)
                    {
                        RoomID2 = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "id", y));
                        RoomName2 = MySQL.SpecificMySQLQueryOutput("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "name", y);
                        RoomStatus2 = MySQL.SpecificMySQLQueryOutput("SELECT `status` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "status", y);
                        Description = MySQL.SpecificMySQLQueryOutput("SELECT `description` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "description", y);
                        CurIn2 = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `curin` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "curin", y));
                        MaxIn2 = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `maxin` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "maxin", y));
                        PacketBuilder4.Append(RoomID2 + Tab + RoomName2 + Tab + Username + Tab + RoomStatus2 + Tab + "x" + Tab + CurIn2 + Tab + MaxIn2 + Tab + Description + Tab + Description +  Chr(13));
                    }
                    createMessage(PacketBuilder4.ToString());
                    break;

                case "@]": // Room Creation

                    RoomName = ThePacket.Split("/".ToCharArray())[2];
                    RoomModel = ThePacket.Split("/".ToCharArray())[3];
                    RoomStatus = ThePacket.Split("/".ToCharArray())[4];
                    ShowOwner = int.Parse(ThePacket.Split("/".ToCharArray())[5]);
                    string TmpHeightmap;
                    string TmpDoor;
                    string TmpAE;
                    TmpHeightmap = MySQL.RunMySQLQuery("SELECT `heightmap` FROM `privstatic` WHERE `model` = '" + RoomModel + "'", "heightmap");
                    TmpDoor = MySQL.RunMySQLQuery("SELECT `door` FROM `privstatic` WHERE `model` = '" + RoomModel + "'", "door");
                    TmpAE = MySQL.RunMySQLQuery("SELECT `aedata` FROM `privstatic` WHERE `model` = '" + RoomModel + "'", "aedata");
                    MySQL.RunMySQLQuery("INSERT INTO `private` (`name`, `model`, `status`, `showname`, `ownerid`, `curin`, `maxin`, `heightmap`, `door`, `aedata`) VALUES ('" + RoomName + "', '" + RoomModel + "', '" + RoomStatus + "', '" + ShowOwner + "', '" + HabboID + "', '0', '25', '" + TmpHeightmap + "', '" + TmpDoor + "', '" + TmpAE + "')", "");

                    RoomCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "name");
                    RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + HabboID + "'", "id", RoomCount - 1));
                    createMessage("@{" + RoomID + Chr(13) + RoomName);
                    break;
                case "@y": // Room Password Check @y125/password
                    string RoomPass;
                    if (ThePacket.IndexOf("/") != -1)
                    {
                        RoomID = int.Parse(ThePacket.Substring(5).Split("/".ToCharArray())[0]);
                        ROwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `ownerid` FROM `private` WHERE `id` = '" + RoomID + "'", "ownerid"));
                        if (ROwnerID == HabboID)
                        {
                            createMessage("DB0");
                            break;
                        }
                        RoomPass = ThePacket.Substring(6 + RoomID.ToString().Length);
                        if (RoomPass != MySQL.RunMySQLQuery("SELECT `password` FROM `private` WHERE `id` = '" + RoomID + "'", "password"))
                        {

                            createMessage("@aIncorrect flat password");
                            break;
                        }
                        createMessage("DB0");
                        break;
                    }

                    RoomID = int.Parse(ThePacket.Substring(5));
                    ROwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `ownerid` FROM `private` WHERE `id` = '" + RoomID + "'", "ownerid"));
                    if (ROwnerID == HabboID)
                    {
                        createMessage("DB0");
                        break;
                    }
                    if (MySQL.RunMySQLQuery("SELECT `status` FROM `private` WHERE `id` = '" + RoomID + "'", "status") == "closed")
                    {
                        createMessage("A]");
                        RoomManager TmpRoomLol = RoomManager.GetInstance(RoomID);
                        if (TmpRoomLol == null)
                        {
                            createMessage("@R");
                            createMessage("BKNobody is in the room!");
                            break;
                        }
                        TmpRoomLol.createMessageRights("A[" + Username);

                        break;
                    }
                    createMessage("DB0");
                    break;
                case "@Y": // "@@p@Y/115/\rdescription=LOL\rpassword=\rallsuperuser=0"
                    if (ThePacket.Substring(5, 1) == "/") // If creating ro
                    {
                        RoomID = int.Parse(ThePacket.Split("/".ToCharArray())[1]);
                        Description = ThePacket.Split("\r".ToCharArray())[1];
                        Description = Description.Substring(12);

                        RoomPass = ThePacket.Split("\r".ToCharArray())[2];
                        RoomPass = RoomPass.Substring(9);

                        AllRights = int.Parse(ThePacket.Split("\r".ToCharArray())[3].Substring(13));

                        MySQL.RunMySQLQuery("UPDATE `private` SET `description` = '" + Description + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");
                        MySQL.RunMySQLQuery("UPDATE `private` SET `password` = '" + RoomPass + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");
                        MySQL.RunMySQLQuery("UPDATE `private` SET `allrights` = '" + AllRights + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");
                    }
                    else // @@u@Y3345861/\rdescription=\rallsuperuser=1\rmaxvisitors=20
                    {
                        RoomID = int.Parse(ThePacket.Split("/".ToCharArray())[0].Substring(5));
                        Description = ThePacket.Split("\r".ToCharArray())[1];
                        Description = Description.Substring(12);
                        MySQL.RunMySQLQuery("UPDATE `private` SET `description` = '" + Description + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");

                        MaxIn = int.Parse(ThePacket.Split("\r".ToCharArray())[3].Substring(12));
                        if (MaxIn > 25) { break; }
                        if (MaxIn < 10) { break; }
                        if (MaxIn % 5 != 0) { break; }
                        MySQL.RunMySQLQuery("UPDATE `private` SET `maxin` = '" + MaxIn + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");

                        AllRights = int.Parse(ThePacket.Split("\r".ToCharArray())[2].Substring(13));
                        MySQL.RunMySQLQuery("UPDATE `private` SET `allrights` = '" + AllRights + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");
                    }
                    break;

                case "@X": // "@@V@X102/Test Room/open/1"
                    RoomID = int.Parse(ThePacket.Split("/".ToCharArray())[0].Substring(5));
                    ShowOwner = int.Parse(ThePacket.Substring(ThePacket.Length - 1, 1));
                    string[] Tmp;
                    Tmp = ThePacket.Split("/".ToCharArray());
                    RoomStatus = ThePacket.Substring(0,ThePacket.Length - 2);

                    RoomStatus = Tmp[Tmp.GetUpperBound(0) - 1];

                    RoomName = Tmp[1];
                    for (int y = 2; y < Tmp.GetUpperBound(0) - 1; y++)
                    {
                        RoomName += Tmp[y] + "/";
                    }
                        break;

                case "@S": // Add Fav Room [ISPub][RoomID]
                    int IsPub;
                    IsPub = HabboEncodinG.decodeVL64(ThePacket.Substring(5));
                    RoomID = HabboEncodinG.decodeVL64(ThePacket.Substring(5 + HabboEncodinG.encodeVL64(IsPub).Length));
                    RoomCount = MySQL.RunCountMySQLQuery("SELECT `ispub` FROM `favrooms` WHERE `habboid` = '" + HabboID + "' AND `roomid` = '" + RoomID  + "'", "model");

                    if (RoomCount == 0) // If Entry Doesn't Exist Already!
                    {
                        MySQL.RunMySQLQuery("INSERT INTO `favrooms` (`habboid`, `ispub`, `roomid`) VALUES ('" + HabboID + "', '" + IsPub + "', '" + RoomID + "')", "");
                    }

                    break;

                case "@R": // Get Fav Rooms
                    RoomCount = MySQL.RunCountMySQLQuery("SELECT `habboid` FROM `favrooms` WHERE `habboid` = '" + HabboID + "'", "habboid");
                    StringBuilder PacketBuilder5 = new StringBuilder(100);
                    PacketBuilder5.Append("@}HHJHHH" + HabboEncodinG.encodeVL64(RoomCount));

                    try
                    {
                        for (int y = 0; y < RoomCount; y++)
                        {
                            RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `roomid` FROM `favrooms` WHERE `habboid` = '" + HabboID + "'", "roomid", y));
                            IsPub = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `ispub` FROM `favrooms` WHERE `habboid` = '" + HabboID + "'", "ispub", y));
                            if (IsPub == 0)
                            {
                                RoomName = MySQL.RunMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "name");
                                RoomStatus = MySQL.RunMySQLQuery("SELECT `status` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "status");
                                Description = MySQL.RunMySQLQuery("SELECT `description` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "description");
                                CurIn = int.Parse(MySQL.RunMySQLQuery("SELECT `curin` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "curin"));
                                MaxIn = int.Parse(MySQL.RunMySQLQuery("SELECT `maxin` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "maxin"));
                                OwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `ownerid` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "ownerid"));
                                ShowOwner = int.Parse(MySQL.RunMySQLQuery("SELECT `showname` FROM `private` WHERE `iscat` = '0' AND `id` = '" + RoomID + "'", "showname"));
                                if (ShowOwner == 0)
                                {
                                    if (OwnerID == HabboID)
                                    {
                                        RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                                    }
                                    else
                                    {
                                        RoomOwner = "";
                                    }
                                }
                                else
                                {
                                    RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                                }
                                PacketBuilder5.Append(HabboEncodinG.encodeVL64(RoomID) + RoomName + "" + RoomOwner + "" + RoomStatus + "" + HabboEncodinG.encodeVL64(CurIn) + HabboEncodinG.encodeVL64(MaxIn) + Description + "");
                            }
                            else // If its a public room!
                            {

                            }
                        }
                    }
                    catch
                    {
                    }
                    createMessage(PacketBuilder5.ToString());
                    break;

                case "@T": // Remove From Favs
                    RoomID = HabboEncodinG.decodeVL64(ThePacket.Substring(6));
                    MySQL.RunMySQLQuery("DELETE FROM `favrooms` WHERE `habboid` = '" + HabboID + "' AND `roomid` = '" + RoomID + "'", "");
                    break;

                case "@Q": // Search Rooms
                    string Criteria;
                    StringBuilder SearchRooms = new StringBuilder(200);
                    Criteria = ThePacket.Substring(6);
                    Criteria = Criteria.Substring(0, Criteria.Length - 1);

                    RoomCount = MySQL.RunCountMySQLQuery("SELECT `username` FROM `users` WHERE `username` = '" + Criteria.ToLower() + "'", "username");
                    SearchRooms.Append("@w");
                    if (RoomCount == 1) // Is a users name!
                    {
                        OwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `users` WHERE `username` = '" + Criteria.ToLower() + "'", "id"));
                        RoomCount = MySQL.RunCountMySQLQuery("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "name");

                        for (int y = 0; y < RoomCount; y++)
                        {
                            RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "id", y));
                            RoomName = MySQL.SpecificMySQLQueryOutput("SELECT `name` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "name", y);
                            ShowOwner = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `showname` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "showname", y));
                            RoomStatus = MySQL.SpecificMySQLQueryOutput("SELECT `status` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "status", y);
                            Description = MySQL.SpecificMySQLQueryOutput("SELECT `description` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "description", y);
                            CurIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `curin` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "curin", y));
                            MaxIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `maxin` FROM `private` WHERE `iscat` = '0' AND `ownerid` = '" + OwnerID + "'", "maxin", y));
                            #region Owner Crap
                            if (ShowOwner == 0)
                            {
                                if (OwnerID == HabboID)
                                {
                                    RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                                }
                                else
                                {
                                    RoomOwner = "";
                                }
                            }
                            else
                            {
                                RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                            }
                            #endregion
                            SearchRooms.Append(RoomID + Tab + RoomName + Tab + RoomOwner + Tab + RoomStatus + Tab + "x" + Tab + CurIn + Tab + MaxIn + Tab + Description + Tab + Description + Chr(13));


                        }
                    }

                    RoomCount = MySQL.RunCountMySQLQuery("SELECT * FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "name");
                    if (RoomCount > 20) { RoomCount = 20; } // Max Of 20+ Name Owners
                    for (int y = 0; y < RoomCount; y++)
                    {
                        RoomID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "id", y));
                        OwnerID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `ownerid` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "ownerid", y));
                        RoomName = MySQL.SpecificMySQLQueryOutput("SELECT `name` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "name", y);
                        ShowOwner = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `showname` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "showname", y));
                        RoomStatus = MySQL.SpecificMySQLQueryOutput("SELECT `status` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "status", y);
                        Description = MySQL.SpecificMySQLQueryOutput("SELECT `description` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "description", y);
                        CurIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `curin` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "curin", y));
                        MaxIn = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `maxin` FROM `private` WHERE `name` like '" + Criteria + "%' AND `iscat` = '0' ORDER BY `id`", "maxin", y));
                        #region Owner Crap
                        if (ShowOwner == 0)
                        {
                            if (OwnerID == HabboID)
                            {
                                RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                            }
                            else
                            {
                                RoomOwner = "";
                            }
                        }
                        else
                        {
                            RoomOwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                        }
                        #endregion
                        SearchRooms.Append(RoomID + Tab + RoomName + Tab + RoomOwner + Tab + RoomStatus + Tab + "x" + Tab + CurIn + Tab + MaxIn + Tab + Description + Tab + Description + Chr(13));
                    }
                    createMessage(SearchRooms.ToString());

                    break;

                case "AA": // Hand
                    string HandShowStat;
                    HandShowStat = ThePacket.Substring(5);
                    PacketBuilder = "BL";

                    int HandItemCount;
                    int RHICount;
                    HandItemCount = MySQL.RunCountMySQLQuery("SELECT `id` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "id");
                    RHICount = HandItemCount;
                    if (HandItemCount == 0)
                    {
                        createMessage(PacketBuilder);
                        break;
                    }
                    int StartFrom;
                    if (HandShowStat == "new")
                        HandPage = 0;
                    if (HandShowStat == "next")
                        HandPage++;

                    if (HandShowStat == "prev")
                        HandPage--;

                        StartFrom = 9 * HandPage;
                        if (HandItemCount > 9 + StartFrom)
                            HandItemCount = 9 + StartFrom;

                    for (int INumber = StartFrom; INumber < HandItemCount; INumber++)
                    {
                        long HItemID;
                        string HItemSprite;
                        string HItemColour;
                        int HItemWidth;
                        int HItemLength;
                        string HItemVariable;
                        HItemID = long.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`","id",INumber));
                        HItemSprite = MySQL.SpecificMySQLQueryOutput("SELECT `sprite` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "sprite", INumber).Replace(" ","");
                        HItemColour = MySQL.SpecificMySQLQueryOutput("SELECT `colour` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "colour", INumber);
                        HItemVariable = MySQL.SpecificMySQLQueryOutput("SELECT `variable` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "variable", INumber);
                        HItemWidth = int.Parse( MySQL.SpecificMySQLQueryOutput("SELECT `width` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "width", INumber));
                        HItemLength = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `length` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "length", INumber));
                        if (HItemColour == "")
                            HItemColour = "null";
                        string HItemType = "S";
                        if (HItemSprite == "floor" || HItemSprite == "wallpaper" || HItemSprite == "poster")
                            HItemType = "I";
                        PacketBuilder += "SI" + Chr(30) + "" + HItemID + Chr(30) + INumber + Chr(30) + HItemType + Chr(30) + HItemID + Chr(30) + HItemSprite + Chr(30) + HItemWidth + Chr(30) + HItemLength + Chr(30) + HItemVariable + Chr(30) + HItemColour + Chr(30) + (INumber +1) + "/";
                    }
                   createMessage(PacketBuilder + Chr(13) + RHICount);
                   //createMessage("BLSI-60356824S6035682small_chair_armas110,0,01/2");
                    break;

                case "AB": // Set Floor/Walls (ABwallpaper/77)
                    if (IsRoomOwner == false)
                        break;
                    string DecorationType = ThePacket.Substring(5).Split("/".ToCharArray())[0];
                    int DecorationsID = int.Parse(ThePacket.Split("/".ToCharArray())[1]);

                    string DecorationVariable = MySQL.RunMySQLQuery("SELECT `colour` FROM `furniture` WHERE `id` = '" + DecorationsID + "' AND `ownerid` = '" + HabboID + "'", "colour");
                    MySQL.RunMySQLQuery("DELETE FROM `furniture` WHERE `id` = '" + DecorationsID + "'", "");
                    MySQL.RunMySQLQuery("UPDATE `private` SET `" + DecorationType + "` = '" + DecorationVariable + "' WHERE `id` = '" + MyRoom.RoomsID + "'", "");
                    MyRoom.createMessage("@n" + DecorationType + "/" + DecorationVariable);
                    break;
                case "BY": // Set Catagory
                    int CatID;
                    RoomID = HabboEncodinG.decodeVL64(ThePacket.Substring(5));
                    string Test;
                    Test = ThePacket.Substring(5 + HabboEncodinG.encodeVL64(RoomID).Length);
                    CatID = HabboEncodinG.decodeVL64(Test);
                    MySQL.RunMySQLQuery("UPDATE `private` SET `incat` = '" + CatID + "' WHERE `id` = '" + RoomID + "' AND `ownerid` = '" + HabboID + "'", "");
                    break;
                case "@U": // Get @v;
                    try
                    {
                        RoomID = int.Parse(ThePacket.Substring(5));
                        EnterRoomID = RoomID;
                        PacketBuilder = "@vH";
                        string ROwner = "";
                        RoomStatus = MySQL.RunMySQLQuery("SELECT `status` FROM `private` WHERE `id` = '" + RoomID + "'", "status");
                        if (RoomStatus == "open") { RoomStatus = "H"; }
                        if (RoomStatus == "closed") { RoomStatus = "I"; }
                        if (RoomStatus == "password") { RoomStatus = "J"; }
                        OwnerID = int.Parse(MySQL.RunMySQLQuery("SELECT `ownerid` FROM `private` WHERE `id` = '" + RoomID + "'", "ownerid"));
                        ShowOwner = int.Parse(MySQL.RunMySQLQuery("SELECT `showname` FROM `private` WHERE `id` = '" + RoomID + "'", "showname"));
                        if (ShowOwner == 1)
                            ROwner = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OwnerID + "'", "habboname");
                        PacketBuilder += RoomStatus + HabboEncodinG.encodeVL64(RoomID) + ROwner + "";
                        RoomModel = MySQL.RunMySQLQuery("SELECT `model` FROM `private` WHERE `id` = '" + RoomID + "'", "model");
                        PacketBuilder += RoomModel + "";
                        RoomName = MySQL.RunMySQLQuery("SELECT `name` FROM `private` WHERE  `id` = '" + RoomID + "'", "name");
                        PacketBuilder += RoomName + "";
                        Description = MySQL.RunMySQLQuery("SELECT `description` FROM `private` WHERE  `id` = '" + RoomID + "'", "description");
                        PacketBuilder += Description + "";
                        PacketBuilder += HabboEncodinG.encodeVL64(ShowOwner) + "I";
                        CatID = int.Parse(MySQL.RunMySQLQuery("SELECT `incat` FROM `private` WHERE `id` = '" + RoomID + "'", "incat"));
                        PacketBuilder += HabboEncodinG.encodeVL64(CatID);
                        MaxIn = int.Parse(MySQL.RunMySQLQuery("SELECT `maxin` FROM `private` WHERE `id` = '" + RoomID + "'", "maxin"));
                        PacketBuilder += HabboEncodinG.encodeVL64(MaxIn) + HabboEncodinG.encodeVL64(25);
                        createMessage(PacketBuilder);
                        if (OwnerID == HabboID || Level  > 6)
                            createMessage("DB0");
                       // createMessage("C^" + HabboEncodinG.encodeVL64(RoomID) + HabboEncodinG.encodeVL64(CatID));
                    }
                    catch
                    {

                    }
                    break;

                case "BX": // Modify Room:
                    RoomID = HabboEncodinG.decodeVL64(ThePacket.Substring(5));
                    CatID = int.Parse(MySQL.RunMySQLQuery("SELECT `incat` FROM `private` WHERE `ownerid` = '" + HabboID + "' AND `id` = '" + RoomID + "'", "incat"));
                    createMessage("C^" + HabboEncodinG.encodeVL64(RoomID) + HabboEncodinG.encodeVL64(CatID));
                    break;
                #endregion
                #region Console Stuff
                case "@g": // Send F/r
                    bool SearchOnline;
                    //int TempInt;
                    int SearchUserID = -1;
                    int TmpSockID = -1;
                    SearchOnline = false;
                    SearchUsaname = ThePacket.Substring(7);
                    for (int x = 0; x < FrmMain.MaxConnections; x++)
                    {
                        if (Warlord.SocketManager.activeSockets.ContainsKey(x) == true)
                        {
                            if (Warlord.SocketManager.GetInstance(x).Username == SearchUsaname)
                            {
                                TmpSockID = x;
                                SearchOnline = true;
                            }
                        }
                    }

                    int GotFrFromMe;
                    int OnFl;
                    SearchUserID = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `users` WHERE `username` = '" + SearchUsaname.ToLower() + "'", "id"));
                    GotFrFromMe = MySQL.RunCountMySQLQuery("SELECT `userid` FROM `friendrequests` WHERE `requesterid` = '" + HabboID + "' AND `userid` = '" + SearchUserID + "'", "");
                    OnFl = MySQL.RunCountMySQLQuery("SELECT `userid` FROM `friendslist` WHERE `friendid` = '" + HabboID + "' AND `userid` = '" + SearchUserID + "'", "");
                    if (GotFrFromMe >= 1 || OnFl >= 1)
                    {
                        break;
                    }


                    AmmountOfFrs = MySQL.RunCountMySQLQuery("SELECT `userid` FROM `friendrequsts` WHERE `userid` = '" + HabboID + "'", "userid");
                    MySQL.RunMySQLQuery("INSERT INTO `friendrequests` (`userid`, `requesterid`, `frid`) VALUES ('" + SearchUserID + "', '" + HabboID + "' , '" + AmmountOfFrs + "')", "");
                    if (SearchOnline == true)
                    {
                        Warlord.SocketManager.GetInstance(TmpSockID).createMessage("BD" + HabboEncodinG.encodeVL64(AmmountOfFrs) + Username + Convert.ToChar(2));
                    }

                    break;

                case "@O": // Update Console Stuff

                    FriendOn = 0;

                    SearchRoom = "";

                    AmOfFriends = MySQL.RunCountMySQLQuery("SELECT `userid` FROM `friendslist` WHERE `userid` = '" + HabboID + "'", "userid");

                    for (int y = 0; y < AmOfFriends; y++)
                    {
                        OthersID = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `friendid` FROM `friendslist` WHERE `userid` = '" + HabboID + "'", "friendid", y));
                        FriendOn = 0;
                        SearchRoom = "offline";
                        for (int x = 0; x < FrmMain.MaxConnections; x++)
                        {
                            if (Warlord.SocketManager.activeSockets.ContainsKey(x) == true)
                            {
                                if (Warlord.SocketManager.GetInstance(x).HabboID == OthersID)
                                {
                                    FriendOn = 1;
                                    SearchRoom = Warlord.SocketManager.GetInstance(x).MyConsoleRoomName;
                                }
                            }
                        }
                        SearchName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OthersID + "'", "habboname");
                        SearchLastLogin = MySQL.RunMySQLQuery("SELECT `lastlogin` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "lastlogin");
                        SearchMission = MySQL.RunMySQLQuery("SELECT `conmission` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "conmission");
                        if (FriendOn == 1)
                        {
                            createMessage("@MI" + HabboEncodinG.encodeVL64(OthersID) + SearchMission + "I" + SearchRoom + "");
                        }
                        else
                        {
                            createMessage("@MI" + HabboEncodinG.encodeVL64(OthersID) + SearchMission + "H" + SearchLastLogin + "");
                        }


                    }
                    break;
                case "@e": // Accept F/r

                    int frid = 0;
                    int skt = 0;
                    FriendOn = 0;
                    OthersID = 0;
                    SearchRoom = "";
                    frid = HabboEncodinG.decodeVL64(ThePacket.Substring(5).ToCharArray());
                    OthersID = int.Parse(MySQL.RunMySQLQuery("SELECT `requesterid` FROM `friendrequests` WHERE `userid` = '" + HabboID + "' AND `frid` = '" + frid + "'", "requesterid"));
                    MySQL.RunMySQLQuery("DELETE FROM `friendrequests` WHERE `userid` = '" + HabboID + "' AND `frid` = '" + frid + "'", "");

                    MySQL.RunMySQLQuery("INSERT INTO `friendslist` (`userid`, `friendid`) VALUES ('" + HabboID + "', '" + OthersID + "')", "");
                    MySQL.RunMySQLQuery("INSERT INTO `friendslist` (`userid`, `friendid`) VALUES ('" + OthersID + "', '" + HabboID + "')", "");

                    for (int x = 0; x < FrmMain.MaxConnections; x++)
                    {
                        if (Warlord.SocketManager.activeSockets.ContainsKey(x) == true)
                        {
                            if (Warlord.SocketManager.GetInstance(x).HabboID == OthersID)
                            {
                                FriendOn = 1;
                                skt = x;
                                SearchRoom = Warlord.SocketManager.GetInstance(x).MyConsoleRoomName;
                            }
                        }
                    }
                    SearchName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + OthersID + "'", "habboname");
                    SearchFigure = MySQL.RunMySQLQuery("SELECT `figure` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "figure");
                    SearchLastLogin = MySQL.RunMySQLQuery("SELECT `lastlogin` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "lastlogin");
                    SearchUsaname = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "habboname");
                    SearchMission = MySQL.RunMySQLQuery("SELECT `conmission` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "conmission");
                    if (FriendOn == 1)
                    {
                        createMessage("BI" + HabboEncodinG.encodeVL64(OthersID) + SearchName + "I" + SearchMission + "I" + SearchRoom + "" + SearchLastLogin + "" + SearchFigure + "");

                        SearchName = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `id` = '" + HabboID + "'", "habboname");
                        SearchFigure = MySQL.RunMySQLQuery("SELECT `figure` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "figure");
                        SearchLastLogin = MySQL.RunMySQLQuery("SELECT `lastlogin` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "lastlogin");
                        SearchUsaname = MySQL.RunMySQLQuery("SELECT `habboname` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "habboname");
                        SearchMission = MySQL.RunMySQLQuery("SELECT `conmission` FROM `users` WHERE `username` = '" + SearchName.ToLower() + "'", "conmission");
                        Warlord.SocketManager.GetInstance(skt).createMessage("BI" + HabboEncodinG.encodeVL64(OthersID) + SearchName + "I" + SearchMission + "I" + MyConsoleRoomName + "" + SearchLastLogin + "" + SearchFigure + "");
                        break;
                    }
                    createMessage("BI" + HabboEncodinG.encodeVL64(OthersID) + SearchName + "H" + SearchMission + "H" + SearchRoom + "" + SearchLastLogin + "" + SearchFigure + "");
                    break;

                case "@f": // Reject F/r

                    if (ThePacket.Substring(1, 2) == "@C") // <--- Not Right! just example!!! Reject All
                    {
                        MySQL.RunMySQLQuery("DELETE FROM `friendrequests` WHERE `userid` = '" + HabboID + "'", "");
                        break;
                    }
                    else
                    {
                        frid = HabboEncodinG.decodeVL64(ThePacket.Substring(6).ToCharArray());
                        MySQL.RunMySQLQuery("DELETE FROM `friendrequests` WHERE `userid` = '" + HabboID + "' AND `frid` = '" + frid + "'", "");
                        break;
                    }

                case "@a": // Send Console Message
                    int SendTo = 0;
                    int AmmountToSendTo;
                    ConsMsgID = 0;
                    SendTo = 0;
                    ConsMsg = "";

                    AmmountToSendTo = HabboEncodinG.decodeVL64(ThePacket.Substring(5).ToCharArray());
                    if (AmmountToSendTo == 1)
                    {
                        SendTo = HabboEncodinG.decodeVL64(ThePacket.Substring(6).ToCharArray());
                        ConsMsg = ThePacket.Substring(8 + HabboEncodinG.GetVL64Length(ThePacket.Substring(6).ToCharArray()));
                        ConsMsgID = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `ConsoleMessages` ORDER BY `id` DESC LIMIT 0,1", "id")) + 1;
                        MySQL.RunMySQLQuery("INSERT INTO `ConsoleMessages` (`toid`, `froid`, `message`, `sent`) VALUES ('" + SendTo + "', '" + HabboID + "', '" + ConsMsg + "', '" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "')", "");
                        for (int x = 0; x < FrmMain.MaxConnections; x++)
                        {
                            if (Warlord.SocketManager.activeSockets.ContainsKey(x) == true)
                            {
                                if (Warlord.SocketManager.GetInstance(x).HabboID == SendTo)
                                {
                                    Warlord.SocketManager.GetInstance(x).ConsMsgCount++;
                                    Warlord.SocketManager.GetInstance(x).createMessage("BFI" + HabboEncodinG.encodeVL64(ConsMsgID) + HabboEncodinG.encodeVL64(HabboID) + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "" + ConsMsg + "");

                                }
                            }
                        }
                        break;
                    }
                    TempInt = 0;
                    for (int x = 0; x < AmmountToSendTo; x++)
                    {
                        SendTo = HabboEncodinG.decodeVL64(ThePacket.Substring(6 + TempInt).ToCharArray());
                        TempInt = TempInt + HabboEncodinG.GetVL64Length(ThePacket.Substring(6 + TempInt).ToCharArray());
                    }

                    ConsMsg = ThePacket.Substring(8 + TempInt);
                    TempInt = 0;
                    for (int x = 0; x < AmmountToSendTo; x++)
                    {
                        SendTo = HabboEncodinG.decodeVL64(ThePacket.Substring(6 + TempInt).ToCharArray());
                        TempInt = TempInt + HabboEncodinG.GetVL64Length(ThePacket.Substring(6 + TempInt).ToCharArray());
                        MySQL.RunMySQLQuery("INSERT INTO `ConsoleMessages` (`toid`, `froid`, `message`, `sent`) VALUES ('" + SendTo + "', '" + HabboID + "', '" + ConsMsg + "', '" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "')", "");
                        for (int y = 0; y < FrmMain.MaxConnections; y++)
                        {
                            if (Warlord.SocketManager.activeSockets.ContainsKey(y) == true)
                            {
                                if (Warlord.SocketManager.GetInstance(y).HabboID == SendTo)
                                {
                                    Warlord.SocketManager.GetInstance(y).ConsMsgCount++;
                                    Warlord.SocketManager.GetInstance(y).createMessage("BFI" + HabboEncodinG.encodeVL64(ConsMsgID) + HabboEncodinG.encodeVL64(HabboID) + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "" + ConsMsg + "");

                                }
                            }
                        }
                    }



                    break;

                case "@`": // Delete Message
                    ConsMsgID = HabboEncodinG.decodeVL64(ThePacket.Substring(5).ToCharArray());
                    MySQL.RunMySQLQuery("DELETE FROM `ConsoleMessages` WHERE `id` = '" + ConsMsgID + "'", "");
                    break;
                #endregion
                #region In Room Stuff

                case "AG": // Open trade window
                    int TradeRoomID = int.Parse(ThePacket.Substring(5));
                    if (MyRoom.UsersInRoom.ContainsKey(TradeRoomID)) // If User Actually Exists...
                    {
                        if (SocketManager.GetInstance(TradeRoomID).IsInTrade == true)
                        {
                            break;    // If Other User is Trading, Fuck it.
                        }
                        // Al[Name][09]false[09][13][Name 2][09][False][09][13]
                        createMessage("Al" + Username + Chr(9) + "false" + Chr(9) + Chr(13) + SocketManager.GetInstance(TradeRoomID).Username + Chr(9) + "false" + Chr(9) + Chr(13));
                        TradingWithID = TradeRoomID;
                        SocketManager.GetInstance(TradeRoomID).TradingWithID = MyID;
                        SocketManager.GetInstance(TradeRoomID).createMessage("Al" + SocketManager.GetInstance(TradeRoomID).Username + Chr(9) + "false" + Chr(9) + Chr(13) + Username + Chr(9) + "false" + Chr(9) + Chr(13));
                        SocketManager.GetInstance(TradeRoomID).IsInTrade = true;
                        IsInTrade = true;
                    }
                    break;


                case "AF": // Closed Trade Window
                    IsInTrade = false;
                    SocketManager.GetInstance(TradingWithID).IsInTrade = false;
                    SocketManager.GetInstance(TradingWithID).createMessage("An");
                    break;

                case "AH": // Put item into trade box
                    int FurniID = int.Parse(ThePacket.Substring(5));
                    string Colour = MySQL.RunMySQLQuery("SELECT `colour` FROM `furniture` WHERE `id` = '" + FurniID + "' AND `inhand` = '1' AND `ownerid` = '" + HabboID + "'", "colour");
                    string Sprite = MySQL.RunMySQLQuery("SELECT `sprite` FROM `furniture` WHERE `id` = '" + FurniID + "' AND `inhand` = '1' AND `ownerid` = '" + HabboID + "'", "sprite");
                    //createMessage("Al" + Username + Chr(9) + "false" + Chr(9) + "SI" + Chr(49) + "-" + FurniID + Chr(49) + "0" + Chr(49) + "S" + Chr(49) + FurniID + Chr(49) + Sprite + Chr(49) + "1" + Chr(49) + "1" + Chr(49) + Chr(49) + Colour + Chr(49) + "1" + Chr(49) + Sprite + Chr(49) + "/" + Chr(13) + SocketManager.GetInstance(TradingWithID).Username + Chr(9) + "false" + Chr(9) + Chr(13) );
                    createMessage("AlGod	false	SI-19688000S1968800chair_plasto*911#ffffff,#533e10,#ffffff,#533e100chair_plasto*9/Jeax	false	");
                    createMessage("B!ID: " + FurniID);
                    break;


                case "AZ": // Put down furniture AZ67 4 13 1 1 0
                    if (MyRoom == null)
                        break;
                    if (IsRoomOwner == false)
                        break;
                    long PDItemID;
                    int PDItemX;
                    int PDItemY;
                    int PDItemWidth;
                    int PDItemLength;
                    int PDItemRotation;
                    PDItemID = long.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[0]);
                    PDItemX = int.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[1]);
                    PDItemY = int.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[2]);
                    PDItemWidth = int.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[3]);
                    PDItemLength = int.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[4]);
                    PDItemRotation = int.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[5]);
                    if (MyRoom.PutDownFurniture(PDItemID, PDItemX, PDItemY, PDItemRotation, PDItemWidth, PDItemLength) == true)
                    {
                        createMessage("Ac" + PDItemID);
                    }
                    break;
                case "AC": //ACnew stuff 1 (Pickup Item)
                    if (MyRoom == null)
                        break;
                    if (IsRoomOwner == false)
                        break;

                    string itemType;
                    long PItemID;
                    itemType = ThePacket.Substring(5).Split(" ".ToCharArray())[1];
                    PItemID = long.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[2]);
                    if (MySQL.RunCountMySQLQuery("SELECT `sprite` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "' AND `id` = '" + PItemID + "'", "sprite") != 1)
                        break;

                    if (Level < 7)
                    {
                        if (MySQL.RunCountMySQLQuery("SELECT `sprite` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `id` = '" + PItemID + "'","sprite") == 0)
                            break;
                    }
                    MySQL.RunMySQLQuery("UPDATE `furniture` SET `inhand` = '1' WHERE `id` = '" + PItemID + "'", "");
                    MySQL.RunMySQLQuery("UPDATE `furniture` SET `roomid` = '0' WHERE `id` = '" + PItemID + "'", "");
                    MySQL.RunMySQLQuery("UPDATE `furniture` SET `ownerid` = '" + HabboID + "' WHERE `id` = '" + PItemID + "'", "");
                    MyRoom.RemoveFurniture(PItemID);
                    RefreshHand();
                    break;
                case "AI": // AI1 9 4 0 (Move Item)
                    if (MyRoom == null)
                        break;
                    if (GotRights == false)
                        break;

                    string[] TmpPackets;
                    TmpPackets = ThePacket.Substring(5).Split(" ".ToCharArray());
                    long ID = long.Parse(TmpPackets[0]);
                    int xmv = int.Parse(TmpPackets[1]);
                    int ymv = int.Parse(TmpPackets[2]);
                    int rotation = int.Parse(TmpPackets[3]);
                    MyHeadDirection = rotation;
                    MyRoom.MoveFurniture(ID, xmv, ymv, rotation);
                    break;
                case "A]":
                    if (MyRoom.IsSeat[MyX, MyY] == 1)
                        break;
                    RemoveContainStatus("dance");
                    RemoveContainStatus("carryd");
                    if (ThePacket.Length == 5 || Level == 1)
                    {
                        AddStatus("dance");
                        MyRoom.BumpCords(MyX, MyY);
                        break;
                    }
                    AddStatus("dance " + HabboEncodinG.decodeVL64(ThePacket.Substring(5)));
                    MyRoom.BumpCords(MyX, MyY);
                   
                    break;
                case "A@":
                    MyRoom.NewUser(MyID);
                    createMessage(MyRoom.RefreshUsers());
                    //SentDg = false;
                    if (SentDg == false)
                    {
                        SentDg = true;
                        #region Dg Data
                        createMessage("DiH");
                        createMessage("Dg[tCshelves_norjaX~Dshelves_polyfonYmAshelves_siloXQHtable_polyfon_smallYmAchair_polyfonZbBtable_norja_medX~Dtable_silo_medX~Dtable_plasto_4legYmAtable_plasto_roundYmAtable_plasto_bigsquareYmAstand_polyfon_zZbBchair_siloX~Dsofa_siloX~Dcouch_norjaX~Dchair_norjaX~Dtable_polyfon_medYmAdoormat_loveZbBdoormat_plainYmAsofachair_polyfonX~Dsofa_polyfonX~Dsofachair_siloX~Dchair_plastyX~Dchair_plastoYmAtable_plasto_squareYmAbed_polyfonX~Dbed_polyfon_oneX~Dbed_trad_oneYmAbed_tradYmAbed_silo_oneYmAbed_silo_twoYmAtable_silo_smallX~Dbed_armas_twoYmAbed_budget_oneXQHbed_budgetXQHshelves_armasYmAbench_armasYmAtable_armasYmAsmall_table_armasZbBsmall_chair_armasYmAfireplace_armasYmAlamp_armasYmAbed_armas_oneYmAcarpet_standardYmAcarpet_armasYmAcarpet_polarYmAfireplace_polyfonYmAtable_plasto_4leg*1YmAtable_plasto_bigsquare*1YmAtable_plasto_round*1YmAtable_plasto_square*1YmAchair_plasto*1YmAcarpet_standard*1YmAdoormat_plain*1YmAtable_plasto_4leg*2YmAtable_plasto_bigsquare*2YmAtable_plasto_round*2YmAtable_plasto_square*2YmAchair_plasto*2YmAdoormat_plain*2YmAcarpet_standard*2YmAtable_plasto_4leg*3YmAtable_plasto_bigsquare*3YmAtable_plasto_round*3YmAtable_plasto_square*3YmAchair_plasto*3YmAcarpet_standard*3YmAdoormat_plain*3YmAtable_plasto_4leg*4YmAtable_plasto_bigsquare*4YmAtable_plasto_round*4YmAtable_plasto_square*4YmAchair_plasto*4YmAcarpet_standard*4YmAdoormat_plain*4YmAdoormat_plain*6YmAdoormat_plain*5YmAcarpet_standard*5YmAtable_plasto_4leg*5YmAtable_plasto_bigsquare*5YmAtable_plasto_round*5YmAtable_plasto_square*5YmAchair_plasto*5YmAtable_plasto_4leg*6YmAtable_plasto_bigsquare*6YmAtable_plasto_round*6YmAtable_plasto_square*6YmAchair_plasto*6YmAtable_plasto_4leg*7YmAtable_plasto_bigsquare*7YmAtable_plasto_round*7YmAtable_plasto_square*7YmAchair_plasto*7YmAtable_plasto_4leg*8YmAtable_plasto_bigsquare*8YmAtable_plasto_round*8YmAtable_plasto_square*8YmAchair_plasto*8YmAtable_plasto_4leg*9YmAtable_plasto_bigsquare*9YmAtable_plasto_round*9YmAtable_plasto_square*9YmAchair_plasto*9YmAcarpet_standard*6YmAchair_plasty*1X~DpizzaYmAdrinksYmAchair_plasty*2X~Dchair_plasty*3X~Dchair_plasty*4X~Dbar_polyfonYmAplant_cruddyYmAbottleYmAbardesk_polyfonX~Dbardeskcorner_polyfonX~DfloortileHbar_armasXQHbartable_armasYmAbar_chair_armasYmAcarpet_softYmAcarpet_soft*1YmAcarpet_soft*2YmAcarpet_soft*3YmAcarpet_soft*4YmAcarpet_soft*5YmAcarpet_soft*6YmAred_tvYmAwood_tvYmAcarpet_polar*1YmAchair_plasty*5X~Dcarpet_polar*2YmAcarpet_polar*3YmAcarpet_polar*4YmAchair_plasty*6X~Dtable_polyfonYmAsmooth_table_polyfonYmAsofachair_polyfon_girlX~Dbed_polyfon_girl_oneX~Dbed_polyfon_girlX~Dsofa_polyfon_girlX~Dbed_budgetb_oneXQHbed_budgetbXQHplant_pineappleYmAplant_fruittreeYmAplant_small_cactusYmAplant_bonsaiYmAplant_big_cactusYmAplant_yukkaYmAcarpet_standard*7YmAcarpet_standard*8YmAcarpet_standard*9YmAcarpet_standard*aYmAcarpet_standard*bYmAplant_sunflowerYmAplant_roseYmAtv_luxusYmAbathZ\\BsinkYmAtoiletYmAduckYmAtileYmAtoilet_redYmAtoilet_yellYmAtile_redYmAtile_yellYmAprize1YmAprize2YmApresent_genZvCpresent_gen1ZvCpresent_gen2ZvCpresent_gen3ZvCpresent_gen4ZvCpresent_gen5ZvCpresent_gen6ZvCbar_basicXcBshelves_basicXQHsoft_sofachair_norjaX~Dsoft_sofa_norjaX~Dlamp_basicXQHlamp2_armasYmAfridgeXQHdoorHdoorBHdoorCHpumpkinYmAskullcandleYmAdeadduckYmAdeadduck2YmAdeadduck3YmAmenorahYmApuddingYmAhamYmAturkeyYmAxmasduckYmAhouseYmAtriplecandleYmAtree3YmAtree4YmAtree5X~Dham2YmAwcandlesetYmArcandlesetYmAstatueYmAheartYmAvaleduckYmAheartsofaX~DthroneYmAsamovarYmAgiftflowersYmAhabbocakeYmAhologramYmAeasterduckYmAbunnyYmAbasketYmAbirdieYmAediceX~Dprize3YmAdivider_poly3X~Ddivider_arm1YmAdivider_arm2YmAdivider_arm3YmAdivider_nor1X~Ddivider_silo1X~Ddivider_nor2X~Ddivider_silo2YmAdivider_nor3X~Ddivider_silo3X~DtypingmachineYmAspyroYmAredhologramYmAcameraHjoulutahtiYmAhyacinth1YmAhyacinth2YmAclub_sofaX~Dchair_plasto*10YmAchair_plasto*11YmAbardeskcorner_polyfon*12X~Dbardeskcorner_polyfon*13X~Dchair_plasto*12YmAchair_plasto*13YmAchair_plasto*14YmAtable_plasto_4leg*14YmAmocchamasterYmAcarpet_legocourtYmAbench_legoYmAlegotrophyYmAvalentinescreenYmAedicehcYmArare_daffodil_rugYmArare_beehive_bulbZ\\BhcsohvaYmAhcammeYmArare_parasolYmArare_elephant_statueYmArare_fountainYmArare_standYmArare_globeYmArare_hammockYmArare_elephant_statue*1YmArare_elephant_statue*2YmArare_fountain*1YmArare_fountain*2YmArare_fountain*3YmArare_beehive_bulb*1Z\\Brare_beehive_bulb*2Z\\Brare_xmas_screenYmArare_parasol*1YmArare_parasol*2YmArare_parasol*3YmAsoft_jaggara_norjaYmAhouse2YmAdjesko_turntableYmAmd_sofaYmAmd_limukaappiYmAtable_plasto_4leg*10YmAtable_plasto_4leg*15YmAtable_plasto_bigsquare*14YmAtable_plasto_bigsquare*15YmAtable_plasto_round*14YmAtable_plasto_round*15YmAtable_plasto_square*14YmAtable_plasto_square*15YmAchair_plasto*15YmAchair_plasty*7X~Dchair_plasty*8X~Dchair_plasty*9X~Dchair_plasty*10X~Dchair_plasty*11X~Dchair_plasto*16YmAtable_plasto_4leg*16YmAhockey_scoreX~Dhockey_lightYmAdoorDHprizetrophy2*3X~Dprizetrophy3*3X~Dprizetrophy4*3X~Dprizetrophy5*3X~Dprizetrophy6*3X~Dprizetrophy7*3X~Dprizetrophy*1YmAprizetrophy2*1X~Dprizetrophy3*1X~Dprizetrophy4*1X~Dprizetrophy5*1X~Dprizetrophy6*1X~Dprizetrophy7*1X~Dprizetrophy*2YmAprizetrophy2*2X~Dprizetrophy3*2X~Dprizetrophy4*2X~Dprizetrophy5*2X~Dprizetrophy6*2X~Dprizetrophy7*2X~Dprizetrophy*3YmAhc_chrYmAhc_tblYmAhc_lmp[fBhc_dskXQHnestHpetfood1ZvCpetfood2ZvCpetfood3ZvCwaterbowl*4XICwaterbowl*5XICwaterbowl*2XICwaterbowl*1XICwaterbowl*3XICtoy1XICtoy1*1XICtoy1*2XICtoy1*3XICtoy1*4XICgoodie1ZvCgoodie1*1ZvCgoodie1*2ZvCgoodie2X~Drare_parasol*tHscifiport*0YmAscifiport*9YmAscifiport*8YmAscifiport*7YmAscifiport*6YmAscifiport*5YmAscifiport*4YmAscifiport*3YmAscifiport*2YmAscifiport*1YmAscifirocket*9X~Dscifirocket*8X~Dscifirocket*7X~Dscifirocket*6X~Dscifirocket*5X~Dscifirocket*4X~Dscifirocket*3X~Dscifirocket*2X~Dscifirocket*1X~Dscifirocket*0X~Dscifidoor*10YmAscifidoor*9YmAscifidoor*8YmAscifidoor*7YmAscifidoor*6YmAscifidoor*5YmAscifidoor*4YmAscifidoor*3YmAscifidoor*2YmAscifidoor*1YmApillow*5YmApillow*8YmApillow*0YmApillow*1YmApillow*2YmApillow*7YmApillow*9YmApillow*4YmApillow*6YmApillow*3YmAmarquee*1YmAmarquee*2YmAmarquee*7YmAmarquee*aYmAmarquee*8YmAmarquee*9YmAmarquee*5YmAmarquee*4YmAmarquee*6YmAmarquee*3YmAwooden_screen*1YmAwooden_screen*2YmAwooden_screen*7YmAwooden_screen*0YmAwooden_screen*8YmAwooden_screen*5YmAwooden_screen*9YmAwooden_screen*4YmAwooden_screen*6YmAwooden_screen*3YmApillar*6YmApillar*1YmApillar*9YmApillar*0YmApillar*8YmApillar*2YmApillar*5YmApillar*4YmApillar*7YmApillar*3YmArare_dragonlamp*4YmArare_dragonlamp*0YmArare_dragonlamp*5YmArare_dragonlamp*2YmArare_dragonlamp*8YmArare_dragonlamp*9YmArare_dragonlamp*7YmArare_dragonlamp*6YmArare_dragonlamp*1YmArare_dragonlamp*3YmArare_icecream*1YmArare_icecream*7YmArare_icecream*8YmArare_icecream*2YmArare_icecream*6YmArare_icecream*9YmArare_icecream*3YmArare_icecream*0YmArare_icecream*4YmArare_icecream*5YmArare_fan*7YxBrare_fan*6YxBrare_fan*9YxBrare_fan*3YxBrare_fan*0YxBrare_fan*4YxBrare_fan*5YxBrare_fan*1YxBrare_fan*8YxBrare_fan*2YxBqueue_tile1*3X~Dqueue_tile1*6X~Dqueue_tile1*4X~Dqueue_tile1*9X~Dqueue_tile1*8X~Dqueue_tile1*5X~Dqueue_tile1*7X~Dqueue_tile1*2X~Dqueue_tile1*1X~Dqueue_tile1*0X~DticketHrare_snowrugX~Dcn_lampYmAcn_sofaYmAsporttrack1*1YmAsporttrack1*3YmAsporttrack1*2YmAsporttrack2*1YmAsporttrack2*2YmAsporttrack2*3YmAsporttrack3*1YmAsporttrack3*2YmAsporttrack3*3YmAfootylampX~Dtree1X~Dbarchair_siloX~Ddivider_nor4*4X~Dtraffic_light*1X~Dtraffic_light*2X~Dtraffic_light*3X~Dtraffic_light*4X~Dtraffic_light*6X~Drubberchair*1X~Drubberchair*2X~Drubberchair*3X~Drubberchair*4X~Drubberchair*5X~Drubberchair*6X~Dbarrier*1X~Dbarrier*2X~Dbarrier*3X~Drubberchair*7X~Drubberchair*8X~Dtable_norja_med*2X~Dtable_norja_med*3X~Dtable_norja_med*4X~Dtable_norja_med*5X~Dtable_norja_med*6X~Dtable_norja_med*7X~Dtable_norja_med*8X~Dtable_norja_med*9X~Dcouch_norja*2X~Dcouch_norja*3X~Dcouch_norja*4X~Dcouch_norja*5X~Dcouch_norja*6X~Dcouch_norja*7X~Dcouch_norja*8X~Dcouch_norja*9X~Dshelves_norja*2X~Dshelves_norja*3X~Dshelves_norja*4X~Dshelves_norja*5X~Dshelves_norja*6X~Dshelves_norja*7X~Dshelves_norja*8X~Dshelves_norja*9X~Dchair_norja*2X~Dchair_norja*3X~Dchair_norja*4X~Dchair_norja*5X~Dchair_norja*6X~Dchair_norja*7X~Dchair_norja*8X~Dchair_norja*9X~Ddivider_nor1*2X~Ddivider_nor1*3X~Ddivider_nor1*4X~Ddivider_nor1*5X~Ddivider_nor1*6X~Ddivider_nor1*7X~Ddivider_nor1*8X~Ddivider_nor1*9X~Dsoft_sofa_norja*2X~Dsoft_sofa_norja*3X~Dsoft_sofa_norja*4X~Dsoft_sofa_norja*5X~Dsoft_sofa_norja*6X~Dsoft_sofa_norja*7X~Dsoft_sofa_norja*8X~Dsoft_sofa_norja*9X~Dsoft_sofachair_norja*2X~Dsoft_sofachair_norja*3X~Dsoft_sofachair_norja*4X~Dsoft_sofachair_norja*5X~Dsoft_sofachair_norja*6X~Dsoft_sofachair_norja*7X~Dsoft_sofachair_norja*8X~Dsoft_sofachair_norja*9X~Dsofachair_silo*2X~Dsofachair_silo*3X~Dsofachair_silo*4X~Dsofachair_silo*5X~Dsofachair_silo*6X~Dsofachair_silo*7X~Dsofachair_silo*8X~Dsofachair_silo*9X~Dtable_silo_small*2X~Dtable_silo_small*3X~Dtable_silo_small*4X~Dtable_silo_small*5X~Dtable_silo_small*6X~Dtable_silo_small*7X~Dtable_silo_small*8X~Dtable_silo_small*9X~Ddivider_silo1*2X~Ddivider_silo1*3X~Ddivider_silo1*4X~Ddivider_silo1*5X~Ddivider_silo1*6X~Ddivider_silo1*7X~Ddivider_silo1*8X~Ddivider_silo1*9X~Ddivider_silo3*2X~Ddivider_silo3*3X~Ddivider_silo3*4X~Ddivider_silo3*5X~Ddivider_silo3*6X~Ddivider_silo3*7X~Ddivider_silo3*8X~Ddivider_silo3*9X~Dtable_silo_med*2X~Dtable_silo_med*3X~Dtable_silo_med*4X~Dtable_silo_med*5X~Dtable_silo_med*6X~Dtable_silo_med*7X~Dtable_silo_med*8X~Dtable_silo_med*9X~Dsofa_silo*2X~Dsofa_silo*3X~Dsofa_silo*4X~Dsofa_silo*5X~Dsofa_silo*6X~Dsofa_silo*7X~Dsofa_silo*8X~Dsofa_silo*9X~Dsofachair_polyfon*2X~Dsofachair_polyfon*3X~Dsofachair_polyfon*4X~Dsofachair_polyfon*6X~Dsofachair_polyfon*7X~Dsofachair_polyfon*8X~Dsofachair_polyfon*9X~Dsofa_polyfon*2X~Dsofa_polyfon*3X~Dsofa_polyfon*4X~Dsofa_polyfon*6X~Dsofa_polyfon*7X~Dsofa_polyfon*8X~Dsofa_polyfon*9X~Dbed_polyfon*2X~Dbed_polyfon*3X~Dbed_polyfon*4X~Dbed_polyfon*6X~Dbed_polyfon*7X~Dbed_polyfon*8X~Dbed_polyfon*9X~Dbed_polyfon_one*2X~Dbed_polyfon_one*3X~Dbed_polyfon_one*4X~Dbed_polyfon_one*6X~Dbed_polyfon_one*7X~Dbed_polyfon_one*8X~Dbed_polyfon_one*9X~Dbardesk_polyfon*2X~Dbardesk_polyfon*3X~Dbardesk_polyfon*4X~Dbardesk_polyfon*5X~Dbardesk_polyfon*6X~Dbardesk_polyfon*7X~Dbardesk_polyfon*8X~Dbardesk_polyfon*9X~Dbardeskcorner_polyfon*2X~Dbardeskcorner_polyfon*3X~Dbardeskcorner_polyfon*4X~Dbardeskcorner_polyfon*5X~Dbardeskcorner_polyfon*6X~Dbardeskcorner_polyfon*7X~Dbardeskcorner_polyfon*8X~Dbardeskcorner_polyfon*9X~Ddivider_poly3*2X~Ddivider_poly3*3X~Ddivider_poly3*4X~Ddivider_poly3*5X~Ddivider_poly3*6X~Ddivider_poly3*7X~Ddivider_poly3*8X~Ddivider_poly3*9X~Dchair_silo*2X~Dchair_silo*3X~Dchair_silo*4X~Dchair_silo*5X~Dchair_silo*6X~Dchair_silo*7X~Dchair_silo*8X~Dchair_silo*9X~Ddivider_nor3*2X~Ddivider_nor3*3X~Ddivider_nor3*4X~Ddivider_nor3*5X~Ddivider_nor3*6X~Ddivider_nor3*7X~Ddivider_nor3*8X~Ddivider_nor3*9X~Ddivider_nor2*2X~Ddivider_nor2*3X~Ddivider_nor2*4X~Ddivider_nor2*5X~Ddivider_nor2*6X~Ddivider_nor2*7X~Ddivider_nor2*8X~Ddivider_nor2*9X~Dsilo_studydeskX~Dsolarium_norjaX~Dsolarium_norja*1X~Dsolarium_norja*2X~Dsolarium_norja*3X~Dsolarium_norja*5X~Dsolarium_norja*6X~Dsolarium_norja*7X~Dsolarium_norja*8X~Dsolarium_norja*9X~DsandrugX~Drare_moonrugYmAchair_chinaYmAchina_tableYmAsleepingbag*1YmAsleepingbag*2YmAsleepingbag*3YmAsleepingbag*4YmAsafe_siloX~Dsleepingbag*7YmAsleepingbag*9YmAsleepingbag*5YmAsleepingbag*10YmAsleepingbag*6YmAsleepingbag*8YmAchina_shelveX~Dtraffic_light*5X~Ddivider_nor4*2X~Ddivider_nor4*3X~Ddivider_nor4*5X~Ddivider_nor4*6X~Ddivider_nor4*7X~Ddivider_nor4*8X~Ddivider_nor4*9X~Ddivider_nor5*2X~Ddivider_nor5*3X~Ddivider_nor5*4X~Ddivider_nor5*5X~Ddivider_nor5*6X~Ddivider_nor5*7X~Ddivider_nor5*8X~Ddivider_nor5*9X~Ddivider_nor5X~Ddivider_nor4X~Dwall_chinaYmAcorner_chinaYmAbarchair_silo*2X~Dbarchair_silo*3X~Dbarchair_silo*4X~Dbarchair_silo*5X~Dbarchair_silo*6X~Dbarchair_silo*7X~Dbarchair_silo*8X~Dbarchair_silo*9X~Dsafe_silo*2X~Dsafe_silo*3X~Dsafe_silo*4X~Dsafe_silo*5X~Dsafe_silo*6X~Dsafe_silo*7X~Dsafe_silo*8X~Dsafe_silo*9X~Dglass_shelfZGGglass_chairZGGglass_stoolZGGglass_sofaZGGglass_tableZGGglass_table*2ZGGglass_table*3ZGGglass_table*4ZGGglass_table*5ZGGglass_table*6ZGGglass_table*7ZGGglass_table*8ZGGglass_table*9ZGGglass_chair*2ZGGglass_chair*3ZGGglass_chair*4ZGGglass_chair*5ZGGglass_chair*6ZGGglass_chair*7ZGGglass_chair*8ZGGglass_chair*9ZGGglass_sofa*2ZGGglass_sofa*3ZGGglass_sofa*4ZGGglass_sofa*5ZGGglass_sofa*6ZGGglass_sofa*7ZGGglass_sofa*8ZGGglass_sofa*9ZGGglass_stool*2ZGGglass_stool*4ZGGglass_stool*5ZGGglass_stool*6ZGGglass_stool*7ZGGglass_stool*8ZGGglass_stool*3ZGGglass_stool*9ZGGCFC_100_coin_goldZvCCFC_10_coin_bronzeZvCCFC_200_moneybagZvCCFC_500_goldbarZvCCFC_50_coin_silverZvCCF_10_coin_goldZvCCF_1_coin_bronzeZvCCF_20_moneybagZvCCF_50_goldbarZvCCF_5_coin_silverZvChc_crptYmAhc_tvZ\\BgothgateX~DgothiccandelabraYxBgothrailingX~Dgoth_tableYmAhc_bkshlfYmAhc_btlrZ\\Bhc_crtnYmAhc_djsetYmAhc_frplcZbBhc_lmpstYmAhc_machineYmAhc_rllrXQHhc_rntgnX~Dhc_trllYmAgothic_chair*1X~Dgothic_sofa*1X~Dgothic_stool*1X~Dgothic_chair*2X~Dgothic_sofa*2X~Dgothic_stool*2X~Dgothic_chair*3X~Dgothic_sofa*3X~Dgothic_stool*3X~Dgothic_chair*4X~Dgothic_sofa*4X~Dgothic_stool*4X~Dgothic_chair*5X~Dgothic_sofa*5X~Dgothic_stool*5X~Dgothic_chair*6X~Dgothic_sofa*6X~Dgothic_stool*6X~DwcandleYxBval_cauldronX~Dtree2ZmBsound_machineX~Dromantique_pianochair*3Z|Eromantique_pianochair*5Z|Eromantique_pianochair*2Z|Eromantique_pianochair*4Z|Eromantique_pianochair*1Z|Eromantique_divan*3Z|Eromantique_divan*5Z|Eromantique_divan*2Z|Eromantique_divan*4Z|Eromantique_divan*1Z|Eromantique_chair*3Z|Eromantique_chair*5Z|Eromantique_chair*2Z|Eromantique_chair*4Z|Eromantique_chair*1Z|ErcandleYxBplant_valentinerose*3XICplant_valentinerose*5XICplant_valentinerose*2XICplant_valentinerose*4XICplant_valentinerose*1XICplant_mazegateYeCplant_mazeZcCplant_bulrushXICpetfood4ZvCcarpet_valentineZ|Egothic_carpetXICgothic_carpet2Z|Egothic_chairX~Dgothic_sofaX~Dgothic_stoolX~Dgrand_piano*3Z|Egrand_piano*5Z|Egrand_piano*2Z|Egrand_piano*4Z|Egrand_piano*1Z|Etheatre_seatYGGromantique_tray2XQHromantique_tray1XQHromantique_smalltabl*3Z|Eromantique_smalltabl*5Z|Eromantique_smalltabl*2Z|Eromantique_smalltabl*4Z|Eromantique_smalltabl*1Z|Eromantique_mirrortablZ|Eromantique_divider*3Z|Eromantique_divider*2Z|Eromantique_divider*4Z|Eromantique_divider*1Z|Ejp_tatami2YGGjp_tatamiYGGhabbowood_chairYGGjp_bambooYGGjp_iroriXQHjp_pillowYGGpura_mdl1*3XQHpura_mdl1*5XQHpura_mdl1*7XQHpura_mdl1*2XQHpura_mdl1*9XQHpura_mdl1*4XQHpura_mdl1*8XQHpura_mdl1*6XQHpura_mdl1*1XQHjp_lanternXQHpura_mdl4*3XQHpura_mdl4*5XQHpura_mdl4*7XQHpura_mdl4*2XQHpura_mdl4*9XQHpura_mdl4*4XQHpura_mdl4*8XQHpura_mdl4*6XQHpura_mdl4*1XQHpura_mdl2*3XQHpura_mdl2*5XQHpura_mdl2*7XQHpura_mdl2*2XQHpura_mdl2*9XQHpura_mdl2*4XQHpura_mdl2*8XQHpura_mdl2*6XQHpura_mdl2*1XQHbed_budget_one*3XQHbed_budget_one*5XQHbed_budget_one*7XQHbed_budget_one*2XQHbed_budget_one*9XQHbed_budget_one*4XQHbed_budget_one*8XQHbed_budget_one*6XQHbed_budget_one*1XQHrare_icecream_campaignXQHpura_mdl5*3XQHpura_mdl5*5XQHpura_mdl5*7XQHpura_mdl5*2XQHpura_mdl5*9XQHpura_mdl5*4XQHpura_mdl5*8XQHpura_mdl5*6XQHpura_mdl5*1XQHrope_dividerXQHromantique_clockXQHjp_drawerXQHchair_basic*3XQHchair_basic*5XQHchair_basic*7XQHchair_basic*2XQHchair_basic*9XQHchair_basic*4XQHchair_basic*8XQHchair_basic*6XQHchair_basic*1XQHpura_mdl3*3XQHpura_mdl3*5XQHpura_mdl3*7XQHpura_mdl3*2XQHpura_mdl3*9XQHpura_mdl3*4XQHpura_mdl3*8XQHpura_mdl3*6XQHpura_mdl3*1XQHbed_budget*3XQHbed_budget*5XQHbed_budget*7XQHbed_budget*2XQHbed_budget*9XQHbed_budget*4XQHbed_budget*8XQHbed_budget*6XQHbed_budget*1XQHsound_set_1XQHsound_set_2XQHsound_set_3XQHsound_set_4XQHsound_set_5XQHsound_set_6XQHsound_set_7XQHsound_set_8XQHsound_set_9XQHsound_machine*1YGGsound_set_23XQHrclr_gardenXQHrclr_sofaXQHsound_set_19XQHsound_set_25XQHsound_set_10XQHsound_set_14XQHsound_set_24XQHsound_set_12XQHsound_set_21XQHsound_set_15XQHsound_set_28XQHsound_set_18XQHsound_set_26XQHspotlightYGGsound_set_22XQHsound_set_13XQHsound_set_20XQHsound_machine*2YGGsound_machine*3YGGsound_machine*4YGGsound_machine*5YGGsound_machine*6YGGsound_machine*7YGGsound_set_27XQHsound_set_17XQHrom_lampZ|Esound_set_16XQHrclr_chairZ|Esound_set_11XQHRDpost.itHpost.it.vdHphotoHChessHTicTacToeHBattleShipHPokerHwallpaperHfloorHposterYxBgothicfountainYxBhc_wall_lampZbBindustrialfanZ`BtorchZ\\Bval_heartXBCwallmirrorZ|Ejp_ninjastarsXQHhabw_mirrorXQH");
                        #endregion
                        //createMessage("Bfhttp://hotel-ca/client.priv.Floor1a.0");
                    }
                    break;
                case "@w": // Shout
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    ThePacket = ThePacket.Replace(Environment.NewLine, "");
                    MyRoom.createMessage("@Z" + HabboEncodinG.encodeVL64(MyID) + ThePacket.Substring(7) + Chr(2));
                    MyHabboMouthMove();
                    break;
                case "B^": // Change Badge // Badge Status
                    string ThBadge;
                    int TheStatus;
                    ThBadge = ThePacket.Substring(7, 3);
                    if (Badges.IndexOf(ThBadge) == -1) { break; }
                    TheStatus = HabboEncodinG.decodeVL64(ThePacket.Substring(10).ToCharArray());

                    switch (TheStatus)
                    {
                        case 0:
                        case 1:
                            break;

                        default:
                            TheStatus = 0;
                            break;
                    }

                    MySQL.RunMySQLQuery("UPDATE `users` SET `curbadge` = '" + ThBadge + "'  WHERE `id` = '" + HabboID + "'", "");
                    MySQL.RunMySQLQuery("UPDATE `users` SET `badgestat` = '" + TheStatus + "'  WHERE `id` = '" + HabboID + "'", "");
                    BadgeStatus = TheStatus;
                    Badge = ThBadge;
                    if (BadgeStatus == 0)
                    {
                        MyRoom.createMessage("Cd" + HabboEncodinG.encodeVL64(MyID));
                        break;
                    }
                    MyRoom.createMessage("Cd" + HabboEncodinG.encodeVL64(MyID) + Badge + "");
                    break;

                case "AO": // Look To
                    if (MyRoom == null)
                        break;
                    TmpX = int.Parse(ThePacket.Substring(5).Split(" ".ToCharArray())[0]);
                    TmpY = int.Parse(ThePacket.Split(" ".ToCharArray())[1]);
                    LookTo(TmpX, TmpY);
                    MyRoom.BumpCords(MyX, MyY);
                    break;
                #endregion
                #region Catalogue
                case "Ae": // Open Cata (Aeproduction/en)
                    PacketBuilder = "";
                    int CataCount = MySQL.RunCountMySQLQuery("SELECT `idname` FROM `cataloguepages` WHERE `level` <= '" + Level + "' ORDER BY `id`", "idname");
                    if (CataCount == 0)
                        break;
                    for (int x = 0; x < CataCount; x++)
                    {
                        string catidname = MySQL.SpecificMySQLQueryOutput("SELECT `idname` FROM `cataloguepages` WHERE `level` <= '" + Level + "'  ORDER BY `id`", "idname", x);
                        string catshowname = MySQL.SpecificMySQLQueryOutput("SELECT `showname` FROM `cataloguepages` WHERE `level` <= '" + Level + "'  ORDER BY `id`", "showname", x);
                        PacketBuilder += catidname + "	" + catshowname + Chr(13);
                    }
                    createMessage("A~" + PacketBuilder);
                    break;
                case "Af": // Show Cata Page (Afproduction/Spaces/en)
                    string catidname2 = ThePacket.Substring(16).Split("/".ToCharArray())[0];
                    string catShowname = MySQL.RunMySQLQuery("SELECT `showname` FROM `cataloguepages` WHERE `idname` = '" + catidname2 + "'", "showname");
                    string catRest = MySQL.RunMySQLQuery("SELECT `rest` FROM `cataloguepages` WHERE `idname` = '" + catidname2 + "'", "rest").Replace(Chr(10),"");
                    int ItemAmmount = MySQL.RunCountMySQLQuery("SELECT `sprite` FROM `furnitureprices` WHERE `inpage` = '" + catidname2 + "'", "sprite");
                    string CatFurni = "";
                    for (int x = 0; x < ItemAmmount; x++)
                    {
                        string TmpItemNam;
                        TmpItemNam = MySQL.SpecificMySQLQueryOutput("SELECT `shownam` FROM `furnitureprices` WHERE `inpage` = '" + catidname2 + "'","shownam",x);
                        string TmpItemDesc = MySQL.SpecificMySQLQueryOutput("SELECT `description` FROM `furnitureprices` WHERE `inpage` = '" + catidname2 + "'","description",x);
                        int TmpItemPrice = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `price` FROM `furnitureprices` WHERE `inpage` = '" + catidname2 + "'","price",x));
                        string TmpItemSprite = MySQL.SpecificMySQLQueryOutput("SELECT `sprite` FROM `furnitureprices` WHERE `inpage` = '" + catidname2 + "'","sprite",x);
                        string TmpItemColour = MySQL.SpecificMySQLQueryOutput("SELECT `colour` FROM `furnitureprices` WHERE `inpage` = '" + catidname2 + "'","colour",x);
                        string TmpLength = MySQL.RunMySQLQuery("SELECT `length` FROM `furnituretypes` WHERE `sprite` = '" + TmpItemSprite + "'", "length");
                        string TmpWidth = MySQL.RunMySQLQuery("SELECT `width` FROM `furnituretypes` WHERE `sprite` = '" + TmpItemSprite + "'", "width");

                        CatFurni += "p:" + TmpItemNam + "	" + TmpItemDesc + "	" + TmpItemPrice + "		s" + "	" + TmpItemSprite + "	0" + "	" + TmpWidth + "," + TmpLength + "	" + TmpItemSprite + "	" + TmpItemColour + Chr(13);
                    }
                    createMessage("A" + Chr(127) + "i:" + catidname2 + Chr(13) + "n:" + catShowname + Chr(13) + catRest + Chr(13) + CatFurni + Chr(13));
                    break;
                case "@t": // Say
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    ThePacket = ThePacket.Replace(Environment.NewLine, "");
                    MyRoom.SayToRoom(ThePacket.Substring(7),MyID);
                    break;

                case "@x": // Whisper  @x@JJeax Test.
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    ThePacket = ThePacket.Replace(Environment.NewLine, "");
                    string WhisperUsername = ThePacket.Substring(7);
                    WhisperUsername = WhisperUsername.Substring(0, WhisperUsername.IndexOf(" "));
                    string WhisperText = ThePacket.Substring(7 + WhisperUsername.Length + 1);
                    MyRoom.WhisperToUser(WhisperText, MyID, WhisperUsername);
                    break;

                case "AJ": // Sign item
                    // @@JAJ@C195@AC
                    ThePacket = ThePacket.Replace(Chr(1), "");
                    ThePacket = ThePacket.Replace(Chr(2), "");
                    ThePacket = ThePacket.Replace(Chr(10), "");
                    ThePacket = ThePacket.Replace(Chr(13), "");
                    ThePacket = ThePacket.Replace(Chr(5), "");
                    int IDLen = HabboEncodinG.decodeB64(ThePacket.Substring(5,2));
                    long ItemID = long.Parse(ThePacket.Substring(7, IDLen));
                    string Signing = ThePacket.Substring(9 + IDLen);
                    string SignSprite = MySQL.RunMySQLQuery("SELECT `sprite` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "' AND `id` = '" + ItemID + "'", "sprite");
                    int WlkVal = 0;
                    if (MySQL.RunMySQLQuery("SELECT `door` FROM `furnituretypes` WHERE `sprite` = '" + SignSprite + "'", "door") == "1")
                    {
                        if (GotRights == false || (Signing != "O" && Signing != "C"))
                            break;
                        int SignX = int.Parse(MySQL.RunMySQLQuery("SELECT `x` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "' AND `id` = '" + ItemID + "'", "x"));
                        int SignY = int.Parse(MySQL.RunMySQLQuery("SELECT `y` FROM `furniture` WHERE `roomid` = '" + MyRoomID + "' AND `id` = '" + ItemID + "'", "y"));
                        if (Signing == "C")
                            MyRoom.MyHeightmap[SignX, SignY] = -1;
                        if (Signing == "O")
                            MyRoom.MyHeightmap[SignX, SignY] = MyRoom.Floormap[SignX, SignY];
                    }
                        MySQL.RunMySQLQuery("UPDATE `furniture` SET `variable` = '" + Signing + "' WHERE `id` = '" + ItemID + "'","");
                        MyRoom.createMessage("AX" + ItemID + "" + Signing + "");
                    


                    break;
                case "A`": // @@OA`DisasterPiece
                    if (IsRoomOwner == false)
                        break;

                    string TmpGivRightsName = ThePacket.Substring(5);
                    if (MySQL.RunCountMySQLQuery("SELECT `username` FROM `users` WHERE `username` = '" + TmpGivRightsName.ToLower() + "'", "username") == 0)
                        break; // User doesn't exist

                    int TmpGivRightsID = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `users` WHERE `username` = '" + TmpGivRightsName + "'", "id"));

                    if (MySQL.RunCountMySQLQuery("SELECT `habboid` FROM `rights` WHERE `habboid` = '" + TmpGivRightsID + "'","habboid") > 0)
                        break; // Already got rights!

                    MySQL.RunMySQLQuery("INSERT INTO `rights` (`habboid`, `roomid`) VALUES ('" + TmpGivRightsID + "', '" + MyRoom.RoomsID + "')", "");
                    foreach (HabboUser GivRights in SocketManager.activeSockets.Values)
                    {
                        if (GivRights.HabboID == TmpGivRightsID)
                        {
                            GivRights.AddStatus("flatctrl ");
                            GivRights.GotRights = true;
                            GivRights.createMessage("@j");
                            MyRoom.BumpCords(GivRights.MyX, GivRights.MyY);
                            break;
                        }
                    }


                    break;
                case "BA": // Redeem Vouche
                    string Voucher = ThePacket.Substring(7);
                    if (MySQL.RunCountMySQLQuery("SELECT `value` FROM `vouchers` WHERE `code` = '" + Voucher + "'", "value") == 0)
                    {
                        createMessage("CU1");
                        break; // Voucher Doesn't exist
                    }
                    try
                    {
                        int VouchVal = int.Parse(MySQL.RunMySQLQuery("SELECT `value` FROM `vouchers` WHERE `code` = '" + Voucher + "'", "value"));
                        MySQL.RunMySQLQuery("DELETE FROM `vouchers` WHERE `code` = '" + Voucher + "'", "");
                        Credits = Credits + VouchVal;
                        MySQL.RunMySQLQuery("UPDATE `users` SET `credits` = '" + Credits + "' WHERE `id` = '" + HabboID + "'", "");
                        createMessage("CU2");
                        createMessage("@F" + Credits);
                    }
                    catch
                    {
                        createMessage("CU0");
                    }
                    break;
                case "Aa": // Remove Rights
                    if (IsRoomOwner == false)
                        break;

                    TmpGivRightsName = ThePacket.Substring(5);
                    if (MySQL.RunCountMySQLQuery("SELECT `username` FROM `users` WHERE `username` = '" + TmpGivRightsName.ToLower() + "'", "username") == 0)
                        break; // User doesn't exist

                    TmpGivRightsID = int.Parse(MySQL.RunMySQLQuery("SELECT `id` FROM `users` WHERE `username` = '" + TmpGivRightsName + "'", "id"));

                    if (MySQL.RunCountMySQLQuery("SELECT `habboid` FROM `rights` WHERE `habboid` = '" + TmpGivRightsID + "'", "habboid") == 0)
                    {
                        foreach (HabboUser TakRights in SocketManager.activeSockets.Values)
                        {
                            if (TakRights.HabboID == TmpGivRightsID)
                            {
                                TakRights.RemoveContainStatus("flatctrl");
                                MyRoom.BumpCords(TakRights.MyX, TakRights.MyY);
                                break;
                            }
                        }
                        break; // Not Got Rights
                    }

                    MySQL.RunMySQLQuery("DELETE FROM `rights` WHERE `habboid` = '" + TmpGivRightsID + "' AND `roomid` = '" + MyRoom.RoomsID + "'", "");
                    foreach (HabboUser GivRights in SocketManager.activeSockets.Values)
                    {
                        if (GivRights.HabboID == TmpGivRightsID)
                        {
                            GivRights.RemoveContainStatus("flatctrl");
                            GivRights.GotRights = false;
                            GivRights.createMessage("@k");
                            MyRoom.BumpCords(GivRights.MyX, GivRights.MyY);
                            break;
                        }
                    }
                    break;
                case "Ad": // Buy Item (@Adproduction[13]Spaces[13]en[13]wallpaper 6[13]601[13]0)
                    string BuySprite = ThePacket.Split(Chr(13).ToCharArray())[3].Split(" ".ToCharArray())[0];
                    string CatPagTmp = ThePacket.Split(Chr(13).ToCharArray())[1];
                    if (CatPagTmp != "Spaces")
                    {
                        if (MySQL.RunCountMySQLQuery("SELECT `sprite` FROM `furnitureprices` WHERE `sprite` = '" + BuySprite + "' AND `inpage` = '" + CatPagTmp + "'", "sprite") == 0)
                            break;
                    }
                    if (BuySprite == "a2")
                        BuySprite = "floor";
                    string BuyVariable = MySQL.RunMySQLQuery("SELECT `variable` FROM `furnituretypes` WHERE `sprite` = '" + BuySprite + "'", "variable");
                    string BuyColour;
                    if (BuySprite == "floor" || BuySprite == "wallpaper")
                    {
                        BuyColour = ThePacket.Split(Chr(13).ToCharArray())[4];
                    }
                    else
                    {
                        BuyColour = MySQL.RunMySQLQuery("SELECT `colour` FROM `furnitureprices` WHERE `sprite` = '" + BuySprite + "' AND `inpage` = '" + CatPagTmp + "'", "colour");
                    }
                    int BuyPrice = int.Parse(MySQL.RunMySQLQuery("SELECT `price` FROM `furnitureprices` WHERE `sprite` = '" + BuySprite + "' AND `inpage` = '" + CatPagTmp + "'","price"));
                    string itemcatpage = CatPagTmp;
                    int PageLevel = int.Parse(MySQL.RunMySQLQuery("SELECT `level` FROM `cataloguepages` WHERE `idname` = '" + itemcatpage + "'", "level"));
                    if (BuyPrice > Credits)
                    {
                        createMessage("BKNot enough credits!");
                        break;
                    }
                    if (Level < PageLevel)
                    {
                        createMessage("BKThis item is not for sale!");
                        break;
                    }
                    if (MySQL.RunMySQLQuery("SELECT `forsale` FROM `furnitureprices` WHERE `sprite` = '" + BuySprite + "'", "forsale") == "0" && Level < 8)
                    {
                        createMessage("BKThis item is not for sale!");
                        break;
                    }
                    Credits = Credits - BuyPrice;
                    createMessage("@F" + Credits);
                    MySQL.RunMySQLQuery("UPDATE `users` SET `credits` = '" + Credits + "' WHERE `id` = '" + HabboID + "'", "");
                    string BuyLength = MySQL.RunMySQLQuery("SELECT `length` FROM `furnituretypes` WHERE `sprite` = '" + BuySprite + "'", "length");
                    string BuyWidth = MySQL.RunMySQLQuery("SELECT `width` FROM `furnituretypes` WHERE `sprite` = '" + BuySprite + "'", "width");
                    MySQL.RunMySQLQuery("INSERT INTO `furniture` (`sprite`, `ownerid`, `inhand`, `length`, `width`, `colour`, `variable`, `rotation`) VALUES ('" + BuySprite + "', '" + HabboID + "', '1', '" + BuyLength + "', '" + BuyWidth + "', '" + BuyColour + "', '" + BuyVariable + "', '0')", "");
                    RefreshHand();
                    break;
                #endregion
            }
        }
        #region Misc Methods
        #region Misc Functions
        internal string Chr(int CharacterValue)
        {
            return Convert.ToChar(CharacterValue).ToString();
        }
        private int BadgeNumber(string BadgeNam)
        {
            int Heh;
            Heh = Badges.IndexOf(BadgeNam);
            return Heh / 4;
        }
        private int DayDiff(string TheDate)
        {
            if (TheDate == "")
            {
                return 31;
            }
            else
            {
                return int.Parse(DateStuff.DateAndTime.DateDiff(DateStuff.DateInterval.Day, DateTime.Parse(TheDate), DateTime.Parse(DateTime.Now.ToString())).ToString());
            }
        }

        internal void RefreshHand(int Handpage)
        {
            int HandItemCount = 0;
            int RHICount = 0;
            string PacketBuilder = "BL";
            HandItemCount = MySQL.RunCountMySQLQuery("SELECT `id` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "id");
            RHICount = HandItemCount;
            if (HandItemCount == 0)
            {
                createMessage(PacketBuilder);
                return;
            }
            int StartFrom;

            StartFrom = 9 * Handpage;
            if (HandItemCount > 9 + StartFrom)
                HandItemCount = 9 + StartFrom;

            for (int INumber = StartFrom; INumber < HandItemCount; INumber++)
            {
                long HItemID;
                string HItemSprite;
                string HItemColour;
                int HItemWidth;
                int HItemLength;
                string HItemVariable;
                HItemID = long.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "id", INumber));
                HItemSprite = MySQL.SpecificMySQLQueryOutput("SELECT `sprite` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "sprite", INumber).Replace(" ", "");
                HItemColour = MySQL.SpecificMySQLQueryOutput("SELECT `colour` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "colour", INumber);
                HItemVariable = MySQL.SpecificMySQLQueryOutput("SELECT `variable` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "variable", INumber);
                HItemWidth = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `width` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "width", INumber));
                HItemLength = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `length` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "length", INumber));
                if (HItemColour == "")
                    HItemColour = "null";
                string HItemType = "S";
                if (HItemSprite == "floor" || HItemSprite == "wallpaper" || HItemSprite == "poster")
                    HItemType = "I";
                PacketBuilder += "SI" + Chr(30) + "" + HItemID + Chr(30) + INumber + Chr(30) + HItemType + Chr(30) + HItemID + Chr(30) + HItemSprite + Chr(30) + HItemWidth + Chr(30) + HItemLength + Chr(30) + HItemVariable + Chr(30) + HItemColour + Chr(30) + (INumber + 1) + "/";
            }
            createMessage(PacketBuilder + Chr(13) + RHICount);
        }
        internal void RefreshHand()
        {

            int HandItemCount = 0;
            int RHICount = 0;
            string PacketBuilder = "BL";
            HandItemCount = MySQL.RunCountMySQLQuery("SELECT `id` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "id");
            RHICount = HandItemCount;
            if (HandItemCount == 0)
            {
                createMessage(PacketBuilder);
                return;
            }
            int StartFrom;

            HandPage = (HandItemCount - 1) / 9;
            StartFrom = 9 * HandPage;
            for (int INumber = StartFrom; INumber < HandItemCount; INumber++)
            {
                long HItemID;
                string HItemSprite;
                string HItemColour;
                int HItemWidth;
                int HItemLength;
                string HItemVariable;
                HItemID = long.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `id` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "id", INumber));
                HItemSprite = MySQL.SpecificMySQLQueryOutput("SELECT `sprite` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "sprite", INumber).Replace(" ", "");
                HItemColour = MySQL.SpecificMySQLQueryOutput("SELECT `colour` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "colour", INumber);
                HItemVariable = MySQL.SpecificMySQLQueryOutput("SELECT `variable` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "variable", INumber);
                HItemWidth = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `width` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "width", INumber));
                HItemLength = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `length` FROM `furniture` WHERE `ownerid` = '" + HabboID + "' AND `inhand` = '1' ORDER BY `id`", "length", INumber));
                if (HItemColour == "")
                    HItemColour = "null";
                string HItemType = "S";
                if (HItemSprite == "floor" || HItemSprite == "wallpaper" || HItemSprite == "poster")
                    HItemType = "I";
                PacketBuilder += "SI" + Chr(30) + "" + HItemID + Chr(30) + INumber + Chr(30) + HItemType + Chr(30) + HItemID + Chr(30) + HItemSprite + Chr(30) + HItemWidth + Chr(30) + HItemLength + Chr(30) + HItemVariable + Chr(30) + HItemColour + Chr(30) + (INumber + 1) + "/";
            }
            createMessage(PacketBuilder + Chr(13) + RHICount);
        }
        #endregion
        #region Walking Functions
        private void WalkLoop()
        {
            IsWalking = true;
            int PrevX = -1;
            int PrevY = -1;
            while (MyRoom != null)
            {
                try
                {
                ReGetItAll:

                    if (GoalX == -1)
                        goto DontDoLoop;

                    Pathfinder PFinder = new Pathfinder();
                    string[] Movements;
                    if (GoalX == PrevX)
                    {
                        GoalX = -1;
                        goto DontDoLoop;
                    }
                    RegetShiz:
                    PrevX = GoalX;
                    PrevY = GoalY;
                    Movements = PFinder.GetPath(MyX + "," + MyY, GoalX + "," + GoalY, MyRoom.MyHeightmap);
                    if (Movements[0] == null)
                        goto DontDoLoop;
                    int i = 1;
                    while (Movements.GetUpperBound(0) > 0)
                    {
                        if (PrevX != GoalX)
                            goto RegetShiz;
                        string TmpCords;
                    RedoCords:
                        TmpCords = Movements[Movements.GetUpperBound(0) - i];
                        TmpX = int.Parse(TmpCords.Split(",".ToCharArray())[0]);
                        TmpY = int.Parse(TmpCords.Split(",".ToCharArray())[1]);
                        if (MyRoom.UsersOnCords[TmpX, TmpY] == -1)
                        {
                            int PrevVal;
                            PrevVal = MyRoom.MyHeightmap[TmpX, TmpY];
                            MyRoom.MyHeightmap[TmpX, TmpY] = -1;
                            Movements = PFinder.GetPath(MyX + "," + MyY, GoalX + "," + GoalY, MyRoom.MyHeightmap);
                            MyRoom.MyHeightmap[TmpX, TmpY] = PrevVal;
                            i = 1;
                            goto RedoCords;
                        }
                        if (MyRoom.MyHeightmap[TmpX, TmpY] == -1)
                        {
                            Movements = PFinder.GetPath(MyX + "," + MyY, GoalX + "," + GoalY, MyRoom.MyHeightmap);
                            i = 1;
                            goto RedoCords;
                        }
                        if (MyRoom.MyHeightmap[TmpX, TmpY] == -2)
                        {
                            if (TmpX != GoalX || TmpY != GoalY)
                            {
                                Movements = PFinder.GetPath(MyX + "," + MyY, GoalX + "," + GoalY, MyRoom.MyHeightmap);
                                i = 1;
                                goto RedoCords;
                            }
                        }
                        MyRoom.MoveMe(TmpX, TmpY, MyID);
                        i++;
                    }
                
                    if (GoalX != -1) { goto ReGetItAll; }
                    GoalX = -1;
                }
                  
                catch
                {

                }
            }
        DontDoLoop:
            IsWalking = false;
        }
        #endregion
        #region Waving And Statuses Etc
        private delegate void RunSepThread();
        internal void WavHab()
        {
            RemoveContainStatus("dance");
            AddStatus("wave");
            MyRoom.BumpCords(MyX, MyY);
            System.Threading.Thread.Sleep(1500);
            RemoveStatus("wave");
            MyRoom.BumpCords(MyX, MyY);
        }
        internal void MyHabboWave()
        {
            RunSepThread delRP2 = new RunSepThread(WavHab);
            IAsyncResult tag2 = delRP2.BeginInvoke(null, null);
        }
        internal void TalkHab()
        {
            AddStatus("talk");
            MyRoom.BumpCords(MyX, MyY);
            System.Threading.Thread.Sleep(3000);
            RemoveStatus("talk");
            MyRoom.BumpCords(MyX, MyY);
        }
        internal void MyHabboMouthMove()
        {
            RunSepThread delRP2 = new RunSepThread(TalkHab);
            IAsyncResult tag2 = delRP2.BeginInvoke(null, null);
        }

        internal void SendToUser(string Username, string Packet)
        {
            foreach (int s in SocketManager.activeSockets.Keys)
            {
                HabboUser N = SocketManager.GetInstance(s);
                if (N.Username.ToLower() == Username.ToLower())
                {
                    N.createMessage(Packet);
                    return;
                }
            }
        }
        internal void LookTo(int x, int y)
        {
            MyHeadDirection = WorkoutHead(x,y);
            if (Seated == false)
                MyBodyDirection = WorkoutBody(x, y);
        }
        private void AddStatus(string Status)
        {
            try
            {
                Statuss.Remove(Status);
            }
            catch
            {

            }
            Statuss.Add(Status, Status);
        }

        internal void RemoveStatus(string Status)
        {
            try
            {
                Statuss.Remove(Status);
            }
            catch
            {

            }
        }
        internal void RemoveContainStatus(string Status)
        {
            foreach (string S in Statuss.AllKeys)
            {
                if (S.IndexOf(Status) != -1)
                    Statuss.Remove(S);
            }
        }
        private void NoStatus()
        {
            Statuss.Clear();
        }

        internal string MyStatus()
        {
            string Tmp = "";
            foreach (string S in Statuss.AllKeys)
            {
                Tmp += S + "/";   
            }
            return Tmp;
        }

        private int WorkoutHead(int x, int y)
        {
            if (MyX == x)
            {
                if (MyY < y)
                {
                    return 4;
                }
                if (MyY > y)
                {
                    return 0;
                }
            }

            if (MyX < x)
            {
                if (MyY == y)
                {
                    return 2;
                }

                if (MyY < y)
                {
                    return 3;
                }

                if (MyY > y)
                {
                    return 1;
                }
            }

            if (MyX > x)
            {
                if (MyY == y)
                {
                    return 6;
                }

                if (MyY < y)
                {
                    return 5;
                }

                if (MyY > y)
                {
                    return 7;
                }
            }
            return 0;
        }
        private int WorkoutBody(int x, int y)
        {
            if (MyX < x) // Above Cords
            {
                if (MyY == y) // Directly Down
                {
                    if (MyBodyDirection == 1)
                        return 1;
                    return 2;
                }

                if (MyY < y) // Down
                {
                    if (MyBodyDirection == 0 || MyBodyDirection == 5)
                        if (MyY == y - 1)
                            return 3;
                        else if (MyX == MyX - 1)
                            return 3;
                        else
                            return 4;
                    if (MyBodyDirection == 7)
                        return 3;
                }

                if (MyY > y) // Up
                {
                    if (MyBodyDirection == 6 || MyBodyDirection == 4 || MyBodyDirection == 5)
                        if (MyY == y + 1)
                            return 1;
                        else
                            return 0;
                }
            }
            if (MyX > x) // Below Cords
            {
                if (MyY == y) // Directly Up
                {
                    return 6;
                }

                if (MyY > y) // Right
                {
                    if (MyBodyDirection == 4)
                        if (MyY == y + 1)
                            return 7;
                    if (MyBodyDirection == 3)
                        return 7;
                    if (MyBodyDirection == 5)
                        return 7;
                    if (MyBodyDirection == 7)
                        return 7;
                    if (MyBodyDirection == 0)
                        return 6;
                    if (MyBodyDirection == 1)
                        return MyBodyDirection;
                    return 0;
                }
                if (MyY < y) // Left
                {
                    if (MyBodyDirection == 4)
                        if (MyY == y + 1)
                            return 3;
                    if (MyBodyDirection == 6)
                        return 6;
                    if (MyBodyDirection == 4)
                        return 4;
                    if (MyBodyDirection == 5)
                        return 5;
                    if (MyBodyDirection == 2)
                        return 5;
                    if (MyBodyDirection == 7)
                        return 7;
                    if (MyBodyDirection == 0)
                        return 4;
                    if (MyBodyDirection == 1)
                        return 5;
                    return 4;
                }
            }
            if (x == MyX) // On Same Line
            {
                if (MyY < y)
                {
                    if (MyBodyDirection == 5)
                        return 5;
                    return 4;
                }
                if (MyY > y)
                {
                    if (MyBodyDirection == 1)
                        return 1;
                    return 0;
                }
            }
            return MyBodyDirection;
        }
        internal void Lookto(int x, int y)
        {
            //Moved up/down  
            if (MyX == x)
            {
                if (MyY < y)
                {
                    //South  
                    if (Seated == false)
                        if (MyBodyDirection != 5 || MyBodyDirection != 3)
                            MyBodyDirection = 4;
                    MyHeadDirection = 4;
                    return;
                }
                else
                {
                    if (Seated == false)
                        if (MyBodyDirection != 7 || MyBodyDirection != 1)
                            MyBodyDirection = 0;
                    MyHeadDirection = 0;
                    return;
                }

            } //Moved Left  
            else if (MyX > x)
            {
                if (MyY == y)
                {
                    if (Seated == false)
                        if (MyBodyDirection != 5 || MyBodyDirection != 7)
                            MyBodyDirection = 6;
                    MyHeadDirection = 6;
                    return;
                }
                else if (MyY < y)
                {
                    if (Seated == false)
                        if (MyBodyDirection != 4 || MyBodyDirection != 6)
                            MyBodyDirection = 5;
                    MyHeadDirection = 5;
                    return;
                }
                else
                {
                    if (Seated == false)
                        if (MyBodyDirection != 0 || MyBodyDirection != 6)
                            MyBodyDirection = 7;
                    MyHeadDirection = 7;
                    return;
                }

            } //Moved Right  
            else if (MyX < x)
            {
                if (MyY == y)
                {
                    if (Seated == false)
                        if (MyBodyDirection != 1 || MyBodyDirection != 3)
                            MyBodyDirection = 2;
                    MyHeadDirection = 2;
                    return;
                }
                else if (MyY < y)
                {
                    if (Seated == false)
                        if (MyBodyDirection != 2 || MyBodyDirection != 4)
                            MyBodyDirection = 3;
                    MyHeadDirection = 3;
                    return;
                }
                else
                {
                    if (Seated == false)
                        if (MyBodyDirection != 0 || MyBodyDirection != 2)
                            MyBodyDirection = 1;
                    MyHeadDirection = 1;
                    return;
                }
            }

            if (Seated == false)
                if (MyBodyDirection != 7 || MyBodyDirection != 1)
                    MyBodyDirection = 0;
            MyHeadDirection = 0;
        }
        #endregion
        #endregion
        internal void createMessage(string TheData)
        {
            try
            {
                if (MySocket.Connected == true)
                {
                    byte[] byData = System.Text.Encoding.Default.GetBytes(TheData + Convert.ToChar(1).ToString());
                    MySocket.Send(byData);
                   Main._Debug("Sent: " + TheData);
                }
            }
            catch
            {

            }
        }
    }
}
        #endregion