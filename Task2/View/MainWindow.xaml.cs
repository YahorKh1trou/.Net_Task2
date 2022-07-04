using System.Windows;
using System.Windows.Controls;
using Task2.ViewModel;
using Microsoft.Win32;
using System;

namespace Task2.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ListView AllBooksView;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DataManageVM();
            AllBooksView = ViewAllBooks;
        }
    }
}
