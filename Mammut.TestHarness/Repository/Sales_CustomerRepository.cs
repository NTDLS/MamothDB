using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Mammut.Client;
using Mammut.Common.Payload.Model;

namespace Mammut.TestHarness.Repository
{
	public partial class Sales_CustomerRepository
	{        
		public void Export_Sales_Customer()
		{
            using (var client = new MammutClient("https://localhost:5001", "root", "p@ssWord!"))
			{

            //if(client.Schema.Exists("AdventureWorks2008R2:Sales:Customer"))
			//{
			//	return;
			//}

            client.Transaction.Enlist();

            client.Schema.CreateAll("AdventureWorks2008R2:Sales:Customer");

			using (SqlConnection connection = new SqlConnection("Server=.;Database=AdventureWorks2008R2;Trusted_Connection=True;"))
			{
				connection.Open();

				try
				{
					using (SqlCommand command = new SqlCommand("SELECT * FROM Sales.Customer", connection))
					{
						command.CommandTimeout = 10000;
						command.CommandType = System.Data.CommandType.Text;

						using (SqlDataReader dataReader = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
						{
                            int indexOfCustomerID = dataReader.GetOrdinal("CustomerID");
						    int indexOfPersonID = dataReader.GetOrdinal("PersonID");
						    int indexOfStoreID = dataReader.GetOrdinal("StoreID");
						    int indexOfTerritoryID = dataReader.GetOrdinal("TerritoryID");
						    int indexOfAccountNumber = dataReader.GetOrdinal("AccountNumber");
						    int indexOfrowguid = dataReader.GetOrdinal("rowguid");
						    int indexOfModifiedDate = dataReader.GetOrdinal("ModifiedDate");
						    
							int rowCount = 0;


							while (dataReader.Read() && rowCount < 1000 /*easy replace*/)
							{
								if(rowCount > 0 && (rowCount % 100) == 0)
								{
									Console.WriteLine("AdventureWorks2008R2:Sales:Customer: {0}", rowCount);
								}

								if(rowCount > 0 && (rowCount % 1000) == 0)
								{
									Console.WriteLine("Comitting...");
									client.Transaction.Commit();
									client.Transaction.Enlist();
								}

								try
								{
									client.Document.Create("AdventureWorks2008R2:Sales:Customer", new Models.Sales_Customer
									{
											CustomerID= dataReader.GetInt32(indexOfCustomerID),
											PersonID= dataReader.GetNullableInt32(indexOfPersonID),
											StoreID= dataReader.GetNullableInt32(indexOfStoreID),
											TerritoryID= dataReader.GetNullableInt32(indexOfTerritoryID),
											AccountNumber= dataReader.GetString(indexOfAccountNumber),
											rowguid= dataReader.GetGuid(indexOfrowguid),
											ModifiedDate= dataReader.GetDateTime(indexOfModifiedDate),
										});
								}
								catch(Exception ex)
								{
									Console.WriteLine(ex.Message);
								}
								
								rowCount++;
							}
						}
					}
					connection.Close();
				}
				catch
				{
					//TODO: add error handling/logging
					throw;
				}

				client.Transaction.Commit();
				}
            }
		}
	}
}

