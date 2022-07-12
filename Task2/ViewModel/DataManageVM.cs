using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task2.Model;
using Task2.View;
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
using System.Runtime.CompilerServices;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Task2.ViewModel
{
    public class DataManageVM : INotifyPropertyChanged
    {
        public static DataRepository db = new DataRepository();
        public List<Books> AllBooks
        {
            get 
            {
                return GetData();
            }
        }
        //Свойства для книг
        public string AuthName { get; set; } = SelectedBook?.Name;
        public string AuthLastname { get; set; } = SelectedBook?.Lastname;
        public string AuthPatro { get; set; } = SelectedBook?.Patro;
        public DateTime? DateOfBirth { get; set; } = SelectedBook?.BirthDate;
        public string bookName { get; set; } = SelectedBook?.BookName;
        public int? YearOfCreate { get; set; } = SelectedBook?.Year;
        public static string? SearchBookName { get; set; }

        public static int PageCounter = 0;
        public static int pageSize = 14;
        public bool IsBusy
        {
            get
            {
                return SearchBookName != null;
            }
        }
        public bool hasNext
        {
            get
            {
                return pageSize * PageCounter < GetCountBooks() - pageSize;
            }
        }
        public bool hasPrev
        {
            get
            {
                return PageCounter > 0;
            }
        }

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
        public readonly string csvExportPath = @"C:\Source\Book_exp.csv";
        public readonly string fileError = "Возникла ошибка! Выберите подходящий файл.";
        public readonly string SuccessfullyUnloaded = "Файл успешно выгружен";
        public readonly string SuccessfullyUploaded = "Файл успешно загружен";
        //Свойства для выделенных элементов
        public static Books? SelectedBook { get; set; }

        #region COMMANDS TO ADD
        private RelayCommand addNewBook;
        public RelayCommand AddNewBook
        {
            get 
            {
                return addNewBook ?? (addNewBook =  new RelayCommand(obj =>
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
                            resultStr = BookSucCreated;
                            wnd.Close();
                        } else
                        {
                            resultStr = BookNotCreated;
                        }
                        ShowMessageToUser(resultStr);
                    }
                }));
            }
        }
        #endregion
        private RelayCommand deleteItem;
        public RelayCommand DeleteItem
        {
            get
            {
                return deleteItem ?? (deleteItem = new RelayCommand(obj => 
                {
                    string resultStr = NotSelected;
                    if(SelectedBook != null)
                    {
                        db.RemoveBook(SelectedBook);
                        resultStr = "Done! Книга " + SelectedBook.BookName + " успешно удалена";
                        NotifyPropertyChanged(nameof(AllBooks));
                    }
                    ShowMessageToUser(resultStr);
                }));
            }
        }
        private RelayCommand filterBook;
        public RelayCommand FilterBook
        {
            get
            {
                return filterBook ?? (filterBook = new RelayCommand(obj => 
                {
                    Window wnd = obj as Window;
                    if (SearchBookName == null || SearchBookName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControl(wnd, BNameBlock);
                    }
                    else
                    {
                        PageCounter = 0;
                        wnd.Close();
                    }
                }));
            }
        }
        private RelayCommand nextPage;
        public RelayCommand NextPage
        {
            get
            {
                return nextPage ?? (nextPage = new RelayCommand(obj => 
                {
                    PageCounter++;
                    NotifyMain();
                }));
            }
        }
        private RelayCommand prevPage;
        public RelayCommand PrevPage
        {
            get
            {
                return prevPage ?? (prevPage = new RelayCommand(obj =>
                {
                    PageCounter--;
                    NotifyMain();
                }));
            }
        }
        private RelayCommand resetFilter;
        public RelayCommand ResetFilter
        {
            get
            {
                return resetFilter ?? (resetFilter = new RelayCommand(obj =>
                {
                    PageCounter = 0;
                    SearchBookName = null;
                    NotifyMain();
                }));
            }
        }
        private RelayCommand xmlExport;
        public RelayCommand XmlExport
        {
            get
            {
                return xmlExport ?? (xmlExport = new RelayCommand(obj => 
                {
                    var bookstoexp = new List<Books>();
                    bookstoexp = GetData();
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
                }));
            }
        }
        #region EDIT COMMANDS
        private RelayCommand editBook;
        public RelayCommand EditBook
        {
            get
            {
                return editBook ?? (editBook = new RelayCommand(obj =>
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
                                wnd.Close();
                            }
                            ShowMessageToUser(resultStr);
                        }
                    }
                    else ShowMessageToUser(resultStr);
                }));
            }
        }
        #endregion

        #region COMMANDS TO OPEN WINDOWS
        private RelayCommand openAddNewBookWnd;
        public RelayCommand OpenAddNewBookWnd
        {
            get 
            {
                return openAddNewBookWnd ?? (openAddNewBookWnd = new RelayCommand(obj =>
                {
                    SelectedBook = null;
                    OpenAddBookWindowMethod();
                    NotifyPropertyChanged(nameof(AllBooks));
                }));
            }
        }
        private RelayCommand openEditItemWnd;
        public RelayCommand OpenEditItemWnd
        {
            get
            {
                return openEditItemWnd ?? (openEditItemWnd = new RelayCommand(obj =>
                {
                    string resultStr = BookNotSelected;
                    if (SelectedBook != null)
                    {
                        OpenEditBookWindowMethod();
                    } else ShowMessageToUser(resultStr);
                }));
            }
        }
        private RelayCommand openFileWnd;
        public RelayCommand OpenFileWnd
        {
            get
            {
                return openFileWnd ?? (openFileWnd = new RelayCommand(async obj =>
                {
                    await OpenFileWindowMethodAsync();
                    NotifyPropertyChanged(nameof(AllBooks));
                }));
            }
        }
        private RelayCommand openExportWnd;
        public RelayCommand OpenExportWnd
        {
            get
            {
                return openExportWnd ?? (openExportWnd = new RelayCommand(obj =>
                {
                    ExportFileWindowMethod();
                    ShowMessageToUser(SuccessfullyUnloaded);
                }));
            }
        }
        private RelayCommand openFilterWnd;
        public RelayCommand OpenFilterWnd
        {
            get
            {
                return openFilterWnd ?? (openFilterWnd = new RelayCommand(obj =>
                {
                    OpenFilterWindowMethod();
                    NotifyMain();
                }));
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
                    await ReadExcel(sFilename);
                    ShowMessageToUser(SuccessfullyUploaded);
                }
                catch
                {
                    ShowMessageToUser(fileError);
                }
            }
        }

        private void ExportFileWindowMethod()
        {
            var bookstoexp = new List<Books>();
            bookstoexp = GetData();

            var csvFileDescription = new CsvFileDescription
            {
                FirstLineHasColumnNames = true,
                SeparatorChar = ';',
                EnforceCsvColumnAttribute = true
            };

            var csvContext = new CsvContext();
            csvContext.Write(bookstoexp, csvExportPath, csvFileDescription);
        }

        private void OpenEditBookWindowMethod()
        {
            EditBookWindow editBookWindow = new EditBookWindow();
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

        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView(message);
            SetCenterPositionAndOpen(messageView);
        }
        private async Task ReadExcel(string Filename)
        {
            bool is_first_row = true;
            //читаем эксель
            await Task.Run(() =>
            {
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
            });
        }
        private List<Books> GetData()
        {
            if (SearchBookName != null)
            {
                return db.FindBooks(SearchBookName).Skip(pageSize * PageCounter).Take(pageSize).ToList();
            } 
            else return db.GetBooks().Skip(pageSize * PageCounter).Take(pageSize).ToList();
        }

        private int GetCountBooks()
        {
            if (SearchBookName != null)
            {
                return db.FindBooks(SearchBookName).Count();
            }
            else return db.GetBooks().Count();
        }

        private void NotifyMain()
        {
            NotifyPropertyChanged(nameof(AllBooks));
            NotifyPropertyChanged(nameof(hasNext));
            NotifyPropertyChanged(nameof(hasPrev));
            NotifyPropertyChanged(nameof(IsBusy));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
