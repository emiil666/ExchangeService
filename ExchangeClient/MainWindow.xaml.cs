using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ExchangeClient
{
    public partial class MainWindow : Window
    {
        private int currentUserId = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetText(ComboBox box)
        {
            return ((ComboBoxItem)box.SelectedItem).Content.ToString();
        }

        private double ParseNum(string text)
        {
            return double.Parse(text.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        private void ConvertBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var c = new ServiceRef.Service1Client();
                string from = GetText(FromBox);
                string to = GetText(ToBox);
                double amt = ParseNum(AmountBox.Text);
                double result = c.Convert(from, to, amt);
                c.Close();
                ResultText.Text = amt + " " + from + "  =  " + result + " " + to;
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cur = GetText(FromBox);
                if (cur == "PLN") { ResultText.Text = "Pick a foreign currency in 'From' for history."; return; }
                var c = new ServiceRef.Service1Client();
                int days = int.Parse(DaysBox.Text);
                string hist = c.GetHistoricalRates(cur, days);
                c.Close();
                ResultText.Text = "Last " + days + " rates for " + cur + ":\n\n" + hist;
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var c = new ServiceRef.Service1Client();
                currentUserId = c.RegisterUser(UsernameBox.Text);
                c.Close();
                UserLabel.Text = "User: " + UsernameBox.Text + "  (ID " + currentUserId + ")";
                ResultText.Text = "Registered '" + UsernameBox.Text + "' with ID " + currentUserId;
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var c = new ServiceRef.Service1Client();
                int id = c.LoginUser(UsernameBox.Text);
                if (id == -1)
                {
                    c.Close();
                    ResultText.Text = "No user named '" + UsernameBox.Text + "' found.";
                    return;
                }
                double bal = c.GetBalance(id);
                c.Close();
                currentUserId = id;
                UserLabel.Text = "User: " + UsernameBox.Text + "  (ID " + id + ")";
                ResultText.Text = "Logged in as '" + UsernameBox.Text + "'\nBalance: " + bal + " PLN";
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void TopUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId == 0) { ResultText.Text = "Register or login first."; return; }
            try
            {
                var c = new ServiceRef.Service1Client();
                double bal = c.TopUp(currentUserId, ParseNum(TopUpBox.Text));
                c.Close();
                ResultText.Text = "Topped up.\nNew balance: " + bal + " PLN";
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void BalanceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId == 0) { ResultText.Text = "Register or login first."; return; }
            try
            {
                var c = new ServiceRef.Service1Client();
                double bal = c.GetBalance(currentUserId);
                c.Close();
                ResultText.Text = "Balance: " + bal + " PLN";
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void BuyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId == 0) { ResultText.Text = "Register or login first."; return; }
            try
            {
                var c = new ServiceRef.Service1Client();
                string cur = GetText(AccCurrencyBox);
                double amt = ParseNum(AmountBox.Text);
                string msg = c.BuyForUser(currentUserId, cur, amt);
                c.Close();
                ResultText.Text = msg;
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void SellBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId == 0) { ResultText.Text = "Register or login first."; return; }
            try
            {
                var c = new ServiceRef.Service1Client();
                string cur = GetText(AccCurrencyBox);
                double amt = ParseNum(AmountBox.Text);
                string msg = c.SellForUser(currentUserId, cur, amt);
                c.Close();
                ResultText.Text = msg;
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }

        private void TransactionsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserId == 0) { ResultText.Text = "Register or login first."; return; }
            try
            {
                var c = new ServiceRef.Service1Client();
                string tx = c.GetUserTransactions(currentUserId);
                c.Close();
                ResultText.Text = "Transactions:\n\n" + tx;
            }
            catch (Exception ex) { ResultText.Text = "Error: " + ex.Message; }
        }
    }
}