static partial class LogMessage
{

  [LoggerMessage(Level = LogLevel.Information, Message = "Access received for user to {dataType} data")]
  public static partial void LogAccessMessage(
      ILogger logger,
      string dataType,
      [LogProperties] AccessLogModel accessLog);

}
