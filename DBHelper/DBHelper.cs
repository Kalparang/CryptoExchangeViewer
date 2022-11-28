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
        EventWaitHandle exitHandle;
        
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

        public class BlackListModel
        {
            public BlackListModel(int Type, string Name)
            {
                this.Type = Type;
                this.Name = Name;
            }

            public int Type;
            public string Name;
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
            public CurrencyExchangeModel(string target, string Stand, string nation, string market, decimal Price)
            {
                this.targetCrypto = target;
                this.maxStandCurrency = Stand;
                this.maxMarketNation = nation;
                this.maxMarketName = market;
                this.TargetPrice = Price;
            }

            public CurrencyExchangeModel(
                string target, string maxStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice,
                double percent, double oneMin, double fiveMin, double oneHour, double oneDay)
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
                this.OneMin = oneMin;
                this.FiveMin = fiveMin;
                this.OneHour = oneHour;
                this.OneDay = oneDay;
            }

            public CurrencyExchangeModel(string target, string maxStand, string minStand, string maxnation, string maxmarket, string minnation, string minmarket, decimal TargetPrice, decimal StandPrice,
                double percent, double oneMin, double fiveMin, double oneHour, double oneDay)
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
                this.OneMin = oneMin;
                this.FiveMin = fiveMin;
                this.OneHour = oneHour;
                this.OneDay = oneDay;
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
            public double OneMin;
            public double FiveMin;
            public double OneHour;
            public double OneDay;
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

        public DBHelper(EventWaitHandle exitHandle)
        {
            this.exitHandle = exitHandle;

            coinList = new Dictionary<string, CryptoExchangeModel>();

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

            DROP VIEW IF EXISTS BanMarketList;
            CREATE VIEW BanMarketList AS
            SELECT Name FROM BlackList WHERE Type=1;

            DROP VIEW IF EXISTS BanNationList;
            CREATE VIEW BanNationList AS
            SELECT Name FROM BlackList WHERE Type=2;

            DROP VIEW IF EXISTS LastDatas2;
            CREATE VIEW LastDatas2 AS
            SELECT A.Date, A.Target_Coin, A.Stand_Coin, A.Price, B.Nation, B.MarketName FROM
            (SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail GROUP BY Target_Coin, Stand_Coin, MarketName) A
            INNER JOIN MarketList B on A.MarketName=B.MarketName;

            DROP VIEW IF EXISTS LastDatas;
            CREATE VIEW LastDatas AS
            SELECT * FROM LastDatas2 WHERE NOT MarketName IN BanMarketList AND NOT Nation IN BanNationList;
            
            DROP VIEW IF EXISTS MinPrices;
            CREATE VIEW MinPrices AS
            SELECT Date, Target_Coin, Stand_Coin, MIN(Price) AS Price, Nation, MarketName
            FROM LastDatas GROUP BY Target_Coin, Stand_Coin;

            DROP VIEW IF EXISTS MaxPrices;
            CREATE VIEW MaxPrices AS
            SELECT Date, Target_Coin, Stand_Coin, MAX(Price) AS Price, Nation, MarketName
            FROM LastDatas GROUP BY Target_Coin, Stand_Coin;

            DROP VIEW IF EXISTS OneMinDatas2;
            CREATE VIEW OneMinDatas2 AS
            SELECT A.Date, A.Target_Coin, A.Stand_Coin, A.Price, B.Nation, B.MarketName
            FROM (SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail WHERE Date <= datetime('now', 'localtime', '-542 minute')
            GROUP BY Target_Coin, Stand_Coin, MarketName) A
            INNER JOIN MarketList B on A.MarketName=B.MarketName;

            DROP VIEW IF EXISTS OneMinDatas;
            CREATE VIEW OneMinDatas AS SELECT * FROM OneMinDatas2 WHERE NOT MarketName IN BanMarketList AND NOT Nation IN BanNationList;

            DROP VIEW IF EXISTS FiveMinDatas2;
            CREATE VIEW FiveMinDatas2 AS
            SELECT A.Date, A.Target_Coin, A.Stand_Coin, A.Price, B.Nation, B.MarketName
            FROM (SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail WHERE Date<=datetime('now', 'localtime', '-547 minute')
            GROUP BY Target_Coin, Stand_Coin, MarketName) A
            INNER JOIN MarketList B on A.MarketName=B.MarketName;

            DROP VIEW IF EXISTS FiveMinDatas;
            CREATE VIEW FiveMinDatas AS SELECT * FROM FiveMinDatas2 WHERE NOT MarketName IN BanMarketList AND NOT Nation IN BanNationList;

            DROP VIEW IF EXISTS OneHourDatas2;
            CREATE VIEW OneHourDatas2 AS
            SELECT A.Date, A.Target_Coin, A.Stand_Coin, A.Price, B.Nation, B.MarketName
            FROM (SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail WHERE Date <= datetime('now', 'localtime', '-10 hour')
            GROUP BY Target_Coin, Stand_Coin, MarketName) A
            INNER JOIN MarketList B on A.MarketName=B.MarketName;

            DROP VIEW IF EXISTS OneHourDatas;
            CREATE VIEW OneHourDatas AS SELECT * FROM OneHourDatas2 WHERE NOT MarketName IN BanMarketList AND NOT Nation IN BanNationList;

            DROP VIEW IF EXISTS OneDayDatas2;
            CREATE VIEW OneDayDatas2 AS
            SELECT A.Date, A.Target_Coin, A.Stand_Coin, A.Price, B.Nation, B.MarketName
            FROM (SELECT ROWID, MAX(date) AS Date, Target_Coin, Stand_Coin, Price, marketlist_maketname AS MarketName
            FROM MarketDetail WHERE Date <= datetime('now', 'localtime', '-33 hour')
            GROUP BY Target_Coin, Stand_Coin, MarketName) A
            INNER JOIN MarketList B on A.MarketName=B.MarketName;

            DROP VIEW IF EXISTS OneDayDatas;
            CREATE VIEW OneDayDatas AS SELECT * FROM OneDayDatas2 WHERE NOT MarketName IN BanMarketList AND NOT Nation IN BanNationList;

            DROP VIEW IF EXISTS CryptoExchange;
            CREATE VIEW CryptoExchange AS
            SELECT A.Date AS Date, A.Target_Coin, A.Stand_Coin, A.Nation AS MaxNation, A.MarketName AS MaxMarketName,
            B.Nation AS MinNation, B.MarketName AS MinMarketName, A.Price AS MaxPrice, B.Price AS MinPrice, 100 - (B.Price / A.Price * 100) AS Percent
            FROM MaxPrices A INNER JOIN MinPrices B
            ON A.Target_Coin = B.Target_Coin AND A.Stand_Coin = B.Stand_Coin AND A.MarketName != B.MarketName
            WHERE Percent > 0;

            DROP VIEW IF EXISTS CryptoExchangeWithTrends;
            CREATE VIEW CryptoExchangeWithTrends as select A.Date as date, A.Target_Coin, A.Stand_Coin, A.MaxNation, A.MaxMarketName, A.MinNation, A.MinMarketName, A.MaxPrice, A.MinPrice, A.Percent,
            CASE WHEN B1.Price > 0 and B2.Price > 0 then A.Percent - (100 - (B2.Price / B1.Price * 100)) else null end AS OneMin,
            case when C1.price > 0 and C2.price > 0 then A.Percent - (100 - (C2.Price / C1.Price * 100)) else null end AS FiveMin,
            case when D1.price > 0 and D2.price > 0 then A.Percent - (100 - (D2.Price / D1.Price * 100)) else null end AS OneHour,
            case when E1.price > 0 and E2.price > 0 then A.Percent - (100 - (E2.Price / E1.Price * 100)) else null end AS OneDay
            FROM cryptoexchange A
            left join OneMinDatas B1 on A.target_coin=B1.target_coin and A.stand_coin=B1.stand_coin and A.MaxMarketName=B1.MarketName
            left join OneMinDatas B2 on A.target_coin=B2.target_coin and A.stand_coin=B2.stand_coin and A.MinMarketName=B2.MarketName
            left join FiveMinDatas C1 on A.target_coin=C1.target_coin and A.stand_coin=C1.stand_coin and A.MaxMarketName=C1.MarketName
            left join FiveMinDatas C2 on A.target_coin=C2.target_coin and A.stand_coin=C2.stand_coin and A.MinMarketName=C2.MarketName
            left join OneHourDatas D1 on A.target_coin=D1.target_coin and A.stand_coin=D1.stand_coin and A.MaxMarketName=D1.MarketName
            left join OneHourDatas D2 on A.target_coin=D2.target_coin and A.stand_coin=D2.stand_coin and A.MinMarketName=D2.MarketName
            left join OneDayDatas E1 on A.target_coin=E1.target_coin and A.stand_coin=E1.stand_coin and A.MaxMarketName=E1.MarketName
            left join OneDayDatas E2 on A.target_coin=E2.target_coin and A.stand_coin=E2.stand_coin and A.MinMarketName=E2.MarketName;

            DROP VIEW IF EXISTS LastExchangeInfo;
            CREATE VIEW LastExchangeInfo AS
            SELECT MAX(date), Target_Currency, Stand_Currency, Price
            FROM ExchangeInfo WHERE Stand_Currency == 'KRW'
            GROUP BY Target_Currency, Stand_Currency;

            DROP VIEW IF EXISTS CurrencyList;
            CREATE VIEW CurrencyList AS
            SELECT * FROM LastDatas WHERE Stand_Coin IN (
            SELECT DISTINCT(Target_Currency) FROM ExchangeInfo)
            GROUP BY Target_Coin, Stand_Coin, MarketName;

            DROP VIEW IF EXISTS KRWList;
            CREATE VIEW KRWList AS
            SELECT a.Date, a.Target_Coin, a.Stand_Coin, a.Price, a.Nation, a.MarketName, a.Price * b.Price AS KRW
            FROM CurrencyList a INNER JOIN LastExchangeInfo b ON a.stand_coin = b.target_currency;

            DROP VIEW IF EXISTS MinKRWList;
            CREATE VIEW MinKRWList AS
            SELECT Date, Target_Coin, Stand_Coin, Price, Nation, MarketName, MIN(KRW) AS KRW
            FROM KRWList GROUP BY Target_Coin;

            DROP VIEW IF EXISTS MaxKRWList;
            CREATE VIEW MaxKRWList AS
             SELECT Date, Target_Coin, Stand_Coin, Price, Nation, MarketName, MAX(KRW) AS KRW
            FROM KRWList GROUP BY Target_Coin;

            DROP VIEW IF EXISTS OneMinKRWDatas;
            CREATE VIEW OneMinKRWDatas AS
            SELECT A.Date as CoinDate, max(B.Date) as ExchangeDate, A.Target_Coin, A.Stand_Coin, A.Price, A.marketname,
            A.Price * B.Price as krw from OneMinDatas A
            INNER JOIN (SELECT * FROM ExchangeInfo WHERE stand_currency=='KRW') B
            on A.stand_coin=B.target_Currency
            GROUP BY A.target_coin, A.stand_Coin, A.marketname;

            DROP VIEW IF EXISTS FiveMinKRWDatas;
            CREATE VIEW FiveMinKRWDatas AS
            SELECT A.Date as CoinDate, max(B.Date) as ExchangeDate, A.Target_Coin, A.Stand_Coin, A.Price, A.marketname,
            A.Price * B.Price as KRW FROM FiveMinDatas A
            INNER JOIN (SELECT * FROM exchangeinfo where stand_currency=='KRW') B
            ON A.stand_coin=B.target_Currency
            GROUP BY A.target_coin, A.stand_Coin, A.marketname;

            DROP VIEW IF EXISTS OneHourKRWDatas;
            CREATE VIEW OneHourKRWDatas AS
            select A.Date as CoinDate, max(B.Date) as ExchangeDate, A.Target_Coin, A.Stand_Coin, A.Price, A.marketname,
            A.Price * B.Price as krw from onehourdatas A
            INNER JOIN (SELECT * from exchangeinfo where stand_currency=='KRW') B
            ON A.stand_coin=B.target_Currency
            GROUP BY A.target_coin, A.stand_Coin, A.marketname;

            DROP VIEW IF EXISTS OneDayKRWDatas;
            CREATE VIEW OneDayKRWDatas AS
            SELECT A.Date as CoinDate, max(B.Date) as ExchangeDate, A.Target_Coin, A.Stand_Coin, A.Price, A.marketname,
            A.Price * B.Price as krw FROM OneDayDatas A
            INNER JOIN (SELECT * from exchangeinfo where stand_currency=='KRW') B
            ON A.stand_coin=B.target_Currency
            GROUP BY A.target_coin, A.stand_Coin, A.marketname;

            DROP VIEW IF EXISTS CurrencyExchange;
            CREATE VIEW CurrencyExchange AS
            SELECT Min.Date, Min.Target_Coin, Max.Stand_Coin AS MaxStand_Coin, Min.Stand_Coin AS MinStand_Coin,
            Max.Nation AS MaxNation, Max.MarketName AS MaxMarketName, Min.Nation AS MinNation, Min.MarketName AS MinMarketName,
            Max.KRW AS MaxPrice, Min.KRW AS MinPrice, 100 - (Min.KRW / Max.KRW * 100) AS Percent
            FROM MinKRWList Min INNER JOIN MaxKRWList Max
            ON Min.Target_Coin=MAX.Target_Coin AND Min.MarketName!=Max.MarketName
            WHERE Percent > 0;

            DROP VIEW IF EXISTS CurrencyExchangeWithTrends;
            CREATE VIEW CurrencyExchangeWithTrends AS
            SELECT A.Date as date, A.Target_Coin, A.MaxStand_Coin, A.MinStand_Coin, A.MaxNation, A.MaxMarketName,
            A.MinNation, A.MinMarketName, A.MaxPrice, A.MinPrice, A.Percent,
            case when B1.krw > 0 and B2.krw > 0 then A.Percent - (100 - (B2.krw / B1.krw * 100)) else null end AS OneMin,
            case when C1.krw > 0 and C2.krw > 0 then A.Percent - (100 - (C2.krw / C1.krw * 100)) else null end AS FiveMin,
            case when D1.krw > 0 and D2.krw > 0 then A.Percent - (100 - (D2.krw / D1.krw * 100)) else null end AS OneHour,
            case when E1.krw > 0 and E2.krw > 0 then A.Percent - (100 - (E2.krw / E1.krw * 100)) else null end AS OneDay
            FROM currencyexchange A
            left join OneMinkrwDatas B1 on A.target_coin=B1.target_coin and A.MaxStand_Coin=B1.stand_coin and A.MaxMarketName=B1.MarketName
            left join OneMinkrwDatas B2 on A.target_coin=B2.target_coin and A.MinStand_Coin=B2.stand_coin and A.MinMarketName=B2.MarketName
            left join FiveMinkrwDatas C1 on A.target_coin=C1.target_coin and A.MaxStand_Coin=C1.stand_coin and A.MaxMarketName=C1.MarketName
            left join FiveMinkrwDatas C2 on A.target_coin=C2.target_coin and A.MinStand_Coin=C2.stand_coin and A.MinMarketName=C2.MarketName
            left join OneHourkrwDatas D1 on A.target_coin=D1.target_coin and A.MaxStand_Coin=D1.stand_coin and A.MaxMarketName=D1.MarketName
            left join OneHourkrwDatas D2 on A.target_coin=D2.target_coin and A.MinStand_Coin=D2.stand_coin and A.MinMarketName=D2.MarketName
            left join OneDaykrwDatas E1 on A.target_coin=E1.target_coin and A.MaxStand_Coin=E1.stand_coin and A.MaxMarketName=E1.MarketName
            left join OneDaykrwDatas E2 on A.target_coin=E2.target_coin and A.MinStand_Coin=E2.stand_coin and A.MinMarketName=E2.MarketName;

            INSERT OR REPLACE INTO ExchangeInfo VALUES ('9999-12-31 23:59:59', 'KRW', 'KRW', 1);
            ";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = TryExecuteNonQuery(command);

            new Thread(new ThreadStart(ClearOldData)).Start();

            conn.Disposed += Conn_Disposed;
            conn.Update += Conn_Update1;
        }

        private void Conn_Update1(object sender, UpdateEventArgs e)
        {
            
        }

        private void Conn_Disposed(object sender, EventArgs e)
        {

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

            string date = model.Date.ToString("yyyy/MM/dd HH:mm:ss");

            if (model.Price == 0)
                return;

            string sql = @"
            INSERT INTO MarketDetail "
            + "VALUES ("
            + "\"" + date + "\"" + ", "
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

            string date = model.Date.ToString("yyyy/MM/dd HH:mm:ss");

            if (model.Price == 0)
                return;

            string sql = @"
            INSERT INTO ExchangeInfo "
            + "VALUES ("
            + "\"" + date + "\"" + ", "
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

        public IEnumerable<CurrencyExchangeModel> select()
        {
            List<CurrencyExchangeModel> model = new List<CurrencyExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM CryptoExchangeWithTrends ORDER BY Percent DESC;";

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
                    decimal MaxPrice = Convert.ToDecimal(rdr["MaxPrice"]);
                    decimal MinPrice = Convert.ToDecimal(rdr["MinPrice"]);
                    double Percent = Convert.ToDouble(rdr["Percent"]);
                    double OneMin = -111;
                    if (rdr["OneMin"].ToString() != "")
                        OneMin = Convert.ToDouble(rdr["OneMin"]);
                    double FiveMin = -111;
                    if (rdr["FiveMin"].ToString() != "")
                        FiveMin = Convert.ToDouble(rdr["FiveMin"]);
                    double OneHour = -111;
                    if (rdr["OneHour"].ToString() != "")
                        OneHour = Convert.ToDouble(rdr["OneHour"]);
                    double OneDay = -111;
                    if (rdr["OneDay"].ToString() != "")
                    {
                        OneDay = Convert.ToDouble(rdr["OneDay"]);
                    }
                    //string Date = (string)rdr["Date"];

                    model.Add(new CurrencyExchangeModel(Target, Stand, MaxNation, MaxMarketName, MinNation, MinMarketName, MaxPrice, MinPrice,
                        Percent, OneMin, FiveMin, OneHour, OneDay));
                }
                catch (InvalidCastException e)
                {
                    var ve = e;
                }
            }

            return model;
        }

        public IEnumerable<CurrencyExchangeModel> select2()
        {
            List<CurrencyExchangeModel> model = new List<CurrencyExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM CurrencyExchangeWithTrends ORDER BY Percent DESC;";

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
                    decimal MaxPrice = Convert.ToDecimal(rdr["MaxPrice"]);
                    decimal MinPrice = Convert.ToDecimal(rdr["MinPrice"]);
                    double Percent = Convert.ToDouble(rdr["Percent"]);
                    double OneMin = -111;
                    if (rdr["OneMin"].ToString() != "")
                        OneMin = Convert.ToDouble(rdr["OneMin"]);
                    double FiveMin = -111;
                    if (rdr["FiveMin"].ToString() != "")
                        FiveMin = Convert.ToDouble(rdr["FiveMin"]);
                    double OneHour = -111;
                    if (rdr["OneHour"].ToString() != "")
                        OneHour = Convert.ToDouble(rdr["OneHour"]);
                    double OneDay = -111;
                    if (rdr["OneDay"].ToString() != "")
                        OneDay = Convert.ToDouble(rdr["OneDay"]);

                    model.Add(new CurrencyExchangeModel(Target, MaxStand, MinStand, MaxNation, MaxMarket, MinNation, MinMarket, MaxPrice, MinPrice,
                        Percent, OneMin, FiveMin, OneHour, OneDay));
                }
                catch (Exception e)
                {
                    var ve = e;
                }
            }
            return model;
        }

        public List<ExchangeInputModel> SelectExchange()
        {
            List<ExchangeInputModel> model = new List<ExchangeInputModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT MAX(Date), Target_Currency, Stand_Currency, Price 
            FROM ExchangeInfo WHERE NOT Target_Currency=Stand_Currency GROUP BY Target_Currency, Stand_Currency;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = TryExecuteReader(command);

            while (rdr.Read())
            {
                try
                {
                    string Target = (string)rdr["Target_Currency"];
                    string Stand = (string)rdr["Stand_Currency"];
                    decimal Price = (decimal)rdr["Price"];

                    model.Add(new ExchangeInputModel(DateTime.Now, Target, Stand, Price));
                }
                catch (Exception e) { }
            }

            return model;
        }

        public List<CurrencyExchangeModel> SelectedList(string TargetCoin, string StandCoin)
        {
            List<CurrencyExchangeModel> model = new List<CurrencyExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = $"SELECT * FROM LastDatas WHERE Target_Coin=\"{TargetCoin}\" AND Stand_Coin=\"{StandCoin}\" ORDER BY Price DESC;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = TryExecuteReader(command);

            while (rdr.Read())
            {
                try
                {
                    string Target = ((string)rdr["Target_Coin"]);
                    string Stand = ((string)rdr["Stand_Coin"]);
                    string Nation = ((string)rdr["Nation"]);
                    string MarketName = (string)rdr["MarketName"];
                    decimal Price = (decimal)rdr["Price"];

                    model.Add(new CurrencyExchangeModel(Target, Stand, Nation, MarketName, Price));
                }
                catch (SQLiteException sqe) { }
            }

            return model;
        }

        public List<CurrencyExchangeModel> SelectedList(string TargetCoin)
        {
            List<CurrencyExchangeModel> model = new List<CurrencyExchangeModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = $"SELECT * FROM KRWList WHERE Target_Coin=\"{TargetCoin}\" ORDER BY KRW DESC;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = TryExecuteReader(command);

            while (rdr.Read())
            {
                try
                {
                    string Target = (string)rdr["Target_Coin"];
                    string Stand = (string)rdr["Stand_Coin"];
                    string Nation = (string)rdr["Nation"];
                    string MarketName = (string)rdr["MarketName"];
                    decimal Price = Convert.ToDecimal(rdr["KRW"]);
                    
                    model.Add(new CurrencyExchangeModel(Target, Stand, Nation, MarketName, Price));
                }
                catch (SQLiteException sqe) { }
            }

            return model;
        }


        public void AddBan(int Type, string BanName)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = $"INSERT INTO BlackList VALUES (\"{BanName}\", {Type});";

            SQLiteCommand command = new SQLiteCommand(sql, conn);

            new Thread(new ParameterizedThreadStart((tcommand) =>
            {
                SQLiteCommand ccomand = (SQLiteCommand)tcommand;
                try
                { int result = TryExecuteNonQuery(ccomand); }
                catch (SQLiteException sqe)
                { if (sqe.ErrorCode != 19) throw sqe; }
            })).Start(command);
            
        }

        public void SubBan(int Type, string Name)
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            string sql = $"DELETE FROM BlackList WHERE Name=\"{Name}\" AND Type={Type};";

            SQLiteCommand command = new SQLiteCommand(sql, conn);

            new Thread(new ParameterizedThreadStart((tcommand) =>
            {
                SQLiteCommand ccomand = (SQLiteCommand)tcommand;
                try
                { int result = TryExecuteNonQuery(ccomand); }
                catch (SQLiteException sqe)
                { if (sqe.ErrorCode != 19) throw sqe; }
            })).Start(command);
        }

        public List<BlackListModel> SelectBlackList()
        {
            List<BlackListModel> model = new List<BlackListModel>();

            if (conn.State != System.Data.ConnectionState.Open)
                return model;

            string sql = @"SELECT * FROM BlackList;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = TryExecuteReader(command);

            while (rdr.Read())
            {
                try
                {
                    int Type = (int)rdr["Type"];
                    string Name = (string)rdr["Name"];

                    model.Add(new BlackListModel(Type, Name));
                }
                catch (SQLiteException sqe)
                { }
            }

            return model;
        }

        private void ClearOldData()
        {
            if (conn.State != System.Data.ConnectionState.Open)
                return;

            while (true)
            {
                string sql = @"
                    DELETE FROM MarketDetail WHERE Date < datetime('now', 'localtime', '-34 hour');
                    DELETE FROM ExchangeInfo WHERE Date < datetime('now', 'localtime', '-34 hour');";

                SQLiteCommand command = new SQLiteCommand(sql, conn);
                int result = TryExecuteNonQuery(command);

                if (exitHandle.WaitOne(1000 * 60 * 60) == true)
                    break;
            }
        }
    }
}
