using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MonMan;
using MonMan;
using Windows.Globalization.DateTimeFormatting;

namespace MonMan
{
    public class TodoItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }
    }

    public class categoryItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]

        public string Type {get;set;}

        [JsonProperty(PropertyName = "category")]

        public string Category { get; set; }

    }

    public class transactionItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string transactionUsername { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime transactionDate {get;set;}

        [JsonProperty(PropertyName = "type")]
        public string transactionType { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string transactionCategory { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public float transactionAmount { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string transactionDescription { get; set; }

        [JsonProperty(PropertyName = "ispayed")]
        public bool transactionIsPayed { get; set; }



    }

    public class testItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string testType { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string testCategory { get; set; }

        [JsonProperty(PropertyName = "ispayed")]
        public bool testIsPayed { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime testDate { get; set; }
        
    }



    public sealed partial class MainPage : Page
    {
        private MobileServiceCollection<TodoItem, TodoItem> items;
        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();

        private MobileServiceCollection<categoryItem, categoryItem> categoryItems;
        private IMobileServiceTable<categoryItem> categoriesTable = App.MobileService.GetTable<categoryItem>();

        private MobileServiceCollection<transactionItem, transactionItem> transactionItems;
        private IMobileServiceTable<transactionItem> transactionsTable = App.MobileService.GetTable<transactionItem>();

        private MobileServiceCollection<testItem, testItem> testItems;
        private IMobileServiceTable<testItem> testTable = App.MobileService.GetTable<testItem>();


        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void DeleteCategoryItem()
        {
            var deleteItem = new categoryItem { Type = "Income", Category = "Salary" };
            await categoriesTable.DeleteAsync(deleteItem);
        }

        private async void InsertCategoryItem(categoryItem categoryItem)
        {
            await categoriesTable.InsertAsync(categoryItem);
            categoryItems.Add(categoryItem);
        }

        private async void InsertTransaction(transactionItem transaction)
        {
            await transactionsTable.InsertAsync(transaction);
            transactionItems.Add(transaction);
        }

        private async void InsertTest(testItem test)
        {
            await testTable.InsertAsync(test);
            testItems.Add(test);
        }


        private async void InsertTodoItem(TodoItem todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await todoTable.InsertAsync(todoItem);
            items.Add(todoItem);            
        }

        private async void RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems
                items = await todoTable
                    .Where(todoItem => todoItem.Complete == false)
                    .ToCollectionAsync();

                categoryItems = await categoriesTable
                    .ToCollectionAsync();


                
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                //ListItems.ItemsSource = items;
            }
        }

        private async void UpdateCheckedTodoItem(TodoItem item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await todoTable.UpdateAsync(item);
            items.Remove(item);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTodoItems();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var todoItem = new TodoItem { Text = TextInput.Text };
            InsertTodoItem(todoItem);
            
            var categoryItem = new categoryItem { Type = "Income", Category = "Salary" };
            InsertCategoryItem(categoryItem);

            //var userItem = new userItem { Email = "gun@iyte.net", Username = TextInput.Text };
            //InsertUserItem(userItem);
            

        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            UpdateCheckedTodoItem(item);
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

            this.userNameTb.Text = "Welcome ";
            loadTypeItems();
            //createNewTransactionItem();
            //createNewTestItem();
            //loadCategoryItems();
            //addNewCategoty();


        }

        private async void createNewTestItem()
        {
           testItems = await testTable.ToCollectionAsync();
            var testItem = new testItem
            {
                testType = "type ulan",
                testCategory = "cat ulan",
                testIsPayed = true,
                testDate = DateTime.Now
            };

            InsertTest(testItem);
        }

        private async void createNewTransactionItem()
        {
            transactionItems = await transactionsTable.ToCollectionAsync();

            var transactionItem = new transactionItem
            {
                transactionUsername = "Gun",
                transactionDate = dateSelection.Date.DateTime, //DateTime.Now,
                transactionType = (string)comboType.SelectedItem,
                transactionCategory = (string)comboCategory.SelectedItem,
                transactionAmount = float.Parse(textAmount.Text),
                transactionDescription = textDescription.Text,
                transactionIsPayed = togglePayed.IsOn
               

            };

            InsertTransaction(transactionItem);

        }

        private void addNewCategoty()
        {
            /*var categoryItem = new categoryItem { Type = "Income", Category = "Salary" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Income", Category = "Asset" };
            InsertCategoryItem(categoryItem);

            // bills, groceries, entertainment, investment
            categoryItem = new categoryItem { Type = "Expense", Category = "Bills" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Expense", Category = "Groceries" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Expense", Category = "Entertainment" };
            InsertCategoryItem(categoryItem);

            categoryItem = new categoryItem { Type = "Expense", Category = "Investment" };
            InsertCategoryItem(categoryItem);*/
        }

        private void loadTypeItems()
        {
            comboType.Items.Add("Income");
            comboType.Items.Add("Expense");

        }

        private async void loadCategoryItems()
        {
            
            var results = await categoriesTable
                                    .Where(categoryItem => categoryItem.Type == comboType.SelectedItem)
                                    .ToListAsync();

            var dialog = new MessageDialog("");
            if (results.Count == 0)
            {
                dialog = new MessageDialog("Not found in table!");
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
            }
            else
            {
                comboCategory.Items.Clear();
                //dialog = new MessageDialog("You're goin' good!");                
                foreach (var item in results)
                {
                    comboCategory.Items.Add(item.Category);
                }
            }
            

        }



        private void comboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadCategoryItems();
        }

        private void ButtonSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
            createNewTransactionItem();
            /*
           var transactionItem = new transactionItem{ 
               //transactionDate = new DateTime(2013,12,24,0,0,0,0),
               transactionType = (string)comboType.SelectedItem,
               transactionCategory = (string)comboCategory.SelectedItem,
               transactionAmount = float.Parse(textAmount.Text),
               transactionDescription = textDescription.Text,
               transactionIsPayed = true,
               transactionUsername ="Gun"

           };
           InsertTransaction(transactionItem);
             */
        }

    
    }
}
