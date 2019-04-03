using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Beet.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Beet.TestsCore
{
	[TestClass]
	public class ShouldRespondToBasics
	{
		static Random _rnd = new Random();

		[TestInitialize]
		public void Init()
		{
			_port = _rnd.Next(ushort.MaxValue);
			_p = Process.Start(typeof(Program).Assembly.Location, $"{_port}");
			_client = new TcpClient("localhost", 123  /*_port*/);
			_stream = _client.GetStream();
			_writer = new BinaryWriter(_stream);
			_reader = new BinaryReader(_stream);
		}

		[TestCleanup]
		public void Clean()
		{
			_p.Kill();
		}

		int _port;
		Process _p;
		TcpClient _client;
		NetworkStream _stream;
		BinaryWriter _writer;
		BinaryReader _reader;

		[TestMethod]
		public void Should_set_and_get()
		{
			var val = (byte)_rnd.Next();

			_writer.Write((byte)1); // set
			_writer.Write((byte)1); // key len 1
			_writer.Write((byte)1); // key val 1
			_writer.Write((byte)1); // val len
			_writer.Write((byte)val); // val

			var b = _reader.ReadByte(); // read confirmation
			Assert.AreEqual((byte)1, b);

			_writer.Write((byte)2); // get
			_writer.Write((byte)1); // key len 1
			_writer.Write((byte)1); // key val 1
			var len = _reader.ReadByte();
			Assert.AreEqual((byte)1, len);
			var storedVal = _reader.ReadByte();
			Assert.AreEqual(val, storedVal);
		}
	}
}
