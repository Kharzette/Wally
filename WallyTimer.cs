using System;
using System.Timers;
using System.IO;

internal class WallyTimer : Timer
{
    internal FileInfo   []mFiles;
    internal int        mIndex;
	internal Random		mRand;
	internal bool		mbShuffle;
}