using LINQtoCSV;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task2.Model
{
    public class Books
    {
        public int Id { get; set; }
        [CsvColumn(Name = "name", FieldIndex = 1)]
        public string Name { get; set; }
        [CsvColumn(Name = "lastname", FieldIndex = 2)]
        public string Lastname { get; set; }
        [CsvColumn(Name = "patro", FieldIndex = 3)]
        public string Patro { get; set; }
        [CsvColumn(Name = "birthdate", FieldIndex = 4)]
        public DateTime? BirthDate { get; set; }
        [CsvColumn(Name = "bookname", FieldIndex = 5)]
        public string BookName { get; set; }
        [CsvColumn(Name = "year", FieldIndex = 6)]
        public int? Year { get; set; }
    }
}
