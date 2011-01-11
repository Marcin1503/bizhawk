﻿
using System;

namespace BizHawk.Emulation.CPUs.M68K
{
    public partial class M68000
    {
        private bool TestCondition(int condition)
        {
            switch (condition)
            {
                case 0x00: return true;     // True
                case 0x01: return false;    // False
                case 0x02: return !C && !Z; // High (Unsigned)
                case 0x03: return C || Z;   // Less or Same (Unsigned)
                case 0x04: return !C;       // Carry Clear (High or Same)
                case 0x05: return C;        // Carry Set (Lower)
                case 0x06: return !Z;       // Not Equal
                case 0x07: return Z;        // Equal
                case 0x08: return !V;       // Overflow Clear
                case 0x09: return V;        // Overflow Set
                case 0x0A: return !N;       // Plus (Positive)
                case 0x0B: return N;        // Minus (Negative)
                case 0x0C: return N && V || !N && !V;             // Greater or Equal
                case 0x0D: return N && !V || !N && V;             // Less Than
                case 0x0E: return N && V && !Z || !N && !V && !Z; // Greater Than
                case 0x0F: return Z || N && !V || !N && V;        // Less or Equal
                default:
                    throw new Exception("Invalid condition "+condition);
            }
        }

        private string DisassembleCondition(int condition)
        {
            switch (condition)
            {
                case 0x00: return "t";  // True
                case 0x01: return "f";  // False
                case 0x02: return "hi"; // High (Unsigned)
                case 0x03: return "ls"; // Less or Same (Unsigned)
                case 0x04: return "cc"; // Carry Clear (High or Same)
                case 0x05: return "cs"; // Carry Set (Lower)
                case 0x06: return "ne"; // Not Equal
                case 0x07: return "eq"; // Equal
                case 0x08: return "vc"; // Overflow Clear
                case 0x09: return "vs"; // Overflow Set
                case 0x0A: return "pl"; // Plus (Positive)
                case 0x0B: return "mi"; // Minus (Negative)
                case 0x0C: return "ge"; // Greater or Equal
                case 0x0D: return "lt"; // Less Than
                case 0x0E: return "gt"; // Greater Than
                case 0x0F: return "le"; // Less or Equal
                default:   return "??"; // Invalid condition
            }
        }

        private void Bcc() // Branch on condition
        {
            sbyte displacement8 = (sbyte) op;
            int cond = (op >> 8) & 0x0F;
            
            if (TestCondition(cond) == true)
            {
                if (displacement8 != 0)
                {
                    // use opcode-embedded displacement
                    PC += displacement8;
                    PendingCycles -= 10;
                } else {
                    // use extension word displacement
                    PC += ReadWord(PC);
                    PendingCycles -= 10;
                }
            } else { // false
                if (displacement8 != 0)
                    PendingCycles -= 8;
                else {
                    PC += 2;
                    PendingCycles -= 12;
                }
            }
        }

        private void Bcc_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            sbyte displacement8 = (sbyte)op;
            int cond = (op >> 8) & 0x0F;

            info.Mnemonic = "b" + DisassembleCondition(cond);
            if (displacement8 != 0)
            {
                info.Args = string.Format("${0:X}", pc + displacement8);
            } else {
                info.Args = string.Format("${0:X}", pc + ReadWord(pc));
                pc += 2;
            }
            info.Length = pc - info.PC;
        }

        private void BRA()
        {
            sbyte displacement8 = (sbyte)op;

            if (displacement8 != 0)
                PC += displacement8;
            else
                PC += ReadWord(PC);
            PendingCycles -= 10;
        }

        private void BRA_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            info.Mnemonic = "bra";

            sbyte displacement8 = (sbyte)op;
            if (displacement8 != 0)
                info.Args = String.Format("${0:X}", pc + displacement8);
            else
            {
                info.Args = String.Format("${0:X}", pc + ReadWord(pc));
                pc += 2;
            }
            info.Length = pc - info.PC;
        }

        private void BSR()
        {
            sbyte displacement8 = (sbyte)op;

            A[7].s32 -= 4;
            if (displacement8 != 0)
            {
                // use embedded displacement
                WriteLong(A[7].s32, PC);
                PC += displacement8;
            } else {
                // use extension word displacement
                WriteLong(A[7].s32, PC + 2);
                PC += ReadWord(PC);
            }
            PendingCycles -= 18;
        }

        private void BSR_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            info.Mnemonic = "bsr";

            sbyte displacement8 = (sbyte)op;
            if (displacement8 != 0)
                info.Args = String.Format("${0:X}", pc + displacement8);
            else {
                info.Args = String.Format("${0:X}", pc + ReadWord(pc));
                pc += 2;
            }
            info.Length = pc - info.PC;
        }

        private void DBcc()
        {
            if (TestCondition((op >> 8) & 0x0F) == true)
            {
                // break out of loop
                PC += 2;
                PendingCycles -= 12;
            } else {
                int reg = op & 7;
                D[reg].u16--;

                if (D[reg].u16 == 0xFFFF)
                {
                    PC += 2;
                    PendingCycles -= 14;
                } else {
                    PC += ReadWord(PC);
                    TotalExecutedCycles -= 10;
                }
            }
        }

        private void DBcc_Disasm(DisassemblyInfo info)
        {
            int cond = (op >> 8) & 0x0F;
            if (cond == 1)
                info.Mnemonic = "dbra";
            else
                info.Mnemonic = "db" + DisassembleCondition(cond);

            int pc = info.PC + 2;
            info.Args = String.Format("D{0}, ${1:X}", op & 7, pc + ReadWord(pc));
            info.Length = 4;
        }

        private void RTS()
        {
            PC = ReadLong(A[7].s32);
            A[7].s32 += 4;
            PendingCycles -= 16;
        }

        private void RTS_Disasm(DisassemblyInfo info)
        {
            info.Mnemonic = "rts";
            info.Args = "";
        }

        private void TST()
        {
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            int value;
            switch (size)
            {
                case 0:  value = ReadValueB(mode, reg); PendingCycles -= 4 + EACyclesBW[mode, reg]; break;
                case 1:  value = ReadValueW(mode, reg); PendingCycles -= 4 + EACyclesBW[mode, reg]; break;
                default: value = ReadValueL(mode, reg); PendingCycles -= 4 + EACyclesL[mode, reg];  break;
            }
            V = false;
            C = false;
            N = (value < 0);
            Z = (value == 0);
        }

        private void TST_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int size = (op >> 6) & 3;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;

            switch (size)
            {
                case 0: info.Mnemonic = "tst.b"; info.Args = DisassembleValue(mode, reg, 1, ref pc); break;
                case 1: info.Mnemonic = "tst.w"; info.Args = DisassembleValue(mode, reg, 2, ref pc); break;
                case 2: info.Mnemonic = "tst.l"; info.Args = DisassembleValue(mode, reg, 4, ref pc); break;
            }
            info.Length = pc - info.PC;
        }

        private void BTSTi()
        {
            int bit = ReadWord(PC);
            PC += 2;
            int mode = (op >> 3) & 7;
            int reg = op & 7;

            if (mode == 0)
            {
                bit &= 31;
                int mask = 1 << bit;
                Z = (D[reg].s32 & mask) == 0;
                PendingCycles -= 10;
            } else {
                bit &= 7;
                int mask = 1 << bit;
                Z = (ReadValueB(mode, reg) & mask) == 0;
                PendingCycles -= 8 + EACyclesBW[mode, reg];
            }
        }

        private void BTSTi_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int bit = ReadWord(pc); pc += 2;
            int mode = (op >> 3) & 7;
            int reg = op & 7;

            info.Mnemonic = "btst";
            info.Args = String.Format("${0:X}, {1}", bit, DisassembleValue(mode, reg, 1, ref pc));

            info.Length = pc - info.PC;
        }

        private void BTSTr()
        {
            throw new NotImplementedException();
        }

        private void BTSTr_Disasm(DisassemblyInfo info)
        {
            throw new NotImplementedException();
        }

        private void JMP()
        {
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
            PC = ReadAddress(mode, reg);

            switch (mode)
            {
                case 2: PendingCycles -= 8; break;
                case 5: PendingCycles -= 10; break;
                case 6: PendingCycles -= 14; break;
                case 7:
                    switch (reg)
                    {
                        case 0: PendingCycles -= 10; break;
                        case 1: PendingCycles -= 12; break;
                        case 2: PendingCycles -= 10; break;
                        case 3: PendingCycles -= 14; break;
                    }
                    break;
            }
        }

        private void JMP_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
            info.Mnemonic = "jmp";
            info.Args = DisassembleValue(mode, reg, 1, ref pc);
            info.Length = pc - info.PC;
        }

        private void JSR()
        {
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
            int addr = ReadAddress(mode, reg);

            A[7].s32 -= 4;
            WriteLong(A[7].s32, PC);
            PC = addr;

            switch (mode)
            {
                case 2: PendingCycles -= 16; break;
                case 5: PendingCycles -= 18; break;
                case 6: PendingCycles -= 22; break;
                case 7:
                    switch (reg)
                    {
                        case 0: PendingCycles -= 18; break;
                        case 1: PendingCycles -= 20; break;
                        case 2: PendingCycles -= 18; break;
                        case 3: PendingCycles -= 22; break;
                    }
                    break;
            }
        }

        private void JSR_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int mode = (op >> 3) & 7;
            int reg  = (op >> 0) & 7;
            info.Mnemonic = "jsr";
            info.Args = DisassembleAddress(mode, reg, ref pc);
            info.Length = pc - info.PC;
        }

        private void LINK()
        {
            int reg = op & 7;
            A[7].s32 -= 4;
            short offset = ReadWord(PC); PC += 2;
            WriteLong(A[7].s32, A[reg].s32);
            A[reg].s32 = A[7].s32 + offset;
            PendingCycles -= 16;
        }

        private void LINK_Disasm(DisassemblyInfo info)
        {
            int pc = info.PC + 2;
            int reg = op & 7;
            info.Mnemonic = "link";
            info.Args = "A"+reg+", "+DisassembleImmediate(2, ref pc); // TODO need a DisassembleSigned or something
            info.Length = pc - info.PC;
        }

        private void NOP()
        {
            PendingCycles -= 4;
        }

        private void NOP_Disasm(DisassemblyInfo info)
        {
            info.Mnemonic = "nop";
        }
    }
}
