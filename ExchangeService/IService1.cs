using System.ServiceModel;

namespace ExchangeService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        double GetExchangeRate(string currencyCode);

        [OperationContract]
        double Convert(string fromCurrency, string toCurrency, double amount);

        [OperationContract]
        double BuyCurrency(string currencyCode, double amount);

        [OperationContract]
        double SellCurrency(string currencyCode, double amount);

        [OperationContract]
        string GetHistoricalRates(string currencyCode, int days);

        [OperationContract]
        int RegisterUser(string username);

        [OperationContract]
        int LoginUser(string username);

        [OperationContract]
        double TopUp(int userId, double amountPLN);

        [OperationContract]
        double GetBalance(int userId);

        [OperationContract]
        string BuyForUser(int userId, string currencyCode, double amount);

        [OperationContract]
        string SellForUser(int userId, string currencyCode, double amount);

        [OperationContract]
        string GetUserTransactions(int userId);
    }
}