using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task2.Model;
using Task2.View;
using Task2.ViewModel;
using Task2.Model.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using LINQtoCSV;
using System.Xml.Linq;

namespace Task2.ViewModel
{
    public class DataManageVM : INotifyPropertyChanged
    {
        private List<Books> allBooks = DataWorker.GetBooks(skip, pageSize);
        public List<Books> AllBooks
        {
            get { return allBooks; }
            set
            {
                allBooks = value;
                NotifyPropertyChanged("AllBooks");
            }
        }
        private List<Books> filterBooks = DataWorker.FindBooks(BName);
        public List<Books> FilterBooks
        {
            get { return filterBooks; }
            set
            {
                filterBooks = value;
                NotifyPropertyChanged("FilterBooks");
            }
        }

        //Свойства для книг
        public static string AuthName { get; set; }
        public static string AuthLastname { get; set; }
        public static string AuthPatro { get; set; }
        public static DateTime? DateOfBirth { get; set; }
        public static string BookName { get; set; }
        public static int? YearOfCreate { get; set; }
        public static string BName { get; set; }
        public static int PageCounter = 1;
        public static int pageSize = 14;
        public static int skip;

        //Строковые константы
        public readonly string NameBlock = "NameBlock";
        public readonly string LastnameBlock = "LastnameBlock";
        public readonly string PatroBlock = "PatroBlock";
        public readonly string DateBlock = "DateBlock";
        public readonly string BookBlock = "BookBlock";
        public readonly string YearBlock = "YearBlock";
        public readonly string BNameBlock = "BnameBlock";

        public readonly string NotSelected = "Ничего не выбрано";
        public readonly string NoMorePages = "Страниц больше нет!";
        public readonly string BookNotSelected = "Не выбрана книга";

        public readonly string xmlPath = @"C:\Source\Book_xml.xml";
        public readonly string csvFilter = "Csvfiles|*.csv";
        public readonly string csvDefault = ".csv";
        public readonly static string csvExportPath = @"C:\Source\Book_exp.csv";
        public readonly string fileError = "Возникла ошибка! Выберите подходящий файл.";
        public readonly string SuccessfullyUnloaded = "Файл успешно выгружен";
        public readonly string SuccessfullyUploaded = "Файл успешно загружен";
        //Свойства для выделенных элементов
        public static Books SelectedBook { get; set; }

        #region COMMANDS TO ADD
        private RelayCommand addNewBook;
        public RelayCommand AddNewBook
        {
            get 
            {
                return addNewBook ?? new RelayCommand(obj =>
                {
                    Window wnd = obj as Window;
                    string resultStr = "";
                    if (AuthName == null || AuthName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, NameBlock);
                    }
                    else if (AuthLastname == null || AuthLastname.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, LastnameBlock);
                    }
                    else if (AuthPatro == null || AuthPatro.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, PatroBlock);
                    }
                    else if (DateOfBirth == null || !DateTime.TryParse(DateOfBirth.ToString(), out DateTime dDate))
                    {
                        SetRedBlockControl(wnd, DateBlock);
                    }
                    else if (BookName == null || BookName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, BookBlock);
                    }
                    else if (YearOfCreate == 0 || YearOfCreate > DateTime.Now.Year || YearOfCreate < 1)
                    {
                        SetRedBlockControl(wnd, YearBlock);
                    }
                    else
                    {
                        resultStr = DataWorker.CreateBook(AuthName, AuthLastname, AuthPatro, DateOfBirth, BookName, YearOfCreate);
                        UpdateAllBooksView();
                        ShowMessageToUser(resultStr);
                        SetNullValuesToProperties();
                        wnd.Close();
                    }
                });
            }
        }
        #endregion

        private RelayCommand deleteItem;
        public RelayCommand DeleteItem
        {
            get
            {
                return deleteItem ?? new RelayCommand(obj => 
                {
                    string resultStr = NotSelected;
                    if(SelectedBook != null)
                    {
                        resultStr = DataWorker.RemoveBook(SelectedBook);
                        UpdateAllBooksView();
                    }
                    SetNullValuesToProperties();
                    ShowMessageToUser(resultStr);
                });
            }
        }

        private RelayCommand filterBook;
        public RelayCommand FilterBook
        {
            get
            {
                return filterBook ?? new RelayCommand(obj => 
                {
                    Window wnd = obj as Window;
                    if (BName == null || BName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, BNameBlock);
                    }
                    else
                    {
                        UpdateAllFilteredBooksView();
                        wnd.Close();
                    }
                });
            }
        }

        private RelayCommand nextPage;
        public RelayCommand NextPage
        {
            get
            {
                return nextPage ?? new RelayCommand(obj => 
                {
                    PageCounter++;
                    var total = DataWorker.CountOfBooks();
                    skip = pageSize * (PageCounter - 1);
                    var canPage = skip < total;
                    if (canPage)
                    {
                        DataWorker.GetBooks(skip, pageSize);
                        UpdateAllBooksView();
                    }
                    else
                    {
                        PageCounter--;
                        ShowMessageToUser(NoMorePages);
                    }
                });
            }
        }

        private RelayCommand prevPage;
        public RelayCommand PrevPage
        {
            get
            {
                return nextPage ?? new RelayCommand(obj =>
                {
                    PageCounter--;
                    if (skip == 0)
                        PageCounter++;
                    skip = pageSize * (PageCounter - 1);
                    var canPage = skip >= 0;
                    if (canPage)
                    {
                        DataWorker.GetBooks(skip, pageSize);
                        UpdateAllBooksView();
                    }
                });
            }
        }

        private RelayCommand xmlExport;
        public RelayCommand XmlExport
        {
            get
            {
                return xmlExport ?? new RelayCommand(obj => 
                {
                    var bookstoexp = new List<Books>();
                    if (BName != null)
                    {
                        bookstoexp = DataWorker.FindBooks(BName);
                    }
                    else
                    {
                        bookstoexp = DataWorker.GetBooks(skip, pageSize);
                    }
                    XDocument xdoc = new XDocument();
                    XElement xbooks = new XElement("TestProgram");

                    foreach (var book in bookstoexp)
                    {
                        XElement xrecord = new XElement("Record");
                        XElement xname = new XElement("FirstName", book.Name);
                        XElement xlastname = new XElement("LastName", book.Lastname);
                        XElement xpatro = new XElement("SurName", book.Patro);
                        XElement xdate = new XElement("BirthDate", book.BirthDate);
                        XElement xbookname = new XElement("BookName", book.BookName);
                        XElement xyear = new XElement("BookYear", book.Year);

                        xrecord.SetAttributeValue("id", book.Id);
                        xrecord.Add(xname, xlastname, xpatro, xdate, xbookname, xyear);
                        xbooks.Add(xrecord);
                    }
                    xdoc.Add(xbooks);
                    File.WriteAllText(xmlPath, xdoc.ToString());
                    ShowMessageToUser(SuccessfullyUnloaded);
                });
            }
        }
        #region EDIT COMMANDS
        private RelayCommand editBook;
        public RelayCommand EditBook
        {
            get
            {
                return editBook ?? new RelayCommand(obj =>
                {
                    Window wnd = obj as Window;
                    string resultStr = BookNotSelected;
                    if (SelectedBook != null)
                    {
                        if (AuthName == null || AuthName.Replace(" ", "").Length == 0)
                        {
                            SetRedBlockControl(wnd, NameBlock);
                        }
                        else if (AuthLastname == null || AuthLastname.Replace(" ", "").Length == 0)
                        {
                            SetRedBlockControl(wnd, LastnameBlock);
                        }
                        else if (AuthPatro == null || AuthPatro.Replace(" ", "").Length == 0)
                        {
                            SetRedBlockControl(wnd, PatroBlock);
                        }
                        else if (DateOfBirth == null || !DateTime.TryParse(DateOfBirth.ToString(), out DateTime dDate))
                        {
                            SetRedBlockControl(wnd, DateBlock);
                        }
                        else if (BookName == null || BookName.Replace(" ", "").Length == 0)
                        {
                            SetRedBlockControl(wnd, BookBlock);
                        }
                        else if (YearOfCreate == 0 || YearOfCreate > DateTime.Now.Year || YearOfCreate < 1)
                        {
                            SetRedBlockControl(wnd, YearBlock);
                        }
                        else
                        {
                            resultStr = DataWorker.EditBook(SelectedBook, AuthName, AuthLastname, AuthPatro, DateOfBirth, BookName, YearOfCreate);
                            UpdateAllBooksView();
                            SetNullValuesToProperties();
                            ShowMessageToUser(resultStr);
                            wnd.Close();
                        }
                    }
                    else ShowMessageToUser(resultStr);
                });
            }
        }
        #endregion

        #region COMMANDS TO OPEN WINDOWS
        private RelayCommand openAddNewBookWnd;
        public RelayCommand OpenAddNewBookWnd
        {
            get 
            {
                return openAddNewBookWnd ?? new RelayCommand(obj =>
                    {
                        SetNullValuesToProperties();
                        OpenAddBookWindowMethod();
                    });
            }
        }
        private RelayCommand openEditItemWnd;
        public RelayCommand OpenEditItemWnd
        {
            get
            {
                return openEditItemWnd ?? new RelayCommand(obj =>
                {
                    string resultStr = BookNotSelected;
                    if (SelectedBook != null)
                    {
                        OpenEditBookWindowMethod(SelectedBook);
                    }
                });
            }

        }
        private RelayCommand openFileWnd;
        public RelayCommand OpenFileWnd
        {
            get
            {
                return openFileWnd ?? new RelayCommand(obj =>
                {
                    OpenFileWindowMethod();
                    UpdateAllBooksView();
                });
            }
        }
        public RelayCommand openExportWnd;
        public RelayCommand OpenExportWnd
        {
            get
            {
                return openExportWnd ?? new RelayCommand(obj =>
                {
                    ExportFileWindowMethod();
                    ShowMessageToUser(SuccessfullyUnloaded);
                });
            }
        }
        public RelayCommand openFilterWnd;
        public RelayCommand OpenFilterWnd
        {
            get
            {
                return openFileWnd ?? new RelayCommand(obj =>
                {
                    OpenFilterWindowMethod();
                });
            }
        }
        #endregion

        #region METHODS TO OPEN WINDOW

        private void OpenAddBookWindowMethod()
        {
            AddNewBookWindow newBookWindow = new AddNewBookWindow();
            SetCenterPositionAndOpen(newBookWindow);
        }

        private void OpenFileWindowMethod()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = csvFilter;
            fileDialog.DefaultExt = csvDefault;
            Nullable<bool> dialogOK = fileDialog.ShowDialog();

            if (dialogOK == true)
            {
                var sFilename = fileDialog.FileName;
                try
                {
                    DataWorker.CreateBookExcel(sFilename);
                    ShowMessageToUser(SuccessfullyUploaded);
                }
                catch
                {
                    ShowMessageToUser(fileError);
                }
            }
        }

        private static void ExportFileWindowMethod()
        {
            var bookstoexp = new List<Books>();
//            bookstoexp = DataWorker.GetBooks();
            if (BName != null)
            {
                bookstoexp = DataWorker.FindBooks(BName);
            }
            else
            {
                bookstoexp = DataWorker.GetBooks(skip, pageSize);
            }

            var csvFileDescription = new CsvFileDescription
            {
                FirstLineHasColumnNames = true,
                SeparatorChar = ';',
                EnforceCsvColumnAttribute = true
            };

            var csvContext = new CsvContext();
            csvContext.Write(bookstoexp, csvExportPath, csvFileDescription);
            BName = null;
        }

        private void OpenEditBookWindowMethod(Books book)
        {
            EditBookWindow editBookWindow = new EditBookWindow(book);
            SetCenterPositionAndOpen(editBookWindow);
        }

        private void OpenFilterWindowMethod()
        {
            FilterWindow filterWindow = new FilterWindow();
            SetCenterPositionAndOpen(filterWindow);
        }

        private void SetCenterPositionAndOpen(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        #endregion

        private void SetRedBlockControl(Window wnd, string blockName)
        {
            Control block = wnd.FindName(blockName) as Control;
            block.BorderBrush = Brushes.Red;
        }

        #region UPDATE VIEWS

        private void SetNullValuesToProperties()
        {
            AuthName = null;
            AuthLastname = null;
            AuthPatro = null;
            DateOfBirth = null; 
            BookName = null;
            YearOfCreate = null;
        }
        private void UpdateAllBooksView()
        {
            AllBooks = DataWorker.GetBooks(skip, pageSize);
            MainWindow.AllBooksView.ItemsSource = null;
            MainWindow.AllBooksView.Items.Clear();
            MainWindow.AllBooksView.ItemsSource = AllBooks;
            MainWindow.AllBooksView.Items.Refresh();
        }
        private void UpdateAllFilteredBooksView()
        {
            FilterBooks = DataWorker.FindBooks(BName);
            MainWindow.AllBooksView.ItemsSource = null;
            MainWindow.AllBooksView.Items.Clear();
            MainWindow.AllBooksView.ItemsSource = FilterBooks;
            MainWindow.AllBooksView.Items.Refresh();
        }

        #endregion
        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView(message);
            SetCenterPositionAndOpen(messageView);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
