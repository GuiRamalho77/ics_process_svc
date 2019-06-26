using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using InsurerServices.Service.Services;

namespace InsurerServices.Service
{
	class Program
	{
		private static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
            //var tempArgs = new List<string>() { "--emissoes" };
            //var tempArgs = new List<string>() { "--cancelamentos" };
            //var tempArgs = new List<string>() { "--emissoes", "--cancelamentos" };
            foreach (var arg in args)
			{
				switch (arg)
				{
					case "--emissoes":
						Issuances.Process();
						break;
					case "--cancelamentos":
						Cancelations.Process();
						break;
					default:
						Console.WriteLine("Argumento Inválido!");
						break;

				}
			}
		}
	}
}
