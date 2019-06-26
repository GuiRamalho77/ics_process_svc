using System;
using System.Collections.Generic;
using System.Text;

namespace InsurerServices.Providers.Hdi.Helpers
{
	internal static class Helper
	{
		public static string GenerateValidCpf()
		{
			int sum = 0, rest = 0;
			int[] x1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			int[] x2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

			Random rnd = new Random();
			string seed = rnd.Next(100000000, 999999999).ToString();

			for (int i = 0; i < 9; i++)
				sum += int.Parse(seed[i].ToString()) * x1[i];

			rest = sum % 11;
			if (rest < 2)
				rest = 0;
			else
				rest = 11 - rest;

			seed = seed + rest;
			sum = 0;

			for (int i = 0; i < 10; i++)
				sum += int.Parse(seed[i].ToString()) * x2[i];

			rest = sum % 11;

			if (rest < 2)
				rest = 0;
			else
				rest = 11 - rest;

			seed = seed + rest;
			return seed;
		}
	}
}
