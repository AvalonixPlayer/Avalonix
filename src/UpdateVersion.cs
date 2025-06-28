using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Avalonix.Program;

namespace Avalonix;

public abstract class UpdateVersion
{
   public static string LocalVersion = "0.1.0"; 

   public static string OnlineVersion { get; set; } = null!;
   private const string ApiUrl = "https://mncrzz.github.io/api/version.txt";

   private static async Task<string> GetOnlineVersion() => await new HttpClient().GetStringAsync(ApiUrl);
   
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

   public static bool IsUpdateAvailable() => OnlineVersion != LocalVersion;

}