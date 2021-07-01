using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace YooKassa4WinForms
{
    public static class YooKassa
    {
        static YooKassa()
        {
            if (!File.Exists("keys.json"))
            {
                throw new Exception("Добавьте keys.json в выходной каталог программы");
            }
            Credentials = JsonConverter.DeserializeJson<Dictionary<string, string>>("keys.json");
            Id = Credentials["store_id"];
            Key = Credentials["secret_key"];
            IdempotenceKey = GetKey(15);
        }

        /// <summary>
        /// Данные магазина, необходимые для доступа к API. Предлагается добавлять в выходную папку файл keys.json вида:<br/>
        /// <code>{ "store_id": "идентификатор", "secret_key": "ключ" }</code>
        /// </summary>
        /// <remarks>
        /// Для разработчиков:
        /// cоздайте в корне проекта такой файл keys.json со свойствами:
        /// <list type="bullet">
        /// <item>Действия при сборке - Внедренный ресурс</item>
        /// <item>Копировать в выходной каталог - Всегда копировать</item>
        /// </list>
        /// Путь к файлу "/YooKassa4WinForms/keys.json" добавлен в .gitignore
        /// </remarks>
        private static readonly Dictionary<string, string> Credentials;
        private static readonly string Id;
        private static readonly string Key;
        private static readonly string IdempotenceKey;
        internal static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        private static string GetKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (var gen = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                gen.GetBytes(data);
            }
            StringBuilder key = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                uint rnd = BitConverter.ToUInt32(data, i * 4);
                long idx = rnd % chars.Length;

                key.Append(chars[idx]);
            }
            return key.ToString();
        }

        /// <summary>
        /// Запрос позволяет передать информацию для создания объекта платежа
        /// </summary>
        /// <typeparam name="T">В качестве типа получаемого объекта рекомендуется выбрать Payment или Dictionary&lt;string, object&gt;</typeparam>
        /// <returns>Объект платежа в актуальном статусе.</returns>
        public static T CreatePaymentObject<T>() where T : class
        {
            string requestUri = $"https://api.yookassa.ru/v3/payments";
            string content = File.ReadAllText("requestPayment.json", Encoding.ASCII);
            string IdKeyPair = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{Id}:{Key}"));

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", IdKeyPair);
            requestMessage.Headers.Add("Idempotence-Key", IdempotenceKey);
            requestMessage.Content = new StringContent(content);
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            #region WebClient
            // Если бы HttpClient не поддерживался (например, при использовании .NET Framework 4),
            // вместо него можно было бы использовать WebClient:
            //HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            //request.Method = "POST";
            //request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + IdKeyPair);
            //request.ContentType = "application/json";
            //request.Headers.Add("Idempotence-Key", IdempotenceKey);
            //using (Stream stream = request.GetRequestStream())
            //{
            //    stream.SerializeJson(content);
            //}
            //try
            //{
            //    return request.GetResponse().GetResponseStream().DeserializeJson<T>();
            //}
            //catch (WebException e)
            //{
            //    throw e;
            //}
            #endregion

            var response = httpClient.SendAsync(requestMessage).Result;
            if (!response.IsSuccessStatusCode)
            {
                string xDesc = response.Content.ReadAsStreamAsync().Result.DeserializeJson<Dictionary<string, string>>()["description"];
                throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode}: {xDesc}\n{response.Content.ReadAsStringAsync().Result}");
            }
            return response.Content.ReadAsStreamAsync().Result.DeserializeJson<T>();
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
