using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MonMan
{
    public class PieChart
    {
        public string Category { get; set; }

        public float Number { get; set; }

    }

    public class ColumnChart
    {
        public string Month { get; set; }

        public float Number { get; set; }

    }


    public class categoryItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]

        public string Type { get; set; }

        [JsonProperty(PropertyName = "category")]

        public string Category { get; set; }

    }

    public class transactionItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string transactionUsername { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime transactionDate { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string transactionType { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string transactionCategory { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public float transactionAmount { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string transactionDescription { get; set; }


    }

    public sealed partial class MainPage : Page
    {
        private MobileServiceCollection<categoryItem, categoryItem> categoryItems;
        private IMobileServiceTable<categoryItem> categoriesTable = App.MobileService.GetTable<categoryItem>();

        private MobileServiceCollection<transactionItem, transactionItem> transactionItems;
        private IMobileServiceTable<transactionItem> transactionsTable = App.MobileService.GetTable<transactionItem>();

        public MainPage()
        {
            this.InitializeComponent();
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //GetUsername();
            LoadTypeItems();

            this.tbUsername.Text = "Gün Karagöz";
            RefreshTransactions();
        }

        public class Graph
        {
            public ObservableCollection<PieChart> MonthlyData { get; private set; }

            public ObservableCollection<ColumnChart> YearlyIncome { get; private set; }

            public ObservableCollection<ColumnChart> YearlyExpense { get; private set; }

            public Graph()
            {
                MonthlyData = new ObservableCollection<PieChart>();

                // TODO : Do it dynamic remove hardcoded
                MonthlyData.Add(new PieChart() { Category = "Income", Number = monthlyIncome });
                MonthlyData.Add(new PieChart() { Category = "Expense", Number = monthlyExpense });

                YearlyExpense = new ObservableCollection<ColumnChart>();

                foreach (var item in yearlyTotals)
                {
                    YearlyExpense.Add(new ColumnChart() { Month = item.Item1, Number = item.Item2 });
                }

            }

            private object selectedItem = null;
            public object SelectedItem
            {
                get
                {
                    return selectedItem;
                }
                set
                {
                    // selected item has changed
                    selectedItem = value;
                }
            }
        }

        private static float monthlyIncome = 0;
        private static float monthlyExpense = 0;


        private static List<Tuple<string, float>> yearlyTotals;


        private async Task<List<Tuple<string, float>>> GetYearlyData(string type)
        {
            var yearlyAmount = new List<Tuple<string, float>>();

            var results = await transactionsTable
                          .Where(transactionItem => transactionItem.transactionUsername == tbUsername.Text
                              && transactionItem.transactionType == type)
                          .OrderBy(transactionItem => transactionItem.transactionDate)
                          .ToListAsync();

            string monthName = "";
            string yearName = "";
            float yearMonthTotal = 0;

            if (results.Count != 0)
            {
                for (int i = 0; i < results.Count; i++)
                {

                    monthName = results[i].transactionDate.ToString("MMM");
                    yearName = results[i].transactionDate.ToString("yy");
                    yearMonthTotal = await GetMonthlyData(type, results[i].transactionDate.Year, results[i].transactionDate.Month);

                    yearlyAmount.Add(new Tuple<string, float>(monthName + "-" + yearName, yearMonthTotal));
                }
            }


            return yearlyAmount;
        }

        private async Task<float> GetMonthlyData(string type, int year, int month)
        {

            float totalAmount = 0;
            var results = await transactionsTable
                          .Where(transactionItem => transactionItem.transactionUsername == tbUsername.Text
                              && transactionItem.transactionType == type
                              && transactionItem.transactionDate.Year == year
                              && transactionItem.transactionDate.Month == month)

                          .ToListAsync();

            if (results.Count != 0)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    totalAmount += results[i].transactionAmount;
                }
            }

            if (type == "Income")
            {
                monthlyIncome = totalAmount;
            }
            else
            {
                monthlyExpense = totalAmount;

            }

            return totalAmount;

        }


        private async void InsertTransaction(transactionItem transaction)
        {
            await transactionsTable.InsertAsync(transaction);
            RefreshTransactions();
        }

        private async void CreateNewTransactionItem()
        {
            transactionItems = await transactionsTable.ToCollectionAsync();

            if (isFormFilled())
            {
                var transactionItem = new transactionItem
                {

                    transactionUsername = tbUsername.Text,
                    transactionDate = dateSelection.Date.DateTime,
                    transactionType = (string)comboType.SelectedItem,
                    transactionCategory = (string)comboCategory.SelectedItem,
                    transactionAmount = float.Parse(textAmount.Text),
                    transactionDescription = textDescription.Text
                };

                InsertTransaction(transactionItem);
                clearFormElements();
            }

            else
            {
                var dialog = new MessageDialog("Please fill required fields in form!");
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
            }
        }

        private bool isFormFilled()
        {
            if (dateSelection.Date.DateTime == null
                    || (string)comboType.SelectedItem == null
                    || (string)comboCategory.SelectedItem == null
                    || textAmount.Text == null
                    || (string)comboType.SelectedItem == ""
                    || (string)comboCategory.SelectedItem == ""
                    || textAmount.Text == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void clearFormElements()
        {
            comboType.SelectedItem = "";
            comboCategory.SelectedItem = "";
            textAmount.Text = "";
            textDescription.Text = "";
        }


        private void LoadTypeItems()
        {
            // TODO : Get Type Elements
            comboType.Items.Add("Income");
            comboType.Items.Add("Expense");

        }

        private async void LoadCategoryItems()
        {

            var results = await categoriesTable
                                    .Where(categoryItem => categoryItem.Type == (string)comboType.SelectedItem)
                                    .ToListAsync();

            var dialog = new MessageDialog("");
            if (results.Count != 0)
            {
                comboCategory.Items.Clear();
                foreach (var item in results)
                {
                    comboCategory.Items.Add(item.Category);
                }
            }
        }

        private void comboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCategoryItems();
        }

        private void ButtonSave_Tapped(object sender, TappedRoutedEventArgs e)
        {

            CreateNewTransactionItem();
        }

        private async void GetUsername()
        {
            try
            {

                await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                var userId = App.MobileService.CurrentUser.UserId;
                var facebookId = userId.Substring(userId.IndexOf(':') + 1);
                var client = new HttpClient();
                var fbUser = await client.GetAsync("https://graph.facebook.com/" + facebookId);
                var response = await fbUser.Content.ReadAsStringAsync();
                var jo = JsonObject.Parse(response);
                var userName = jo["name"].GetString();
                tbUsername.Text = userName;

                //InsertUserItem(appUser);
            }
            catch (InvalidOperationException)
            {


            }
        }
        private async void RefreshTransactions()
        {

            gridTransactions.Children.Clear();

            var results = await transactionsTable
                          .Where(transactionItem => transactionItem.transactionUsername == tbUsername.Text)
                          .OrderByDescending(transactionItem => transactionItem.transactionDate)
                          .ToListAsync();

            if (results.Count != 0)
            {


                int colNum = 0;
                // TODO Do it dynamically! Change columns dynamically.
                for (int i = 0; i < results.Count; i++)
                {

                    colNum = 0;
                    TextBlock cellRowNum = new TextBlock();
                    cellRowNum.Text = (i + 1).ToString();
                    cellRowNum.SetValue(Grid.RowProperty, i);
                    cellRowNum.SetValue(Grid.ColumnProperty, colNum);
                    cellRowNum.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellRowNum.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellRowNum);

                    colNum++;
                    TextBlock cellDate = new TextBlock();
                    cellDate.Text = String.Format("{0:dd/MM/yy}", results[i].transactionDate);
                    cellDate.SetValue(Grid.RowProperty, i);
                    cellDate.SetValue(Grid.ColumnProperty, colNum);
                    cellDate.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellDate.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellDate);

                    colNum++;
                    TextBlock cellType = new TextBlock();
                    cellType.Text = results[i].transactionType.ToString();
                    cellType.SetValue(Grid.RowProperty, i);
                    cellType.SetValue(Grid.ColumnProperty, colNum);
                    cellType.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellType.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellType);

                    colNum++;
                    TextBlock cellCategory = new TextBlock();
                    cellCategory.Text = results[i].transactionCategory.ToString();
                    cellCategory.SetValue(Grid.RowProperty, i);
                    cellCategory.SetValue(Grid.ColumnProperty, colNum);
                    cellCategory.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellCategory.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellCategory);

                    colNum++;
                    TextBlock cellAmount = new TextBlock();
                    cellAmount.Text = results[i].transactionAmount.ToString();
                    cellAmount.SetValue(Grid.RowProperty, i);
                    cellAmount.SetValue(Grid.ColumnProperty, colNum);
                    cellAmount.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellAmount.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellAmount);

                    colNum++;
                    TextBlock cellDescription = new TextBlock();
                    cellDescription.Text = results[i].transactionDescription.ToString();
                    cellDescription.SetValue(Grid.RowProperty, i);
                    cellDescription.SetValue(Grid.ColumnProperty, colNum);
                    cellDescription.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellDescription.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellDescription);

                    colNum++;
                    Button cellDelButton = new Button();
                    cellDelButton.Tag = results[i].Id.ToString();
                    cellDelButton.Content = "x";
                    cellDelButton.SetValue(Grid.RowProperty, i);
                    cellDelButton.SetValue(Grid.ColumnProperty, colNum);
                    cellDelButton.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellDelButton.VerticalAlignment = VerticalAlignment.Center;
                    cellDelButton.HorizontalAlignment = HorizontalAlignment.Left;
                    cellDelButton.Style = (Style)(this.Resources["ButtonStyle"]);
                    cellDelButton.Click += deleteButton_Click;
                    gridTransactions.Children.Add(cellDelButton);

                }
            }


            string type = "";

            if (radioIncome.IsChecked == true)
                type = "Income";
            else
                type = "Expense";

            yearlyTotals = await GetYearlyData(type);
            monthlyIncome = await GetMonthlyData("Income", DateTime.Now.Year, DateTime.Now.Month);
            monthlyExpense = await GetMonthlyData("Expense", DateTime.Now.Year, DateTime.Now.Month);

            setRecommendationText();

            this.DataContext = new Graph();
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateDummyTransaction();
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            MessageDialog dialog = new MessageDialog("Are you sure to delete?");
            UICommand okCommand = new UICommand("OK");
            UICommand cancelCommand = new UICommand("Cancel");

            dialog.Commands.Add(okCommand);
            dialog.Commands.Add(cancelCommand);

            IUICommand response = await dialog.ShowAsync();

            

            if (response == okCommand)
            {
                var results = await transactionsTable
                         .Where(transactionItem => transactionItem.Id == (string)clickedButton.Tag)
                         .ToListAsync();

                if (results.Count != 0)
                {
                    DeleteTransaction(results[0]);
                }
                RefreshTransactions();

            }


        }

        private async void DeleteTransaction(transactionItem transaction)
        {
            await transactionsTable.DeleteAsync(transaction);

        }

        private async void setRecommendationText()
        {
            float yearlyIncomeAvg = await getYearlyAvg("Income");
            float yearlyExpenseAvg = await getYearlyAvg("Expense");

            // Decide to talk about income or outcome
            if (Math.Abs(yearlyIncomeAvg - monthlyIncome) > Math.Abs(yearlyExpenseAvg - monthlyExpense))
            {
                // Income recommendation
                if (yearlyIncomeAvg > monthlyIncome)
                {
                    tbRecommendation.Text = "Current month's income("
                        + monthlyIncome
                        + ") is less than yearly average("
                        + yearlyIncomeAvg
                        + ").  Work harder!";
                }
                else if (yearlyIncomeAvg < monthlyIncome)
                {
                    tbRecommendation.Text = "Current month's income("
                        + monthlyIncome
                        + ") is more than yearly average ("
                        + yearlyIncomeAvg
                        + "). Think about investment!";
                }
                else
                {
                    tbRecommendation.Text = "Current month's income("
                        + monthlyIncome
                        + ") is exactly on yearly average ("
                        + yearlyIncomeAvg
                        + "). Go on your life!";
                }

            }
            else
            {
                // Expense receommendation
                if (yearlyExpenseAvg > monthlyExpense)
                {
                    tbRecommendation.Text = "Current month's expense("
                        + monthlyExpense
                        + ") is less than yearly average ("
                        + yearlyExpenseAvg
                        + "). What about new movies this week?";
                }
                else if (yearlyExpenseAvg < monthlyExpense)
                {
                    tbRecommendation.Text = "Current month's expense("
                        + monthlyExpense
                        + ") is more than yearly average ("
                        + yearlyExpenseAvg
                        + "). Stay home a bit!";
                }

                else
                {
                    tbRecommendation.Text = "Current month's income("
                        + monthlyIncome
                        + ") is exactly on yearly average ("
                        + yearlyIncomeAvg
                        + "). Go on your life!";
                }
            }
        }

        private async Task<float> getYearlyAvg(string type)
        {
            float yearlyAvg = 0;
            float yearlyTotal = 0;

            List<Tuple<string, float>> yearlyTotalData = await GetYearlyData(type);

            foreach (var item in yearlyTotalData)
            {
                yearlyTotal += item.Item2;
            }

            yearlyAvg = yearlyTotal / yearlyTotalData.Count;

            return yearlyAvg;
        }


        /*  ----- TESTS & NOT IMPLEMENTED YET ----- */

        private void btn_refreshTransactions(object sender, RoutedEventArgs e)
        {

            RefreshTransactions();
        }

        //TODO : Repair twitter authentication
        //TODO : Grid Scroll

        private void CreateCategoryItem()
        {
            /*var categoryItem = new categoryItem { Type = "Income", Category = "Salary" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Income", Category = "Asset" };
            InsertCategoryItem(categoryItem);

            // bills, groceries, entertainment, investment
            var categoryItem = new categoryItem { Type = "Expense", Category = "Bills" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Expense", Category = "Groceries" };
            InsertCategoryItem(categoryItem);

            var categoryItem = new categoryItem { Type = "Expense", Category = "Entertainment" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Expense", Category = "Investment" };
            InsertCategoryItem(categoryItem);*/
        }


        private async void InsertCategoryItem(categoryItem categoryItem)
        {
            await categoriesTable.InsertAsync(categoryItem);
            categoryItems.Add(categoryItem);
        }

        private void CreateDummyTransaction()
        {

            Random rand = new Random();

            DateTime start = new DateTime(2012, 1, 1);
            var range = DateTime.Now - start;
            var randTimeSpan = new TimeSpan((long)(rand.NextDouble() * range.Ticks));
            DateTime randomDate =  start + randTimeSpan;

            var types = new List<string> { "Income", "Expense" };
            int indType = rand.Next(types.Count);

            var categories = new List<string>();
            if (indType == 0)
            {
                categories.Add("Salary");
                categories.Add("Asset");
            }
            else
            {
                categories.Add("Entertainment");
                categories.Add("Bills");
                categories.Add("Groceries");
                categories.Add("Investment");
            }
            int indCat = rand.Next(categories.Count);



            var transactionItem = new transactionItem
            {
                transactionDate = randomDate,
                transactionType = types[indType],
                transactionCategory = categories[indCat],
                transactionAmount = rand.Next(100),
                transactionDescription = "Test data goes here.",
                transactionUsername = tbUsername.Text

            };
            InsertTransaction(transactionItem);
        }

    }
}
