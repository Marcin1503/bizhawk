﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace BizHawk.Emulation.Cores.Nintendo.N64.NativeApi
{
	class mupen64plusVideoApi
	{
		IntPtr GfxDll;// Graphics plugin specific

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		/// <summary>
		/// Fills a provided buffer with the mupen64plus framebuffer
		/// </summary>
		/// <param name="framebuffer">The buffer to fill</param>
		/// <param name="width">A pointer to a variable to fill with the width of the framebuffer</param>
		/// <param name="height">A pointer to a variable to fill with the height of the framebuffer</param>
		/// <param name="buffer">Which buffer to read: 0 = front, 1 = back</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void ReadScreen2(int[] framebuffer, ref int width, ref int height, int buffer);
		ReadScreen2 GFXReadScreen2;

		/// <summary>
		/// Gets the width and height of the mupen64plus framebuffer
		/// </summary>
		/// <param name="dummy">Use IntPtr.Zero</param>
		/// <param name="width">A pointer to a variable to fill with the width of the framebuffer</param>
		/// <param name="height">A pointer to a variable to fill with the height of the framebuffer</param>
		/// <param name="buffer">Which buffer to read: 0 = front, 1 = back</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void ReadScreen2Res(IntPtr dummy, ref int width, ref int height, int buffer);
		ReadScreen2Res GFXReadScreen2Res;

		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate Int32 GetScreenTextureID();
		GetScreenTextureID GFXGetScreenTextureID;


		public mupen64plusVideoApi(mupen64plusApi core, VideoPluginSettings settings)
		{
			string videoplugin;
			bool jaboReady = false;
			switch (settings.Plugin)
			{
				default:
				case PluginType.Rice:
					videoplugin = "mupen64plus-video-rice.dll";
					break;
				case PluginType.Glide:
					videoplugin = "mupen64plus-video-glide64.dll";
					break;
				case PluginType.GlideMk2:
					videoplugin = "mupen64plus-video-glide64mk2.dll";
					break;
				case PluginType.Jabo:
					videoplugin = "mupen64plus-video-jabo.dll";

					//THIS IS HORRIBLE! PATH MUST BE PASSED IN SOME OTHER WAY
					string dllDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "dll");
					string rawPath = Path.Combine(dllDir, "Jabo_Direct3D8.dll");
					string patchedPath = Path.Combine(dllDir, "Jabo_Direct3D8_patched.dll");

					if (File.Exists(patchedPath))
					{
						byte[] hash = MD5.Create().ComputeHash(File.ReadAllBytes(patchedPath));
						string hash_string = BitConverter.ToString(hash).Replace("-", "");
						if (hash_string == "F4D6E624489CD88C68A5850426D4D70E")
						{
							jaboReady = true;
						}
					}

					if (!jaboReady && File.Exists(rawPath))
					{
						byte[] hash = MD5.Create().ComputeHash(File.ReadAllBytes(rawPath));
						string hash_string = BitConverter.ToString(hash).Replace("-", "");
						if (hash_string == "4F353AA71E7455B81205D8EC0AA339E1")
						{
							byte[] jaboDLL = File.ReadAllBytes(rawPath);
							jaboDLL[583] = 0xA0;
							jaboDLL[623] = 0xA0;
							jaboDLL[663] = 0xA0;
							jaboDLL[703] = 0xA0;
							jaboDLL[743] = 0xA0;
							jaboDLL[783] = 0xA0;
							jaboDLL[823] = 0xA0;
							jaboDLL[863] = 0xA0;
							File.WriteAllBytes(patchedPath, jaboDLL);
							jaboReady = true;
						}
					}

					if (!jaboReady)
					{
						throw new InvalidOperationException(string.Format("Error: Jabo dll was not found. please copy Jabo_Direct3D8.dll from a Project64 v1.6.1 installation into Bizhawk's dll directory."));
					}
					break;
			}

			GfxDll = core.AttachPlugin(mupen64plusApi.m64p_plugin_type.M64PLUGIN_GFX,
				videoplugin);
			GFXReadScreen2 = (ReadScreen2)Marshal.GetDelegateForFunctionPointer(GetProcAddress(GfxDll, "ReadScreen2"), typeof(ReadScreen2));
			GFXReadScreen2Res = (ReadScreen2Res)Marshal.GetDelegateForFunctionPointer(GetProcAddress(GfxDll, "ReadScreen2"), typeof(ReadScreen2Res));
			if(GetProcAddress(GfxDll, "GetScreenTextureID") != IntPtr.Zero)
				GFXGetScreenTextureID = (GetScreenTextureID)Marshal.GetDelegateForFunctionPointer(GetProcAddress(GfxDll, "GetScreenTextureID"), typeof(GetScreenTextureID));
		}

		public void GetScreenDimensions(ref int width, ref int height)
		{
			GFXReadScreen2Res(IntPtr.Zero, ref width, ref height, 0);
		}

		private int[] m64pBuffer = new int[0];
		/// <summary>
		/// This function copies the frame buffer from mupen64plus
		/// </summary>
		public void Getm64pFrameBuffer(int[] buffer, ref int width, ref int height)
		{
			if (m64pBuffer.Length != width * height)
				m64pBuffer = new int[width * height];
			// Actually get the frame buffer
			GFXReadScreen2(m64pBuffer, ref width, ref height, 0);

			// vflip
			int fromindex = width * (height - 1) * 4;
			int toindex = 0;

			for (int j = 0; j < height; j++)
			{
				Buffer.BlockCopy(m64pBuffer, fromindex, buffer, toindex, width * 4);
				fromindex -= width * 4;
				toindex += width * 4;
			}

			// opaque
			unsafe
			{
				fixed (int* ptr = &buffer[0])
				{
					int l = buffer.Length;
					for (int i = 0; i < l; i++)
					{
						ptr[i] |= unchecked((int)0xff000000);
					}
				}
			}
		}
	}


	public class VideoPluginSettings
	{
		public PluginType Plugin;
		//public Dictionary<string, int> IntParameters = new Dictionary<string,int>();
		//public Dictionary<string, string> StringParameters = new Dictionary<string,string>();

		public Dictionary<string, object> Parameters = new Dictionary<string, object>();
		public int Height;
		public int Width;

		public VideoPluginSettings(PluginType Plugin, int Width, int Height)
		{
			this.Plugin = Plugin;
			this.Width = Width;
			this.Height = Height;
		}
	}
}
