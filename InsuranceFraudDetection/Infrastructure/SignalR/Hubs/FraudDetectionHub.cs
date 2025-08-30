using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace InsuranceFraudDetection.Infrastructure.SignalR.Hubs
{
    public class FraudDetectionHub : Hub
    {
        private static readonly Random _random = new Random();
 

        public async Task AnalyzeMultipleClaims(int[] claimIds)
        {
            var results = new List<object>();

            foreach (var claimId in claimIds)
            { 
                await Task.Delay(500 + _random.Next(1000));

                bool isFraudulent = _random.Next(2) == 1;
                
                var result = new
                {
                    ClaimId = claimId,
                    IsFraudulent = isFraudulent,
                    Confidence = _random.Next(70, 99),
                    Timestamp = DateTime.UtcNow,
                    Message = isFraudulent ? "Potential fraud detected!" : "Claim appears legitimate."
                };

                results.Add(result); 
                await Clients.Caller.SendAsync("FraudAnalysisResult", result);
            }
             
            var summary = new
            {
                TotalClaims = claimIds.Length,
                FraudulentClaims = results.Count(r => (bool)r.GetType().GetProperty("IsFraudulent").GetValue(r)),
                LegitimateClaims = results.Count(r => !(bool)r.GetType().GetProperty("IsFraudulent").GetValue(r)),
                Timestamp = DateTime.UtcNow,
                Message = "Batch analysis completed."
            };

            await Clients.Caller.SendAsync("BatchAnalysisComplete", summary);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", "Connected to Fraud Detection Hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.Caller.SendAsync("Disconnected", "Disconnected from Fraud Detection Hub");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
