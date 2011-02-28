using System;
using System.Diagnostics;

namespace BizHawk.Emulation.Consoles.Nintendo.Boards
{
	//mapper 11

	public class Discrete_74x377 : NES.NESBoardBase
	{
		int prg_mask, chr_mask;
		int prg, chr;
		bool bus_conflict = true;

		public override void Initialize(NES.RomInfo romInfo, NES nes)
		{
			base.Initialize(romInfo, nes);
			
			Debug.Assert(romInfo.PRG_Size == 2 || romInfo.PRG_Size == 4 || romInfo.PRG_Size == 8);
			prg_mask = (romInfo.PRG_Size/2)-1;

			Debug.Assert(romInfo.CHR_Size == 2 || romInfo.CHR_Size == 4 || romInfo.CHR_Size == 8 || romInfo.CHR_Size == 16);
			chr_mask = (romInfo.CHR_Size - 1);
		}
		public override byte ReadPRG(int addr)
		{
			return RomInfo.ROM[addr + (prg<<15)];
		}

		public override byte ReadPPU(int addr)
		{
			if (addr < 0x2000)
			{
				return RomInfo.VROM[addr + (chr << 13)];
			}
			else return base.ReadPPU(addr);
		}

		public override void WritePRG(int addr, byte value)
		{
			if (bus_conflict)
			{
				byte old_value = value;
				value &= ReadPRG(addr);
				Debug.Assert(old_value == value, "Found a test case of Discrete_74x377 bus conflict. please report.");
			}

			prg = (value & 3) & prg_mask;
			chr = (value >> 4) & chr_mask;
		}
	}
}