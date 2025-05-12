//using System.Collections.Generic;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;

//namespace Metapsi;

//public static partial class ServiceDoc
//{
//    public class Client<T>
//    {
//        public Client() { }
//        public Client(HttpClient httpClient, string apiUrl)
//        {
//            this.HttpClient = httpClient;
//            this.ApiUrl = apiUrl;
//        }

//        public HttpClient HttpClient { get; set; }
//        public string ApiUrl { get; set; }
//    }

//    public static async Task<List<T>> List<T>(this ServiceDoc.Client<T> client)
//    {
//        return await client.HttpClient.GetFromJsonAsync<List<T>>(client.ApiUrl);
//    }

//    public static async Task Save<T>(this ServiceDoc.Client<T> client, T entity)
//    {
//        var response = await client.HttpClient.PostAsJsonAsync<T>(client.ApiUrl, entity);
//        response.EnsureSuccessStatusCode();
//    }

//    public static async Task<T> Get<T>(this ServiceDoc.Client<T> client, string key)
//    {
//        return await client.HttpClient.GetFromJsonAsync<T>(client.ApiUrl + "/" + key);
//    }

//    public static async Task Delete<T>(this ServiceDoc.Client<T> client, string key)
//    {
//        await client.HttpClient.DeleteAsync(client.ApiUrl + "/" + key);
//    }
//}