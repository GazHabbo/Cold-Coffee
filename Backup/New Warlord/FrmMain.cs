using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Warlord

{
    public partial class FrmMain : Form
    {
        private ContextMenu UserMenu = new ContextMenu();
        public delegate void ADbg(string Text);
        public delegate void ADbg2(string Key, string Text, int Socket);
        public delegate void ADbg3(int Integer);
        public ADbg _Debug;
        public ADbg3 _RemoveNode;
        public ADbg2 _NewNode;
        private MySQL_Manager MySQL;
        private const int BackLog = 5;
        public static int MaxConnections = 100;
        public Socket s_Listener;
        public Socket[] s_Worker = new Socket[MaxConnections];
        internal FrmMain()
        {
            _Debug = new ADbg(AddToDebug);
            _NewNode = new ADbg2(NewNode);
            _RemoveNode = new ADbg3(RemoveNode);
            InitializeComponent();
            Ini.IniFile DBDetails = new Ini.IniFile(Application.StartupPath + "\\Config.ini");
            string DBHost = DBDetails.IniReadValue("database", "host");
            string DBName = DBDetails.IniReadValue("database", "db");
            string DBUsername = DBDetails.IniReadValue("database", "username");
            string DBPassword = DBDetails.IniReadValue("database", "password");
            int DBPort = int.Parse(DBDetails.IniReadValue("database", "port"));
            MySQL = new MySQL_Manager(this);
            MySQL.MySQL_DB = DBName;
            MySQL.MySQL_IP = DBHost;
            MySQL.MySQL_Username = DBUsername;
            MySQL.MySQL_Port = DBPort;
            MySQL.MySQL_Password = DBPassword;
            MySQL.Connect();
            StaticValues B = new StaticValues(this,MySQL);
            UserMenu.MenuItems.Add("&Disconnect",new EventHandler(this.MnuUsersDC_Click));
            UserMenu.MenuItems.Add("&Alert", new EventHandler(this.MnuUsersAlert_Click));
            
        }



        private void MnuUsersDC_Click(object sender, EventArgs e)
        {
            DialogResult B =  MessageBox.Show("Are you sure you wish to disconnect this user?", "",  MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if (B.ToString() == "No") { return; }
            SocketManager.GetInstance(int.Parse(TVClients.SelectedNode.Name)).DropConnection();
        }

        private void MnuUsersAlert_Click(object sender, EventArgs e)
        {
            InputBoxResult result = InputBox.Show("Insert the alert text", "Project Cold Coffee", "", null);
            if (result.OK)
            {
                SocketManager.GetInstance(int.Parse(TVClients.SelectedNode.Name)).createMessage("BK" + result.Text);
            }
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            TVClients.Nodes.Add("Unestablished", "Not Logged In");
            TVClients.Nodes.Add("Users", "Normal Users");
            TVClients.Nodes.Add("Staff", "Adminstrators");
            MySQL.RunMySQLQuery("UPDATE `public` SET `CurIn` = '0'", "");
            MySQL.RunMySQLQuery("UPDATE `private` SET `CurIn` = '0'", "");
            #region Setup Sockets
            s_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint LocalCrap = new IPEndPoint(IPAddress.Any, 1337);
            s_Listener.Bind(LocalCrap);
            s_Listener.Listen(BackLog);
            _Debug("Started listening for connection requests on port '1337'");
            s_Listener.BeginAccept(new AsyncCallback(ConnectionRequest), null);
            #endregion
        }
        private void ConnectionRequest(IAsyncResult syncc)
        {
            try
            {
                int MaSocket;
                MaSocket = FindEmptySocket();
                if (MaSocket == 0) { _Debug("Connection Rejected - No Socket Found!"); return; } 
                _Debug("New connection established {" + MaSocket + "}");
                s_Worker[MaSocket] = s_Listener.EndAccept(syncc);
                _NewNode("Unestablished",s_Worker[MaSocket].RemoteEndPoint.ToString(),MaSocket);
                s_Listener.BeginAccept(new AsyncCallback(ConnectionRequest), null);
                HabboUser Newconnection = new HabboUser(s_Worker[MaSocket], MaSocket,this, MySQL);
                SocketManager.activeSockets.Add(MaSocket, Newconnection);
            }
            catch (Exception ex)
            {
                _Debug(ex.Message);
            }
        }

        private int FindEmptySocket()
        {
            for (int x = 1; x < MaxConnections; x++)
            {
                if (!SocketManager.activeSockets.ContainsKey(x))
                {
                    return x;
                }
            }
            return 0;
        }
        internal void AddToDebug(string Text)
        {
            if (this.InvokeRequired)
            { this.Invoke(_Debug, Text); }
            else
            {
                if (PauseLog.Checked == true)
                {
                }
                else
                {
                    TxtLog.AppendText("(" + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + ") -- " + Text + "\r\n");
                }
            }
        }

        internal void NewNode(string Key, string NodeText, int Socket )
        {
            try
            {
                //  TVClients.Nodes["Users"].Nodes.Add("Hello!");
                if (this.InvokeRequired)
                { this.Invoke(_NewNode, Key, NodeText, Socket); }
                else
                {
                    TVClients.Nodes[Key].Nodes.Add(Socket.ToString(), NodeText);
                }

            }
            catch
            {

            }
        }


        internal void RemoveNode(int Socket)
        {
            try
            {
                try
                {
                    //  TVClients.Nodes["Users"].Nodes.Add("Hello!");
                    if (this.InvokeRequired)
                    { this.Invoke(_RemoveNode, Socket); }
                    else
                    {
                        TVClients.Nodes["Unestablished"].Nodes[Socket.ToString()].Remove();
                    }
                }
                catch
                {
                    try
                    {
                        TVClients.Nodes["Users"].Nodes[Socket.ToString()].Remove();
                    }
                    catch
                    {
                        TVClients.Nodes["Staff"].Nodes[Socket.ToString()].Remove();
                    }
                }
            }
            catch
            {

            }
        }

        private void TVClients_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
           if (e.Button == MouseButtons.Right)
            {
                TVClients.SelectedNode = e.Node;
                if (TVClients.SelectedNode.Name == "Users") { return; }
                if (TVClients.SelectedNode.Name == "Unestablished") { return; }
                if (TVClients.SelectedNode.Name == "Staff") { return; }
                 UserMenu.Show(TVClients, new Point(50, 30));
                //SocketManager.GetInstance(int.Parse(TVClients.SelectedNode.Name)).DropConnection();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtLog.SelectedText);
            }
            catch { }
            }

    }
}