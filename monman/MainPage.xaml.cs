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
            //await AuthenticateFacebook();
            //await AuthenticateTwitter();
            /*var parameter = e.Parameter as userItem;
            if (parameter != null)
            {
                this.userNameTb.Text = parameter.Username;
            }
            else
            {
                this.userNameTb.Text = "Could not fetch name.";
            }*/
            //this.userNameTb.Text = "";
            //RefreshTodoItems();

            //this.tbUsername.Text = "Welcome ";
            //GetUsername();
            //CreateCategoryItem();
            LoadTypeItems();
            //GetMonthlyData("Income", 1);
            RefreshTransactions();
        }

        public class Graph
        {
            public ObservableCollection<PieChart> MonthlyData { get; private set; }

            public ObservableCollection<ColumnChart> YearlyIncome { get; private set; }

            public ObservableCollection<ColumnChart> YearlyExpense { get; private set; }

            public Graph()
            {
                //float monthlyIncome = await GetMonthlyData("Income",1);
                MonthlyData = new ObservableCollection<PieChart>();
                
                // TODO : Do it dynamic remove hardcoded              
                MonthlyData.Add(new PieChart() { Category = "Income", Number = monthlyIncome });
                MonthlyData.Add(new PieChart() { Category = "Expense", Number = monthlyExpense });

                YearlyExpense = new ObservableCollection<ColumnChart>();

                foreach (var item in yearlyExpense)
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

        private static List<Tuple<string,float>> yearlyIncome;
        private static List<Tuple<string, float>> yearlyExpense; 

        private async Task<List<Tuple<string,float>>> GetYearlyData(string type)
        {
            var yearlyAmount = new List<Tuple<string, float>>();

            // TODO : Get total by year and month

            /*yearlyAmount.Add(new Tuple<string, float>("Oct-13", 100));
            yearlyAmount.Add(new Tuple<string, float>("Nov-13", 200));
            yearlyAmount.Add(new Tuple<string, float>("Dec-13", 150));
            yearlyAmount.Add(new Tuple<string, float>("Jan-14", 120));*/

            string username = "Gün Karagöz";
            //string username = tbUsername.Text;

            var results = await transactionsTable
                          .Where(transactionItem => transactionItem.transactionUsername == username
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
                    //outTotals = GetMonthlyData(type, DateTime.Now.Month);
                    //monthTotalAmount = await outTotals;

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
            string username = "Gün Karagöz";
            //string username = tbUsername.Text;


            float totalAmount = 0;
            var results = await transactionsTable
                          .Where(transactionItem => transactionItem.transactionUsername == username
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

            var transactionItem = new transactionItem
            {

                transactionUsername = "Gün Karagöz", //tbUsername.Text,
                transactionDate = dateSelection.Date.DateTime, //DateTime.Now,
                transactionType = (string)comboType.SelectedItem,
                transactionCategory = (string)comboCategory.SelectedItem,
                transactionAmount = float.Parse(textAmount.Text),
                transactionDescription = textDescription.Text
            };

            InsertTransaction(transactionItem);

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
                //this.lblTitle.Text = "Multi-auth Blog: " + userName;

                //InsertUserItem(appUser);
            }
            catch (InvalidOperationException)
            {


            }
        }
        private async void RefreshTransactions()
        {

            Task<float> inTotals = GetMonthlyData("Income", DateTime.Now.Year, DateTime.Now.Month);
            monthlyIncome = await inTotals;

            Task<float> outTotals = GetMonthlyData("Expense", DateTime.Now.Year, DateTime.Now.Month);
            monthlyExpense = await outTotals;

            Task<List<Tuple<string,float>>> inYearly = GetYearlyData("Income");
            yearlyIncome = await inYearly;

            Task<List<Tuple<string, float>>> outYearly = GetYearlyData("Outcome");
            yearlyExpense = await inYearly;

            //this.DataContext = this;
            this.DataContext = new Graph();

            gridTransactions.Children.Clear();
            string username = "Gün Karagöz";
            //string username = tbUsername.Text;
            var results = await transactionsTable
                          .Where(transactionItem => transactionItem.transactionUsername == username)
                          .ToListAsync();

            var dialog = new MessageDialog("");
            if (results.Count == 0)
            {
                dialog = new MessageDialog("Transaction history is empty!");
                dialog.Commands.Add(new UICommand("OK"));
                //await dialog.ShowAsync();
            }
            else
            {
                //comboCategory.Items.Clear();


                // TODO Do it dynamically! Change columns dynamically.
                for (int i = 0; i < results.Count; i++)
                {
                    int colNum = 0;

                    TextBlock cellRowNum = new TextBlock();
                    //cellRowNum.Name = "no_" + results[i].Id;
                    cellRowNum.Text = (i + 1).ToString();
                    cellRowNum.SetValue(Grid.RowProperty, i + 1);
                    cellRowNum.SetValue(Grid.ColumnProperty, colNum);
                    cellRowNum.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellRowNum.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellRowNum);
                    // gridTransactions.RowDefinitions();

                    /*
                    TextBlock cellIsPayed = new TextBlock();
                    //cellIsPayed.Name = "isPayed_" + results[i].Id;
                    cellIsPayed.Text = results[i].transactionIsPayed.ToString();
                    cellIsPayed.SetValue(Grid.RowProperty, i);
                    cellIsPayed.SetValue(Grid.ColumnProperty, 1);
                    cellIsPayed.Margin = new Thickness(0, (i+1) * 30, 0, 0);
                    cellIsPayed.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellIsPayed);
                     */
                    colNum++;
                    TextBlock cellDate = new TextBlock();
                    //cellDate.Name = "date_" + results[i].Id;;
                    cellDate.Text = String.Format("{0:dd/MM/yy}", results[i].transactionDate);
                    cellDate.SetValue(Grid.RowProperty, i);
                    cellDate.SetValue(Grid.ColumnProperty, colNum);
                    cellDate.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellDate.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellDate);

                    colNum++;
                    TextBlock cellType = new TextBlock();
                    //cellType.Name = "type_" + results[i].Id;;
                    cellType.Text = results[i].transactionType.ToString();
                    cellType.SetValue(Grid.RowProperty, i);
                    cellType.SetValue(Grid.ColumnProperty, colNum);
                    cellType.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellType.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellType);

                    colNum++;
                    TextBlock cellCategory = new TextBlock();
                    //cellCategory.Name = "categoty_" + results[i].Id;;
                    cellCategory.Text = results[i].transactionCategory.ToString();
                    cellCategory.SetValue(Grid.RowProperty, i);
                    cellCategory.SetValue(Grid.ColumnProperty, colNum);
                    cellCategory.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellCategory.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellCategory);

                    colNum++;
                    TextBlock cellAmount = new TextBlock();
                    //cellAmount.Name = "amount_" + results[i].Id;;
                    cellAmount.Text = results[i].transactionAmount.ToString();
                    cellAmount.SetValue(Grid.RowProperty, i);
                    cellAmount.SetValue(Grid.ColumnProperty, colNum);
                    cellAmount.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellAmount.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellAmount);

                    colNum++;
                    TextBlock cellDescription = new TextBlock();
                    //cellDescription.Name = "desc_" + results[i].Id;;
                    cellDescription.Text = results[i].transactionDescription.ToString();
                    cellDescription.SetValue(Grid.RowProperty, i);
                    cellDescription.SetValue(Grid.ColumnProperty, colNum);
                    cellDescription.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellDescription.Style = (Style)(this.Resources["RowStyle"]);
                    gridTransactions.Children.Add(cellDescription);

                    colNum++;
                    Button cellDelButton = new Button();
                    cellDelButton.Tag = results[i].Id.ToString();
                    cellDelButton.Content = "X";
                    cellDelButton.SetValue(Grid.RowProperty, i);
                    cellDelButton.SetValue(Grid.ColumnProperty, colNum);
                    cellDelButton.Margin = new Thickness(0, (i + 1) * 30, 0, 0);
                    cellDelButton.VerticalAlignment = VerticalAlignment.Top;
                    cellDelButton.HorizontalAlignment = HorizontalAlignment.Left;
                    cellDelButton.Click += deleteButton_Click;
                    gridTransactions.Children.Add(cellDelButton);

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateDummyTransaction();
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Are you sure to delete?");
            UICommand okCommand = new UICommand("OK");
            UICommand cancelCommand = new UICommand("Cancel");

            dialog.Commands.Add(okCommand);
            dialog.Commands.Add(cancelCommand);

            IUICommand response = await dialog.ShowAsync();

            Button clickedButton = sender as Button;

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

            //if(transactionItems.TotalCount != null && transactionItems.IndexOf(transaction) != null)
            //transactionItems.Remove(transaction);

        }


        private void btn_refreshTransactions(object sender, RoutedEventArgs e)
        {
            
            //this.DataContext = new Graph();

            RefreshTransactions();
        }
        //TODO : Check if data entry empty.
        //TODO : Clear form after entry
        //TODO : Repair twitter authentication
        //TODO : Grid Scroll
        //TODO : Add Recommendations



        /*  ----- TESTS & NOT IMPLEMENTED YET ----- */
        // TODO : To Be Implemented in Next Versions
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
            var types = new List<string> { "Income", "Expense" };
            int indType = rand.Next(types.Count);

            var categories = new List<string> { "Entertainment", "Bills", "Groceries", "Investment" };
            int indCat = rand.Next(categories.Count);

            var transactionItem = new transactionItem
            {
                transactionDate = DateTime.Now,
                transactionType = types[indType],
                transactionCategory = categories[indCat],
                transactionAmount = rand.Next(100),
                transactionDescription = "Test data goes here.",
                transactionUsername = "Gün Karagöz"

            };
            InsertTransaction(transactionItem);
        }

    }
}
