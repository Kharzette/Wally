using System;
using System.Timers;
using System.IO;
using System.Collections.Generic;


internal class WallyTimer : Timer
{
	internal List<FileInfo>	mFiles;
	internal int			mIndex;
	internal Random			mRand;
	internal bool			mbShuffle;
}