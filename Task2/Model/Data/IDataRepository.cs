using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2.Model.Data
{
    public interface IDataRepository
    {
        void AddBook(Books book);
        List<Books> GetBooks();
        void RemoveBook(Books book);
        List<Books> FindBooks(string bookname);
        bool HasBooks(string bookname);
        Books FindFirstBook(Books oldBook);
        void EditBook(Books book, string newName, string newLastname, string newPatro, DateTime? newBitrhdate, string newBookname, int? newYear);
    }
}
