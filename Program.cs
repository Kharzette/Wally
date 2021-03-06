﻿using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;


namespace Wally
{
	class Program
	{
		enum WallOptions
		{
			none, wallpaper, centered, scaled, stretched, zoom, spanned
		};

		const string	SetDTop					="set org.gnome.desktop.background";
		const string	ChangeCommand			="picture-uri";
		const string	PicOptions				="picture-options";
		const string	DTopIcons				="show-desktop-icons";
		const float		SpanRatio				=2.5f;	//greater than
		const float		TileRatio				=1f;	//less than
		const int		WallFolderCheckInterval	=5000;
		
		static void Main(string[] args)
		{
			if(args.Length < 2 || args.Length > 4)
			{
				Console.WriteLine("Usage: Wally IntervalInSeconds WallFolderPath Shuffle(optional) Recurse(optional)");
				return;
			}

			int		interval	=Convert.ToInt32(args[0]);			
			string	wallPath	=args[1];
			bool	bShuffle	=false;
			bool	bRecurse	=false;

			if(args.Length == 3)
			{
				bShuffle	=(args[2] == "Shuffle");
				bRecurse	=(args[2] == "Recurse");
			}
			else if(args.Length == 4)
			{
				bShuffle	=(args[2] == "Shuffle") || (args[3] == "Shuffle");
				bRecurse	=(args[2] == "Recurse") || (args[3] == "Recurse");
			}

			bool	bDir	=false;
			while(!bDir)
			{
				bDir	=Directory.Exists(wallPath);
				if(!bDir)
				{
					Console.WriteLine("Path: " + wallPath + " does not exist.  Waiting...");
					Thread.Sleep(WallFolderCheckInterval);
				}
			}
			
			DirectoryInfo   di  =new DirectoryInfo(wallPath);
			List<FileInfo>	fi		=new List<FileInfo>();
			FileInfo[]      fij		=di.GetFiles("*.jpg", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			FileInfo[]      fij2	=di.GetFiles("*.jpeg", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			FileInfo[]      fip		=di.GetFiles("*.png", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
//			FileInfo[]      fig		=di.GetFiles("*.gif", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

			//combine into fi
			fi.AddRange(fij);
			fi.AddRange(fij2);
			fi.AddRange(fip);
//			fij.Concat(fig);

			if(fi.Count < 1)
			{
				Console.WriteLine("Wall folder has no usable walls!");
				return;
			}

			WallyTimer   t   =new WallyTimer();
			
			t.mRand		=new Random();
			t.mbShuffle	=bShuffle;
			t.Interval	=interval * 1000;	//convert to ms
			t.AutoReset	=true;
			t.Elapsed	+=OnWallTimer;
			t.mFiles	=fi;
			t.mIndex	=0;

			t.Start();

			//make sure icons draw
			SetShowIcons(true);

			Console.WriteLine("Press any key to quit...");

			//if you want to run this from command line and
			//have the option of pressing a key to exit, use
			//this
//			Console.ReadKey();

			//Use this when running from the debugger
//			Console.Read();

			//use this when you want to launch from gnome
			Thread.Sleep(Timeout.Infinite);
		}


		static void DoGSCommand(string args)
		{
			Process	proc	=new Process();

			proc.StartInfo	=new ProcessStartInfo();

			proc.StartInfo.FileName					="/usr/bin/gsettings";
			proc.StartInfo.CreateNoWindow			=true;
			proc.StartInfo.RedirectStandardOutput	=true;
			proc.StartInfo.UseShellExecute			=false;
			proc.StartInfo.Arguments				=args;

			proc.Start();

			string	res	=proc.StandardOutput.ReadToEnd();

			proc.WaitForExit();
			proc.Close();
		}


		static void SetShowIcons(bool bShow)
		{
			DoGSCommand(SetDTop + " " + DTopIcons + " " + bShow.ToString().ToLower());
		}


		static void SetPicOptions(WallOptions wo)
		{
			//run with the options command
			DoGSCommand(SetDTop + " " + PicOptions + " " + wo.ToString());
		}
		
		
		static void OnWallTimer(object sender, EventArgs ea)
		{
			WallyTimer  wt  =sender as WallyTimer;

			bool	bGood	=false;
			string	path	="";

			while(!bGood)
			{
				if(wt.mbShuffle)
				{
					wt.mIndex	=wt.mRand.Next(wt.mFiles.Count);
				}
				else
				{
					wt.mIndex++;
				}

				if(wt.mIndex >= wt.mFiles.Count)
				{
					wt.mIndex	=0;
				}

				//make sure it is still there
				path	=wt.mFiles[wt.mIndex].FullName;
				if(File.Exists(path))
				{
					bGood	=true;
				}
			}

			bool	bSpan	=false;
			bool	bTile	=false;

			//check size if possible TODO: other formats
			if(path.EndsWith(".png"))
			{
				int	width, height;
				if(PNGSizeGrabber.GetSize(path, out width, out height))
				{
					float	ratio	=width / (float)height;
					bSpan	=(ratio > SpanRatio);
					bTile	=(ratio < TileRatio);
				}
			}

			if(bSpan)
			{
				SetPicOptions(WallOptions.spanned);
			}
			else if(bTile)
			{
				SetPicOptions(WallOptions.wallpaper);
			}
			else
			{
				SetPicOptions(WallOptions.zoom);
			}

			//run with the change command
			DoGSCommand(SetDTop + " " + ChangeCommand
				+ " \"" + path + "\"");
		}
	}
}
