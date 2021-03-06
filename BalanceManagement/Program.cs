﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Data.OleDb;
using TerpExpressDepositManagement;

// Adapted From https://www.c-sharpcorner.com/article/read-microsoft-access-database-in-C-Sharp/

namespace BalanceManagement
{
	delegate void KeyPressDelegate(ConsoleKeyInfo key);
	delegate void HintDelegate();
	delegate void ReferredFunctionDelegate(int xCoord, int yCoord);

	class Program
	{
		// Connection string and SQL query  
		public static string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\yisha\OneDrive\Documents\MyDataSource\BalanceManagement.mdb";
		public static OleDbConnection connection;
		public static OleDbCommand command;

		public static TableElement[,] FormattedData;

		public const short Columnnumber = 4;
		public static string TableName = "BalanceManagement";

		static Dictionary<int, double> Month_Cost_Mapping = new Dictionary<int, double>();

		static KeyPressDelegate KeyPressHandler = (ConsoleKeyInfo key) => { try { Console.CursorLeft--; } catch { } };
		static HintDelegate HintHandler;
		static ReferredFunctionDelegate ReferedFunctionHandler;

		//static Communicator<OleDbConnection, OleDbCommand> DataCommunicator;

		static Cursor SystemCursor { get; set; } = new Cursor();

		static void Main(string[] args)
		{
			//args = new string[1] { "[Subscription] [Spotify] StudentPlan" };
			if (initialization(args))
			{
				while (true)
				{
					ExecuteCommand(KeyPressHandler, HintHandler);
				}
			}

			Finalization();
		}

		static bool initialization(string[] args)
		{
			try
			{
				//ReadSubscriptionConfigurationFile();
				Console.BackgroundColor = ConsoleColor.White;
				Console.ForegroundColor = ConsoleColor.Black;

				// Create a connection
				connection = new OleDbConnection(connectionString);

				// Create a command and set its connection
				command = connection.CreateCommand();
				command.Connection = connection;

				// Open connecton
				connection.Open();

				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Connection Susseccfully Opened");
				Console.WriteLine(connectionString);

				KeyPressHandler += MainOption;
				HintHandler = Hint.MainOption;

				if (DataHelper.RetrieveData())
				{
					FormatRawData();

					SetUpCost_MonthMapping();

					if (args.Length != 0)
					{
						ExternalInvocationHandler(args);
					}

					Console.ForegroundColor = ConsoleColor.DarkGreen;
					Console.WriteLine("--Initialization SUCCESSFUL--\n");
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("--Initialization FAILED--");

					return false;
				}

				Console.ForegroundColor = ConsoleColor.Black;

				return true;
			}
			catch (Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("--Initialization FAILED--");

				Console.WriteLine(exception.Message + "\n");
				Console.ForegroundColor = ConsoleColor.Black;

				Console.WriteLine("Do you wish to continue?\nThe program may malfunction");
				Console.WriteLine("Press \"1\" to continue\n      \"2\" to exit");

				return false;
			}
		}

		static void Finalization()
		{
			connection?.Close();
		}

		static void FormatRawData()
		{
			FormattedData = new TableElement[Columnnumber, DataHelper.rawData[0].Count];

			for (int j = 0; j < DataHelper.rawData[0].Count; j++)
			{
				for (int i = 0; i < Columnnumber; i++)
				{
					FormattedData[i, j] = new TableElement("");

					FormattedData[i, j].ContentColor = ConsoleColor.Black;
					FormattedData[i, j].BackgroundColor = ConsoleColor.White;
				}
			}

			for (int j = 0; j < DataHelper.rawData[0].Count; j++)
			{
				FormattedData[0, j].Content = string.Format("{0, -20}", DataHelper.rawData[0][j]);
				FormattedData[1, j].Content = string.Format("{0, 7}", DataHelper.rawData[1][j]);
				FormattedData[2, j].Content = string.Format("{0, 10}", DataHelper.rawData[2][j]);
				FormattedData[3, j].Content = DataHelper.rawData[3][j];
			}
		}

		static void PrintData()
		{
			for (int j = 0; j < FormattedData.GetLength(1); j++)
			{
				for (int i = 0; i < FormattedData.GetLength(0); i++)
				{
					Console.ForegroundColor = FormattedData[i, j].ContentColor;
					Console.BackgroundColor = FormattedData[i, j].BackgroundColor;
					Console.Write(FormattedData[i, j].Content + "    ");

					Console.ForegroundColor = ConsoleColor.Black;
					Console.BackgroundColor = ConsoleColor.White;
				}
				Console.WriteLine();
			}

			Console.WriteLine("\n\n");

			DrawDiagram();

			Console.WriteLine($"TerpExpressDeposit: {GetTerpExpressDeposit()}");
		}
		
		static void MainOption(ConsoleKeyInfo key)
		{
			switch (key.Key)
			{
				case ConsoleKey.D1:
					Console.Clear();
					if (DataHelper.RetrieveData())
					{
						PrintData();
					}
					break;

				case ConsoleKey.D2:
					HintHandler = Hint.Modify;
					KeyPressHandler -= MainOption;
					KeyPressHandler += ModifyData;
					break;

				case ConsoleKey.D:
					DoCamouflage();
					//PrintData();
					break;

				case ConsoleKey.Escape:
				case ConsoleKey.Backspace:
				case ConsoleKey.Delete:
					Environment.Exit(0);
					break;

				default:
					Console.CursorTop--;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("\nIllegal Input, please check your input again");
					Console.ForegroundColor = ConsoleColor.Black;
					break;
			}
		}
		
		static void ModifyData(ConsoleKeyInfo key)
		{
			switch (key.Key)
			{
				case ConsoleKey.D1:
					// Add Data
					Hint.AddData();
					DataHelper.Add();
					break;

				case ConsoleKey.D2:
					// Update Data
					Hint.UpdateData();
					HintHandler = null;
					ReferedFunctionHandler = DataHelper.Update;
					Console.CursorTop -= FormattedData.GetLength(1);
					KeyPressHandler -= ModifyData;
					KeyPressHandler += MoveCursor;
					break;

				case ConsoleKey.D3:
					// Delete Data
					Hint.DeleteData();
					HintHandler = null;
					ReferedFunctionHandler = DataHelper.Delete;
					Console.CursorTop -= FormattedData.GetLength(1) - 6;
					KeyPressHandler -= ModifyData;
					KeyPressHandler += MoveCursor;
					break;

				case ConsoleKey.Escape:
				case ConsoleKey.Backspace:
				case ConsoleKey.Delete:
					// Go Back
					HintHandler = Hint.MainOption;
					KeyPressHandler -= ModifyData;
					KeyPressHandler += MainOption;
					return;

				default:
					return;
			}

			PrintData();
		}

		static void DrawDiagram()
		{
			Console.ForegroundColor = ConsoleColor.Blue;

			for (int j = 1; j <= 12; j++)
			{
				Console.Write(j + "\t");

				int bar = (int)Month_Cost_Mapping[j] / 2;
				bool hasRemainder = (int)Month_Cost_Mapping[j] % 2 > 0;

				for (int i = 0; i < bar; i++)
				{
					Console.Write('█');
				}
				if (hasRemainder)
				{
					Console.Write('▌');
				}
				else if (bar == 0)
				{
					Console.Write('▬');
				}

				Console.WriteLine("    {0}", Month_Cost_Mapping[j]);
			}

			Console.ForegroundColor = ConsoleColor.Black;
		}

		static void MoveCursor(ConsoleKeyInfo key)
		{
			FormattedData[SystemCursor.XCoord, SystemCursor.YCoord].ContentColor = ConsoleColor.Black;
			FormattedData[SystemCursor.XCoord, SystemCursor.YCoord].BackgroundColor = ConsoleColor.White;

			if (ReferedFunctionHandler == DataHelper.Delete)
			{
				for (int i = 0; i < Columnnumber; i++)
				{
					FormattedData[i, SystemCursor.YCoord].ContentColor = ConsoleColor.Black;
					FormattedData[i, SystemCursor.YCoord].BackgroundColor = ConsoleColor.White;
				}
			}

			switch (key.Key)
			{
				case ConsoleKey.UpArrow:
				case ConsoleKey.W:
				case ConsoleKey.J:
					SystemCursor.YCoord--;
					break;

				case ConsoleKey.DownArrow:
				case ConsoleKey.S:
				case ConsoleKey.K:
					SystemCursor.YCoord++;
					break;

				case ConsoleKey.LeftArrow:
				case ConsoleKey.A:
				case ConsoleKey.H:

					if (ReferedFunctionHandler != DataHelper.Delete)
					{
						SystemCursor.XCoord--;
					}
					break;

				case ConsoleKey.RightArrow:
				case ConsoleKey.D:
				case ConsoleKey.L:

					if (ReferedFunctionHandler != DataHelper.Delete)
					{
						SystemCursor.XCoord++;
					}
					break;

				case ConsoleKey.Enter:
				case ConsoleKey.Spacebar:
					ReferedFunctionHandler(SystemCursor.XCoord, SystemCursor.YCoord);
					break;

				case ConsoleKey.Escape:
				case ConsoleKey.Backspace:
				case ConsoleKey.Delete:
					ReferedFunctionHandler = null;
					HintHandler = Hint.Modify;
					KeyPressHandler -= MoveCursor;
					KeyPressHandler += ModifyData;
					return;

				default:
					return;
			}

			FormattedData[SystemCursor.XCoord, SystemCursor.YCoord].ContentColor = ConsoleColor.White;
			FormattedData[SystemCursor.XCoord, SystemCursor.YCoord].BackgroundColor = ConsoleColor.DarkBlue;

			if (ReferedFunctionHandler == DataHelper.Delete)
			{
				for (int i = 0; i < Columnnumber; i++)
				{
					FormattedData[i, SystemCursor.YCoord].ContentColor = ConsoleColor.White;
					FormattedData[i, SystemCursor.YCoord].BackgroundColor = ConsoleColor.DarkBlue;
				}
			}
			Console.CursorLeft = 0;
			Console.CursorTop -= FormattedData.GetLength(1);
			PrintData();
		}

		static void DoCamouflage()
		{
			TableName = "Camouflage";
		}

		static void ExecuteCommand(KeyPressDelegate keyPressHandler, HintDelegate hintHandler)
		{
			try
			{
				hintHandler();
			}
			catch { }

			keyPressHandler(Console.ReadKey());
		}

		#region Subscription Module

		static void ExternalInvocationHandler(string[] args)
		{
			string prefix = args[0].Split(' ')[0];

			switch (prefix)
			{
				case "[Subscription]":
					SubscriptionEventHandler(args[0]);
					break;

				default:
					break;
			}
		}

		static void SubscriptionEventHandler(string subscriptionInfo)
		{
			string[] subscriptionInfoSplit = subscriptionInfo.Split(' ');

			string itemName = string.Format("{0} {1}", subscriptionInfoSplit[1], subscriptionInfoSplit[2]);

			string[,] dataMap = ReadSubscriptionConfigurationFile();

			for (int j = 0; j < dataMap.GetLength(1); j++)
			{
				if (subscriptionInfoSplit[1].Equals(dataMap[0, j]))
				{
					// Assume the external invocation executes monthly
					string date = DateTime.Now.Month.ToString() + "/27/" + DateTime.Now.Year;

					DataHelper.Add(string.Format("{0},{1},{2},Auto Executed Deduction", itemName, dataMap[1, j], date));
				}
			}
		}

		static string[,] ReadSubscriptionConfigurationFile(string address = "")
		{
			address = address != "" ? address : @"C:\Users\yisha\source\repos\BalanceManagement\BalanceManagement\Subscription\Subscription Configuration.txt";

			using (StreamReader reader = new StreamReader(address))
			{
				List<string> content = new List<string>();

				while (!reader.EndOfStream)
				{
					content.Add(reader.ReadLine());
				}

				string[,] dataMap = new string[content[0].Split(' ').GetLength(0), content.Count];

				for (int j = 0; j < dataMap.GetLength(1); j++)
				{
					string[] currentRow = content[j].Split(' ');

					for (int i = 0; i < dataMap.GetLength(0); i++)
					{
						dataMap[i, j] = currentRow[i];
					}
				}

				return dataMap;
			}
		}

		#endregion

		static string InputText()
		{
			Console.CursorLeft = 0;
			return (Console.ReadLine());
		}

		static void SetUpCost_MonthMapping()
		{
			try
			{
				for (int i = 1; i <= 12; i++)
				{
					Month_Cost_Mapping.Add(i, 0);
				}

				for (int i = 1; i < DataHelper.rawData[0].Count; i++)
				{
					Month_Cost_Mapping[DateTime.Parse(DataHelper.rawData[2][i]).Month] += double.Parse(DataHelper.rawData[1][i]);
				}
				return;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.Black;
			}
		}

		public static double GetTerpExpressDeposit()
		{
			return new TXDeposit(DataHelper.rawData).Deposit;
		}

		static class DataHelper
		{
			public static List<List<string>> rawData { get; set; }

			public static void Add(string data = "")
			{
				string[] tempString = new string[Columnnumber];

				string[] tempInputString;

				if (data == "")
				{
					tempInputString = Console.ReadLine().Split(',');
				}
				else
				{
					tempInputString = data.Split(',');
				}

				try
				{
					for (int i = 0; i < Columnnumber; i++)
					{
						tempString[i] = "";
					}

					for (int i = 0; i < Columnnumber; i++)
					{
						tempString[i] = tempInputString[i];
					}
				}
				catch { }

				for (int i = 0; i <= 1; i++)
				{
					if (tempString[i] == "")
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Some required field is empty\n");
						Console.ForegroundColor = ConsoleColor.Black;

						Add();
						return;
					}

					if (tempString[2] == "")
					{
						tempString[2] = DateTime.Now.ToShortDateString();
					}
				}

				command.CommandText = string.Format(
					"INSERT INTO {4}(ItemName,Cost,PurchaseDate,Comment)Values('{0}','{1}','{2}','{3}')"
					, tempString[0], tempString[1], tempString[2], tempString[3], TableName);

				command.ExecuteNonQuery();
			}

			public static void Update(int xCoord, int yCoord)
			{
				Console.CursorLeft = 0;
				Console.CursorTop++;

				Console.ForegroundColor = ConsoleColor.Blue;
				Console.Write("Enter Text Here >>");
				Console.ForegroundColor = ConsoleColor.Black;

				string inputField = Console.ReadLine();

				command.CommandText = string.Format("UPDATE {3} SET {0} = '{1}' WHERE {0} = '{2}'",
					rawData[xCoord][0], inputField, rawData[xCoord][yCoord], TableName);

				command.ExecuteNonQuery();
			}

			public static void Delete(int xCoord, int yCoord)
			{
				command.CommandText = string.Format("DELETE FROM {0} WHERE Name = 'Updated Name'", TableName);

				command.ExecuteNonQuery();
			}


			public static bool RetrieveData()
			{
				try
				{
					command.CommandText = string.Format("SELECT * FROM {0}", TableName);

					using (OleDbDataReader reader = command.ExecuteReader())
					{
						rawData = new List<List<string>>();

						for (int i = 0; i < Columnnumber; i++)
						{
							rawData.Add(new List<string>());
							rawData[i].Add("");
						}

						rawData[0][0] = "ItemName";
						rawData[1][0] = "Cost";
						rawData[2][0] = "Date";
						rawData[3][0] = "Comment";

						int j = 1;

						while (reader.Read())
						{
							for (int i = 0; i < Columnnumber; i++)
							{
								rawData[i].Add("");
							}

							rawData[0][j] = reader["ItemName"].ToString();
							rawData[1][j] = reader["Cost"].ToString();
							rawData[2][j] = reader["PurchaseDate"].ToString();
							rawData[3][j] = reader["Comment"].ToString();

							j++;
						}
					}

					FormatRawData();

					Console.ForegroundColor = ConsoleColor.DarkGreen;
					Console.WriteLine("\n--Retrieved data successfully--\n");
					Console.ForegroundColor = ConsoleColor.Black;

					return true;
				}
				catch (IndexOutOfRangeException ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("\n--FAILED to retrieve data--\n");
					Console.WriteLine(ex.Message);
					Console.WriteLine("\nPlease check and make sure your database field name is not changed");
					Console.ForegroundColor = ConsoleColor.Black;
				}
				catch (OleDbException ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("\n--FAILED to retrieve data--\n");
					Console.WriteLine(ex.Message);
					Console.WriteLine("\nPlease check and make sure your SQL Syntax is correct");
					Console.ForegroundColor = ConsoleColor.Black;
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("\n--FAILED to retrieve data--\n");
					Console.WriteLine(ex.Message);
					Console.WriteLine("\nUnknown Error");
					Console.ForegroundColor = ConsoleColor.Black;
				}

				return false;
			}
		}
	}

	enum Month
	{
		January  , Febuary, March   , April   , 
		May      , June   , July    , August  , 
		September, October, November, December		
	}

	class Hint
	{
		public static void MainOption()
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("What do you want to do next? ");
			Console.WriteLine("Press\"1\" to Retrieve Data");
			Console.WriteLine("     \"2\" to modify data");
			Console.WriteLine("     \"Esc\" to exit\n\n");
			Console.ForegroundColor = ConsoleColor.Black;
		}

		public static void Modify()
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("What do you wish to do? ");
			Console.WriteLine("Press \"1\" to ADD new data row");
			Console.WriteLine("      \"2\" to MODIFY existing data rows");
			Console.WriteLine("      \"3\" to DELETE existing data rows");
			Console.WriteLine("      \"Esc\" to go back to last menu\n");
			Console.ForegroundColor = ConsoleColor.Black;
		}

		public static void AddData()
		{
			try
			{
				Console.CursorLeft--;
			}
			catch{ }

			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("Please enter data in the following format: ");
			Console.WriteLine("NOTE: NO SPACE AFTER COMMA");
			Console.ForegroundColor = ConsoleColor.Black;
			Console.WriteLine("ItemName,Cost,Date,Comment");
		}

		public static void UpdateData()
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("Press keyboard \"Up\" to move cursor UP");
			Console.WriteLine("               \"Down\" to move mouse DOWN");
			Console.WriteLine("               \"Left\" to move cursor LEFT");
			Console.WriteLine("               \"Right\" to move cursor RIGHT\n");
			Console.WriteLine("Press \"Enter\" to select");
			Console.ForegroundColor = ConsoleColor.Black;
		}

		public static void DeleteData()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Important notice: This action is NOT REDOABLE");
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("Press keyboard \"Up\" to move cursor UP");
			Console.WriteLine("               \"Down\" to move cursor down");
			Console.WriteLine("Press \"Enter\" to select");
			Console.ForegroundColor = ConsoleColor.Black;
		}
	}

	class TableElement
	{
		public TableElement(string content)
		{
			Content = content;
		}

		public string Content { get; set; }

		public ConsoleColor ContentColor { get; set; } = ConsoleColor.Black;

		public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.White;
	}

	class Cursor
	{
		int xCoord = 0;
		public int XCoord
		{
			get
			{
				return xCoord;
			}

			set
			{
				if ((value < Program.Columnnumber) && (value >= 0))
				{
					xCoord = value;
				}
			}
		}

		int yCoord = 1;
		public int YCoord
		{
			get
			{
				return yCoord;
			}

			set
			{
				if ((value < Program.FormattedData.GetLength(1)) && (value > 0))
				{
					yCoord = value;					
				}
			}
		}
	}
}
