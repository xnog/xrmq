namespace X;

public class XrmqProperties
{
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VHost { get; set; } = "/";
    public int MaxPoolSize { get; set; } = 100;
    public ushort PrefetchCount { get; set; } = 20;
    public int NumberOfRetries { get; set; } = 3;
    public TimeSpan RetryDelayMs { get; set; } = TimeSpan.FromSeconds(10);
    public bool WaitForConfirm { get; set; } = true;
    public TimeSpan WaitForConfirmTimeout { get; set; } = TimeSpan.FromSeconds(1);
}
