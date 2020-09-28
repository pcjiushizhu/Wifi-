// Created by Leestar54
// E-Mail:178078114@qq.com
using NativeWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wifi连接器
{
    public partial class Form1 : Form
    {
        private List<Wlan.WlanAvailableNetwork> NetWorkList = new List<Wlan.WlanAvailableNetwork>();
        private WlanClient.WlanInterface WlanIface;
        public Form1()
        {
            InitializeComponent();

        }

        void WlanIface_WlanConnectionNotification(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
        {
            if (notifyData.notificationSource == NativeWifi.Wlan.WlanNotificationSource.MSM)
            {
                //这里是完成连接
                if ((NativeWifi.Wlan.WlanNotificationCodeMsm)notifyData.NotificationCode == NativeWifi.Wlan.WlanNotificationCodeMsm.Connected)
                {
                    Invoke(new Action(() =>
                    {
                        label2.Text = connNotifyData.profileName;
                    }));
                }
            }
            else if (notifyData.notificationSource == NativeWifi.Wlan.WlanNotificationSource.ACM)
            {
                //连接失败
                if ((NativeWifi.Wlan.WlanNotificationCodeAcm)notifyData.NotificationCode == NativeWifi.Wlan.WlanNotificationCodeAcm.ConnectionAttemptFail)
                {
                    Invoke(new Action(() =>
                    {
                        label2.Text = "未连接";
                    }));
                    MessageBox.Show("连接失败，请检查密码是否正确");
                    WlanIface.DeleteProfile(connNotifyData.profileName);
                }
                if ((NativeWifi.Wlan.WlanNotificationCodeAcm)notifyData.NotificationCode == NativeWifi.Wlan.WlanNotificationCodeAcm.Disconnected)
                {
                    Invoke(new Action(() =>
                    {
                        label2.Text = "未连接";
                    }));
                }
                if ((NativeWifi.Wlan.WlanNotificationCodeAcm)notifyData.NotificationCode == NativeWifi.Wlan.WlanNotificationCodeAcm.Disconnecting)
                {
                    Invoke(new Action(() =>
                    {
                        label2.Text = "未连接";
                    }));
                }
                if ((NativeWifi.Wlan.WlanNotificationCodeAcm)notifyData.NotificationCode == NativeWifi.Wlan.WlanNotificationCodeAcm.ConnectionStart)
                {
                    Invoke(new Action(() =>
                    {
                        label2.Text = "连接中…";
                    }));
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WlanClient client = new WlanClient();
            WlanIface = client.Interfaces[0];//一般就一个网卡，有2个没试过。
            WlanIface.WlanConnectionNotification += WlanIface_WlanConnectionNotification;
            LoadNetWork();
        }

        private void LoadNetWork()
        {
            //System.Int32 dwFlag = new Int32();
            Wlan.WlanAvailableNetwork[] networks = WlanIface.GetAvailableNetworkList(0);
            foreach (Wlan.WlanAvailableNetwork network in networks)
            {
                string SSID = WlanHelper.GetStringForSSID(network.dot11Ssid);
                if (network.flags.HasFlag(Wlan.WlanAvailableNetworkFlags.Connected))
                {
                    label2.Text = SSID;
                }
                //如果有配置文件的SSID会重复出现。过滤掉
                if (!listBox1.Items.Contains(SSID))
                {
                    listBox1.Items.Add(SSID);
                    NetWorkList.Add(network);
                }
            }

            //信号强度排序
            NetWorkList.Sort(delegate(Wlan.WlanAvailableNetwork a, Wlan.WlanAvailableNetwork b)
            {
                return b.wlanSignalQuality.CompareTo(a.wlanSignalQuality);
            });
            listBox1.Items.Clear();
            foreach (Wlan.WlanAvailableNetwork network in NetWorkList)
            {
                listBox1.Items.Add(WlanHelper.GetStringForSSID(network.dot11Ssid));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Wlan.WlanAvailableNetwork wn = NetWorkList[listBox1.SelectedIndex];
            if (wn.securityEnabled && !WlanHelper.HasProfile(WlanIface, WlanHelper.GetStringForSSID(wn.dot11Ssid)))
            {
                Form_Password fp = new Form_Password();
                if (fp.ShowDialog() == DialogResult.OK)
                {
                    string pw = fp.Password;

                    WlanHelper.ConnetWifi(WlanIface, wn, pw);
                    fp.Dispose();
                }
            }
            else
            {
                WlanHelper.ConnetWifi(WlanIface, wn);
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Wlan.WlanAvailableNetwork wn = NetWorkList[listBox1.SelectedIndex];
            toolTip1.SetToolTip(listBox1, WlanHelper.GetWifiToolTip(wn));
        }

        private void label3_Click(object sender, EventArgs e)
        {
            WlanIface.Scan();
            NetWorkList.Clear();
            listBox1.Items.Clear();
            LoadNetWork();
        }
    }
}
