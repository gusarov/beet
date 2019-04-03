using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisCore.TestsFull
{
	[TestClass]
	public class KeyPerformanceTests
	{
		Random _rnd = new Random();

		RedisCoreImpl _sut = new RedisCoreImpl();

		const int m = 1000000;

		[TestMethod]
		public void KeyPerformance()
		{
			var list = new List<byte[]>();
			for (int i = 0; i < m; i++)
			{
				var key = new byte[32];
				_rnd.NextBytes(key);
				list.Add(key);
			}

			var val = new byte[] { 1, 2, 3, 4 };

			// fill
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < m; i++)
			{
				_sut.SetA(list[i], val);
			}
			Console.WriteLine(sw.ElapsedMilliseconds);
			// get
			sw = Stopwatch.StartNew();
			for (int i = 0; i < m; i++)
			{
				_sut.GetA(list[i]);
			}
			Console.WriteLine(sw.ElapsedMilliseconds);
		}

		/*
		[TestMethod]
		public void KeyPerformanceString()
		{
			var list = new List<string>();
			for (int i = 0; i < m; i++)
			{
				var key = new byte[32];
				_rnd.NextBytes(key);
				list.Add(string.Join("", key.Select(x => x.ToString())));
			}

			var val = new byte[] { 1, 2, 3, 4 };

			// fill
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < m; i++)
			{
				_sut.SetB(list[i], val);
			}
			Console.WriteLine(sw.ElapsedMilliseconds);
			// get
			sw = Stopwatch.StartNew();
			for (int i = 0; i < m; i++)
			{
				_sut.GetB(list[i]);
			}
			Console.WriteLine(sw.ElapsedMilliseconds);
		}
		*/

	}
}
