using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using System.Linq;

namespace ANT.Model
{
    public class DatabaseInfo<CollectionT> : ObservableObject
    {
        public DatabaseInfo(LiteDB.LiteDatabase liteDatabase)
        {
            LiteDatabase = liteDatabase;

            DatabaseName = GetDatabaseName();
            DatabaseSize = GetDatabaseSize();
            CollectionDataCount = GetDatabaseCollectionDataCount();
        }

        public LiteDB.LiteDatabase LiteDatabase { get; }

        private string _databaseName;
        public string DatabaseName
        {
            get { return _databaseName; }
            set { SetProperty(ref _databaseName, value); }
        }

        private double _databaseSize;
        public double DatabaseSize
        {
            get { return _databaseSize; }
            set { SetProperty(ref _databaseSize, value); }
        }

        private int _collectionDataCount;
        public int CollectionDataCount
        {
            get { return _collectionDataCount; }
            set { SetProperty(ref _collectionDataCount, value); }
        }

        public string GetDatabaseName()
        {
            var keyPairCollection = (IEnumerable<KeyValuePair<string, LiteDB.BsonValue>>)LiteDatabase
                .Execute("SELECT name FROM $database").Current.RawValue;

            string result = keyPairCollection.First().Value.AsString;

            return result;
        }

        public double GetDatabaseSize()
        {
            var keyPairCollection = (IEnumerable<KeyValuePair<string, LiteDB.BsonValue>>)LiteDatabase
                .Execute("SELECT dataFileSize FROM $database").Current.RawValue;

            double result = keyPairCollection.First().Value.AsDouble;
            
            return result;
        }

        public int GetDatabaseCollectionDataCount()
        {
            return LiteDatabase.GetCollection<CollectionT>().Count();
        }
    }
}
