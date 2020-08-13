using Domino_Queue_Handler.Model;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domino_Queue_Handler.Class
{
	public class DBCom : MainWindow
    {
		public ScannerData DBGet(string ScanData)
		{
			using (FbConnection connection = new FbConnection(@"Server=localhost;User=SYSDBA;Password=masterkey;Database=C:\var\db\UTF-8\IKEA.fdb"))
            {
				connection.Open();
				ScannerData queueProduct = new ScannerData();
				using (var transaction = connection.BeginTransaction())
				{
					using (var command = new FbCommand("SELECT * FROM TBLWARE WHERE BARCODEC = " + ScanData, connection, transaction))
					{
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								var values = new object[reader.FieldCount];
								reader.GetValues(values);

								queueProduct.ArticleNumber = values[0].ToString().Replace(" ", "");
								queueProduct.ArticleName = values[1].ToString();
								queueProduct.Supplier = values[11].ToString();
								queueProduct.EAN = values[41].ToString().Replace(" ", "");
								queueProduct.Quantity = values[44].ToString();
								queueProduct.GrossWeight = values[60].ToString();
								queueProduct.UnitLoadFootprint1 = values[66].ToString();
								queueProduct.UnitLoadFootprint2 = values[68].ToString();
								queueProduct.UnitLoadStackingCapacity = values[69].ToString();
							}
							if(queueProduct.EAN == null)
							{
								return null;
							}
							else
							{
								return queueProduct;
							}
						}
					}
					
				}
			}
		}
    }
}
