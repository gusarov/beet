using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Beet;

namespace Beet.Server
{
	public class Program
	{
		static void Main(string[] args)
		{
			new Program().MainInstance(args);
		}

		BeetImpl _beet = new BeetImpl();

		void MainInstance(string[] args)
		{
			var listener = TcpListener.Create(ushort.Parse(args[0]));
			listener.Start();
			while (true) {
				try
				{
					var cli = listener.AcceptTcpClient();
					Console.WriteLine("New Client");
					var stream = cli.GetStream();
					_ = new BeetBinaryRequestHandler(_beet, stream, false).Serve();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}
	}
}
