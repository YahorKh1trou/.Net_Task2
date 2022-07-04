using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Task2.Model.Data
{
    public class DataRepository : IDataRepository
    {
        public string result = string.Empty;

        public readonly string BookAlreadyExists = "Такая книга уже существует в библиотеке";
        public readonly string BookSucCreated = "Книга успешно добавлена";
        public readonly string ListLoadFail = "Не удалось загрузить список книг!";
        public readonly string BooksSucUploaded = "Книги успешно загружены из Excel";
        public readonly string BookDoesNotExist = "Такой книги не существует!";

        private readonly ApplicationContext db = new ApplicationContext();
        public DataRepository()
        {
            this.db = db;
        }
        public List<Books> GetBooks(int skip, int take)
        {
            return db.Books.Skip(skip).Take(take).ToList();
        }
        public int CountOfBooks()
        {
            return db.Books.Count();
        }
        public List<Books> AddBook(Books book, string bookname)
        {
            result = BookAlreadyExists;
            bool checkIsExist = db.Books.Any(el => el.Name == bookname);
            if (!checkIsExist)
            {
                db.Books.Add(book);
                db.SaveChanges();
                result = BookSucCreated;
            }
            return db.Books.ToList();
        }

        public List<Books> AddBookExcel(string path)
        {
            bool is_first_row = true;
            //читаем эксель
            using (StreamReader reader = new StreamReader(path))
            {
                // построчно заносим данные в бд
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (is_first_row == false)
                    {
                        var values = line.Split(';');
                        if (values.Count()>5)
                        {
                            Books book = new Books { Name = values[0], Lastname = values[1], Patro = values[2], BirthDate = DateTime.Parse(values[3]), BookName = values[4], Year = Int32.Parse(values[5]) };
                            db.Books.Add(book);
                            db.SaveChanges();
                        }
                        else
                        {
                            result = ListLoadFail;
                        }
                    }
                    is_first_row = false;
                }
                result = BooksSucUploaded;
            }
            return db.Books.ToList();
        }
        public List<Books> RemoveBook(Books book)
        {
            result = BookDoesNotExist;
            if (book != null)
            {
                db.Books.Remove(book);
                db.SaveChanges();
                result = "Done! Книга " + book.BookName + " успешно удалена";
            }
            return db.Books.ToList();
        }
        public List<Books> EditBook(Books book, Books oldBook, string newName, string newLastname, string newPatro, DateTime? newBitrhdate, string newBookname, int? newYear)
        {
            result = BookDoesNotExist;
            book = db.Books.FirstOrDefault(b => b.Id == oldBook.Id);
            if (book != null)
            {
                book.Name = newName;
                book.Lastname = newLastname;
                book.Patro = newPatro;
                book.BirthDate = newBitrhdate;
                book.BookName = newBookname;
                book.Year = newYear;
                db.SaveChanges();
                result = "Done! Книга " + book.BookName + " успешно изменена";
            }
            return db.Books.ToList();
        }
        public List<Books> FindBooks(string bookname) => db.Books.Where(p => EF.Functions.Like(p.BookName, $"%{bookname}%")).ToList();
    }
}
