using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Avalonix.Program;

namespace Avalonix;

public abstract class UpdateVersion
{
   public static string LocalVersion { get; }
   public static string OnlineVersion { get; set; } = null!;
   private const string ApiUrl = "https://mncrzz.github.io/api/version.txt";

   private static string GetLocalVersion()
   {
      return "0.1.0";
   }

   private static async Task<string> GetOnlineVersion()
   {
       using var httpClient = new HttpClient();
       return await httpClient.GetStringAsync(ApiUrl);
   }
   
   public static async void CheckUpdates()
   {
       try
       {
           OnlineVersion = await GetOnlineVersion();
           Logger.Info($"Version {OnlineVersion}");
       }
       catch (Exception ex)
       {
           Logger.Error(ex.ToString());
       }
   }

   private static bool IsUpdateAvailable() => OnlineVersion == LocalVersion;

}