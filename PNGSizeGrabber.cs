using System;
using System.IO;
using System.Buffers.Binary;


internal class PNGSizeGrabber
{
	internal static bool	GetSize(string filePath, out int x, out int y)
	{
		x	=y	=0;

		FileStream	fs	=new FileStream(filePath, FileMode.Open, FileAccess.Read);
		if(fs == null)
		{
			return	false;
		}

		BinaryReader	br	=new BinaryReader(fs);
		if(br == null)
		{
			return	false;
		}

		UInt64	sig	=br.ReadUInt64();
		if(sig != 0xA1A0A0D474E5089)
		{
			br.Close();
			fs.Close();
			return	false;
		}

		UInt32	chunkLen	=br.ReadUInt32();

		//IHDR chunk
		UInt32	ihdr	=br.ReadUInt32();
		if(ihdr != 0x52444849)
		{
			br.Close();
			fs.Close();
			return	false;
		}

		x	=br.ReadInt32();
		y	=br.ReadInt32();

		//not sure why these are backwards
		x	=BinaryPrimitives.ReverseEndianness(x);
		y	=BinaryPrimitives.ReverseEndianness(y);

		br.Close();
		fs.Close();

		return	true;
	}

}