using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

public static class GlobalConfiguration
{
   public static OpenApiDocument? ApiDocument { get; set; }

   public static IOpenApiAny? ApiExamples { get; set; }

   public static SettingsModel? ApiSettings { get; set; }

   public class SettingsModel
   {
      public required EntraIdOptions EntraId { get; set; }
      public required TokenValidationParameters TokenValidation { get; set; }
      public required PageSizeModel PageSize { get; set; }
      public required GenericBoundariesModel GenericBoundaries { get; set; }
      public required FixedWindowRateLimiterOptions FixedWindowRateLimit { get; set; }
      public required HybridCacheEntryOptions Cache { get; set; }
      public required string DatabaseName { get; set; }
   }

   public class EntraIdOptions
   {
      public string TenantId { get; set; } = "";
      public string ClientId { get; set; } = "";
   }

   public class PageSizeModel
   {
      public int Default { get; set; }
      public int Minimum { get; set; }
      public int Maximum { get; set; }
   }

   public class GenericBoundariesModel
   {
      public int Minimum { get; set; }
      public int Maximum { get; set; }
      public string Regex { get; set; } = "";
   }
}