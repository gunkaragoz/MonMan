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

namespace monman
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

    public class userItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "username")]

        public string Username { get; set; }

    }

    public sealed partial class MainPage : Page
    {
        private MobileServiceCollection<TodoItem, TodoItem> items;
        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();

        private MobileServiceCollection<categoryItem, categoryItem> categoryItems;
        private IMobileServiceTable<categoryItem> categoriesTable = App.MobileService.GetTable<categoryItem>();

        private MobileServiceCollection<userItem, userItem> userItems;
        private IMobileServiceTable<userItem> usersTable = App.MobileService.GetTable<userItem>();

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

        private async void InsertUserItem(userItem userItem)
        {
            // TODO _GUN: Check if user is registered
            if (userItems != null && userItems.Contains(userItem))
            {
                await new MessageDialog("User Exists!", "Email is already registered.").ShowAsync();
            }
            else
            {
                    await usersTable.InsertAsync(userItem);
                    userItems.Add(userItem);
            }
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

                userItems = await usersTable
                    .Where(userItem => userItem.Email != "")
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
                ListItems.ItemsSource = userItems;
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

            var userItem = new userItem { Email = "gun@iyte.net", Username = TextInput.Text };
            InsertUserItem(userItem);
            

        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            UpdateCheckedTodoItem(item);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshTodoItems();
        }

    
    }
}
