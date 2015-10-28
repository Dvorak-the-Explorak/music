﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WpfApplication1
{
    public class wavefile
    {
        public float[] wave;
        public byte[] data;
        public char[] ChunkID = new char[4];
	    public int ChunkSize;
	    public char[] Format = new char[4];
	    public char[] Subchunk1ID = new char[4];
	    public int Subchunk1Size;
	    public short AudioFormat;
	    public short NumChannels;
	    public int SampleRate;
	    public int ByteRate;
	    public short BlockAlign;
	    public short BitsPerSample;
	    public char[] Subchunk2ID = new char[4];
	    public int Subchunk2Size;

        public wavefile(FileStream file)
        {
            BinaryReader binRead = new BinaryReader(file);

            //head data
            ChunkID =  binRead.ReadChars(4);
            ChunkSize = binRead.ReadInt32();
            Format = binRead.ReadChars(4);
            Subchunk1ID = binRead.ReadChars(4);
            Subchunk1Size = binRead.ReadInt32();//in bytes
            AudioFormat = binRead.ReadInt16();
            NumChannels = binRead.ReadInt16();
            SampleRate = binRead.ReadInt32();//samples per second?
            ByteRate = binRead.ReadInt32();
            BlockAlign = binRead.ReadInt16();
            BitsPerSample = binRead.ReadInt16();
            Subchunk2ID = binRead.ReadChars(4);
            Subchunk2Size = binRead.ReadInt32();//in bytes


            int numSamples = Subchunk2Size / (BitsPerSample / 8);
            data = new byte[numSamples];
            wave = new float[numSamples];

            data = binRead.ReadBytes(numSamples);

            for (int i = 0; i < numSamples; i++)
            {
                wave[i] = ((float)data[i] - 128) / 128;
            }

        }
    }
}
