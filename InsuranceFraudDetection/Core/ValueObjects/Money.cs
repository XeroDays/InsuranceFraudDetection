namespace InsuranceFraudDetection.Core.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal amount, string currency = "USD")
        {
            if (amount < 0)  throw new ArgumentException("Amount cannot be negative", nameof(amount));
            
            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string currency = "USD")
        {
            return new Money(amount, currency);
        }

        public override string ToString()
        {
            return $"{Amount:C}";
        }

        public static implicit operator decimal(Money money) => money.Amount;
    }
}
