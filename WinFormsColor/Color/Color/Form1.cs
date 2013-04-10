using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Color
{
    public partial class Form1 : Form {
        private bool _listeningToServiceBus;
        private string _subscriptionResource;
        private HttpServiceBus _httpSb;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            System.Threading.ThreadPool.QueueUserWorkItem(ListenToServiceBus);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _listeningToServiceBus = false;
            if (_httpSb != null)
            {
                _httpSb.DeleteResource(_subscriptionResource);
            }
        }


        private void ListenToServiceBus(object state) {
            _httpSb = new HttpServiceBus(serviceNamespace: "harding",
                                            issuerName: "owner",
                                            issuerSecret: "Q8Kf3NiSIzVxSAjzBpLZhYkRusN19lgbxToEi1B5toI=");

            string subscriptionName = "VMWare" + Guid.NewGuid().ToString();
            var topicName = "color";
            _httpSb.CreateSubscription(topicName, subscriptionName);
            _httpSb.CreateSubscriptionRule(topicName, subscriptionName, "VMWareOrWindows", "who='All' OR who='VMWare' OR who='Windows'");
            _httpSb.DeleteSubscriptionRule(topicName, subscriptionName, "$Default");

            _listeningToServiceBus = true;
            _subscriptionResource = topicName + "/Subscriptions/" + subscriptionName;

            do
            {
                try
                {
                    var receivedColor = _httpSb.ReceiveAndDeleteMessage(_subscriptionResource);
                    if (receivedColor != string.Empty)
                    {
                        var screenColor = ConvertToUIColor(receivedColor);
                        this.UIThread(() => this.BackColor = screenColor);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            } while (_listeningToServiceBus);
        }

        private System.Drawing.Color ConvertToUIColor(string colorText)
        {
            if (colorText == "Red")
            {
                return System.Drawing.Color.Red;
            }

            if (colorText == "Blue")
            {
                return System.Drawing.Color.Blue;
            }

            if (colorText == "Green")
            {
                return System.Drawing.Color.Green;
            }

            if (colorText == "Orange")
            {
                return System.Drawing.Color.Orange;
            }
            return System.Drawing.Color.White;
        }
    }

    static class ControlExtensions
    {
        static public void UIThread(this Control control, Action code)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(code);
                return;
            }
            code.Invoke();
        }
    }
}
