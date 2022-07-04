using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task2.Model.Data;

namespace Task2.Model
{
    public static class DataWorker
    {
        public static string CreateBook(string name, string lastname, string patro, DateTime? birthdate, string bookname, int? year)
        {
            DataRepository db = new DataRepository();
            Books book = new Books { Name = name, Lastname = lastname, Patro = patro, BirthDate = birthdate, BookName = bookname, Year = year };
            db.AddBook(book, bookname);
            return db.result;
        }
        public static List<Books> GetBooks(int skip, int take)
        {
            DataRepository db = new DataRepository();
            return db.GetBooks(skip, take);
 //           return db.result;
        }
        public static List<Books> FindBooks(string bookname)
        {
            DataRepository db = new DataRepository();
            return db.FindBooks(bookname);
        }
        public static string RemoveBook(Books book)
        {
            DataRepository db = new DataRepository();
            db.RemoveBook(book);
            return db.result;
        }

        public static string CreateBookExcel(string path)
        {
            DataRepository db = new DataRepository();
            db.AddBookExcel(path);
            return db.result;
        }

        public static int CountOfBooks()
        {
            DataRepository db = new DataRepository();
            return db.CountOfBooks();
        }
        public static string EditBook(Books oldBook, string newName, string newLastname, string newPatro, DateTime? newBitrhdate, string newBookname, int? newYear)
        {
            DataRepository db = new DataRepository();
            Books book = new Books();
            db.EditBook(book, oldBook, newName, newLastname, newPatro, newBitrhdate, newBookname, newYear);
            return db.result;
        }
    }
}
