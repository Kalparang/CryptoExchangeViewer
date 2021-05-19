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
            public CryptoInputModel(string Target, string Stand, double Price, string Market, DateTime Date)
            {
                this.Target = Target.ToUpper();
                this.Stand = Stand.ToUpper();
                this.Price = Price;
                this.Market = Market;
                this.Date = Date;
            }

            public string Target;
            public string Stand;
            public double Price;
            public string Market;
            public DateTime Date;
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
            public ExchangeInputModel(DateTime Date, string Target, string Stand, double Price)
            {
                this.Date = Date;
                this.Target = Target.ToUpper();
                this.Stand = Stand.ToUpper();
                this.Price = Price;
            }

            public DateTime Date;
            public string Target;
            public string Stand;
            public double Price;
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
            `Price` DOUBLE NOT NULL,
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
              `Price` DOUBLE NOT NULL);


            -- -----------------------------------------------------
            -- Table `BlackList`
            -- -----------------------------------------------------
            CREATE TABLE IF NOT EXISTS `BlackList` (
              `Name` VARCHAR(45) NOT NULL,
              `Type` INT NOT NULL,
              PRIMARY KEY (`Name`));

            CREATE VIEW IF NOT EXISTS LastDatas AS
            SELECT * FROM
            (SELECT ROWID, MAX(date) AS Date, target_coin, stand_coin, Price, marketlist_maketname AS MarketName
            FROM marketdetail GROUP BY target_coin, stand_coin, MarketName);
            
            CREATE VIEW IF NOT EXISTS MinPrices AS
            SELECT ROWID, Date, target_coin || '-' || stand_coin AS Coins, MIN(Price) AS Price, MarketName
            FROM LastDatas GROUP BY Coins;

            CREATE VIEW IF NOT EXISTS MaxPrices AS
            SELECT ROWID, Date, target_coin || '-' || stand_coin AS Coins, MAX(Price) AS Price, MarketName
            FROM LastDatas GROUP BY Coins;

            CREATE VIEW IF NOT EXISTS CryptoExchange AS
            SELECT MaxPrices.ROWID AS MaxRowID, MinPrices.ROWID as MinRowID, MaxPrices.Date AS Date,
            MaxPrices.Coins as Coins, MaxPrices.MarketName AS MaxMarketName, MinPrices.MarketName AS MinMarketName,
            100 - (MinPrices.Price / MaxPrices.Price * 100) AS Percent
            FROM MaxPrices INNER JOIN MinPrices ON MaxPrices.Coins = MinPrices.Coins AND MaxPrices.MarketName != MinPrices.MarketName;

            CREATE VIEW IF NOT EXISTS LastExchangeInfo AS
            SELECT MAX(date), Target_Currency, Stand_Currency, Price
            FROM ExchangeInfo WHERE Stand_Currency == ""KRW""
            GROUP BY Target_Currency, Stand_Currency;

            CREATE VIEW IF NOT EXISTS CurrencyList AS
            SELECT * FROM (
            SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail WHERE Stand_Coin IN (
            SELECT DISTINCT(Target_Currency) FROM ExchangeInfo)
            GROUP BY Target_Coin, Stand_Coin, MarketName);

            CREATE VIEW IF NOT EXISTS KRWList AS
            SELECT a.Date, a.Target_Coin, a.Stand_Coin, a.Price, a.MarketName, a.Price * b.Price AS KRW
            FROM CurrencyList a INNER JOIN LastExchangeInfo b ON a.stand_coin = b.target_currency;

            CREATE VIEW IF NOT EXISTS MinKRWList AS
            SELECT Date, Target_Coin, Stand_Coin, Price, MarketName, MIN(KRW) AS KRW
            FROM KRWList GROUP BY Target_Coin;

            CREATE VIEW IF NOT EXISTS MaxKRWList AS
            SELECT Date, Target_Coin, Stand_Coin, Price, MarketName, MAX(KRW) AS KRW
            FROM KRWList GROUP BY Target_Coin;

            CREATE VIEW IF NOT EXISTS CurrencyExchange AS
            SELECT Min.Date, Min.Target_Coin, Max.Stand_Coin, Min.Stand_Coin, Max.MarketName AS MaxMarketName, Min.MarketName AS MinMarketName,
            100 - (Min.KRW / Max.KRW * 100) AS Percent
            FROM MinKRWList Min INNER JOIN MaxKRWList Max
            ON Min.Target_Coin=MAX.Target_Coin AND Min.MarketName!=Max.MarketName;

            INSERT OR REPLACE INTO ExchangeInfo VALUES (""2021-05-20 오전 00:00:00"", ""KRW"", ""KRW"", 1);
            ";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery();

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

            SQLiteDataReader rdr = command.ExecuteReader();

            if (rdr.HasRows)
                return;

            sql = @"
            INSERT INTO MarketList "
            + "VALUES ("
            + "\"" + model.Market + "\"" + ", " + "\"" + model.Nation + "\""
            + @");";
            
            command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery();
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
            int result = command.ExecuteNonQuery();
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
            int result = command.ExecuteNonQuery();
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


        public async Task<IEnumerable<CryptoInputModel>> SelectTest()
        {
            List<CryptoInputModel> model = new List<CryptoInputModel>();
            
            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM MarketDetail ORDER BY Date DESC LIMIT 100;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = await sqAsync.ExecuteReaderAsync(command);

            while(rdr.Read())
            {
                string Target = (string)rdr["Target_Coin"];
                string Stand = (string)rdr["Stand_Coin"];
                double price = (double)rdr["Price"];
                string Market = (string)rdr["MarketList_MaketName"];
                //string Date = (string)rdr["Date"];

                model.Add(new CryptoInputModel(Target, Stand, price, Market, DateTime.Now));
            }

            return model;
        }

        public IEnumerable<CryptoExchangeModel> select()
        {
            List<CryptoExchangeModel> model = new List<CryptoExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM CryptoExchange ORDER BY Percent DESC;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = command.ExecuteReader();

            while (rdr.Read())
            {
                string[] Coins = ((string)rdr["Coins"]).Split('-');
                string Target = Coins[0];
                string Stand = Coins[1];
                string MaxMarketName = (string)rdr["MaxMarketName"];
                string MinMarketName = (string)rdr["MinMarketName"];
                double Percent = (double)rdr["Percent"];
                //string Date = (string)rdr["Date"];

                model.Add(new CryptoExchangeModel(DateTime.Now, Target, Stand, MaxMarketName, MinMarketName, Percent));
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
    }
}
