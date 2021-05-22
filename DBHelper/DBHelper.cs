using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Threading;

namespace DBHelper
{
    public class DBHelper
    {
        SQLiteConnection conn;
        SQLiteAsync sqAsync;
        private Dictionary<string, CryptoExchangeModel> coinList;
        public class MarketInputModel
        {
            public MarketInputModel() { }

            public MarketInputModel(string Market, string Nation)
            {
                this.Market = Market;
                this.Nation = Nation;
            }

            public string Market;
            public string Nation;
        }

        public class CryptoInputModel
        {
            public CryptoInputModel(string Target, string Stand, decimal Price, string Market, DateTime Date)
            {
                this.Target = Target.ToUpper();
                this.Stand = Stand.ToUpper();
                this.Price = Price;
                this.Market = Market;
                this.Date = Date;
            }

            public string Target;
            public string Stand;
            public decimal Price;
            public string Market;
            public DateTime Date;
        }

        public class CurrencyExchangeModel
        {
            public CurrencyExchangeModel(string target, string maxStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice, double percent)
            {
                this.targetCrypto = target;
                this.maxStandCurrency = maxStand;
                this.minStandCurrency = "";
                this.maxMarketNation = maxnation;
                this.maxMarketName = maxmarket;
                this.minMarketNation = minnation;
                this.minMarketName = minmarket;
                this.TargetPrice = TargetPrice;
                this.StandPrice = StandPrice;
                this.percent = percent;
            }

            public CurrencyExchangeModel(string target, string maxStand, string minStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice, double percent)
            {
                this.targetCrypto = target;
                this.maxStandCurrency = maxStand;
                this.minStandCurrency = minStand;
                this.maxMarketNation = maxnation;
                this.maxMarketName = maxmarket;
                this.minMarketNation = minnation;
                this.minMarketName = minmarket;
                this.TargetPrice = TargetPrice;
                this.StandPrice = StandPrice;
                this.percent = percent;
            }

            public string targetCrypto;
            public string maxStandCurrency;
            public string minStandCurrency;
            public string maxMarketNation;
            public string maxMarketName;
            public string minMarketNation;
            public string minMarketName;
            public decimal TargetPrice;
            public decimal StandPrice;
            public double percent;
        }

        public class CryptoExchangeModel
        {
            public CryptoExchangeModel(DateTime Date, string Target, string Stand, string MaxMarketName, string MinMarketName, double Percent, long RowID = 0)
            {
                this.RowID = RowID;
                this.Date = Date;
                this.Target = Target;
                this.Stand = Stand;
                this.MaxMarketName = MaxMarketName;
                this.MinMarketName = MinMarketName;
                this.Percent = Percent;
            }

            public long RowID;
            public DateTime Date;
            public string Target;
            public string Stand;
            public string MaxMarketName;
            public string MinMarketName;
            public double Percent;
        }

        public class ExchangeInputModel
        {
            public ExchangeInputModel(DateTime Date, string Target, string Stand, decimal Price)
            {
                this.Date = Date;
                this.Target = Target.ToUpper();
                this.Stand = Stand.ToUpper();
                this.Price = Price;
            }

            public DateTime Date;
            public string Target;
            public string Stand;
            public decimal Price;
        }

        public DBHelper()
        {
            sqAsync = new SQLiteAsync();
            coinList = new Dictionary<string, CryptoExchangeModel>();

            //var list = select();


            string DBLocation = Environment.CurrentDirectory + @"\CryptoExchange.sqlite";

            //SQLiteConnection.CreateFile(DBLocation);
            conn = new SQLiteConnection(@"Data Source=" + DBLocation);

            conn.Open();
            
            string sql = @"
            -- -----------------------------------------------------
            -- Table `MarketList`
            -- -----------------------------------------------------
            CREATE TABLE IF NOT EXISTS `MarketList` (
              `MarketName` VARCHAR(45) NOT NULL,
              `Nation` VARCHAR(45) NULL,
              PRIMARY KEY (`MarketName`));

            -- -----------------------------------------------------
            -- Table `MarketDetail`
            -- -----------------------------------------------------
            CREATE TABLE IF NOT EXISTS `MarketDetail` (
            `Date` DATETIME NOT NULL,
            `Target_Coin` CHAR(4) NOT NULL,
            `Stand_Coin` CHAR(4) NOT NULL,
            `Price` DECIMAL(20, 10) NOT NULL,
            `MarketList_MaketName` VARCHAR(45) NOT NULL,
            CONSTRAINT `fk_MarketDetail_MarketList1`
            FOREIGN KEY(`MarketList_MaketName`)
            REFERENCES `MarketList` (`MarketName`)
            ON DELETE NO ACTION
            ON UPDATE NO ACTION);

            -- -----------------------------------------------------
            -- Table `ExchangeInfo`
            -- -----------------------------------------------------
            CREATE TABLE IF NOT EXISTS `ExchangeInfo` (
              `Date` DATETIME NOT NULL,
              `Target_Currency` CHAR(4) NOT NULL,
              `Stand_Currency` CHAR(4) NOT NULL,
              `Price` DECIMAL(20, 10) NOT NULL);


            -- -----------------------------------------------------
            -- Table `BlackList`
            -- -----------------------------------------------------
            CREATE TABLE IF NOT EXISTS `BlackList` (
              `Name` VARCHAR(45) NOT NULL,
              `Type` INT NOT NULL,
              PRIMARY KEY (`Name`));

            CREATE VIEW IF NOT EXISTS BanMarketList AS
            SELECT Name FROM BlackList WHERE Type=1;

            CREATE VIEW IF NOT EXISTS BanNationList AS
            SELECT Name FROM BlackList WHERE Type=2;

            CREATE VIEW IF NOT EXISTS LastDatas2 AS
            SELECT A.Date, A.Target_Coin, A.Stand_Coin, A.Price, B.Nation, B.MarketName FROM
            (SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail GROUP BY Target_Coin, Stand_Coin, MarketName) A
            INNER JOIN MarketList B on A.MarketName=B.MarketName;

            CREATE VIEW IF NOT EXISTS LastDatas AS
            SELECT * FROM LastDatas2 WHERE NOT MarketName IN BanMarketList AND NOT Nation IN BanNationList;
            
            CREATE VIEW IF NOT EXISTS MinPrices AS
            SELECT Date, Target_Coin, Stand_Coin, MIN(Price) AS Price, Nation, MarketName
            FROM LastDatas GROUP BY Target_Coin, Stand_Coin;

            CREATE VIEW IF NOT EXISTS MaxPrices AS
            SELECT Date, Target_Coin, Stand_Coin, MAX(Price) AS Price, Nation, MarketName
            FROM LastDatas GROUP BY Target_Coin, Stand_Coin;

            CREATE VIEW IF NOT EXISTS CryptoExchange AS
            SELECT A.Date AS Date, A.Target_Coin, A.Stand_Coin, A.Nation AS MaxNation, A.MarketName AS MaxMarketName,
            B.Nation AS MinNation, B.MarketName AS MinMarketName, A.Price AS MaxPrice, B.Price AS MinPrice, 100 - (B.Price / A.Price * 100) AS Percent
            FROM MaxPrices A INNER JOIN MinPrices B
            ON A.Target_Coin = B.Target_Coin AND A.Stand_Coin = B.Stand_Coin AND A.MarketName != B.MarketName;

            CREATE VIEW IF NOT EXISTS LastExchangeInfo AS
            SELECT MAX(date), Target_Currency, Stand_Currency, Price
            FROM ExchangeInfo WHERE Stand_Currency == ""KRW""
            GROUP BY Target_Currency, Stand_Currency;

            CREATE VIEW IF NOT EXISTS CurrencyList AS
            SELECT * FROM LastDatas WHERE Stand_Coin IN (
            SELECT DISTINCT(Target_Currency) FROM ExchangeInfo)
            GROUP BY Target_Coin, Stand_Coin, MarketName;

            CREATE VIEW IF NOT EXISTS KRWList AS
            SELECT a.Date, a.Target_Coin, a.Stand_Coin, a.Price, a.Nation, a.MarketName, a.Price * b.Price AS KRW
            FROM CurrencyList a INNER JOIN LastExchangeInfo b ON a.stand_coin = b.target_currency;

            CREATE VIEW IF NOT EXISTS MinKRWList AS
            SELECT Date, Target_Coin, Stand_Coin, Price, Nation, MarketName, MIN(KRW) AS KRW
            FROM KRWList GROUP BY Target_Coin;

            CREATE VIEW IF NOT EXISTS MaxKRWList AS
             SELECT Date, Target_Coin, Stand_Coin, Price, Nation, MarketName, MAX(KRW) AS KRW
            FROM KRWList GROUP BY Target_Coin;

            CREATE VIEW IF NOT EXISTS CurrencyExchange AS
            SELECT Min.Date, Min.Target_Coin, Max.Stand_Coin AS MaxStand_Coin, Min.Stand_Coin AS MinStand_Coin,
            Max.Nation AS MaxNation, Max.MarketName AS MaxMarketName, Min.Nation AS MinNation, Min.MarketName AS MinMarketName,
            Max.Price AS MaxPrice, Min.Price AS MinPrice, 100 - (Min.KRW / Max.KRW * 100) AS Percent
            FROM MinKRWList Min INNER JOIN MaxKRWList Max
            ON Min.Target_Coin=MAX.Target_Coin AND Min.MarketName!=Max.MarketName;

            INSERT OR REPLACE INTO ExchangeInfo VALUES (""2021-05-20 오전 00:00:00"", ""KRW"", ""KRW"", 1);
            ";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = TryExecuteNonQuery(command);

            conn.Disposed += Conn_Disposed;
            conn.Update += Conn_Update1;
        }

        private void Conn_Update1(object sender, UpdateEventArgs e)
        {
            
        }

        private void Conn_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void InputMarketData(MarketInputModel model)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = @"SELECT MarketName FROM MarketList "
            + "WHERE MarketName = \"" + model.Market + "\";";

            SQLiteCommand command = new SQLiteCommand(sql, conn);

            SQLiteDataReader rdr = TryExecuteReader(command);

            if (rdr.HasRows)
                return;

            sql = @"
            INSERT INTO MarketList "
            + "VALUES ("
            + "\"" + model.Market + "\"" + ", " + "\"" + model.Nation + "\""
            + @");";
            
            command = new SQLiteCommand(sql, conn);
            int result = TryExecuteNonQuery(command);
        }

        public void InputCryptoData(CryptoInputModel model)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = @"
            INSERT INTO MarketDetail "
            + "VALUES ("
            + "\"" +  model.Date + "\"" + ", "
            + "\"" + model.Target + "\"" + ", "
            + "\"" + model.Stand + "\"" + ", "
            + model.Price + ", "
            + "\"" + model.Market + "\"" + ");";

            SQLiteCommand command = new SQLiteCommand(sql, conn);

            int result = TryExecuteNonQuery(command);
        }

        public void InputExchangeData(ExchangeInputModel model)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = @"
            INSERT INTO ExchangeInfo "
            + "VALUES ("
            + "\"" + model.Date + "\"" + ", "
            + "\"" + model.Target + "\"" + ", "
            + "\"" + model.Stand + "\"" + ", "
            + model.Price + ");";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = TryExecuteNonQuery(command);
        }

        int TryExecuteNonQuery(SQLiteCommand cmd)
        {
            int result = 0;

            while (true)
            {
                try
                {
                    result = cmd.ExecuteNonQuery();
                }
                catch (SQLiteException sqe)
                {
                    if (sqe.ErrorCode != 5) throw sqe;
                    System.Threading.Thread.Sleep(10);
                }

                break;
            }

            return result;
        }

        SQLiteDataReader TryExecuteReader(SQLiteCommand cmd)
        {
            SQLiteDataReader result = null;

            while (true)
            {
                try
                {
                    result = cmd.ExecuteReader();
                }
                catch (SQLiteException sqe)
                {
                    if (sqe.ErrorCode != 5) throw sqe;
                    System.Threading.Thread.Sleep(10);
                }

                break;
            }

            return result;
        }

        class SQLiteAsync
        {
            public virtual Task<int> ExecuteNonQueryAsync(SQLiteCommand cmd)
            {
                return Task.FromResult<int>(cmd.ExecuteNonQuery());
            }

            public virtual Task<SQLiteDataReader> ExecuteReaderAsync(SQLiteCommand cmd)
            {
                return Task.FromResult<SQLiteDataReader>(cmd.ExecuteReader());
            }
        }

        public IEnumerable<CurrencyExchangeModel> select()
        {
            List<CurrencyExchangeModel> model = new List<CurrencyExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM CryptoExchange ORDER BY Percent DESC;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = TryExecuteReader(command);

            while (rdr.Read())
            {
                try
                {
                    string Target = ((string)rdr["Target_Coin"]);
                    string Stand = ((string)rdr["Stand_Coin"]);
                    string MaxNation = ((string)rdr["MaxNation"]);
                    string MaxMarketName = (string)rdr["MaxMarketName"];
                    string MinNation = ((string)rdr["MinNation"]);
                    string MinMarketName = (string)rdr["MinMarketName"];
                    decimal MaxPrice = (decimal)(double)rdr["MaxPrice"];    //DB에 Decimal로 지정했는데 반환은 Double로 옴. 머선????
                    decimal MinPrice = (decimal)(double)rdr["MinPrice"];
                    double Percent = (double)rdr["Percent"];
                    //string Date = (string)rdr["Date"];

                    model.Add(new CurrencyExchangeModel(Target, Stand, MaxNation, MaxMarketName, MinNation, MinMarketName, MaxPrice, MinPrice, Percent));
                }
                catch (InvalidCastException e) { }
            }

            return model;
        }

        public IEnumerable<CurrencyExchangeModel> select2()
        {
            List<CurrencyExchangeModel> model = new List<CurrencyExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM CurrencyExchange ORDER BY Percent DESC;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = TryExecuteReader(command);

            while (rdr.Read())
            {
                try
                {
                    string Target = (string)rdr["Target_Coin"];
                    string MaxStand = (string)rdr["MaxStand_Coin"];
                    string MinStand = (string)rdr["MinStand_Coin"];
                    string MaxNation = (string)rdr["MaxNation"];
                    string MaxMarket = (string)rdr["MaxMarketName"];
                    string MinNation = (string)rdr["MinNation"];
                    string MinMarket = (string)rdr["MinMarketName"];
                    decimal MaxPrice = (decimal)rdr["MaxPrice"];    //안니;;; 얘는 Decimal로 반환됨;;
                    decimal MinPrice = (decimal)rdr["MinPrice"];
                    double Percent = (double)rdr["Percent"];

                    model.Add(new CurrencyExchangeModel(Target, MaxStand, MinStand, MaxNation, MaxMarket, MinNation, MinMarket, MaxPrice, MinPrice, Percent));
                }
                catch (Exception e) { }
            }
            return model;
        }

        public async Task<IEnumerable<CryptoExchangeModel>> SelectTest2()
        {
            List<CryptoExchangeModel> model = new List<CryptoExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM CryptoExchange WHERE Percent < 30 ORDER BY Percent DESC LIMIT 20;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = await sqAsync.ExecuteReaderAsync(command);

            while (rdr.Read())
            {
                string[] Coins = ((string)rdr["Coins"]).Split('-');
                string Target = Coins[0];
                string Stand = Coins[1];
                string MaxMarketName = (string)rdr["MaxMarketName"];
                string MinMarketName = (string)rdr["MinMarketName"];
                decimal MaxPrice = (decimal)rdr["MaxPrice"];
                decimal MinPrice = (decimal)rdr["MinPrice"];
                double Percent = (double)rdr["Percent"];
                //string Date = (string)rdr["Date"];

                model.Add(new CryptoExchangeModel(DateTime.Now, Target, Stand, MaxMarketName, MinMarketName, Percent));
            }

            return model;
        }

        private void Conn_Update(object sender, UpdateEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void AddBan(int Type, string BanName)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = $"INSERT INTO BlackList VALUES (\"{BanName}\", {Type});";

            SQLiteCommand command = new SQLiteCommand(sql, conn);

            try
            { int result = command.ExecuteNonQuery(); }
            catch(SQLiteException sqe)
            { if(sqe.ErrorCode != 19) throw sqe; }
        }

        public void SubBan(int Type, string Name)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = $"DELETE FROM BlackList WHERE Name={Name} AND Type={Type};";

            SQLiteCommand command = new SQLiteCommand(sql, conn);

            try
            { int result = command.ExecuteNonQuery(); }
            catch (SQLiteException sqe)
            { }

        }
    }
}
