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

        [BsonId(true)]
        public int Id { get; set; }

        public System.Exception Exception { get; set; }

        public Type ExceptionType { get; set; }

        public DateTime ExceptionDate { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
