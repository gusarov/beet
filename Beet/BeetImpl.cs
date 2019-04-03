using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Beet
{
	public class BeetImpl
	{
		Dictionary<byte[], byte[]> _dataA = new Dictionary<byte[], byte[]>(QuickHashArrayComparer.Instance);

		public byte[] Get(byte[] key)
		{
			_dataA.TryGetValue(key, out var val);
			return val;
		}

		public void Set(byte[] key, byte[] val)
		{
			_dataA[key] = val;
		}

	}

	public class BeetBinaryRequestHandler
	{
		private readonly BeetImpl _engine;
		private readonly Stream _stream;
		private readonly bool _leaveOpen;

		public BeetBinaryRequestHandler(BeetImpl engine, Stream stream, bool leaveOpen)
		{
			_engine = engine;
			_stream = stream;
			_leaveOpen = leaveOpen;
		}

		UTF8Encoding _encoding = new UTF8Encoding(false, false);

		public async Task Serve()
		{
			try
			{
				using (var br = new BinaryReader(_stream, _encoding, _leaveOpen))
				{
					using (var bw = new BinaryWriter(_stream, _encoding, _leaveOpen))
					{
						while (true)
						{
							var cmd = br.ReadByte();

							switch (cmd)
							{
								case 1: // set
									{
										var keyLen = br.ReadByte();
										var key = new byte[keyLen];
										br.Read(key, 0, keyLen);
										var valLen = br.ReadByte();
										var val = new byte[valLen];
										br.Read(val, 0, valLen);
										_engine.Set(key, val);
										bw.Write((byte)1); // success
										break;
									}
								case 2: // get
									{
										var keyLen = br.ReadByte();
										var key = new byte[keyLen];
										br.Read(key, 0, keyLen);
										var val = _engine.Get(key);
										bw.Write(checked((byte)val.Length));
										bw.Write(val);
										break;
									}
								default:
									bw.Write(0); // error
									bw.Write(1); // unknown command
									break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}

	/// <summary>
	/// Hash every byte with FNV 1a algorithm
	/// </summary>
	class FullHashArrayComparer : ByteArrayComparer
	{
		public static FullHashArrayComparer Instance = new FullHashArrayComparer();

		public override int GetHashCode(byte[] key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			// FNV1a
			uint hash = 2166136261;
			for (var i = 0; i < key.Length; i++)
			{
				hash = (hash ^ key[i]) * 16777619;
			}
			return (int)hash;
		}
	}

	class QuickHashArrayComparer : ByteArrayComparer
	{
		public static QuickHashArrayComparer Instance = new QuickHashArrayComparer();

		public override int GetHashCode(byte[] key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (key.Length >= 4)
			{
				// first 2 & last 2 bytes
				var m = key.Length;
				return
					   key[0] << 24
					| (key[1] << 16)
					| (key[m - 2] << 8)
					| (key[m - 1])
					;
			}
			else
			{
				var hash = 0;
				for (int i = 0; i < key.Length; i++)
				{
					hash |= key[i] << (8 * i);
				}
				return hash;
			}
		}
	}

	abstract class ByteArrayComparer : IEqualityComparer<byte[]>
	{

		public bool Equals(byte[] left, byte[] right)
		{
			if (left == null || right == null)
			{
				return ReferenceEquals(left, right);
			}
			return left.SequenceEqual(right);
			// return UnSafeEquals(left, right);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static unsafe bool UnSafeEquals(byte[] strA, byte[] strB)
		{
			int length = strA.Length;
			if (length != strB.Length)
			{
				return false;
			}
			fixed (byte* str = strA)
			{
				byte* chPtr = str;
				fixed (byte* str2 = strB)
				{
					byte* chPtr2 = str2;
					byte* chPtr3 = chPtr;
					byte* chPtr4 = chPtr2;
					while (length >= 10)
					{
						if ((((*(((int*)chPtr3)) != *(((int*)chPtr4))) || (*(((int*)(chPtr3 + 2))) != *(((int*)(chPtr4 + 2))))) || ((*(((int*)(chPtr3 + 4))) != *(((int*)(chPtr4 + 4)))) || (*(((int*)(chPtr3 + 6))) != *(((int*)(chPtr4 + 6)))))) || (*(((int*)(chPtr3 + 8))) != *(((int*)(chPtr4 + 8)))))
						{
							break;
						}
						chPtr3 += 10;
						chPtr4 += 10;
						length -= 10;
					}
					while (length > 0)
					{
						if (*(((int*)chPtr3)) != *(((int*)chPtr4)))
						{
							break;
						}
						chPtr3 += 2;
						chPtr4 += 2;
						length -= 2;
					}
					return (length <= 0);
				}
			}
		}
		
		public abstract int GetHashCode(byte[] data);
	}
}

