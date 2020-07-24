using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class ErrorLog
    {
        public ErrorLog()
        {}

        public int Id { get; set; }

        public  string StackTrace { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }

        public string ExceptionTypeName { get; set; }

        public DateTime ExceptionDate { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
