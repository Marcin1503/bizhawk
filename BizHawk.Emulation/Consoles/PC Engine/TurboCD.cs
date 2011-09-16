﻿using System;

namespace BizHawk.Emulation.Consoles.TurboGrafx
{
    public partial class PCEngine
    {
        public static byte[] CdIoPorts = new byte[16];

        public bool IntADPCM // INTA
        {
            get { return (CdIoPorts[3] & 0x04) != 0; }
            set { CdIoPorts[3] = (byte)((CdIoPorts[3] & ~0x04) | (value ? 0x04 : 0x00)); }
        }
        public bool IntStop // INTSTOP
        {
            get { return (CdIoPorts[3] & 0x08) != 0; }
            set { CdIoPorts[3] = (byte)((CdIoPorts[3] & ~0x08) | (value ? 0x8 : 0x00)); }
        }
        public bool IntSubchannel // INTSUB
        {
            get { return (CdIoPorts[3] & 0x10) != 0; }
            set { CdIoPorts[3] = (byte)((CdIoPorts[3] & ~0x10) | (value ? 0x10 : 0x00)); }
        }
        public bool IntDataTransferComplete // INTM
        {
            get { return (CdIoPorts[3] & 0x20) != 0; }
            set { CdIoPorts[3] = (byte)((CdIoPorts[3] & ~0x20) | (value ? 0x20 : 0x00)); }
        }
        public bool IntDataTransferReady // INTD
        {
            get { return (CdIoPorts[3] & 0x40) != 0; }
            set { CdIoPorts[3] = (byte)((CdIoPorts[3] & ~0x40) | (value ? 0x40 : 0x00)); }
        }

        void SetCDAudioCallback()
        {
            CDAudio.CallbackAction = () =>
                {
                    Console.WriteLine("FIRING CD-AUDIO STOP IRQ MAYBE!");
                    IntDataTransferReady = false;
                    IntDataTransferComplete = true;
                };
        }

        void WriteCD(int addr, byte value)
        {
            switch (addr & 0x1FFF)
            {
                case 0x1800: // SCSI Drive Control Line
                    CdIoPorts[0] = value;
                    SCSI.SEL = true;
                    SCSI.Think();
                    SCSI.SEL = false;
                    SCSI.Think();

                    break;

                case 0x1801: // CDC Command
                    CdIoPorts[1] = value;
                    SCSI.DataBits = value;
                    SCSI.Think();
                    break;

                case 0x1802: // ACK and Interrupt Control
                    CdIoPorts[2] = value;
                    SCSI.ACK = ((value & 0x80) != 0);

                    if ((CdIoPorts[2] & 0x04) != 0) Log.Error("CD", "INTA enable");
                    if ((CdIoPorts[2] & 0x08) != 0) Log.Error("CD", "INTSTOP enable");
                    if ((CdIoPorts[2] & 0x10) != 0) Log.Error("CD", "INTSUB enable");
                    if ((CdIoPorts[2] & 0x20) != 0) Log.Error("CD", "INTM enable");
                    if ((CdIoPorts[2] & 0x40) != 0) Log.Error("CD", "INTD enable");
                    if ((Cpu.IRQControlByte & 0x01) == 0 &&
                        (CdIoPorts[2] & 0x7C) != 0) Log.Error("CD", "BTW, IRQ2 is not masked");

                    SCSI.Think();
                    RefreshIRQ2();
                    break;

                case 0x1804: // CD Reset Command
                    CdIoPorts[4] = value;
                    SCSI.RST = ((value & 0x02) != 0);
                    SCSI.Think();
                    if (SCSI.RST)
                    {
                        CdIoPorts[3] &= 0x8F; // Clear interrupt control bits
                        RefreshIRQ2();
                    }
                    break;

                case 0x1805:
                case 0x1806:
                    // Latch CDDA data... no action needed for us
                    break;

                case 0x1807: // BRAM Unlock
                    if (BramEnabled && (value & 0x80) != 0)
                        BramLocked = false;
                    break;

                case 0x1808: // ADPCM address LSB
                    ADPCM.IOAddress &= 0xFF00;
                    ADPCM.IOAddress |= value;
                    if ((CdIoPorts[0x0D] & 0x10) != 0)
                    {
                        Console.WriteLine("doing silly thing");
                        ADPCM.AdpcmLength = ADPCM.IOAddress;
                    }
                    break;

                case 0x1809: // ADPCM address MSB
                    ADPCM.IOAddress &= 0x00FF;
                    ADPCM.IOAddress |= (ushort)(value << 8);
                    if ((CdIoPorts[0x0D] & 0x10) != 0)
                    {
                        Console.WriteLine("doing silly thing");
                        ADPCM.AdpcmLength = ADPCM.IOAddress;
                    }
                    break;

                case 0x180A: // ADPCM Memory Read/Write Port
                    ADPCM.Port180A = value;
                    break;

                case 0x180B: // ADPCM DMA Control
                    ADPCM.Port180B = value;
                    if (ADPCM.AdpcmCdDmaRequested)
                        Console.WriteLine("          ADPCM DMA REQUESTED");
                    break;

                case 0x180D: // ADPCM Address Control
                    ADPCM.AdpcmControlWrite(value);
                    break;

                case 0x180E: // ADPCM Playback Rate
                    ADPCM.Port180E = value;
                    break;

                case 0x180F: // Audio Fade Timer
                    CdIoPorts[0x0F] = value;
                    // TODO ADPCM fades/vol control also.
                    switch (value)
                    {
                        case 0:
                            CDAudio.LogicalVolume = 100;
                            break;

                        case 8:
                        case 9:
                            if (CDAudio.FadeOutFramesRemaining == 0)
                                CDAudio.FadeOut(360); // 6 seconds
                            break;

                        case 12:
                        case 13:
                            if (CDAudio.FadeOutFramesRemaining == 0)
                                CDAudio.FadeOut(120); // 2 seconds
                            break;
                    }
                    break;

                default:
                    Log.Error("CD", "unknown write to {0:X4}:{1:X2} pc={2:X4}", addr, value, Cpu.PC);
                    break;
            }
        }

        public byte ReadCD(int addr)
        {
            byte returnValue = 0;
            short sample;

            switch (addr & 0x1FFF)
            {
                case 0x1800: //  SCSI Drive Control Line
                    if (SCSI.IO)  returnValue |= 0x08;
                    if (SCSI.CD)  returnValue |= 0x10;
                    if (SCSI.MSG) returnValue |= 0x20;
                    if (SCSI.REQ) returnValue |= 0x40;
                    if (SCSI.BSY) returnValue |= 0x80;
                    return returnValue;

                case 0x1801: // Read data bus
                    return SCSI.DataBits;

                case 0x1802: // ADPCM / CD Control
                    return CdIoPorts[2];

                case 0x1803: // BRAM Lock
                    if (BramEnabled)
                        BramLocked = true;

                    returnValue = CdIoPorts[3];
                    CdIoPorts[3] ^= 2;
                    return returnValue;

                case 0x1804: // CD Reset
                    return CdIoPorts[4];

                case 0x1805: // CD audio data Low
                    if ((CdIoPorts[3] & 0x2) == 0)
                        sample = CDAudio.VolumeLeft;
                    else
                        sample = CDAudio.VolumeRight;
                    return (byte) sample;

                case 0x1806: // CD audio data High
                    if ((CdIoPorts[3] & 0x2) == 0)
                        sample = CDAudio.VolumeLeft;
                    else
                        sample = CDAudio.VolumeRight;
                    return (byte) (sample >> 8);

                case 0x1808: // Auto Handshake Data Input
                    returnValue = SCSI.DataBits;
                    if (SCSI.REQ && SCSI.IO && !SCSI.CD)
                    {
                        SCSI.ACK = false;
                        SCSI.REQ = false;
                        SCSI.Think();
                    }
                    return returnValue;

                case 0x180A: // ADPCM Memory Read/Write Port
                    return ADPCM.Port180A;

                case 0x180B: // ADPCM Data Transfer Control
                    return ADPCM.Port180B;

                case 0x180C: // ADPCM Status
                    returnValue = 0;
                    if (ADPCM.EndReached)
                        returnValue |= 0x01;
                    if (ADPCM.AdpcmIsPlaying)
                        returnValue |= 0x08;
                    if (ADPCM.AdpcmBusyWriting)
                        returnValue |= 0x04;
                    if (ADPCM.AdpcmBusyReading)
                        returnValue |= 0x80;
                    //Log.Error("CD", "Read ADPCM Status {0:X2}", returnValue);

                    return returnValue;

                case 0x180D: // ADPCM Play Control
                    return CdIoPorts[0x0D];

                case 0x180F: // Audio Fade Timer
                    return CdIoPorts[0x0F];

                // These are some retarded version check
                case 0x18C1: return 0xAA;
                case 0x18C2: return 0x55;
                case 0x18C3: return 0x00;
                case 0x18C5: return 0xAA;
                case 0x18C6: return 0x55;
                case 0x18C7: return 0x03;

                default:
                    Log.Error("CD", "unknown read to {0:X4}", addr);
                    return 0xFF;
            }
        }

        public void RefreshIRQ2()
        {
            int mask = CdIoPorts[2] & CdIoPorts[3] & 0x7C;
            Cpu.IRQ2Assert = (mask != 0);
        }
    }
}