using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.OleDb;

// Adapted From https://www.c-sharpcorner.com/article/read-microsoft-access-database-in-C-Sharp/

namespace BalanceManagement
{
	delegate void KeyPressDelegate(ConsoleKeyInfo key);
	delegate void HintDelegate();
	delegate void ReferredFunctionDelegate(int xCoord, int yCoord);

	class Program
	{			
		// Connection string and SQL query  
		static string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\yisha\source\repos\BalanceManagement\BalanceManagement\BalanceManagement.mdb";
		static OleDbConnection connection;
		static OleDbCommand command;

		public static List<List<TableElement>> rawData;

		public const short Columnnumber = 4;

		static Dictionary<Month, int> Cost_Month_Mapping;

		static KeyPressDelegate KeyPressHandler;
		static HintDelegate HintHandler;
		static ReferredFunctionDelegate ReferedFunctionHandler;

		static void Main(string[] args)
		{
			if (initialization())
			{
				while (true)
				{
					ExecuteCommand(KeyPressHandler, HintHandler);
				}
			}

			Finalization();
		}

		static bool initialization()
		{
			try
			{
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

				KeyPressHandler = MainOption;
				HintHandler = Hint.MainOption;

				Console.WriteLine("--Initialization SUCCESSFUL--\n");
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
			connection.Close();
		}

		static bool RetrieveData()
		{
			try
			{
				command.CommandText = "SELECT * FROM BalanceManagement";

				using (OleDbDataReader reader = command.ExecuteReader())
				{

					rawData = new List<List<TableElement>>();

					for (int i = 0; i < Columnnumber; i++)
					{
						rawData.Add(new List<TableElement>());
						rawData[i].Add(new TableElement(""));
					}

					rawData[0][0].Content = "ItemName";
					rawData[1][0].Content = "Cost";
					rawData[2][0].Content = "Date";
					rawData[3][0].Content = "Comment";

					int j = 1;

					while (reader.Read())
					{
						for (int i = 0; i < Columnnumber; i++)
						{
							rawData[i].Add(new TableElement(""));
						}

						rawData[0][j].Content = reader["ItemName"].ToString();
						rawData[1][j].Content = reader["Cost"].ToString();
						rawData[2][j].Content = reader["PurchaseDate"].ToString();
						rawData[3][j].Content = reader["Comment"].ToString();

						j++;
					}
				}

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

		static TableElement[,] FormatRawData()
		{
			TableElement[,] formattedData = new TableElement[Columnnumber, rawData[0].Count];

			for (int j = 0; j < rawData[0].Count; j++)
			{
				for (int i = 0; i < Columnnumber; i++)
				{
					formattedData[i, j] = new TableElement("");
				}
			}

			for (int j = 0; j < rawData[0].Count; j++)
			{
				formattedData[0, j].Content = string.Format("{0, -20}", rawData[0][j].Content);
				formattedData[1, j].Content = string.Format("{0, 7}", rawData[1][j].Content);
				formattedData[2, j].Content = string.Format("{0, 10}", rawData[2][j].Content);
				formattedData[3, j] = rawData[3][j];
			}

			return formattedData;
		}

		static void PrintData(TableElement[,] inputData)
		{
			for (int j = 0; j < inputData.GetLength(1); j++)
			{
				for (int i = 0; i < inputData.GetLength(0); i++)
				{
					Console.ForegroundColor = inputData[i, j].ContentColor;
					Console.BackgroundColor = inputData[i, j].BackgroundColor;
					Console.Write(inputData[i, j].Content + "    ");

					Console.ForegroundColor = ConsoleColor.Black;
					Console.BackgroundColor = ConsoleColor.White;
				}
				Console.WriteLine();
			}
		}

		static void AddData()
		{
			string[] tempString = new string[Columnnumber];
			string[] tempInputString = Console.ReadLine().Split(' ');

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

					AddData();
					return;
				}

				if (tempString[2] == "")
				{
					tempString[2] = DateTime.Now.ToShortDateString();
				}
			}

			command.CommandText = string.Format("INSERT INTO BalanceManagement(ItemName,Cost,PurchaseDate,Comment)Values('{0}','{1}','{2}','{3}')", tempString[0], tempString[1], tempString[2], tempString[3]);

			command.ExecuteNonQuery();
		}

		static void MainOption(ConsoleKeyInfo key)
		{
			switch (key.Key)
			{
				case ConsoleKey.D1:
					Console.Clear();
					if (RetrieveData())
					{
						PrintData(FormatRawData());
					}
					break;

				case ConsoleKey.D2:
					HintHandler = Hint.Modify;
					KeyPressHandler = ModifyData;
					return;

				case ConsoleKey.D:
					PrintData(Disguise());
					return;

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

		static void UpdateData(int xCoord, int yCoord)
		{
			command.CommandText = "UPDATE Developer SET Name = 'Updated Name' WHERE Name = 'New Developer'";

			command.ExecuteNonQuery();
		}

		static void ModifyData(ConsoleKeyInfo key)
		{
			// Console.CursorLeft--;

			switch (key.Key)
			{
				case ConsoleKey.D1:
					// Add Data
					Hint.AddData();
					AddData();
					return;

				case ConsoleKey.D2:
					// Update Data
					HintHandler = Hint.UpdateData;					
					ReferedFunctionHandler = UpdateData;
					KeyPressHandler = MoveCursor;
					return;

				case ConsoleKey.D3:
					// Delete Data
					HintHandler = Hint.DeleteData;
					ReferedFunctionHandler = DeleteData;
					KeyPressHandler = MoveCursor;
					return;

				case ConsoleKey.Escape:
				case ConsoleKey.Backspace:
				case ConsoleKey.Delete:
					// Go Back
					HintHandler = Hint.MainOption;
					KeyPressHandler = MainOption;
					break;

				default:
					break;
			}
		}

		static void GroupInMonth()
		{
			
		}

		static void DrawDiagram()
		{
			
		}

		static void DeleteData(int xCoord, int yCoord)
		{
			command.CommandText = "DELETE FROM BalanceManagement WHERE Name = 'Updated Name'";
			
			command.ExecuteNonQuery();
		}

		static void MoveCursor(ConsoleKeyInfo key)
		{
			switch (key.Key)
			{
				case ConsoleKey.UpArrow:
				case ConsoleKey.W:
					Cursor.YCoord--;
					break;

				case ConsoleKey.DownArrow:
				case ConsoleKey.S:
					Cursor.YCoord++;
					break;

				case ConsoleKey.LeftArrow:
				case ConsoleKey.A:
					Cursor.XCoord--;
					break;

				case ConsoleKey.RightArrow:
				case ConsoleKey.D:
					Cursor.XCoord++;
					break;

				case ConsoleKey.Enter:
				case ConsoleKey.Spacebar:
					ReferedFunctionHandler(Console.CursorLeft, Console.CursorTop);
					break;

				case ConsoleKey.Escape:
				case ConsoleKey.Backspace:
				case ConsoleKey.Delete:
					ReferedFunctionHandler(Console.CursorLeft, Console.CursorTop);
					break;

				default:
					break;
			}

			PrintData(FormatRawData());
		}

		static TableElement[,] Disguise()
		{
			TableElement[,] fakeData = new TableElement[Columnnumber,5];

			// Initialize fakeData Here

			return fakeData;
		}

		static void ExecuteCommand(KeyPressDelegate keyPressHandler, HintDelegate hintHandler)
		{
			Thread.Sleep(250);

			hintHandler();

			keyPressHandler(Console.ReadKey());

			try
			{
				Console.CursorLeft--;
			}
			catch { }
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
			Console.WriteLine("      \"Esc\" to go back tO last menu\n");
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
			Console.ForegroundColor = ConsoleColor.Black;
			Console.WriteLine("ItemName                   Cost          Date    Comment");
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
		static int xCoord = 0;
		public static int XCoord
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

		static int yCoord = 0;
		public static int YCoord
		{
			get
			{
				return yCoord;
			}

			set
			{
				if ((value < Program.rawData[0].Count) && (value >= 0))
				{
					xCoord = value;
				}
			}
		}
	}
}
