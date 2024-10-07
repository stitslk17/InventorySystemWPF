using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using InventorySystem.Model;
using InventorySystem.MVVM;
using MySql.Data.MySqlClient;

namespace InventorySystem.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Item>  Items { get; set; }

        public RelayCommand AddCommand => 
            new RelayCommand( execute => AddItem(), canExecute => { return true ; } );
        public RelayCommand DeleteCommand => 
            new RelayCommand(execute => DeleteItem(), canExecute => SelectedItem!= null );

        public RelayCommand SaveCommand =>
            new RelayCommand(execute => Save(), canExecute => CanSave());

        public MainWindowViewModel() 
        {

            Items = new ObservableCollection<Item>();
            LoadItems();
        }
        private Item selectedItem;

        public Item SelectedItem
        {
            get { return selectedItem; }
            set 
            { 
                selectedItem = value;
                OnPropertyChanged();
            }
        }
        private void AddItem()
        {
            Items.Add(new Item 
            { 
                Name = "NEW ITEM",
                SerialNumber = "XXXXX",
                Quantity = 0,
            }
            );
        }

        private void DeleteItem()
        {
            if (SelectedItem == null) return;

            string query = "DELETE FROM items WHERE serialnumber = @serialnumber";

            using (MySqlConnection connection = new DatabaseConnection().GetConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@serialnumber", SelectedItem.SerialNumber);
                    command.ExecuteNonQuery();
                }
            }

            // Remove the item from the ObservableCollection
            Items.Remove(SelectedItem);
        }

        private void Save()
        {
            string query = @"
        INSERT INTO items(name, serialnumber, quantity) 
        VALUES (@name, @serialnumber, @quantity)
        ON DUPLICATE KEY UPDATE 
            name = VALUES(name),
            quantity = VALUES(quantity);";

            using (MySqlConnection connection = new DatabaseConnection().GetConnection())
            {
                connection.Open();

                foreach (var item in Items)
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", item.Name);
                        command.Parameters.AddWithValue("@serialnumber", item.SerialNumber);
                        command.Parameters.AddWithValue("@quantity", item.Quantity);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool CanSave()
        {
            return true;
        }

        private void LoadItems()
        {
            string query = "SELECT name, serialnumber, quantity FROM items";

            using (MySqlConnection connection = new DatabaseConnection().GetConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        Items.Clear(); // Clear existing items to avoid duplication

                        while (reader.Read())
                        {
                            Items.Add(new Item
                            {
                                Name = reader["name"].ToString(),
                                SerialNumber = reader["serialnumber"].ToString(),
                                Quantity = reader.GetInt32("quantity")
                            });
                        }
                    }
                }
            }
        }
    }
}
