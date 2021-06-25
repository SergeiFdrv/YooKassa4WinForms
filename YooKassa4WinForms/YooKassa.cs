using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class YooKassa
    {
        public static T CreatePaymentObject<T>() where T : class
        {
            string requestUri = $"https://api.yookassa.ru/v3/payments";
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{ourId}:{secretKey}")));
            request.ContentType = "application/json";
            request.Headers.Add("Idempotence-Key", "1234567890");
            string content = "{\"amount\": {\"value\": \"100.00\", \"currency\": \"RUB\"}," +
                "\"capture\": true, " +
                "\"confirmation\": {\"type\": \"redirect\", '\"return_url\": \"https://www.merchant-website.com/return_url\"}, " +
                "\"description\": \"Заказ №1\"}";
            using (Stream stream = request.GetRequestStream())
            {
                stream.SerializeJson(content);
            }
            return request.GetResponse().GetResponseStream().DeserializeJson<T>();
        }

        private const string ourId = "";
        private const string secretKey = "";

        //https://yookassa.ru/developers/api?lang=bash#get_payment
        public static T GetPaymentObject<T>(string paymentId) where T : class
        {
            string requestUri;
            if (paymentId.StartsWith("http"))
                requestUri = paymentId;
            else
                requestUri = $"https://api.yookassa.ru/v3/payments/{paymentId}";
            WebRequest request = WebRequest.Create(requestUri);
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{ourId}:{secretKey}")));
            request.ContentType = "application/json";
            return request.GetResponse().GetResponseStream().DeserializeJson<T>();
        }
    }
}
