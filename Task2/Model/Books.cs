using LINQtoCSV;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Task2.Model
{
    public class Books : INotifyPropertyChanged
    {
        private string name;
        private string lastname;
        private string patro;
        private DateTime? birthDate;
        private string bookName;
        private int? year;
        public int Id { get; set; }
        [CsvColumn(Name = "name", FieldIndex = 1)]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }
        [CsvColumn(Name = "lastname", FieldIndex = 2)]
        public string Lastname
        {
            get { return lastname; }
            set
            {
                lastname = value;
                NotifyPropertyChanged("Lastname");
            }
        }
        [CsvColumn(Name = "patro", FieldIndex = 3)]
        public string Patro
        {
            get { return patro; }
            set
            {
                patro = value;
                NotifyPropertyChanged("Patro");
            }
        }
        [CsvColumn(Name = "birthdate", FieldIndex = 4)]
        public DateTime? BirthDate
        {
            get { return birthDate; }
            set
            {
                birthDate = value;
                NotifyPropertyChanged("BirthDate");
            }
        }
        [CsvColumn(Name = "bookname", FieldIndex = 5)]
//        public string BookName { get; set; }
        public string BookName
        {
            get { return bookName; }
            set
            {
                bookName = value;
                NotifyPropertyChanged("BookName");
            }
        }
        [CsvColumn(Name = "year", FieldIndex = 6)]
        public int? Year
        {
            get { return year; }
            set
            {
                year = value;
                NotifyPropertyChanged("Year");
            }
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
