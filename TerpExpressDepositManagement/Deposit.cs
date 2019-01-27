using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerpExpressDepositManagement
{
	public class TXDeposit
	{
		public double Deposit { get; private set; }

		private static List<List<string>> data;

		public TXDeposit(List<List<string>> tempData)
		{
			data = tempData;
			UpdateDeposit();
		}

		public void UpdateDeposit()
		{
			for (int i = 0; i < data[0].Count; i++)
			{
				if (data[3][i] == "DfT")
				{
					Deposit -= double.Parse(data[1][i]);
				}

				string itemName = data[0][i].ToLower();
				if (itemName.Contains("terp") && itemName.Contains("express") && itemName.Contains("add"))
				{
					Deposit += double.Parse(data[1][i]);
				}
			}
		}
    }
}
