using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Avalonix.Program;

namespace Avalonix;

public abstract class UpdateVersion
{
   public const string LocalVersion = "0.1.0";
   public static string OnlineVersion { get; private set; } = "0.1.0" ;
   private const string ApiUrl = "https://raw.githubusercontent.com/Nokskiy/Avalonix/refs/heads/main/src/version";
   private static readonly HttpClient HttpClient = new HttpClient();

   private static async Task<string> MakeHttpRequest(string url) => await HttpClient.GetStringAsync(url);   
   
   public static async Task CheckUpdates()
   {
       try
       {
           OnlineVersion = await MakeHttpRequest(ApiUrl);
       }
       catch (Exception ex)
       {
           Logger.Error(ex.ToString());
       }
   }

   public static bool IsUpdateAvailable() => OnlineVersion != LocalVersion;

}