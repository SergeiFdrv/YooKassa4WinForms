using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YooKassa4WinForms
{
    public static class YooKassa
    {
        static YooKassa()
        {
            Credentials = JsonConverter.DeserializeJson<Dictionary<string, string>>("keys.json");
            Id = Credentials["store_id"];
            Key = Credentials["secret_key"];
        }

        private static readonly Dictionary<string, string> Credentials;
        private static readonly string Id;
        private static readonly string Key;

        /// <summary>
        /// Запрос позволяет передать информацию дял создания объекта платежа
        /// </summary>
        /// <typeparam name="T">В качестве типа получаемого объекта рекомендуется выбрать Payment или Dictionary&lt;string, object&gt;</typeparam>
        /// <returns>Объект платежа в актуальном статусе.</returns>
        public static T CreatePaymentObject<T>() where T : class
        {
            string requestUri = $"https://api.yookassa.ru/v3/payments";
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.Authorization,
                "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{Id}:{Key}")));
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

        //https://yookassa.ru/developers/api?lang=bash#get_payment
        /// <summary>
        /// Запрос позволяет получить информацию о текущем состоянии платежа по его уникальному идентификатору.
        /// </summary>
        /// <typeparam name="T">В качестве типа получаемого объекта рекомендуется выбрать Payment или Dictionary&lt;string, object&gt;</typeparam>
        /// <param name="paymentId"></param>
        /// <returns>Объект платежа в актуальном статусе.</returns>
        public static T GetPaymentObject<T>(string paymentId) where T : class
        {
            string requestUri;
            if (paymentId.StartsWith("http"))
                requestUri = paymentId;
            else
                requestUri = $"https://api.yookassa.ru/v3/payments/{paymentId}";
            WebRequest request = WebRequest.Create(requestUri);
            request.Headers.Add(HttpRequestHeader.Authorization,
                "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{Id}:{Key}")));
            request.ContentType = "application/json";
            return request.GetResponse().GetResponseStream().DeserializeJson<T>();
        }
    }
}
