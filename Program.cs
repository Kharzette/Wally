using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Diagnostics;


namespace Wally
{
	class Program
	{
		enum WallOptions
		{
			none, wallpaper, centered, scaled, stretched, zoom, spanned
		};

		const string	SetDTop			="set org.gnome.desktop.background";
		const string	ChangeCommand	="picture-uri";
		const string	PicOptions		="picture-options";
		const string	DTopIcons		="show-desktop-icons";
		
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

			if(!Directory.Exists(wallPath))
			{
				Console.WriteLine("Path: " + wallPath + " does not exist...");
				return;
			}
			
			DirectoryInfo   di  =new DirectoryInfo(wallPath);
			FileInfo[]      fij		=di.GetFiles("*.jpg", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			FileInfo[]      fij2	=di.GetFiles("*.jpeg", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			FileInfo[]      fip		=di.GetFiles("*.png", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
//			FileInfo[]      fig		=di.GetFiles("*.gif", bRecurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

			//combine into fij
			fij.Concat(fij2);
			fij.Concat(fip);
//			fij.Concat(fig);

			if(fij.Length < 1)
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
			t.mFiles	=fij;
			t.mIndex	=0;

			t.Start();

			//usually will want zoom
			SetPicOptions(WallOptions.zoom);
			SetShowIcons(true);

			Console.WriteLine("Press any key to quit...");
			Console.ReadKey();
//			Console.Read();
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

//			Console.WriteLine("Args: " + args);

			proc.Start();

			string	res	=proc.StandardOutput.ReadToEnd();

//			Console.WriteLine("Response: " + res);

			proc.WaitForExit();

//			Console.WriteLine("Process Exited...");

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
//			Console.WriteLine("Timer tic...");
			WallyTimer  wt  =sender as WallyTimer;

			if(wt.mbShuffle)
			{
				wt.mIndex	=wt.mRand.Next(wt.mFiles.Length);
			}
			else
			{
				wt.mIndex++;
			}

			if(wt.mIndex >= wt.mFiles.Length)
			{
				wt.mIndex	=0;
			}

			//run with the change command
			DoGSCommand(SetDTop + " " + ChangeCommand + " \""
				+ wt.mFiles[wt.mIndex].FullName + "\"");
		}
	}
}
