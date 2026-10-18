using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AwasBeyPCHost
{
    class Program
    {
        private const string DEVELOPER_SIGNATURE = "By: AwasBey";
        private static HttpListener listener;
        private static string url = "http://localhost:8080/";

        static async Task Main(string[] args)
        {
            Console.Title = $"AwasBey PC Bridge Host - [{DEVELOPER_SIGNATURE}]";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Clear();

            Console.WriteLine("==================================================");
            Console.WriteLine($"   AWASBEY PC REMOTE HOST SERVICE");
            Console.WriteLine($"   Geliştirici / Owner: {DEVELOPER_SIGNATURE}");
            Console.WriteLine("==================================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[Servis]: Sunucu dinleniyor: {url}");
            Console.WriteLine($"[İmza]: Bütün işlemler {DEVELOPER_SIGNATURE} altyapısıyla çalışmaktadır.\n");
            Console.ResetColor();

            listener = new HttpListener();
            listener.Prefixes.Add(url);
            
            try
            {
                listener.Start();
                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    ProcessRequest(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hata]: {ex.Message}");
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // CORS İzinleri (Vercel Web Sitesinden Erişilebilmesi İçin)
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            string responseString = "{\"status\":\"ok\", \"signature\":\"By: AwasBey\"}";

            try
            {
                string path = request.Url.AbsolutePath;

                if (path == "/ping")
                {
                    responseString = "{\"online\": true, \"message\": \"PC Aktif\", \"by\": \"AwasBey\"}";
                }
                else if (path == "/get-drives")
                {
                    // Bilgisayardaki diskleri listele
                    var drives = DriveInfo.GetDrives();
                    StringBuilder sb = new StringBuilder("[");
                    foreach (var d in drives)
                    {
                        if (d.IsReady)
                        {
                            sb.Append($"{{\"name\":\"{d.Name.Replace("\\", "\\\\")}\",\"totalGB\":{d.TotalSize / (1024*1024*1024)},\"freeGB\":{d.AvailableFreeSpace / (1024*1024*1024)}}},");
                        }
                    }
                    if (sb.Length > 1) sb.Remove(sb.Length - 1, 1);
                    sb.Append("]");
                    responseString = sb.ToString();
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                byte[] errorBuffer = Encoding.UTF8.GetBytes($"{{\"error\":\"{ex.Message}\"}}");
                response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
            }
            finally
            {
                response.OutputStream.Close();
            }
        }
    }
}
