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
        private readonly ApplicationContext db = new ApplicationContext();
        public DataRepository()
        {
            this.db = db;
        }
        public List<Books> GetBooks()
        {
            return db.Books.ToList();
        }
        public void AddBook(Books book)
        {
            db.Books.Add(book);
            db.SaveChanges();
        }
        public void RemoveBook(Books book)
        {
            db.Books.Remove(book);
            db.SaveChanges();
        }
        public void EditBook(Books book, string newName, string newLastname, string newPatro, DateTime? newBitrhdate, string newBookname, int? newYear)
        {
            book.Name = newName;
            book.Lastname = newLastname;
            book.Patro = newPatro;
            book.BirthDate = newBitrhdate;
            book.BookName = newBookname;
            book.Year = newYear;
            db.SaveChanges();
        }
        public List<Books> FindBooks(string bookname) => db.Books.Where(p => EF.Functions.Like(p.BookName, $"%{bookname}%")).ToList();
        public bool HasBooks(string bookname) => db.Books.Any(el => el.BookName == bookname);
        public Books FindFirstBook(Books oldBook) => db.Books.FirstOrDefault(b => b.Id == oldBook.Id);
    }
}
