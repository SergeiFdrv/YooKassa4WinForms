using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace YooKassa4WinForms
{
    public partial class ClientForm : Form
    {
        public ClientForm()
        {
            InitializeComponent();
        }

        private void OpenLinkInBrowser(string url) => System.Diagnostics.Process.Start(url);

        private void Test()
        {
            string jsonPlaceholder = "https://jsonplaceholder.typicode.com/todos/1";

            /// словарь
            Dictionary<string, object> results = YooKassa.GetPaymentObject<Dictionary<string, object>>(jsonPlaceholder);
            string msg = string.Empty;
            foreach (var s in results)
            {
                msg += s.Key + " = " + s.Value + '\n';
            }
            StatusLabel.Text = msg;

            /// сериализуемый объект
            TestJsonObject jsonObject = YooKassa.GetPaymentObject<TestJsonObject>(jsonPlaceholder);
            OpenLinkInBrowser(jsonPlaceholder);
            MessageBox.Show($"id: {jsonObject.Id}\nuserid: {jsonObject.UserId}\ntitle: {jsonObject.Title}\ncompleted: {jsonObject.Completed}");

            /// тестовый json
            var keys = JsonConverter.DeserializeJson<Dictionary<string, string>>("keys.json");
            StatusLabel.Text = keys["store_id"] + '\n' + keys["secret_key"];

            /// десериализация в строку (TODO: не работает, починить)
            StatusLabel.Text = JsonConverter.DeserializeJson<string>("keys.json");
        }

        private Payment Payment;

        private void button1_Click(object sender, EventArgs e)
        {
            bool _test = false;
            if (_test)
            {
                Test();
                return;
            }
            Payment = YooKassa.CreatePaymentObject<Payment>();
            OpenLinkInBrowser(Payment.Confirmation.ConfirmationUrl);
            StatusLabel.Text = "Ожидание оплаты...";
        }

        private void ClientForm_Activated(object sender, EventArgs e)
        {
            if (!(Payment == null && Payment.IsOver))
            {
                Payment = YooKassa.GetPaymentObject<Payment>(Payment.PaymentId);
                switch (Payment.Status)
                {
                    case "canceled":
                        StatusLabel.Text = "Платеж отменен"; break;
                    case "succeeded":
                        StatusLabel.Text = "Платеж завершен. Спасибо"; break;
                    default:
                        StatusLabel.Text = "Платеж не был завершен"; return;
                }
                Payment.IsOver = true;
            }
        }
    }
}
