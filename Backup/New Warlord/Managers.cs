using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Warlord
{
    public static class SocketManager
    {
        public static System.Collections.Hashtable activeSockets = new System.Collections.Hashtable();


        internal static Warlord.HabboUser GetInstance(int socketId)
        {
                //Socket exists?
                if (activeSockets.ContainsKey(socketId) == true)
                {
                    //Return ref to the socket object
                    return (Warlord.HabboUser)activeSockets[socketId];
                }
                else
                {
                    //Throw a null socket exception
                    throw new Exception("ID Doesn't exist");
                }
        }
    }

    public class RoomManager
    {
        //System.Collections.Specialized.
        private static System.Collections.Hashtable activeRooms = new System.Collections.Hashtable();
        internal System.Collections.Hashtable UsersInRoom = new System.Collections.Hashtable();
        internal System.Collections.Specialized.NameValueCollection UsersInRoom2 = new System.Collections.Specialized.NameValueCollection();
        internal int[,] MyHeightmap = new int[300, 300];
        internal int[,] FurnitureRotation = new int[300, 300];
        internal int[,] FurnitureElevation = new int[300, 300];
        internal int[,] IsSeat = new int[300, 300];
        internal int[,] Floormap = new int[300, 300];
        internal string[,] SitStatus = new string[300, 300];
        private HabboEncoding HEncoding = new HabboEncoding();
        internal int[,] UsersOnCords = new int[300, 300];
        private string RoomType;
        internal int RoomsID;
        MySQL_Manager MySQL;

        internal static Warlord.RoomManager GetInstance(int RoomId)
        {
            //Socket exists?
            if (activeRooms.ContainsKey(RoomId) == true)
            {
                //Return ref to the socket object
                return (Warlord.RoomManager)activeRooms[RoomId];
            }
            else
            {
                //Throw a null socket exception
                return null;
            }
        }

        public RoomManager(int RoomID, int SocketID, MySQL_Manager MySQl, string RoomType)
        {
            UsersInRoom2.Add(SocketID.ToString(), SocketID.ToString());
            UsersInRoom.Add(SocketID, SocketManager.GetInstance(SocketID));
            if (activeRooms.ContainsKey(RoomID) == false)
            {
                activeRooms.Add(RoomID, this);
                RoomsID = RoomID;
                MySQL = MySQl;
                this.RoomType = RoomType;
                Initialise(SocketID);
                
            }
            //Initialise(SocketID);
           // Initialise();
        }


        internal void RemoveFurniture(long ID)
        {
            int PrevX = int.Parse(MySQL.RunMySQLQuery("SELECT `x` FROM `furniture` WHERE `id` = '" + ID + "'", "x"));
            int PrevY = int.Parse(MySQL.RunMySQLQuery("SELECT `y` FROM `furniture` WHERE `id` = '" + ID + "'", "y"));
            int Width = int.Parse(MySQL.RunMySQLQuery("SELECT `width` FROM `furniture` WHERE `id` = '" + ID + "'", "width"));
            int Length = int.Parse(MySQL.RunMySQLQuery("SELECT `length` FROM `furniture` WHERE `id` = '" + ID + "'", "length"));
            int rotation = int.Parse(MySQL.RunMySQLQuery("SELECT `rotation` FROM `furniture` WHERE `id` = '" + ID + "'", "rotation"));
            int TmpPlusX = 0;
            int TmpPlusY = 0;
            createMessage("A^" + ID);

                for (int i = 0; i < Width; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusY = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusX = i;

                    if (MyHeightmap[PrevX + TmpPlusX, PrevY + TmpPlusY] == -2)
                    {
                        IsSeat[PrevX + TmpPlusX, PrevY + TmpPlusY] = 0;
                    }
                    MyHeightmap[PrevX + TmpPlusX, PrevY + TmpPlusY] = Floormap[PrevX + TmpPlusX, PrevY + TmpPlusY];
                    BumpCords(PrevX + TmpPlusX, PrevY + TmpPlusY);
                }
            
        }

        internal bool PutDownFurniture(long ID, int x, int y, int rotation, int Width, int Length)
        {
            int TmpPlusX = 0;
            int TmpPlusY = 0;
            if (UsersOnCords[x, y] == -1)
                return false;

            if (MyHeightmap[x, y] < 0)
                return false;

            string Sprite = MySQL.RunMySQLQuery("SELECT `sprite` FROM `furniture` WHERE `id` = '" + ID + "'", "sprite");
            double Elevation = double.Parse(MySQL.RunMySQLQuery("SELECT `elevation` FROM `furniture` WHERE `id` = '" + ID + "'", "elevation"));
            string Colour = MySQL.RunMySQLQuery("SELECT `colour` FROM `furniture` WHERE `id` = '" + ID + "'", "colour");
            string Variable = MySQL.RunMySQLQuery("SELECT `variable` FROM `furniture` WHERE `id` = '" + ID + "'", "variable");
            for (int i = 1; i < Width; i++)
            {
                TmpPlusX = 0;
                TmpPlusY = 0;
                if (rotation == 2 || rotation == 6)
                    TmpPlusY = i;
                if (rotation == 4 || rotation == 0)
                    TmpPlusX = i;
                if (UsersOnCords[x + TmpPlusX, y + TmpPlusY] == -1)
                    return false;
                if (MyHeightmap[x + TmpPlusX, y + TmpPlusY] < 0)
                    return false;
            }
            for (int i = 1; i < Length; i++)
            {
                TmpPlusX = 0;
                TmpPlusY = 0;
                if (rotation == 2 || rotation == 6)
                    TmpPlusX = i;
                if (rotation == 4 || rotation == 0)
                    TmpPlusY = i;
                if (UsersOnCords[x + TmpPlusX, y + TmpPlusY] == -1)
                    return false;
                if (MyHeightmap[x + TmpPlusX, y + TmpPlusY] < 0)
                    return false;
            }
            //A_706165chair_plasty*4SARAIIH0.0#ffffff,#00cc00,#ffffff,#00cc00H
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `x` = '" + x + "' WHERE `id` = '" + ID + "'", "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `y` = '" + y + "' WHERE `id` = '" + ID + "'", "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `rotation` = '" + rotation + "' WHERE `id` = '" + ID + "'", "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `inhand` = '0' WHERE `id` = '" + ID + "'", "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `roomid` = '" + RoomsID + "' WHERE `id` = '" + ID + "'", "");
            createMessage("A]" + ID + "" + Sprite + "" + HEncoding.encodeVL64(x) + HEncoding.encodeVL64(y) + HEncoding.encodeVL64(Width) + HEncoding.encodeVL64(Length) + HEncoding.encodeVL64(rotation) + Elevation + "" + Colour + "" + Variable + "");

            bool IsSeaty = false;
            if (MySQL.RunMySQLQuery("SELECT `seat` FROM `furnituretypes` WHERE `sprite` = '" + Sprite + "'", "seat") == "1")
                IsSeaty = true;
            if (IsSeaty == true)
            {
                for (int i = 0; i < Width; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusY = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusX = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 1;
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = -2;
                    SitStatus[x + TmpPlusX, y + TmpPlusY] = MySQL.RunMySQLQuery("SELECT `sitstatus` FROM `furnituretypes` WHERE `sprite` ='" + Sprite + "'", "sitstatus");
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
                for (int i = 0; i < Length; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusX = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusY = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 1;
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = -2;
                    SitStatus[x + TmpPlusX, y + TmpPlusY] = MySQL.RunMySQLQuery("SELECT `sitstatus` FROM `furnituretypes` WHERE `sprite` ='" + Sprite + "'", "sitstatus");
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
            }
            int Walkable = int.Parse(MySQL.RunMySQLQuery("SELECT `walkable` FROM `furnituretypes` WHERE `sprite` = '" + Sprite + "'", "walkable"));
            int HmpVal = -1;
            if (Walkable == 1)
                HmpVal = Floormap[x, y];
            if (IsSeaty == false)
            {
                for (int i = 0; i < Width; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusY = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusX = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 0;
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = HmpVal;
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
                for (int i = 0; i < Length; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusX = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusY = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 0;
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = HmpVal;
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
            }
            BumpCords(x, y);
            return true;
        }
        internal void MoveFurniture(long ID, int x, int y, int rotation)
        {
            int TmpPlusX = 0;
            int TmpPlusY = 0;
            int TmpPlusPrevX = 0;
            int TmpPlusPrevY = 0;
            int PrevX = int.Parse(MySQL.RunMySQLQuery("SELECT `x` FROM `furniture` WHERE `id` = '" + ID + "'","x"));
            int PrevY = int.Parse(MySQL.RunMySQLQuery("SELECT `y` FROM `furniture` WHERE `id` = '" + ID + "'","y"));
            int PrevRotation = int.Parse(MySQL.RunMySQLQuery("SELECT `rotation` FROM `furniture` WHERE `id` = '" + ID + "'", "rotation"));
            if (PrevX != x || PrevY != y)
            {
                if (UsersOnCords[x, y] == -1)
                    return;

                if (MyHeightmap[x, y] < 0)
                    return;
            }
            int Width = int.Parse(MySQL.RunMySQLQuery("SELECT `width` FROM `furniture` WHERE `id` = '" + ID + "'", "width"));
            int Length = int.Parse(MySQL.RunMySQLQuery("SELECT `length` FROM `furniture` WHERE `id` = '" + ID + "'", "length"));
            string Sprite = MySQL.RunMySQLQuery("SELECT `sprite` FROM `furniture` WHERE `id` = '" + ID + "'", "sprite");
            double Elevation = double.Parse(MySQL.RunMySQLQuery("SELECT `elevation` FROM `furniture` WHERE `id` = '" + ID + "'", "elevation"));
            string Colour = MySQL.RunMySQLQuery("SELECT `colour` FROM `furniture` WHERE `id` = '" + ID + "'", "colour");
            string Variable = MySQL.RunMySQLQuery("SELECT `variable` FROM `furniture` WHERE `id` = '" + ID + "'", "variable");
            int i = 0;
            if (PrevX == x && PrevY == y)
                i = 1;
            for (; i < Width; i++)
            {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusY = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusX = i;
                    if (UsersOnCords[x + TmpPlusX,y + TmpPlusY] == -1)
                        return;
                    if (MyHeightmap[x + TmpPlusX, y + TmpPlusY] < 0)
                        return;
            }
            i = 0;
            if (PrevX == x && PrevY == y)
                i = 1;
            for (; i < Length; i++)
            {
                TmpPlusX = 0;
                TmpPlusY = 0;
                if (rotation == 2 || rotation == 6)
                    TmpPlusX = i;
                if (rotation == 4 || rotation == 0)
                    TmpPlusY = i;
                if (UsersOnCords[x + TmpPlusX, y + TmpPlusY] == -1)
                    return;
                if (MyHeightmap[x + TmpPlusX, y + TmpPlusY] < 0)
                    return;
            }
            //A_706165chair_plasty*4SARAIIH0.0#ffffff,#00cc00,#ffffff,#00cc00H
            createMessage("A_" + ID + "" + Sprite + "" + HEncoding.encodeVL64(x) + HEncoding.encodeVL64(y) + HEncoding.encodeVL64(Width) + HEncoding.encodeVL64(Length) + HEncoding.encodeVL64(rotation) + Elevation + "" + Colour + "H" + Variable + "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `x` = '" + x + "' WHERE `id` = '" + ID + "'", "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `y` = '" + y + "' WHERE `id` = '" + ID + "'", "");
            MySQL.RunMySQLQuery("UPDATE `furniture` SET `rotation` = '" + rotation + "' WHERE `id` = '" + ID + "'","");
            if (MyHeightmap[PrevX, PrevY] == -2)
            {
                i = 0;
                for (; i < Width; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    TmpPlusPrevX = 0;
                    TmpPlusPrevY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusY = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusX = i;
                    if (PrevRotation == 2 || PrevRotation == 6)
                        TmpPlusPrevY = i;
                    if (PrevRotation == 4 || PrevRotation == 0)
                        TmpPlusPrevX = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[PrevX + TmpPlusY, PrevY + TmpPlusX] = 0;
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 1;
                    MyHeightmap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY] = Floormap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY];
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = -2;
                    SitStatus[x + TmpPlusX, y + TmpPlusY] = SitStatus[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY];
                    BumpCords(PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY);
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
                i = 0;
                for (; i < Length; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    TmpPlusPrevX = 0;
                    TmpPlusPrevY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusX = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusY = i;
                    if (PrevRotation == 2 || PrevRotation == 6)
                        TmpPlusPrevY = i;
                    if (PrevRotation == 4 || PrevRotation == 0)
                        TmpPlusPrevX = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[PrevX + TmpPlusY, PrevY + TmpPlusX] = 0;
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 1;
                    MyHeightmap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY] = Floormap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY];
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = -2;
                    SitStatus[x + TmpPlusX, y + TmpPlusY] = SitStatus[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY];
                    BumpCords(PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY);
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
            }
            if (MyHeightmap[PrevX, PrevY] == -1)
            {
                for (i = 0; i < Width; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    TmpPlusPrevX = 0;
                    TmpPlusPrevY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusY = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusX = i;
                    if (PrevRotation == 2 || PrevRotation == 6)
                        TmpPlusPrevY = i;
                    if (PrevRotation == 4 || PrevRotation == 0)
                        TmpPlusPrevX = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[PrevX + TmpPlusY, PrevY + TmpPlusX] = 0;
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 0;
                    MyHeightmap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY] = Floormap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY];
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = -1;
                    BumpCords(PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY);
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
                for (i = 0; i < Length; i++)
                {
                    TmpPlusX = 0;
                    TmpPlusY = 0;
                    TmpPlusPrevX = 0;
                    TmpPlusPrevY = 0;
                    if (rotation == 2 || rotation == 6)
                        TmpPlusX = i;
                    if (rotation == 4 || rotation == 0)
                        TmpPlusY = i;
                    if (PrevRotation == 2 || PrevRotation == 6)
                        TmpPlusPrevY = i;
                    if (PrevRotation == 4 || PrevRotation == 0)
                        TmpPlusPrevX = i;
                    FurnitureElevation[x + TmpPlusX, y + TmpPlusY] = Floormap[x, y];
                    IsSeat[PrevX + TmpPlusY, PrevY + TmpPlusX] = 0;
                    IsSeat[x + TmpPlusX, y + TmpPlusY] = 0;
                    MyHeightmap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY] = Floormap[PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY];
                    MyHeightmap[x + TmpPlusX, y + TmpPlusY] = -1;
                    BumpCords(PrevX + TmpPlusPrevX, PrevY + TmpPlusPrevY);
                    FurnitureRotation[x + TmpPlusX, y + TmpPlusY] = rotation;
                }
            }
            BumpCords(PrevX, PrevY);
        }

        internal void BumpCords(int x, int y)
        {
            if (UsersOnCords[x, y] != -1)
                return;

            foreach (string s in UsersInRoom2.AllKeys)
            {
                HabboUser TheHabbo = SocketManager.GetInstance(int.Parse(s));
                if (TheHabbo.MyX == x && TheHabbo.MyY == y)
                    if (IsSeat[TheHabbo.MyX, TheHabbo.MyY] != 1)
                    {
                        createMessage("@b" + s + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyBodyDirection + "/" + TheHabbo.MyStatus() + "/");
                    }
                    else
                    {
                        //TheHabbo.MyHeadDirection = FurnitureRotation[x, y];
                        TheHabbo.MyBodyDirection = FurnitureRotation[x, y];
                        createMessage("@b" + s + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyBodyDirection + "/" + SitStatus[x, y] + " " + (TheHabbo.MyElevation + 1) + "/" + TheHabbo.MyStatus() + "/");
                    }
            }
        }
        private void Initialise(int MySocketID)
        {
            string tmplin = "";
            string TmpHeightmap = MySQL.RunMySQLQuery("SELECT `heightmap` FROM `" + RoomType + "` WHERE `id` = '" + RoomsID + "'", "heightmap");
            int i = 0;
            int p = 0;
            HabboUser B = SocketManager.GetInstance(MySocketID);
            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 300; y++)
                {
                    MyHeightmap[x, y] = -1;
                    Floormap[x, y] = -1;
                }
            }
            for (int x = 0; x < TmpHeightmap.Length + 1; x++, p++)
            {
                try
                {
                    string TmpChar;
                SkipMe:
                    TmpChar = TmpHeightmap.Substring(x, 1);
                    if (TmpChar == "\r") { i++; x = x + 2; p = 0; B.Main._Debug(tmplin); tmplin = "" ; goto SkipMe; }
                    if (p == 10 && i == 5) { }
                    int TheHmapVal;
                    switch (TmpChar)
                    {
                        case "x":
                            TheHmapVal = -1;
                            break;

                        case "0":
                            TheHmapVal = 0;
                            break;

                        default:
                            TheHmapVal = int.Parse(TmpChar);
                            break;
                    }

                    MyHeightmap[p, i] = TheHmapVal;
                    Floormap[p, i] = TheHmapVal;
                    
                    tmplin+= TmpChar;
                }
                catch
                {
                    Floormap[p, i] = -1;
                    MyHeightmap[p, i] = -1;
                }
            
            }
            int FurniCount;
            FurniCount = MySQL.RunCountMySQLQuery("SELECT `id` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "id");
            for (int y = 0; y < FurniCount; y++)
            {
                string TmpFurniSprite;
                int TmpFurniX;
                int TmpFurniY;
                int TmpFurniWidth;
                int TmpFurniLength;
                int TmpFurniRotation;
                double TmpFurniElevation;
            RetryIt:
                if (RoomsID == 0)
                {
                    System.Threading.Thread.Sleep(500);
                    goto RetryIt;
                }
                TmpFurniSprite = MySQL.SpecificMySQLQueryOutput("SELECT `sprite` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "sprite", y);
                string TmpFurniVariable = MySQL.SpecificMySQLQueryOutput("SELECT `variable` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "variable", y);
                TmpFurniX = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `x` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "x", y));
                TmpFurniY = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `y` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "y", y));
                TmpFurniWidth = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `width` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "width", y));
                TmpFurniLength = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `length` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "length", y));
                TmpFurniRotation = int.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `rotation` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "rotation", y));
                TmpFurniElevation = double.Parse(MySQL.SpecificMySQLQueryOutput("SELECT `elevation` FROM `furniture` WHERE `roomid` = '" + RoomsID + "'", "elevation", y));

                int TmpPlusY = 0;
                int TmpPlusX = 0;
                int WALKABLE = int.Parse(MySQL.RunMySQLQuery("SELECT `walkable` FROM `furnituretypes` WHERE `sprite` = '" + TmpFurniSprite + "'", "walkable"));
                int IsDoor = int.Parse(MySQL.RunMySQLQuery("SELECT `door` FROM `furnituretypes` WHERE `sprite` = '" + TmpFurniSprite + "'", "door"));
                if (IsDoor == 1)
                {
                    if (TmpFurniVariable == "O")
                        WALKABLE = 1;
                }
                if (WALKABLE == 0)
                {
                    MyHeightmap[TmpFurniX, TmpFurniY] = -1;
                    for (int x = 0; x < TmpFurniWidth; x++)
                    {
                        TmpPlusX = 0;
                        TmpPlusY = 0;
                        if (TmpFurniRotation == 2 || TmpFurniRotation == 6)
                            TmpPlusY = x;
                        if (TmpFurniRotation == 4 || TmpFurniRotation == 0)
                            TmpPlusX = x;
                        if (MyHeightmap[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] > -1)
                        MyHeightmap[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = -1;
                    }
                    for (int x = 0; x < TmpFurniLength; x++)
                    {
                        TmpPlusX = 0;
                        TmpPlusY = 0;
                        if (TmpFurniRotation == 2 || TmpFurniRotation == 6)
                            TmpPlusX = x;
                        if (TmpFurniRotation == 4 || TmpFurniRotation == 0)
                            TmpPlusY = x;
                        if (MyHeightmap[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] > -1)
                        MyHeightmap[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = -1;
                    }
                }


                int ISSEAT = int.Parse(MySQL.RunMySQLQuery("SELECT `seat` FROM `furnituretypes` WHERE `sprite` = '" + TmpFurniSprite + "'", "seat"));

                if (ISSEAT == 1)
                {
                    IsSeat[TmpFurniX, TmpFurniY] = 1;
                    MyHeightmap[TmpFurniX, TmpFurniY] = -2;
                    FurnitureRotation[TmpFurniX, TmpFurniY] = TmpFurniRotation;

                    for (int x = 0; x < TmpFurniWidth; x++)
                    {
                        TmpPlusY = 0;
                        TmpPlusX = 0;
                        if (TmpFurniRotation == 2 || TmpFurniRotation == 6)
                            TmpPlusY = x;
                        if (TmpFurniRotation == 4 || TmpFurniRotation == 0)
                            TmpPlusX = x;

                        IsSeat[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = 1;
                        MyHeightmap[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = -2;
                        FurnitureRotation[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = TmpFurniRotation;
                    }
                    for (int x = 0; x < TmpFurniLength; x++)
                    {
                        TmpPlusY = 0;
                        TmpPlusX = 0;
                        if (TmpFurniRotation == 2 || TmpFurniRotation == 6)
                            TmpPlusX = x;
                        if (TmpFurniRotation == 4 || TmpFurniRotation == 0)
                            TmpPlusY = x;

                        IsSeat[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = 1;
                        MyHeightmap[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = -2;
                        FurnitureRotation[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = TmpFurniRotation;
                    }

                    string SITSTATUS = MySQL.RunMySQLQuery("SELECT `sitstatus` FROM `furnituretypes` WHERE `sprite` = '" + TmpFurniSprite + "'", "sitstatus");
                    SitStatus[TmpFurniX, TmpFurniY] = SITSTATUS;
                    for (int x = 0; x < TmpFurniWidth; x++)
                    {
                        TmpPlusX = 0;
                        TmpPlusY = 0;
                        if (TmpFurniRotation == 2 || TmpFurniRotation == 6)
                            TmpPlusY = x;
                        if (TmpFurniRotation == 4 || TmpFurniRotation == 0)
                            TmpPlusX = x;
                        SitStatus[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = SITSTATUS;
                    }
                    for (int x = 0; x < TmpFurniLength; x++)
                    {
                        TmpPlusX = 0;
                        TmpPlusY = 0;
                        if (TmpFurniRotation == 2 || TmpFurniRotation == 6)
                            TmpPlusX = x;
                        if (TmpFurniRotation == 4 || TmpFurniRotation == 0)
                            TmpPlusY = x;
                        SitStatus[TmpFurniX + TmpPlusX, TmpFurniY + TmpPlusY] = SITSTATUS;
                    }
                }
                }




            }

        internal void MoveMe(int x, int y, int MySocketID)
        {
            HabboUser TheHabbo = SocketManager.GetInstance(MySocketID);
            TheHabbo.Seated = false;
            TheHabbo.Lookto(x, y);
            if (IsSeat[x, y] == 1)
            {
                TheHabbo.RemoveContainStatus("dance");
                TheHabbo.Seated = true;
                createMessage("@b" + MySocketID + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyBodyDirection + "/" + TheHabbo.MyStatus() + "/mv " + x + "," + y + "," + FurnitureElevation[y,x] + "/");
                UsersOnCords[TheHabbo.MyX, TheHabbo.MyY] = 0;
                TheHabbo.MyX = x;
                TheHabbo.MyY = y;
                System.Threading.Thread.Sleep(390);
                TheHabbo.MyElevation = FurnitureElevation[x, y];
                TheHabbo.MyHeadDirection = FurnitureRotation[x, y];
                TheHabbo.MyBodyDirection = FurnitureRotation[x, y];
                UsersOnCords[x, y] = -1;
                
                createMessage("@b" + MySocketID + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyBodyDirection + "/" + SitStatus[x,y] + " " + (TheHabbo.MyElevation + 1) + "/" + TheHabbo.MyStatus() + "/");
            }
            else
            {
                TheHabbo.Seated = false;
                createMessage("@b" + MySocketID + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyBodyDirection + "/" + TheHabbo.MyStatus() + "/mv " + x + "," + y + "," + MyHeightmap[x,y] + "/");
                UsersOnCords[TheHabbo.MyX, TheHabbo.MyY] = 0;
                TheHabbo.MyX = x;
                TheHabbo.MyY = y;
                System.Threading.Thread.Sleep(390);
                TheHabbo.MyElevation = MyHeightmap[x, y];
                UsersOnCords[x,y] = -1;
                createMessage("@b" + MySocketID + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyBodyDirection + "/" + TheHabbo.MyStatus() + "/");
            }
            //System.Threading.Thread.Sleep(500);
        }
        private delegate void RunPacketDelegate(string TheData);
        internal void createMessage(string TheData)
        {
            RunPacketDelegate delRP2 = new RunPacketDelegate(CrMsg);
             IAsyncResult tag2 = delRP2.BeginInvoke(TheData, null, null);

        }
        private void CrMsg(string TheData)
        {
            foreach (string s in UsersInRoom2.AllKeys)
            {
                HabboUser TheHabbo = SocketManager.GetInstance(int.Parse(s));
                TheHabbo.createMessage(TheData);
            }
        }

        internal void createMessageRights(string TheData)
        {
            foreach (string s in UsersInRoom2.AllKeys)
            {
                HabboUser TheHabbo = SocketManager.GetInstance(int.Parse(s));
                if (TheHabbo.GotRights == true)
                    TheHabbo.createMessage(TheData);

            }
        }
        internal void WhisperToUser(string TheText, int SocketID, string Username)
        {
            int StartX = SocketManager.GetInstance(SocketID).MyX;
            int StartY = SocketManager.GetInstance(SocketID).MyY;

            SocketManager.GetInstance(SocketID).createMessage("@Y" + HEncoding.encodeVL64(SocketID) + TheText + Convert.ToChar(2).ToString());
            //TheText = TheText.Split(" ");
            //TheText = T
            bool InRange;
            string[] Coords = new string[20];
            int CordNum = 0;
            for (int x = (StartX - 1); x <= (StartX + 1); x++)
            {
                for (int y = (StartY - 1); y <= (StartY + 1); y++)
                {
                    Coords[CordNum] = x + "-" + y;
                    CordNum++;
                }
            }
            HabboUser TheHabbo = SocketManager.GetInstance(SocketID);
            foreach (string s in UsersInRoom2.AllKeys)
            {
                TheHabbo = SocketManager.GetInstance(int.Parse(s));
                if (TheHabbo.Username.ToLower() == Username.ToLower())
                {
                    int TheirID = int.Parse(s);
                    break;
                }
            }
                for (int i = 0; i <= 16; i++)
                {
                    if (Coords[i] == TheHabbo.MyX + "-" + TheHabbo.MyY)
                    {
                        TheHabbo.createMessage("@Y" + HEncoding.encodeVL64(SocketID) + TheText + Convert.ToChar(2).ToString());
                        break;
                    }
                    else
                    {
                    }
                }

            

        }
        internal void SayToRoom(string TheText, int SocketID)
        {
            int StartX = SocketManager.GetInstance(SocketID).MyX;
            int StartY = SocketManager.GetInstance(SocketID).MyY;

         //SocketManager.GetInstance(SocketID).createMessage("@X" + HEncoding.encodeVL64(SocketID) + TheText + Convert.ToChar(2).ToString());
         
            bool InRange;
            string[] Coords = new string[100];
            int CordNum = 0;
                for (int x = (StartX - 2); x <= (StartX + 2); x++)
                {
                    for (int y = (StartY - 2); y <= (StartY + 2); y++)
                    {
                       Coords[CordNum] = x + "-" + y;
                       CordNum++;
                    }
                }

                string[] CoordsDotted = new string[100];
                int CordNum2 = 0;
                for (int x = (StartX - 4); x <= (StartX + 4); x++)
                {
                    for (int y = (StartY - 4); y <= (StartY + 4); y++)
                    {
                        CoordsDotted[CordNum2] = x + "-" + y;
                        CordNum2++;
                    }
                }
            foreach (string s in UsersInRoom2.AllKeys)
            {
                HabboUser TheHabbo = SocketManager.GetInstance(int.Parse(s));
                // All you need to do is do the "InRange" Part.
                // Have Fun! :P

                
                //if (Coords[TheHabbo.MyX + "-" + TheHabbo.MyY] == true)
                //{
                       //TheHabbo.createMessage("@X" + HEncoding.encodeVL64(SocketID) + TheText + Convert.ToChar(2).ToString());
                //}
                       for (int i = 0; i <= 81; i++)
                       {
                           if (Coords[i] == TheHabbo.MyX + "-" + TheHabbo.MyY)
                           {
                               TheHabbo.createMessage("@X" + HEncoding.encodeVL64(SocketID) + TheText + Convert.ToChar(2).ToString());
                               break;
                           }
                           else if(CoordsDotted[i] == TheHabbo.MyX + "-" + TheHabbo.MyY)
                           {
                               TheHabbo.createMessage("@X" + HEncoding.encodeVL64(SocketID) + DotTheText(TheText) + Convert.ToChar(2).ToString());
                               break;
                           }
                           else
                           {
                           }
                       }
                

            }

        }
        internal string DotTheText(string TheText)
        {
            Random Rnd = new Random();
            string RebuiltText = "";
            for (int i = 0; i < TheText.Length; i++)
            {
                if (TheText.Substring(i, 1) == " ")
                {
                    RebuiltText += " ";
                    continue;
                }
                if (Rnd.Next(0, 5) == 1)
                {
                    RebuiltText += ".";
                }
                else
                {
                    RebuiltText += TheText.Substring(i, 1);
                }
            }
            return RebuiltText;
        }

        internal string BuildRoomUsers()
        {
            string TmpBuilder = "@\\";
            foreach (string s in UsersInRoom2.AllKeys)
            {
                HabboUser TheHabbo = SocketManager.GetInstance(int.Parse(s));
                        MyHeightmap[TheHabbo.MyX, TheHabbo.MyY] = -1;
                        //SendData("@b" + TheHabbo.SocketID + " " + TheHabbo.MyX + "," + TheHabbo.MyY + "," + TheHabbo.MyElevation + "," + TheHabbo.MyHeadDirection + "," + TheHabbo.MyHeadDirection + "/" + TheHabbo.MyStatus() + "/");
                        if (TheHabbo.BadgeStatus == 1)
                        {
                            TmpBuilder += "i:" + s.ToString() + Convert.ToChar(13) + "a:" + TheHabbo.HabboID + Convert.ToChar(13) + "n:" + TheHabbo.Username + Convert.ToChar(13) + "f:" + TheHabbo.Figure + Convert.ToChar(13) + "l:" + TheHabbo.MyX + " " + TheHabbo.MyY + " " + TheHabbo.MyElevation + Convert.ToChar(13) + "c:" + TheHabbo.Mission + Convert.ToChar(13) + "s:" + TheHabbo.Sex + Convert.ToChar(13) + "b:" + TheHabbo.Badge + Convert.ToChar(13);
                        }
                        else
                        {
                            TmpBuilder += "i:" + s.ToString() + Convert.ToChar(13) + "a:" + TheHabbo.HabboID + Convert.ToChar(13) + "n:" + TheHabbo.Username + Convert.ToChar(13) + "f:" + TheHabbo.Figure + Convert.ToChar(13) + "l:" + TheHabbo.MyX + " " + TheHabbo.MyY + " " + TheHabbo.MyElevation + Convert.ToChar(13) + "c:" + TheHabbo.Mission + Convert.ToChar(13) + "s:" + TheHabbo.Sex + Convert.ToChar(13);
                        }
            }
            return TmpBuilder;
        }

        internal string RefreshUsers()
        {
            StringBuilder PacketBuilder = new StringBuilder("@b");
            foreach (string s in UsersInRoom2.AllKeys)
            {
                HabboUser TheHabbo = SocketManager.GetInstance(int.Parse(s));
                    PacketBuilder.Append(s + " " + TheHabbo.MyX.ToString() + "," + TheHabbo.MyY.ToString() + "," + TheHabbo.MyElevation.ToString() + "," + TheHabbo.MyHeadDirection.ToString() + "," + TheHabbo.MyBodyDirection.ToString() + "/" + SitStatus[TheHabbo.MyX,TheHabbo.MyY] + "/" + TheHabbo.MyStatus() + "/" + TheHabbo.Chr(13));
            }
            return PacketBuilder.ToString();
        }

        internal void RemoveUser(int SocketID)
        {
            try
            {
                SocketManager.GetInstance(SocketID).MyRoomID = 0;
            }
            catch
            {

            }
            createMessage("@]" + SocketID);
            try
            {
                UsersInRoom.Remove(SocketID);
                UsersInRoom2.Remove(SocketID.ToString());
            }
            catch
            {
            }
            try
            {
                SocketManager.GetInstance(SocketID).createMessage("@R");
                UsersOnCords[SocketManager.GetInstance(SocketID).MyX, SocketManager.GetInstance(SocketID).MyY] = 0;
            }
            catch
            {

            }
            finally
            {
                int TempInt2 = int.Parse(MySQL.RunMySQLQuery("SELECT `curin` FROM `" + RoomType + "` WHERE `id` = '" + RoomsID + "'", "curin"));
                if (TempInt2 < 1) { TempInt2 = 1; }
                MySQL.RunMySQLQuery("UPDATE `" + RoomType + "` SET `curin` = '" + (TempInt2 - 1) + "'  WHERE `id` = '" + RoomsID + "'", "");
            }
        }

        internal void RemoveUserNoKick(int SocketID)
        {
            try
            {
                UsersInRoom.Remove(SocketID);
                UsersInRoom2.Remove(SocketID.ToString());
                createMessage("@]" + SocketID);
                SocketManager.GetInstance(SocketID).MyRoomID = 0;
                UsersOnCords[SocketManager.GetInstance(SocketID).MyX, SocketManager.GetInstance(SocketID).MyY] = 0;

            }
            catch
            {

            }
            finally
            {
                int TempInt2 = int.Parse(MySQL.RunMySQLQuery("SELECT `curin` FROM `" + RoomType + "` WHERE `id` = '" + RoomsID + "'", "curin"));
                if (TempInt2 < 1) { TempInt2 = 1; }
                MySQL.RunMySQLQuery("UPDATE `" + RoomType + "` SET `curin` = '" + (TempInt2 - 1) + "'  WHERE `id` = '" + RoomsID + "'", "");
            }
        }

        internal void NewUser(int SocketID)
        {
            string TmpBuilder = "";
            HabboUser TheHabbo = SocketManager.GetInstance(SocketID);
            int TempInt2 = int.Parse(MySQL.RunMySQLQuery("SELECT `curin` FROM `" + RoomType + "` WHERE `id` = '" + RoomsID + "'", "curin"));
            if (TempInt2 < 0) { TempInt2 = 0; }
            MySQL.RunMySQLQuery("UPDATE `" + RoomType + "` SET `curin` = '" + (TempInt2 + 1) + "'  WHERE `id` = '" + RoomsID + "'", "");
            if (TheHabbo.BadgeStatus == 1)
            {
                TmpBuilder += "i:" + SocketID.ToString() + Convert.ToChar(13) + "a:" + TheHabbo.HabboID + Convert.ToChar(13) + "n:" + TheHabbo.Username + Convert.ToChar(13) + "f:" + TheHabbo.Figure + Convert.ToChar(13) + "l:" + TheHabbo.MyX + " " + TheHabbo.MyY + " " + TheHabbo.MyElevation + Convert.ToChar(13) + "c:" + TheHabbo.Mission + Convert.ToChar(13) + "s:" + TheHabbo.Sex + Convert.ToChar(13) + "b:" + TheHabbo.Badge + Convert.ToChar(13);
            }
            else
            {
                TmpBuilder += "i:" + SocketID.ToString() + Convert.ToChar(13) + "a:" + TheHabbo.HabboID + Convert.ToChar(13) + "n:" + TheHabbo.Username + Convert.ToChar(13) + "f:" + TheHabbo.Figure + Convert.ToChar(13) + "l:" + TheHabbo.MyX + " " + TheHabbo.MyY + " " + TheHabbo.MyElevation + Convert.ToChar(13) + "c:" + TheHabbo.Mission + Convert.ToChar(13) + "s:" + TheHabbo.Sex + Convert.ToChar(13);
            }
            createMessage("@\\" + TmpBuilder);
            BumpCords(TheHabbo.MyX, TheHabbo.MyY);
        }


                ~RoomManager()
        {
             foreach (string s in UsersInRoom2.AllKeys)
            {
                createMessage("B!Room Disposed! If you see this, then somethings fucked up! :D");
                RemoveUser(int.Parse(s.ToString()));
            }
        }
    }

        public class MySQL_Manager
        {
            private System.Data.Odbc.OdbcConnection OdbcCon;
            private System.Data.Odbc.OdbcCommand OdbcCom;
            private System.Data.Odbc.OdbcDataReader OdbcDR;
            private string ConStr;
            FrmMain Main;
            #region MySQL Connection Information
            public string MySQL_IP = "localhost";
            public int MySQL_Port = 3306;
            public string MySQL_DB = "jeax_server";
            public string MySQL_Username = "root";
            public string MySQL_Password = "";
            #endregion

            public MySQL_Manager(FrmMain FrmMainRef)
            {
                Main = FrmMainRef;
            }
            public void Connect()
            {
                ConStr = "DRIVER={MySQL ODBC 3.51 Driver};SERVER=" + MySQL_IP + ";PORT=" + MySQL_Port + ";DATABASE=" + MySQL_DB + ";UID=" + MySQL_Username + ";PWD=" + MySQL_Password + ";OPTION=3";
                OdbcCon = new System.Data.Odbc.OdbcConnection(ConStr);


                try
                {

                    if (OdbcCon.State == ConnectionState.Closed)
                    {
                        OdbcCon.Open();
                    }

                    Main._Debug("Connected to MySQL");
                }
                catch (System.Data.Odbc.OdbcException Ex)
                {
                    Main._Debug("MySQL Error - " + Ex.Message);
                }
            }

            internal string RunMySQLQuery(string TheQuery, string FieldName)
            {
                try
                {
                    OdbcCom = new System.Data.Odbc.OdbcCommand(TheQuery, OdbcCon);
                    OdbcDR = OdbcCom.ExecuteReader();
                    string TempData = "";
                    try
                    {
                        while (OdbcDR.Read())
                        {
                            TempData = TempData + OdbcDR[FieldName] + Convert.ToChar(5);
                        }
                        TempData = TempData.Substring(0, TempData.Length - 1);
                    }
                    catch
                    {
                        //Main._Debug("Error running Query: " + TheQuery + " in '" + FieldName + "'");
                    }

                    //Main._Debug("Ran " + TheQuery);
                    return TempData;
                    

                }
                catch (System.Data.Odbc.OdbcException Ex)
                {
                    Main._Debug("Error with running query '" + TheQuery + "' Error Returned: " + Ex.Message);
                    return "";
                }
            }
            internal string SpecificMySQLQueryOutput(string TheQuery, string FieldName, int ReturnValue)
            {
                try
                {
                    OdbcCom = new System.Data.Odbc.OdbcCommand(TheQuery, OdbcCon);
                    OdbcDR = OdbcCom.ExecuteReader();
                    string TempData = "";
                    int Counterr = 0;
                    try
                    {
                        while (OdbcDR.Read())
                        {
                            if (Counterr == ReturnValue)
                            {
                                TempData = OdbcDR[FieldName].ToString();
                            }
                            Counterr++;
                        }

                    }
                    catch
                    {

                        //txtLog.AppendText("No Data to output from " + FieldName + "\r\n");
                    }

                    return TempData;

                }
                catch (System.Data.Odbc.OdbcException Ex)
                {
                    Main._Debug("Error with running query '" + TheQuery + "' Error Returned: " + Ex.Message + "\r\n");
                    return "";
                }
            }

            internal int RunCountMySQLQuery(string TheQuery, string FieldName)
            {
                try
                {
                    OdbcCom = new System.Data.Odbc.OdbcCommand(TheQuery, OdbcCon);
                    OdbcDR = OdbcCom.ExecuteReader();
                    int TempCounter = 0;
                    try
                    {
                        while (OdbcDR.Read())
                        {
                            TempCounter++;
                        }

                    }
                    catch
                    {

                        //txtLog.AppendText("No Data to output from " + FieldName + "\r\n");
                    }

                    return TempCounter;

                }
                catch (System.Data.Odbc.OdbcException Ex)
                {
                    Main._Debug("Error with running query '" + TheQuery + "' Error Returned: " + Ex.Message + "\r\n");
                    return 0;
                }
            }

        }


}