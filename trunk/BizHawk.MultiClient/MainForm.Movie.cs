﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BizHawk.MultiClient
{
	partial class MainForm
	{
		public bool ReadOnly = true;	//Global Movie Read only setting

		public void StartNewMovie(Movie m, bool record)
		{
			Global.MovieSession = new MovieSession();
			Global.MovieSession.Movie = m;
			RewireInputChain();

			LoadRom(Global.MainForm.CurrentlyOpenRom);
			if (!record)
				Global.MovieSession.Movie.LoadMovie();
			Global.Config.RecentMovies.Add(m.Filename);
			if (Global.MovieSession.Movie.StartsFromSavestate)
			{
				LoadStateFile(Global.MovieSession.Movie.Filename, Path.GetFileName(Global.MovieSession.Movie.Filename));
				Global.Emulator.ResetFrameCounter();
			}
			if (record)
			{
				Global.MovieSession.Movie.StartNewRecording();
				ReadOnly = false;
			}
			else
			{
				Global.MovieSession.Movie.StartPlayback();
			}
			SetMainformMovieInfo();
			TAStudio1.Restart();
		}

		public void SetMainformMovieInfo()
		{
			if (Global.MovieSession.Movie.Mode == MOVIEMODE.PLAY || Global.MovieSession.Movie.Mode == MOVIEMODE.FINISHED)
			{
				Text = DisplayNameForSystem(Global.Game.System) + " - " + Global.Game.Name + " - " + Path.GetFileName(Global.MovieSession.Movie.Filename);
				PlayRecordStatus.Image = BizHawk.MultiClient.Properties.Resources.Play;
				PlayRecordStatus.ToolTipText = "Movie is in playback mode";
			}
			else if (Global.MovieSession.Movie.Mode == MOVIEMODE.RECORD)
			{
				Text = DisplayNameForSystem(Global.Game.System) + " - " + Global.Game.Name + " - " + Path.GetFileName(Global.MovieSession.Movie.Filename);
				PlayRecordStatus.Image = BizHawk.MultiClient.Properties.Resources.RecordHS;
				PlayRecordStatus.ToolTipText = "Movie is in record mode";
			}
			else
			{
				Text = DisplayNameForSystem(Global.Game.System) + " - " + Global.Game.Name;
				PlayRecordStatus.Image = BizHawk.MultiClient.Properties.Resources.Blank;
				PlayRecordStatus.ToolTipText = "";
			}
		}

		public bool MovieActive()
		{
			if (Global.MovieSession.Movie.Mode != MOVIEMODE.INACTIVE)
				return true;
			else
				return false;
		}

		public void PlayMovie()
		{
			PlayMovie p = new PlayMovie();
			DialogResult d = p.ShowDialog();
		}

		public void RecordMovie()
		{
			RecordMovie r = new RecordMovie();
			r.ShowDialog();
		}

		public void PlayMovieFromBeginning()
		{
			if (Global.MovieSession.Movie.Mode != MOVIEMODE.INACTIVE)
			{
				LoadRom(CurrentlyOpenRom);
				if (Global.MovieSession.Movie.StartsFromSavestate)
				{
					LoadStateFile(Global.MovieSession.Movie.Filename, Path.GetFileName(Global.MovieSession.Movie.Filename));
					Global.Emulator.ResetFrameCounter();
				}
				Global.MovieSession.Movie.StartPlayback();
				SetMainformMovieInfo();
				Global.OSD.AddMessage("Replaying movie file in read-only mode");
				Global.MainForm.ReadOnly = true;
			}
		}

		public void StopMovie()
		{
			string message = "Movie ";
			if (Global.MovieSession.Movie.Mode == MOVIEMODE.RECORD)
				message += "recording ";
			else if (Global.MovieSession.Movie.Mode == MOVIEMODE.PLAY
				|| Global.MovieSession.Movie.Mode == MOVIEMODE.FINISHED)
				message += "playback ";
			message += "stopped.";
			if (Global.MovieSession.Movie.Mode != MOVIEMODE.INACTIVE)
			{
				Global.MovieSession.Movie.StopMovie();
				Global.OSD.AddMessage(message);
				SetMainformMovieInfo();
				Global.MainForm.ReadOnly = true;
			}
		}

		private bool HandleMovieLoadState(string path)
		{
			//Note, some of the situations in these IF's may be identical and could be combined but I intentionally separated it out for clarity
			if (Global.MovieSession.Movie.Mode == MOVIEMODE.INACTIVE)
			{
				return true;
			}
			
			else if (Global.MovieSession.Movie.Mode == MOVIEMODE.RECORD)
			{
				
				if (ReadOnly)
				{
					if (!Global.MovieSession.Movie.CheckTimeLines(path, false))
					{
						return false;	//Timeline/GUID error
					}
					else
					{
						Global.MovieSession.Movie.WriteMovie();
						Global.MovieSession.Movie.StartPlayback();
						SetMainformMovieInfo();
					}
				}
				else
				{
					if (!Global.MovieSession.Movie.CheckTimeLines(path, true))
					{
						return false;	//GUID Error
					}
					Global.MovieSession.Movie.LoadLogFromSavestateText(path);
				}
			}

			else if (Global.MovieSession.Movie.Mode == MOVIEMODE.PLAY)
			{
				if (ReadOnly)
				{
					if (!Global.MovieSession.Movie.CheckTimeLines(path, false))
					{
						return false;	//Timeline/GUID error
					}
					//Frame loop automatically handles the rewinding effect based on Global.Emulator.Frame so nothing else is needed here
				}
				else
				{
					if (!Global.MovieSession.Movie.CheckTimeLines(path, true))
					{
						return false;	//GUID Error
					}
					Global.MovieSession.Movie.ResumeRecording();
					SetMainformMovieInfo();
					Global.MovieSession.Movie.LoadLogFromSavestateText(path);
				}
			}
			else if (Global.MovieSession.Movie.Mode == MOVIEMODE.FINISHED)
			{
				if (ReadOnly)
				{
					{
						if (!Global.MovieSession.Movie.CheckTimeLines(path, false))
						{
							return false;	//Timeline/GUID error
						}
						else if (Global.MovieSession.Movie.Mode == MOVIEMODE.FINISHED) //TimeLine check can change a movie to finished, hence the check here (not a good design)
						{
							Global.MovieSession.LatchInputFromPlayer(Global.MovieInputSourceAdapter);
						}
						else
						{
							Global.MovieSession.Movie.StartPlayback();
							SetMainformMovieInfo();
						}
					}
				}
				else
				{
					{
						if (!Global.MovieSession.Movie.CheckTimeLines(path, true))
						{
							return false;	//GUID Error
						}
						else if (Global.MovieSession.Movie.Mode == MOVIEMODE.FINISHED)
						{
							Global.MovieSession.LatchInputFromPlayer(Global.MovieInputSourceAdapter);
						}
						else
						{
							Global.MovieSession.Movie.StartNewRecording();
							SetMainformMovieInfo();
							Global.MovieSession.Movie.LoadLogFromSavestateText(path);
						}
					}
				}
			}
			return true;
		}

		private void HandleMovieSaveState(StreamWriter writer)
		{
			if (Global.MovieSession.Movie.Mode != MOVIEMODE.INACTIVE)
			{
				Global.MovieSession.Movie.DumpLogIntoSavestateText(writer);
			}
		}

		private void HandleMovieOnFrameLoop()
		{
			switch (Global.MovieSession.Movie.Mode)
			{
				case MOVIEMODE.RECORD:
					Global.MovieSession.Movie.CaptureState();
					if (Global.MovieSession.MultiTrack.IsActive)
					{
						Global.MovieSession.LatchMultitrackPlayerInput(Global.MovieInputSourceAdapter, Global.MultitrackRewiringControllerAdapter);
					}
					else
					{
						Global.MovieSession.LatchInputFromPlayer(Global.MovieInputSourceAdapter);
					}
					//the movie session makes sure that the correct input has been read and merged to its MovieControllerAdapter;
					//this has been wired to Global.MovieOutputHardpoint in RewireInputChain
					Global.MovieSession.Movie.CommitFrame(Global.Emulator.Frame, Global.MovieOutputHardpoint);
					break;
				case MOVIEMODE.PLAY:
					int x = Global.MovieSession.Movie.TotalFrames;
					if (Global.Emulator.Frame >= Global.MovieSession.Movie.TotalFrames)
					{
						Global.MovieSession.Movie.SetMovieFinished();
					}
					else
					{
						Global.MovieSession.Movie.CaptureState();
						Global.MovieSession.LatchInputFromLog();
					}
					x++;
					break;
				case MOVIEMODE.FINISHED:
					int xx = Global.MovieSession.Movie.TotalFrames;
					if (Global.Emulator.Frame < Global.MovieSession.Movie.TotalFrames) //This scenario can happen from rewinding (suddenly we are back in the movie, so hook back up to the movie
					{
						Global.MovieSession.Movie.StartPlayback();
						Global.MovieSession.LatchInputFromLog();
					}
					else
					{
						Global.MovieSession.LatchInputFromPlayer(Global.MovieInputSourceAdapter);
					}
					xx++;
					break;
				case MOVIEMODE.INACTIVE:
					Global.MovieSession.LatchInputFromPlayer(Global.MovieInputSourceAdapter);
					break;
			}

			//adelikat; Scheduled for deletion:  RestoreReadWriteOnStop,  should just be a type of movie finished, we need a menu item for what to do when a movie finishes (closes, resumes recording, goes into finished mode)
			//if (StopOnFrame != -1 && StopOnFrame == Global.Emulator.Frame + 1)
			//{
			//    if (StopOnFrame == Global.MovieSession.Movie.LogLength())
			//    {
			//        Global.MovieSession.Movie.SetMovieFinished();
			//    }
			//    if (Global.MovieSession.Movie.TastudioOn == true)
			//    {
			//        PauseEmulator();
			//        StopOnFrame = -1;
			//    }
			//    if (RestoreReadWriteOnStop == true)
			//    {
			//        Global.MovieSession.Movie.Mode = MOVIEMODE.RECORD;
			//        RestoreReadWriteOnStop = false;
			//    }
			//}
		}
	}
}
