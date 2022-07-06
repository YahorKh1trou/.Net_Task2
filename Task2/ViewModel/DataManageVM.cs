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
using System.Collections.ObjectModel;

namespace Task2.ViewModel
{
    public class DataManageVM : INotifyPropertyChanged
    {
        public static DataRepository db = new DataRepository();

        private ObservableCollection<Books> allBooks = new ObservableCollection<Books>(db.GetBooks().Skip(skip).Take(pageSize));
        public ObservableCollection<Books> AllBooks

        {
            get { return allBooks; }
            set
            {
                allBooks = value;
                NotifyPropertyChanged("AllBooks");
            }
        }

        private ObservableCollection<Books> filterBooks = new ObservableCollection<Books>(db.FindBooks(BName));
        public ObservableCollection<Books> FilterBooks
        {
            get { return filterBooks; }
            set
            {
                filterBooks = value;
//                NotifyPropertyChanged("AllBooks");
                //                UpdateAllBooksView(FilterBooks);
            }
        }

        //Свойства для книг
        public static string AuthName { get; set; }
        public static string AuthLastname { get; set; }
        public static string AuthPatro { get; set; }
        public static DateTime? DateOfBirth { get; set; }
        public static string bookName { get; set; }
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
        public readonly string BookSucCreated = "Книга успешно добавлена";
        public readonly string BookNotCreated = "Такая книга уже существует!";
        public readonly string BookDoesNotExist = "Такой книги не существует!";

        public readonly string xmlPath = @"C:\Source\Book_xml.xml";
        public readonly string csvPath = @"C:\Source\Book_exp.csv";
        public readonly string csvFilter = "Csvfiles|*.csv";
        public readonly string csvDefault = ".csv";
        public readonly static string csvExportPath = @"C:\Source\Book_exp.csv";
        public readonly string fileError = "Возникла ошибка! Выберите подходящий файл.";
        public readonly string SuccessfullyUnloaded = "Файл успешно выгружен";
        public readonly string SuccessfullyUploaded = "Файл успешно загружен";
        //Свойства для выделенных элементов
        public static Books SelectedBook { get; set; }

        #region COMMANDS TO ADD
        public RelayCommand AddNewBook
        {
            get 
            {
                return new RelayCommand(obj =>
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
                    else if (bookName == null || bookName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, BookBlock);
                    }
                    else if (YearOfCreate == 0 || YearOfCreate > DateTime.Now.Year || YearOfCreate < 1)
                    {
                        SetRedBlockControl(wnd, YearBlock);
                    }
                    else
                    {
                        Books book = new Books { Name = AuthName, Lastname = AuthLastname, Patro = AuthPatro, BirthDate = DateOfBirth, BookName = bookName, Year = YearOfCreate };
                        bool checkIsExist = db.HasBooks(bookName);
                        if (!checkIsExist)
                        {
                            db.AddBook(book);
                            GetterForUpdate();
                            resultStr = BookSucCreated;
                            SetNullValuesToProperties();
                            wnd.Close();
                        } else
                        {
                            resultStr = BookNotCreated;
                        }
                        ShowMessageToUser(resultStr);
                    }
                });
            }
        }
        #endregion

        public RelayCommand DeleteItem
        {
            get
            {
                return new RelayCommand(obj => 
                {
                    string resultStr = NotSelected;
                    if(SelectedBook != null)
                    {
                        db.RemoveBook(SelectedBook);
                        resultStr = "Done! Книга " + SelectedBook.BookName + " успешно удалена";
                        GetterForUpdate();
                    }
                    SetNullValuesToProperties();
                    ShowMessageToUser(resultStr);
                });
            }
        }

        public RelayCommand FilterBook
        {
            get
            {
                return new RelayCommand(obj => 
                {
                    Window wnd = obj as Window;
                    if (BName == null || BName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, BNameBlock);
                    }
                    else
                    {
                        FilterBooks = new ObservableCollection<Books>(db.FindBooks(BName));
                        wnd.Close();
                    }
                });
            }
        }

        public RelayCommand NextPage
        {
            get
            {
                return new RelayCommand(obj => 
                {
                    BName = null;
                    PageCounter++;
                    var total = db.GetBooks().Count();
                    skip = pageSize * (PageCounter - 1);
                    var canPage = skip < total;
                    if (canPage)
                    {
                        GetterForUpdate();
                    }
                    else
                    {
                        PageCounter--;
                        ShowMessageToUser(NoMorePages);
                    }
                });
            }
        }

        public RelayCommand PrevPage
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    BName = null;
                    PageCounter--;
                    if (skip == 0)
                        PageCounter++;
                    skip = pageSize * (PageCounter - 1);
                    var canPage = skip >= 0;
                    if (canPage)
                    {
                        GetterForUpdate();
                    }
                });
            }
        }
        public RelayCommand XmlExport
        {
            get
            {
                return new RelayCommand(obj => 
                {
                    var bookstoexp = new List<Books>();
                    if (BName != null)
                    {
                        bookstoexp = db.FindBooks(BName);
                    }
                    else
                    {
                        bookstoexp = db.GetBooks().Skip(skip).Take(pageSize).ToList();
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
        public RelayCommand EditBook
        {
            get
            {
                return new RelayCommand(obj =>
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
                        else if (bookName == null || bookName.Replace(" ", "").Length == 0)
                        {
                            SetRedBlockControl(wnd, BookBlock);
                        }
                        else if (YearOfCreate == 0 || YearOfCreate > DateTime.Now.Year || YearOfCreate < 1)
                        {
                            SetRedBlockControl(wnd, YearBlock);
                        }
                        else
                        {
                            Books book = db.FindFirstBook(SelectedBook);
                            resultStr = BookDoesNotExist;
                            if (book != null)
                            {
                                db.EditBook(book, AuthName, AuthLastname, AuthPatro, DateOfBirth, bookName, YearOfCreate);
                                resultStr = "Done! Книга " + book.BookName + " успешно изменена";
                                GetterForUpdate();
//                                NotifyPropertyChanged("AllBooks");
                                SetNullValuesToProperties();
                                wnd.Close();
                            }
                            ShowMessageToUser(resultStr);
                        }
                    }
                    else ShowMessageToUser(resultStr);
                });
            }
        }
        #endregion

        #region COMMANDS TO OPEN WINDOWS
        public RelayCommand OpenAddNewBookWnd
        {
            get 
            {
                return new RelayCommand(obj =>
                    {
                        SetNullValuesToProperties();
                        OpenAddBookWindowMethod();
                    });
            }
        }
        public RelayCommand OpenEditItemWnd
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    string resultStr = BookNotSelected;
                    if (SelectedBook != null)
                    {
                        OpenEditBookWindowMethod(SelectedBook);
                    }
                });
            }

        }
        public RelayCommand OpenFileWnd
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    await OpenFileWindowMethodAsync();
                    GetterForUpdate();
                });
            }
        }
        public RelayCommand OpenExportWnd
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    ExportFileWindowMethod();
                    ShowMessageToUser(SuccessfullyUnloaded);
                });
            }
        }
        public RelayCommand OpenFilterWnd
        {
            get
            {
                return new RelayCommand(obj =>
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

        private async Task OpenFileWindowMethodAsync()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = csvFilter;
            fileDialog.DefaultExt = csvDefault;
            bool? dialogOK = fileDialog.ShowDialog();

            if (dialogOK == true)
            {
                var sFilename = fileDialog.FileName;
                try
                {
                    await Task.Run(() => ReadExcel(sFilename));
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
            if (BName != null)
            {
                bookstoexp = db.FindBooks(BName);
            }
            else
            {
                bookstoexp = db.GetBooks().Skip(skip).Take(pageSize).ToList();
            }

            var csvFileDescription = new CsvFileDescription
            {
                FirstLineHasColumnNames = true,
                SeparatorChar = ';',
                EnforceCsvColumnAttribute = true
            };

            var csvContext = new CsvContext();
            csvContext.Write(bookstoexp, csvExportPath, csvFileDescription);
//            BName = null;
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
            bookName = null;
            YearOfCreate = null;
        }
/*
        private void UpdateAllBooksView(ObservableCollection<Books> ISource)
        {
//            AllBooks = db.GetBooks().Skip(skip).Take(pageSize).ToList();
            MainWindow.AllBooksView.ItemsSource = null;
            MainWindow.AllBooksView.Items.Clear();
            MainWindow.AllBooksView.ItemsSource = ISource;
            MainWindow.AllBooksView.Items.Refresh();
        }
*/
        #endregion
        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView(message);
            SetCenterPositionAndOpen(messageView);
        }

        private void ReadExcel(string Filename)
        {
            bool is_first_row = true;
            //читаем эксель
            using (StreamReader reader = new StreamReader(Filename))
            {
                // построчно заносим данные в бд
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (is_first_row == false)
                    {
                        var values = line.Split(';');
                        if (values.Count() > 5)
                        {
                            Books book = new Books { Name = values[0], Lastname = values[1], Patro = values[2], BirthDate = DateTime.Parse(values[3]), BookName = values[4], Year = Int32.Parse(values[5]) };
                            db.AddBook(book);
                        }
                    }
                    is_first_row = false;
                }
            }
        }

        private void GetterForUpdate()
        {
            AllBooks = new ObservableCollection<Books>(db.GetBooks().Skip(skip).Take(pageSize));
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
