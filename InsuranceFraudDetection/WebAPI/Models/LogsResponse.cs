namespace InsuranceFraudDetection.WebAPI.Models
{
    public class LogsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Logs { get; set; } = new List<string>();
        public int TotalLines { get; set; }
        public DateTime? RetrievedAt { get; set; }
    }
}
