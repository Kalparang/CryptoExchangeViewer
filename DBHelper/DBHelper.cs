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
        public DBHelper()
        {
            string DBLocation = Environment.CurrentDirectory + @"\CryptoExchange.sqlite";

            SQLiteConnection.CreateFile(DBLocation);
            SQLiteConnection conn = new SQLiteConnection(@"Data Source=" + DBLocation);

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
        }
    }
}
