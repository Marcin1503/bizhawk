namespace BizHawk.Emulation.CPUs.Z80GB
{
	public partial class Z80 
    {
		private void InitializeTables() 
        {
			InitTableDaa();
		}

	    private static readonly byte[] IncTable = 
        {
            160, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            032, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };
               
		private static readonly byte[] DecTable =
        {
            192, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96,
            064, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 96
        };

	    private static readonly byte[] SwapTable =
        {
            0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0,
            0x01, 0x11, 0x21, 0x31, 0x41, 0x51, 0x61, 0x71, 0x81, 0x91, 0xA1, 0xB1, 0xC1, 0xD1, 0xE1, 0xF1,
            0x02, 0x12, 0x22, 0x32, 0x42, 0x52, 0x62, 0x72, 0x82, 0x92, 0xA2, 0xB2, 0xC2, 0xD2, 0xE2, 0xF2,
            0x03, 0x13, 0x23, 0x33, 0x43, 0x53, 0x63, 0x73, 0x83, 0x93, 0xA3, 0xB3, 0xC3, 0xD3, 0xE3, 0xF3,
            0x04, 0x14, 0x24, 0x34, 0x44, 0x54, 0x64, 0x74, 0x84, 0x94, 0xA4, 0xB4, 0xC4, 0xD4, 0xE4, 0xF4,
            0x05, 0x15, 0x25, 0x35, 0x45, 0x55, 0x65, 0x75, 0x85, 0x95, 0xA5, 0xB5, 0xC5, 0xD5, 0xE5, 0xF5,
            0x06, 0x16, 0x26, 0x36, 0x46, 0x56, 0x66, 0x76, 0x86, 0x96, 0xA6, 0xB6, 0xC6, 0xD6, 0xE6, 0xF6,
            0x07, 0x17, 0x27, 0x37, 0x47, 0x57, 0x67, 0x77, 0x87, 0x97, 0xA7, 0xB7, 0xC7, 0xD7, 0xE7, 0xF7,
            0x08, 0x18, 0x28, 0x38, 0x48, 0x58, 0x68, 0x78, 0x88, 0x98, 0xA8, 0xB8, 0xC8, 0xD8, 0xE8, 0xF8,
            0x09, 0x19, 0x29, 0x39, 0x49, 0x59, 0x69, 0x79, 0x89, 0x99, 0xA9, 0xB9, 0xC9, 0xD9, 0xE9, 0xF9,
            0x0A, 0x1A, 0x2A, 0x3A, 0x4A, 0x5A, 0x6A, 0x7A, 0x8A, 0x9A, 0xAA, 0xBA, 0xCA, 0xDA, 0xEA, 0xFA,
            0x0B, 0x1B, 0x2B, 0x3B, 0x4B, 0x5B, 0x6B, 0x7B, 0x8B, 0x9B, 0xAB, 0xBB, 0xCB, 0xDB, 0xEB, 0xFB,
            0x0C, 0x1C, 0x2C, 0x3C, 0x4C, 0x5C, 0x6C, 0x7C, 0x8C, 0x9C, 0xAC, 0xBC, 0xCC, 0xDC, 0xEC, 0xFC,
            0x0D, 0x1D, 0x2D, 0x3D, 0x4D, 0x5D, 0x6D, 0x7D, 0x8D, 0x9D, 0xAD, 0xBD, 0xCD, 0xDD, 0xED, 0xFD,
            0x0E, 0x1E, 0x2E, 0x3E, 0x4E, 0x5E, 0x6E, 0x7E, 0x8E, 0x9E, 0xAE, 0xBE, 0xCE, 0xDE, 0xEE, 0xFE,
            0x0F, 0x1F, 0x2F, 0x3F, 0x4F, 0x5F, 0x6F, 0x7F, 0x8F, 0x9F, 0xAF, 0xBF, 0xCF, 0xDF, 0xEF, 0xFF
        };

        private static readonly byte[] mCycleTable = new byte[] 
        {
            1, 3, 2, 2, 1, 1, 2, 1, 5, 2, 2, 2, 1, 1, 2, 1,
            1, 3, 2, 2, 1, 1, 2, 1, 3, 2, 2, 2, 1, 1, 2, 1,
            3, 3, 2, 2, 1, 1, 2, 1, 3, 2, 2, 2, 1, 1, 2, 1,
            3, 3, 2, 2, 1, 3, 3, 3, 3, 2, 2, 2, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            2, 2, 2, 2, 2, 2, 1, 2, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1,
            5, 3, 4, 4, 6, 4, 2, 4, 5, 4, 4, 1, 6, 6, 2, 4,
            5, 3, 4, 0, 6, 4, 2, 4, 5, 4, 4, 0, 6, 0, 2, 4,
            3, 3, 2, 0, 0, 4, 2, 4, 4, 1, 4, 0, 0, 0, 2, 4,
            3, 3, 2, 1, 0, 4, 2, 4, 3, 2, 4, 1, 0, 0, 2, 4,
        };

        private static readonly byte[] cbMCycleTable = new byte[]
        {
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
	        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
	        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
	        2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 3, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
	        2, 2, 2, 2, 2, 2, 4, 2, 2, 2, 2, 2, 2, 2, 4, 2,
        };

        private ushort[] TableDaa;
        private void InitTableDaa()
        {
            TableDaa = new ushort[65536];
            for (int af = 0; af < 65536; ++af)
            {
                byte a = (byte)(af >> 8);
                byte tmp = a;

                if (IsN(af))
                {
                    if (IsH(af) || ((a & 0x0F) > 0x09)) tmp -= 0x06;
                    if (IsC(af) || a > 0x99) tmp -= 0x60;
                }
                else
                {
                    if (IsH(af) || ((a & 0x0F) > 0x09)) tmp += 0x06;
                    if (IsC(af) || a > 0x99) tmp += 0x60;
                }

                TableDaa[af] = (ushort)((tmp * 256) + FlagByte(IsC(af) || a > 0x99, ((a ^ tmp) & 0x10) != 0, IsN(af), tmp == 0));
            }
        }

        private static byte FlagByte(bool C, bool H, bool N, bool Z) 
        {
			return (byte)(
				(C ? 0x10 : 0) +
				(H ? 0x20 : 0) +
                (N ? 0x40 : 0) +
				(Z ? 0x80 : 0)
			 );
		}

		private static bool IsC(int value) { return (value & 0x10) != 0; }
        private static bool IsH(int value) { return (value & 0x20) != 0; }
        private static bool IsN(int value) { return (value & 0x40) != 0; }
		private static bool IsZ(int value) { return (value & 0x80) != 0; }
	}
}
