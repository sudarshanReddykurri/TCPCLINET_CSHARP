using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace TcpClient01
{
    public partial class Form1 : Form
    {

        TcpClient mTcpClient;
        byte[] mRx;

        public Form1()
        {
            InitializeComponent();
        }

        private void tbConsole_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            IPAddress ipa;
            int nPort;
            try
            {
                if(string.IsNullOrEmpty(tbServerIP.Text) || string.IsNullOrEmpty(tbServerPort.Text))
                {
                    return;
                }

                if(!IPAddress.TryParse(tbServerIP.Text, out ipa))
                {
                    MessageBox.Show("Please supply an IP Address.");
                }

                if(!int.TryParse(tbServerPort.Text, out nPort))
                {
                    nPort = 23000;
                }

                mTcpClient = new TcpClient();
                mTcpClient.BeginConnect(ipa, nPort, onCompleteConnect, mTcpClient);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        void onCompleteConnect(IAsyncResult iar)
        {
            TcpClient tcpc;
            try
            {
                tcpc = (TcpClient)iar.AsyncState;
                tcpc.EndConnect(iar);
                mRx = new byte[512];
                tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpc);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] tx;
            if (string.IsNullOrEmpty(tbPayload.Text))
            {
                return;
            }
            try
            {
                tx = Encoding.ASCII.GetBytes(tbPayload.Text);

                if (mTcpClient != null)
                {
                    if (mTcpClient.Client.Connected)
                    {
                        mTcpClient.GetStream().BeginWrite(tx,0,tx.Length, onCompleteWriteToServer, mTcpClient);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void onCompleteWriteToServer(IAsyncResult iar)
        {
            TcpClient tcpc;
            try
            {
                tcpc = (TcpClient)iar.AsyncState;
                tcpc.GetStream().EndWrite(iar);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void onCompleteReadFromServerStream(IAsyncResult iar)
        {
            TcpClient tcpc;
            int nCountBytesReceivedFromServer;
            string strReceived;
            try
            {
                tcpc = (TcpClient)iar.AsyncState;
                nCountBytesReceivedFromServer = tcpc.GetStream().EndRead(iar);

                if(nCountBytesReceivedFromServer == 0)
                {
                    MessageBox.Show("Connection broken.");
                    return;
                }
                strReceived = Encoding.ASCII.GetString(mRx, 0, nCountBytesReceivedFromServer);

                printLine(strReceived);

                mRx = new byte[512];
                tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromServerStream, tcpc);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void printLine(string _strPrint)
        {
            tbConsole.Invoke(new Action<string>(doInvoke), _strPrint);
        }

        public void doInvoke(string _strPrint)
        {
            tbConsole.Text = _strPrint + Environment.NewLine + tbConsole.Text;
        }
    }
}
