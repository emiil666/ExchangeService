using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;

namespace ExchangeService
{
    public class Service1 : IService1
    {
        private const string ConnString =
            @"Data Source=(localdb)\ProjectModels;Initial Catalog=ExchangeDB;" +
            "Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";

        // ---------- NBP rate methods ----------
        public double GetExchangeRate(string currencyCode)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string url = "https://api.nbp.pl/api/exchangerates/rates/a/"
                         + currencyCode + "/?format=json";
            string json;
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                json = client.DownloadString(url);
            }
            int midIndex = json.IndexOf("\"mid\":");
            if (midIndex == -1)
                throw new Exception("Could not find rate in NBP response: " + json);
            int start = midIndex + 6;
            int end = json.IndexOf("}", start);
            string midText = json.Substring(start, end - start).Trim();
            return double.Parse(midText, CultureInfo.InvariantCulture);
        }

        // Converts any currency to any other, with PLN allowed on either side (official rate, no margin)
        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            if (fromCurrency == toCurrency) return Math.Round(amount, 2);
            double fromRate = fromCurrency == "PLN" ? 1.0 : GetExchangeRate(fromCurrency);
            double toRate = toCurrency == "PLN" ? 1.0 : GetExchangeRate(toCurrency);
            return Math.Round(amount * fromRate / toRate, 2);
        }

        public double BuyCurrency(string currencyCode, double amount)
        {
            double rate = GetExchangeRate(currencyCode);
            return Math.Round(amount * rate * 1.02, 2);
        }

        public double SellCurrency(string currencyCode, double amount)
        {
            double rate = GetExchangeRate(currencyCode);
            return Math.Round(amount * rate * 0.98, 2);
        }

        public string GetHistoricalRates(string currencyCode, int days)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string url = "https://api.nbp.pl/api/exchangerates/rates/a/"
                         + currencyCode + "/last/" + days + "/?format=json";
            string json;
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                json = client.DownloadString(url);
            }
            string result = "";
            int pos = 0;
            while (true)
            {
                int dateIdx = json.IndexOf("\"effectiveDate\":\"", pos);
                if (dateIdx == -1) break;
                int dateStart = dateIdx + 17;
                int dateEnd = json.IndexOf("\"", dateStart);
                string date = json.Substring(dateStart, dateEnd - dateStart);
                int midIdx = json.IndexOf("\"mid\":", dateEnd);
                int midStart = midIdx + 6;
                int midEnd = json.IndexOf("}", midStart);
                string mid = json.Substring(midStart, midEnd - midStart).Trim();
                result += date + "   ->   " + mid + " PLN\n";
                pos = midEnd;
            }
            return result;
        }

        // ---------- Database methods ----------
        public int RegisterUser(string username)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                string sql = "INSERT INTO Users (Username, BalancePLN) VALUES (@u, 0); " +
                             "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public int LoginUser(string username)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 Id FROM Users WHERE Username = @u ORDER BY Id DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    object r = cmd.ExecuteScalar();
                    return r == null ? -1 : (int)r;
                }
            }
        }

        public double TopUp(int userId, double amountPLN)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                string sql = "UPDATE Users SET BalancePLN = BalancePLN + @amt WHERE Id = @id;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@amt", amountPLN);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            return GetBalance(userId);
        }

        public double GetBalance(int userId)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT BalancePLN FROM Users WHERE Id = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    object r = cmd.ExecuteScalar();
                    if (r == null) throw new Exception("User not found.");
                    return System.Convert.ToDouble(r);
                }
            }
        }

        // Buy foreign currency using PLN balance (office margin applies)
        public string BuyForUser(int userId, string currencyCode, double amount)
        {
            double cost = BuyCurrency(currencyCode, amount);
            double balance = GetBalance(userId);
            if (balance < cost)
                return "Not enough PLN. Need " + cost + ", you have " + balance + ".";

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE Users SET BalancePLN = BalancePLN - @cost WHERE Id = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("@cost", cost);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(
                    "INSERT INTO Transactions (UserId, Operation, CurrencyCode, Amount, ValuePLN) " +
                    "VALUES (@id, 'BUY', @cur, @amt, @val);", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@cur", currencyCode);
                    cmd.Parameters.AddWithValue("@amt", amount);
                    cmd.Parameters.AddWithValue("@val", cost);
                    cmd.ExecuteNonQuery();
                }
            }
            return "Bought " + amount + " " + currencyCode + " for " + cost +
                   " PLN. New balance: " + GetBalance(userId) + " PLN.";
        }

        // Sell foreign currency, adding PLN to balance (office margin applies)
        public string SellForUser(int userId, string currencyCode, double amount)
        {
            double gain = SellCurrency(currencyCode, amount);

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE Users SET BalancePLN = BalancePLN + @gain WHERE Id = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("@gain", gain);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(
                    "INSERT INTO Transactions (UserId, Operation, CurrencyCode, Amount, ValuePLN) " +
                    "VALUES (@id, 'SELL', @cur, @amt, @val);", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@cur", currencyCode);
                    cmd.Parameters.AddWithValue("@amt", amount);
                    cmd.Parameters.AddWithValue("@val", gain);
                    cmd.ExecuteNonQuery();
                }
            }
            return "Sold " + amount + " " + currencyCode + " for " + gain +
                   " PLN. New balance: " + GetBalance(userId) + " PLN.";
        }

        public string GetUserTransactions(int userId)
        {
            string result = "";
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT Operation, CurrencyCode, Amount, ValuePLN, Date " +
                    "FROM Transactions WHERE UserId = @id ORDER BY Date DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result += reader["Date"] + "  " + reader["Operation"] + "  " +
                                      reader["Amount"] + " " + reader["CurrencyCode"] +
                                      "  (" + reader["ValuePLN"] + " PLN)\n";
                        }
                    }
                }
            }
            return result == "" ? "No transactions yet." : result;
        }
    }
}