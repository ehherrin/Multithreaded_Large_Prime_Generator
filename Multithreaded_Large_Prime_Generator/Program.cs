//Author: Edward Herrin
//Description: Finds large primes using multithreading and brute force.
//Date Created: 06/26/2019

using System;
using System.Security.Cryptography;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;


//This is my namespace, there are many like it but this one is mine.
namespace Multithreaded_Large_Prime_Generator
{
	//This class serves the purpose of holding the extension method that was given in the lab write-up.
	public static class Extension
	{
		//This is the supplied prime-finding method
		public static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10)
		{
			if (value <= 1)
			{
				return false;
			};
			if (witnesses <= 0)
			{
				witnesses = 10;
			}
			BigInteger d = value - 1;
			int s = 0;
			while (d % 2 == 0)
			{
				d /= 2;
				s += 1;
			}
			Byte[] bytes = new Byte[value.ToByteArray().LongLength];
			BigInteger a;
			for (int i = 0; i < witnesses; i++)
			{
				do
				{
					var Gen = new Random();
					Gen.NextBytes(bytes);
					a = new BigInteger(bytes);
				} while (a < 2 || a >= value - 2);
				BigInteger x = BigInteger.ModPow(a, d, value);
				if (x == 1 || x == value - 1) continue;
				for (int r = 1; r < s; r++)
				{
					x = BigInteger.ModPow(x, 2, value);
					if (x == 1) return false;
					if (x == value - 1) break;
				}
				if (x != value - 1) return false;
			}
			return true;
		}
	}

	//This class serves the purpose of assuring command validity with my improved assess_command_validity
	//method. Also, after the command has been passes the validity assessment, then the multithreaded
	//brute_force_prime_search method begins and finds primes to display. If the command is not valid,
	//then the first error that occured is shown along with usage instructions.
	class Program
	{
		int count = 1;
		int bits;

		//Creates a new instance of RNGCryptoServiceProvider named rngCsp.
		private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

		//Assures the command is valid by testing syntax.
		public int assess_command_validity(string[] args)
		{
			//If there are between one and two arguments.
			if (args.Length > 0 & args.Length <= 2)
			{
				//If the command has an integer value of bits.
				if (int.TryParse(args[0], out int bitValue))
				{
					//Parse the number of bits into an integer.
					bits = bitValue;
					// If the command have a second argument.
					if (args.Length == 2)
					{
						//If second argument be parsed as an integer.
						if (int.TryParse(args[1], out int countValue))
						{
							if (countValue > 0)
							{
								//Then parse it and set the count value to it.
								count = countValue;
							}
							else
							{
								return -1;
							}
						}
						else
						{
							//Unique error message: You did not specify an integer value for count.
							return -5;
						}
					}
					if (bits % 8 == 0 & bits >= 0)
					{
						return 1;
					}
					else
					{
						//Unique error message: You did not specify a positive multiple of 8 for bits.
						return -2;
					}
				}
				else
				{
					//Unique error message: You did not specify an integer value for bits.
					return -3;
				}
			}
			else
			{
				//Unique error message: Wrong number of arguments.
				return -4;
			}
		}

		//Displays the usage help which is followed by the unique error that occured.
		public void error_statement(int errorType)
		{
			System.Console.WriteLine("Usage : dotnet run PrimeGen <bits> <count=1>");
			System.Console.WriteLine("\t- bits - the number of bits of the prime number, this must be a");
			System.Console.WriteLine("\t  multiple of 8, and at least 32 bits.");
			System.Console.WriteLine("\t- count - the number of prime numbers to generate, defaults to 1");
			System.Console.WriteLine("");
			System.Console.WriteLine("First Error That Occured:");
			switch (errorType)
			{
				case -1:
					System.Console.WriteLine("\tYou did not specify a positive integer value for count");
					break;
				case -2:
					System.Console.WriteLine("\tYou did not specify a positive multiple of 8 for bits");
					break;
				case -3:
					System.Console.WriteLine("\tYou did not specify a integer value for bits");
					break;
				case -4:
					System.Console.WriteLine("\tYou an incorrect number of arguments");
					break;
				case -5:
					System.Console.WriteLine("\tYou did not specify an integer value for count");
					break;
			}
		}

		//Keeps calling the prime-finding extension method until a prime is found. When
		//a prime is found by a thread, it is written to the console in an orderly fashion.
		public void brute_force_prime_search()
		{
			int j = 1;
			Parallel.For(0, count, i =>
			{
				byte[] randomNumber = new byte[bits / 8];
				string outputString = "";
				rngCsp.GetNonZeroBytes(randomNumber);
				BigInteger value = new BigInteger(randomNumber);
				value = BigInteger.Abs(value);

				while (!value.IsProbablyPrime())
				{
					rngCsp.GetNonZeroBytes(randomNumber);
					value = new BigInteger(randomNumber);
					value = BigInteger.Abs(value);
				}
				lock (this)
				{
					outputString = "";
					outputString += j.ToString();
					outputString += ": ";
					outputString += value.ToString();
					for (int charNum = 0; charNum <= outputString.Length - 1; charNum += 1)
					{
						if (charNum % 64 == 0)
						{
							System.Console.Write("\n");
						}
						System.Console.Write(outputString[charNum]);
					}
					System.Console.Write("\n");
					j += 1;
				}
			});
		}

		//Creates an instance of the Program class and calls all the functions in order.
		static void Main(string[] args)
		{
			Stopwatch parTime;
			Program primeFinder = new Program();
			int errorType = primeFinder.assess_command_validity(args);
			if (errorType < 0)
			{
				primeFinder.error_statement(errorType);
			}
			else
			{
				System.Console.Write("BitLength: {0} bits", args[0]);
				parTime = Stopwatch.StartNew();
				primeFinder.brute_force_prime_search();
				parTime.Stop();
				System.Console.WriteLine("Time to Generate: {00:00:00:00.0000000}", parTime.Elapsed.TotalSeconds);
			}
		}
	}
}

