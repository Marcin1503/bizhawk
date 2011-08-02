﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

//TODO - reading big files across the network is slow due to the small sector read sizes. make some kind of read-ahead thing
//TODO - add lead-in generation (so we can clarify the LBA addresses and have a place to put subcode perhaps)
//the bin dumper will need to start at LBA 150

//http://www.pctechguide.com/iso-9660-data-format-for-cds-cd-roms-cd-rs-and-cd-rws
//http://linux.die.net/man/1/cue2toc

//apparently cdrdao is the ultimate linux tool for doing this stuff but it doesnt support DAO96 (or other DAO modes) that would be necessary to extract P-Q subchannels
//(cdrdao only supports R-W)

//here is a featureset list of windows cd burning programs (useful for cuesheet compatibility info)
//http://www.dcsoft.com/cue_mastering_progs.htm

//good
//http://linux-sxs.org/bedtime/cdapi.html
//http://en.wikipedia.org/wiki/Track_%28CD%29
//http://docs.google.com/viewer?a=v&q=cache:imNKye05zIEJ:www.13thmonkey.org/documentation/SCSI/mmc-r10a.pdf+q+subchannel+TOC+format&hl=en&gl=us&pid=bl&srcid=ADGEEShtYqlluBX2lgxTL3pVsXwk6lKMIqSmyuUCX4RJ3DntaNq5vI2pCvtkyze-fumj7vvrmap6g1kOg5uAVC0IxwU_MRhC5FB0c_PQ2BlZQXDD7P3GeNaAjDeomelKaIODrhwOoFNb&sig=AHIEtbRXljAcFjeBn3rMb6tauHWjSNMYrw
//r:\consoles\~docs\yellowbook
//http://digitalx.org/cue-sheet/examples/
//

//"qemu cdrom emulator"
//http://www.koders.com/c/fid7171440DEC7C18B932715D671DEE03743111A95A.aspx
 
//less good
//http://www.cyberciti.biz/faq/getting-volume-information-from-cds-iso-images/
//http://www.cims.nyu.edu/cgi-systems/man.cgi?section=7I&topic=cdio

//ideas:
/*
 * do some stuff asynchronously. for example, decoding mp3 sectors.
 * keep a list of 'blobs' (giant bins or decoded wavs likely) which can reference the disk
 * keep a list of sectors and the blob/offset from which they pull -- also whether the sector is available
 * if it is not available and something requests it then it will have to block while that sector gets generated
 * perhaps the blobs know how to resolve themselves and the requested sector can be immediately resolved (priority boost)
 * mp3 blobs should be hashed and dropped in %TEMP% as a wav decode
*/

//here is an MIT licensed C mp3 decoder
//http://core.fluendo.com/gstreamer/src/gst-fluendo-mp3/

/*information on saturn TOC and session data structures is on pdf page 58 of System Library User's Manual;
 * as seen in yabause, there are 1000 u32s in this format:
 * Ctrl[4bit] Adr[4bit] StartFrameAddressFAD[24bit] (nonexisting tracks are 0xFFFFFFFF)
 * Followed by Fist Track Information, Last Track Information..
 * Ctrl[4bit] Adr[4bit] FirstTrackNumber/LastTrackNumber[8bit] and then some stuff I dont understand
 * ..and Read Out Information:
 * Ctrl[4bit] Adr[4bit] ReadOutStartFrameAddress[24bit]
 * 
 * Also there is some stuff about FAD of sessions.
 * This should be generated by the saturn core, but we need to make sure we pass down enough information to do it
*/

//2048 bytes packed into 2352: 
//12 bytes sync(00 ff ff ff ff ff ff ff ff ff ff 00)
//3 bytes sector address (min+A0),sec,frac //does this correspond to ccd `point` field in the TOC entries?
//sector mode byte (0: silence; 1: 2048Byte mode (EDC,ECC,CIRC), 2: 2352Byte mode (CIRC only)
//user data: 2336 bytes
//cue sheets may use mode1_2048 (and the error coding needs to be regenerated to get accurate raw data) or mode1_2352 (the entire sector is present)
//mode2_2352 is the only kind of mode2, by necessity
//audio is a different mode, seems to be just 2352 bytes with no sync, header or error correction. i guess the CIRC error correction is still there

namespace BizHawk.Disc
{
	public partial class Disc
	{
		//TODO - separate these into Read_2352 and Read_2048 (optimizations can be made by ISector implementors depending on what is requested)
		//(for example, avoiding the 2048 byte sector creating the ECC data and then immediately discarding it)
		public interface ISector
		{
			int Read(byte[] buffer, int offset);
		}

		public interface IBlob
		{
			int Read(long byte_pos, byte[] buffer, int offset, int count);
			void Dispose();
		}

		class Blob_RawFile : IBlob
		{
			public string PhysicalPath { 
				get { return physicalPath; }
				set
				{
					physicalPath = value;
					length = new FileInfo(physicalPath).Length;
				}
			}
			string physicalPath;
			long length;

			public long Offset;

			BufferedStream fs;
			public void Dispose()
			{
				if (fs != null)
				{
					fs.Dispose();
					fs = null;
				}
			}
			public int Read(long byte_pos, byte[] buffer, int offset, int count)
			{
				//use quite a large buffer, because normally we will be reading these sequentially
				const int buffersize = 2352 * 75 * 2;
				if (fs == null)
					fs = new BufferedStream(new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read), buffersize);
				long target = byte_pos + Offset;
				if(fs.Position != target)
					fs.Position = target;
				return fs.Read(buffer, offset, count);
			}
			public long Length
			{
				get
				{
					return length;
				}
			}
		}

		class Sector_RawSilence : ISector
		{
			public int Read(byte[] buffer, int offset)
			{
				Array.Clear(buffer, 0, 2352);
				return 2352;
			}
		}

		class Sector_RawBlob : ISector
		{
			public IBlob Blob;
			public long Offset;
			public int Read(byte[] buffer, int offset)
			{
				return Blob.Read(Offset, buffer, offset, 2352);
			}
		}

		class Sector_Zero : ISector
		{
			public int Read(byte[] buffer, int offset)
			{
				for (int i = 0; i < 2352; i++)
					buffer[offset + i] = 0;
				return 2352;
			}
		}

		class Sector_ZeroPad : ISector
		{
			public ISector BaseSector;
			public int BaseLength;
			public int Read(byte[] buffer, int offset)
			{
				int read = BaseSector.Read(buffer, offset);
				if(read < BaseLength) return read;
				for (int i = BaseLength; i < 2352; i++)
					buffer[offset + i] = 0;
				return 2352;
			}
		}

		class Sector_Raw : ISector
		{
			public ISector BaseSector;
			public int Read(byte[] buffer, int offset)
			{
				return BaseSector.Read(buffer, offset);
			}
		}

		protected static byte BCD_Byte(byte val)
		{
			byte ret = (byte)(val % 10);
			ret += (byte)(16 * (val / 10));
			return ret;
		}

		//a blob that also has an ECM cache associated with it. maybe one day.
		class ECMCacheBlob
		{
			public ECMCacheBlob(IBlob blob)
			{
				BaseBlob = blob;
			}
			public IBlob BaseBlob;
		}

		class Sector_Mode1_2048 : ISector
		{
			public Sector_Mode1_2048(int LBA)
			{
				byte lba_min = (byte)(LBA / 60 / 75);
				byte lba_sec = (byte)((LBA / 75) % 60);
				byte lba_frac = (byte)(LBA % 75);
				bcd_lba_min = BCD_Byte(lba_min);
				bcd_lba_sec = BCD_Byte(lba_sec);
				bcd_lba_frac = BCD_Byte(lba_frac);
			}
			byte bcd_lba_min, bcd_lba_sec, bcd_lba_frac;

			public ECMCacheBlob Blob;
			public long Offset;
			byte[] extra_data;
			bool has_extra_data;
			public int Read(byte[] buffer, int offset)
			{
				//user data
				int read = Blob.BaseBlob.Read(Offset, buffer, offset + 16, 2048);

				//if we read the 2048 physical bytes OK, then return the complete sector
				if (read == 2048 && has_extra_data)
				{
					Buffer.BlockCopy(extra_data, 0, buffer, offset, 16);
					Buffer.BlockCopy(extra_data, 16, buffer, offset + 2064, 4 + 8 + 172 + 104);
					return 2352;
				}

				//sync
				buffer[offset + 0] = 0x00; buffer[offset + 1] = 0xFF; buffer[offset + 2] = 0xFF; buffer[offset + 3] = 0xFF;
				buffer[offset + 4] = 0xFF; buffer[offset + 5] = 0xFF; buffer[offset + 6] = 0xFF; buffer[offset + 7] = 0xFF;
				buffer[offset + 8] = 0xFF; buffer[offset + 9] = 0xFF; buffer[offset + 10] = 0xFF; buffer[offset + 11] = 0x00;
				//sector address
				buffer[offset + 12] = bcd_lba_min;
				buffer[offset + 13] = bcd_lba_sec;
				buffer[offset + 14] = bcd_lba_frac;
				//mode 1
				buffer[offset + 15] = 1;
				//EDC
				ECM.edc_computeblock(buffer, offset+2064, buffer, offset+2064);
				//intermediate
				for (int i = 0; i < 8; i++) buffer[offset + 2068 + i] = 0;
				//ECC
				ECM.ecc_generate(buffer, offset, false, buffer, offset+2076);
				
				//if we read the 2048 physical bytes OK, then return the complete sector
				if (read == 2048)
				{
					extra_data = new byte[16 + 4 + 8 + 172 + 104];
					Buffer.BlockCopy(buffer, 0, extra_data, 0, 16);
					Buffer.BlockCopy(buffer, 2064, extra_data, 16, 4 + 8 + 172 + 104);
					has_extra_data = true;
					return 2352;
				}
				//otherwise, return a smaller value to indicate an error
				else return read;
			}
		}

		//this is a physical 2352 byte sector.
		public class SectorEntry
		{
			public SectorEntry(ISector sec) { this.Sector = sec; }
			public ISector Sector;
		}

		public List<IBlob> Blobs = new List<IBlob>();
		public List<SectorEntry> Sectors = new List<SectorEntry>();
		public DiscTOC TOC = new DiscTOC();

		void FromIsoPathInternal(string isoPath)
		{
			var session = new DiscTOC.Session();
			session.num = 1;
			TOC.Sessions.Add(session);
			var track = new DiscTOC.Track();
			track.num = 1;
			session.Tracks.Add(track);
			var index = new DiscTOC.Index();
			index.num = 0;
			track.Indexes.Add(index);
			index = new DiscTOC.Index();
			index.num = 1;
			track.Indexes.Add(index);

			var fiIso = new FileInfo(isoPath);
			Blob_RawFile blob = new Blob_RawFile();
			blob.PhysicalPath = fiIso.FullName;
			Blobs.Add(blob);
			int num_lba = (int)(fiIso.Length / 2048);
			track.length_lba = num_lba;
			if (fiIso.Length % 2048 != 0)
				throw new InvalidOperationException("invalid iso file (size not multiple of 2048)");
			//TODO - handle this with Final Fantasy 9 cd1.iso

			var ecmCacheBlob = new ECMCacheBlob(blob);
			for (int i = 0; i < num_lba; i++)
			{
				Sector_Mode1_2048 sector = new Sector_Mode1_2048(i+150);
				sector.Blob = ecmCacheBlob;
				sector.Offset = i * 2048;
				Sectors.Add(new SectorEntry(sector));
			}

			TOC.AnalyzeLengthsFromIndexLengths();
		}

		
		public CueBin DumpCueBin(string baseName, CueBinPrefs prefs)
		{
			if (TOC.Sessions.Count > 1)
				throw new NotSupportedException("can't dump cue+bin with more than 1 session yet");

			CueBin ret = new CueBin();
			ret.baseName = baseName;
			ret.disc = this;

			if (!prefs.OneBinPerTrack)
			{
				string cue = TOC.GenerateCUE(prefs);
				var bfd = new CueBin.BinFileDescriptor();
				bfd.name = baseName + ".bin";
				ret.cue = string.Format("FILE \"{0}\" BINARY\n", bfd.name) + cue;
				ret.bins.Add(bfd);
				for (int i = 0; i < TOC.length_lba; i++)
				{
					bfd.lbas.Add(i+150);
					bfd.lba_zeros.Add(false);
				}
			}
			else
			{
				StringBuilder sbCue = new StringBuilder();
				
				for (int i = 0; i < TOC.Sessions[0].Tracks.Count; i++)
				{
					var track = TOC.Sessions[0].Tracks[i];
					var bfd = new CueBin.BinFileDescriptor();
					bfd.name = baseName + string.Format(" (Track {0:D2}).bin", track.num);
					ret.bins.Add(bfd);
					int lba=0;

					for (; lba < track.length_lba; lba++)
					{
						int thislba = track.Indexes[0].lba + lba;
						bfd.lbas.Add(thislba + 150);
						bfd.lba_zeros.Add(false);
					}
					sbCue.AppendFormat("FILE \"{0}\" BINARY\n", bfd.name);

					sbCue.AppendFormat("  TRACK {0:D2} {1}\n", track.num, Cue.TrackTypeStringForTrackType(track.TrackType));
					foreach (var index in track.Indexes)
					{
						int x = index.lba - track.Indexes[0].lba;
						sbCue.AppendFormat("    INDEX {0:D2} {1}\n", index.num, new Cue.CueTimestamp(x).Value);
					}
				}

				ret.cue = sbCue.ToString();
			}

			return ret;
		}

		public void DumpBin_2352(string binPath)
		{
			byte[] temp = new byte[2352];
			using(FileStream fs = new FileStream(binPath,FileMode.Create,FileAccess.Write,FileShare.None))
				for (int i = 0; i < Sectors.Count; i++)
				{
					ReadLBA_2352(150+i, temp, 0);
					fs.Write(temp, 0, 2352);
				}
		}

		public static Disc FromCuePath(string cuePath)
		{
			var ret = new Disc();
			ret.FromCuePathInternal(cuePath);
			return ret;
		}

		public static Disc FromIsoPath(string isoPath)
		{
			var ret = new Disc();
			ret.FromIsoPathInternal(isoPath);
			return ret;
		}
	}

	public enum ETrackType
	{
		Mode1_2352,
		Mode1_2048,
		Mode2_2352,
		Audio
	}


	public class CueBinPrefs
	{
		/// <summary>
		/// Controls general operations: should the output be split into several bins, or just use one?
		/// </summary>
		public bool OneBinPerTrack;

		/// <summary>
		/// turn this on to dump bins instead of just cues
		/// </summary>
		public bool ReallyDumpBin;

		/// <summary>
		/// generate remarks and other annotations to help humans understand whats going on, but which will confuse many cue parsers
		/// </summary>
		public bool AnnotateCue;

		/// <summary>
		/// you may find that some cue parsers are upset by index 00
		/// if thats the case, then we can emit pregaps instead.
		/// you might also want to use this to save disk space (without pregap commands, the pregap must be stored as empty sectors)
		/// </summary>
		public bool PreferPregapCommand = false;

		/// <summary>
		/// some cue parsers cant handle sessions. better not emit a session command then. multi-session discs will then be broken
		/// </summary>
		public bool SingleSession;
	}

	/// <summary>
	/// Encapsulates an in-memory cue+bin (complete cuesheet and a little registry of files)
	/// it will be based on a disc (fro mwhich it can read sectors to avoid burning through extra memory)
	/// TODO - we must merge this with whatever reads in cue+bin
	/// </summary>
	public class CueBin
	{
		public string cue;
		public string baseName;
		public Disc disc;

		public class BinFileDescriptor
		{
			public string name;
			public List<int> lbas = new List<int>();
			public List<bool> lba_zeros = new List<bool>();
		}

		public List<BinFileDescriptor> bins = new List<BinFileDescriptor>();

		public string CreateRedumpReport()
		{
			if (disc.TOC.Sessions[0].Tracks.Count != bins.Count)
				throw new InvalidOperationException("Cannot generate redump report on CueBin lacking OneBinPerTrack property");
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < disc.TOC.Sessions[0].Tracks.Count; i++)
			{
				var track = disc.TOC.Sessions[0].Tracks[i];
				var bfd = bins[i];
				
				//dump the track
				byte[] dump = new byte[track.length_lba * 2352];
				for (int lba = 0; lba < track.length_lba; lba++)
					disc.ReadLBA_2352(bfd.lbas[lba],dump,lba*2352);
				string crc32 = string.Format("{0:X8}", CRC32.Calculate(dump));
				string md5 = Util.Hash_MD5(dump, 0, dump.Length);
				string sha1 = Util.Hash_SHA1(dump, 0, dump.Length);

				int pregap = track.Indexes[1].lba - track.Indexes[0].lba;
				Cue.CueTimestamp pregap_ts = new Cue.CueTimestamp(pregap);
				Cue.CueTimestamp len_ts = new Cue.CueTimestamp(track.length_lba);
				sb.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\n", 
					i, 
					Cue.RedumpTypeStringForTrackType(track.TrackType),
					pregap_ts.Value,
					len_ts.Value,
					track.length_lba,
					track.length_lba*Cue.BINSectorSizeForTrackType(track.TrackType),
					crc32,
					md5,
					sha1
					);
			}
			return sb.ToString();
		}

		public void Dump(string directory, CueBinPrefs prefs)
		{
			string cuePath = Path.Combine(directory, baseName + ".cue");
			File.WriteAllText(cuePath, cue);
			if(prefs.ReallyDumpBin)
				foreach (var bfd in bins)
				{
					byte[] temp = new byte[2352];
					byte[] empty = new byte[2352];
					string trackBinFile = bfd.name;
					string trackBinPath = Path.Combine(directory, trackBinFile);
					using (FileStream fs = new FileStream(trackBinPath, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						for(int i=0;i<bfd.lbas.Count;i++)
						{
							int lba = bfd.lbas[i];
							if (bfd.lba_zeros[i])
							{
								fs.Write(empty, 0, 2352);
							}
							else
							{
								disc.ReadLBA_2352(lba, temp, 0);
								fs.Write(temp, 0, 2352);
							}
						}
					}
				}
		}
	}
}