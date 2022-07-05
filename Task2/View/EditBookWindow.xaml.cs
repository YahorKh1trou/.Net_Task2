using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Task2.Model;
using Task2.ViewModel;

namespace Task2.View
{
    /// <summary>
    /// Логика взаимодействия для EditBook.xaml
    /// </summary>
    public partial class EditBookWindow : Window
    {
        public EditBookWindow(Books bookToEdit)
        {
            InitializeComponent();
            DataContext = new DataManageVM();
            DataManageVM.SelectedBook = bookToEdit;
            DataManageVM.AuthName = bookToEdit.Name;
            DataManageVM.AuthLastname = bookToEdit.Lastname;
            DataManageVM.AuthPatro = bookToEdit.Patro;
            DataManageVM.DateOfBirth = bookToEdit.BirthDate;
            DataManageVM.bookName = bookToEdit.BookName;
            DataManageVM.YearOfCreate = bookToEdit.Year;
        }
    }
}
