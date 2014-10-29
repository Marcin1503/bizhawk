﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BizHawk.Common.NumberExtensions;
using BizHawk.Common;

namespace BizHawk.Emulation.Cores.Nintendo.NES
{
	// mostly jacked from nestopia's NstBoardBandaiDatach.cpp
	// very dirty, needs cleanup and such

	public class DatachBarcode
	{
		static readonly byte[,] prefixParityType = new byte[10, 6]
		{
			{8,8,8,8,8,8}, {8,8,0,8,0,0},
			{8,8,0,0,8,0}, {8,8,0,0,0,8},
			{8,0,8,8,0,0}, {8,0,0,8,8,0},
			{8,0,0,0,8,8}, {8,0,8,0,8,0},
			{8,0,8,0,0,8}, {8,0,0,8,0,8}
		};

		static readonly byte[,] dataLeftOdd = new byte[10, 7]
		{
			{8,8,8,0,0,8,0}, {8,8,0,0,8,8,0},
			{8,8,0,8,8,0,0}, {8,0,0,0,0,8,0},
			{8,0,8,8,8,0,0}, {8,0,0,8,8,8,0},
			{8,0,8,0,0,0,0}, {8,0,0,0,8,0,0},
			{8,0,0,8,0,0,0}, {8,8,8,0,8,0,0}
		};

		static readonly byte[,] dataLeftEven = new byte[10, 7]
		{
			{8,0,8,8,0,0,0}, {8,0,0,8,8,0,0},
			{8,8,0,0,8,0,0}, {8,0,8,8,8,8,0},
			{8,8,0,0,0,8,0}, {8,0,0,0,8,8,0},
			{8,8,8,8,0,8,0}, {8,8,0,8,8,8,0},
			{8,8,8,0,8,8,0}, {8,8,0,8,0,0,0}
		};

		static readonly byte[,] dataRight = new byte[10, 7]
		{
			{0,0,0,8,8,0,8}, {0,0,8,8,0,0,8},
			{0,0,8,0,0,8,8}, {0,8,8,8,8,0,8},
			{0,8,0,0,0,8,8}, {0,8,8,0,0,0,8},
			{0,8,0,8,8,8,8}, {0,8,8,8,0,8,8},
			{0,8,8,0,8,8,8}, {0,0,0,8,0,8,8}
		};

		const int MIN_DIGITS = 8;
		const int MAX_DIGITS = 13;
		const int MAX_DATA_LENGTH = 0x100;
		const byte END = 0xFF;
		const int CC_INTERVAL = 1000;

		int cycles;
		byte output;
		int stream_idx;
		byte[] data = new byte[MAX_DATA_LENGTH];

		public void SyncState(Serializer ser)
		{
			ser.BeginSection("DatachBarcode");
			ser.Sync("cycles", ref cycles);
			ser.Sync("output", ref output);
			ser.Sync("stream_idx", ref stream_idx);
			ser.Sync("data", ref data, false);
			ser.EndSection();
		}


		public void Reset()
		{
			cycles = 0; // MAX
			//output = 0;
			stream_idx = 0;
			for (int i = 0; i < data.Length; i++)
				data[i] = END;
		}

		public bool IsTransferring()
		{
			return data[stream_idx] != END;
		}
		private static bool IsDigtsSupported(int count)
		{
			return count.In(MIN_DIGITS, MAX_DIGITS);
		}

		public static bool ValidString(string s, out string why)
		{
			if (s == null)
				throw new ArgumentNullException("s");
			if (!s.Length.In(MIN_DIGITS, MAX_DIGITS))
			{
				why = string.Format("String must be {0} or {1} digits long!", MIN_DIGITS, MAX_DIGITS);
				return false;
			}
			foreach (char c in s)
			{
				if (c < '0' || c > '9')
				{
					why = "String must be numeric only!";
					return false;
				}
			}
			why = "String is OK.";
			return true;
		}

		public void Transfer(string s)
		{
			string why;
			if (!ValidString(s, out why))
				throw new InvalidOperationException(why);

			Reset();

			byte[] code = new byte[16];

			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] >= '0' && s[i] <= '9')
					code[i] = (byte)(s[i] - '0');
				else
					throw new InvalidOperationException("s must be numeric only");
			}

			int out_ptr = 0;
			for (int i = 0; i < 33; i++)
				data[out_ptr++] = 8;

			data[out_ptr++] = 0;
			data[out_ptr++] = 8;
			data[out_ptr++] = 0;

			int sum = 0;

			if (s.Length == MAX_DIGITS)
			{
				for (int i = 0; i < 6; i++)
				{
					if (prefixParityType[code[0], i] != 0)
					{
						for (int j = 0; j < 7; j++)
							data[out_ptr++] = dataLeftOdd[code[i + 1], j];
					}
					else
					{
						for (int j = 0; j < 7; j++)
							data[out_ptr++] = dataLeftEven[code[i + 1], j];
					}
				}

				data[out_ptr++] = 8;
				data[out_ptr++] = 0;
				data[out_ptr++] = 8;
				data[out_ptr++] = 0;
				data[out_ptr++] = 8;

				for (int i = 7; i < 12; i++)
					for (int j = 0; j < 7; j++)
						data[out_ptr++] = dataRight[code[i], j];

				for (int i = 0; i < 12; i++)
					sum += code[i] * ((i & 1) != 0 ? 3 : 1);
			}
			else // s.Length == MIN_DIGITS
			{
				for (int i = 0; i < 4; i++)
					for (int j = 0; j < 7; j++)
						data[out_ptr++] = dataLeftOdd[code[i], j];

				data[out_ptr++] = 8;
				data[out_ptr++] = 0;
				data[out_ptr++] = 8;
				data[out_ptr++] = 0;
				data[out_ptr++] = 8;

				for (int i = 4; i < 7; i++)
					for (int j = 0; j < 7; j++)
						data[out_ptr++] = dataRight[code[i], j];


				for (int i = 0; i < 7; i++)
					sum += code[i] * ((i & 1) != 0 ? 3 : 1);
			}
			sum = (10 - (sum % 10)) % 10;

			for (int j = 0; j < 7; j++)
				data[out_ptr++] = dataRight[sum, j];

			data[out_ptr++] = 0;
			data[out_ptr++] = 8;
			data[out_ptr++] = 0;

			for (int i = 0; i < 32; i++)
				data[out_ptr++] = 8;

			cycles = CC_INTERVAL;
			output = data[stream_idx]; // ??
		}

		public void Clock()
		{
			if (cycles <= 0 || !IsTransferring())
				return;
			cycles--;
			if (cycles <= 0)
			{
				stream_idx++;
				output = data[stream_idx];
				if (output == END)
					output = 0;
				else
					cycles = CC_INTERVAL;
			}
		}

		/// <summary>
		/// d3
		/// </summary>
		/// <returns></returns>
		public bool GetOutput()
		{
			return output == 8;
		}
	}
}
