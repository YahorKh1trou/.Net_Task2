using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2.Model.Data
{
    public interface IDataRepository
    {
        List<Books> AddBook(Books book, string name);
        List<Books> AddBookExcel(string path);
        List<Books> GetBooks(int skip, int take);
        List<Books> RemoveBook(Books book);
        List<Books> FindBooks(string bookname);
        int CountOfBooks();
        List<Books> EditBook(Books book, Books oldBook, string newName, string newLastname, string newPatro, DateTime? newBitrhdate, string newBookname, int? newYear);
    }
}
