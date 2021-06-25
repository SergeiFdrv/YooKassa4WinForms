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

        private void TestWithJsonPlaceholder()
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
#if DEBUG
            TestWithJsonPlaceholder();
#else
            Payment payment = YooKassa.CreatePaymentObject<Payment>();
            OpenLinkInBrowser(payment.Confirmation.ConfirmationUrl);
            StatusLabel.Text = "Ожидание оплаты...";
            do
            {
                Thread.Sleep(10000);
                payment = YooKassa.GetPaymentObject<Payment>(payment.PaymentId);
            } while (payment.Status == "pending");
            switch (payment.Status)
            {
                case "canceled":
                    StatusLabel.Text = "Платеж отменен"; break;
                default:
                    StatusLabel.Text = "Платеж завершен. Спасибо"; break;
            }
#endif
        }
    }
}
