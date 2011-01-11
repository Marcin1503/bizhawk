﻿namespace BizHawk
{
 	public static class CRC32 
    {
		// Lookup table for speed.
		private static uint[] CRC32Table;

		static CRC32() 
        {
			//unchecked {
				CRC32Table = new uint[256];
			    for (uint i = 0; i < 256; ++i) 
                {
					uint crc = i;
					for (int j = 8; j > 0; --j) 
                    {
						if ((crc & 1) == 1)
							crc = ((crc >> 1) ^ 0xEDB88320);
					    else
							crc >>= 1;
					}
					CRC32Table[i] = crc;
				}
			//}
		}

        public static int Calculate(byte[] data) 
        {
			//unchecked {
			    uint Result = 0xFFFFFFFF;
				foreach (var b in data) 
                    Result = (((Result) >> 8) ^ CRC32Table[b ^ ((Result) & 0xFF)]);				
				return (int)~Result;
			//}
		}
	}
}
