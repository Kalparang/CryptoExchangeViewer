using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace DBHelper
{
    public class DBHelper
    {
        SQLiteConnection conn;

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
            public CryptoInputModel() { }
            public CryptoInputModel(string Target, string Stand, double Price, string Market, DateTime Date)
            {
                this.Target = Target;
                this.Stand = Stand;
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

        public class ExchangeInputModel
        {
            public ExchangeInputModel(DateTime Date, string Target, string Stand, double Price)
            {
                this.Date = Date;
                this.Target = Target;
                this.Stand = Stand;
                this.Price = Price;
            }

            public DateTime Date;
            public string Target;
            public string Stand;
            public double Price;
        }

        public DBHelper()
        {
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
    FOREIGN KEY (`MarketList_MaketName`)
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
";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery();

            conn.Disposed += Conn_Disposed;
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
    }
}
