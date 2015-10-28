using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace WpfApplication1
{

    public class Timelogger : Stopwatch
    {
        public string filename;

        public Timelogger(string _filename, string header)
        {
            filename = @"C:\Users\Joe\Documents\fftmusic\music\" + _filename;

            //create the file if not exists
            if (!File.Exists(filename))
            {
                using (StreamWriter stream = File.CreateText(filename))
                {
                    stream.WriteLine(header);
                }
            }

        }

        public override string ToString()
        {
            TimeSpan ts = Elapsed;
            return String.Format("{0:00}.{1:00}", ts.Hours * 3600 + ts.Minutes * 50 + ts.Seconds, ts.Milliseconds / 10);
        }

        public void log() { log(true); }

        public void log(Boolean writeConsole)
        {
            TimeSpan ts = Elapsed;
            string time = String.Format("{0:00}.{1:00}", ts.Hours * 3600 + ts.Minutes * 50 + ts.Seconds, ts.Milliseconds / 10);
            using (StreamWriter stream = File.AppendText(filename))
            {
                stream.WriteLine(time);
            }
            if (writeConsole)
                Console.WriteLine(time + " added to " + filename);
        }
    }

    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;

        public Timelogger time;

        public timefreq(float[] x, int windowSamp)
        {
            //time = new Timelogger(@"fft\splitLoopRefactored.txt", "time in fft split loop sequential refactored");
            int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];
            for (ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }

            timeFreqData = new float[wSamp/2][];

            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];
            for (int kk = 0; kk < nearest; kk++)
            {
                if (kk < x.Length)
                {
                    compX[kk] = x[kk];
                }
                else
                {
                    compX[kk] = Complex.Zero;
                }
            }


            int cols = 2 * nearest /wSamp;

            for (int jj = 0; jj < wSamp / 2; jj++)
            {
                timeFreqData[jj] = new float[cols];
            }

            timeFreqData = stft(compX, wSamp);


        }

        float[][] stft(Complex[] x, int wSamp)
        {

            Timelogger totalRuntime = new Timelogger(@"totalTime\invokeSometimes256.txt", "time for freqDomain() with fft invoke down to N=256");
            totalRuntime.Start();
            int N = x.Length;
            float fftMax = 0;
            float[] fftMaxes = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            
            float[][] Y = new float[wSamp / 2][];
            float[][] Z = new float[wSamp / 2][];

            for (int ll = 0; ll < wSamp / 2; ll++)
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
                Z[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            }



            //Timelogger sw = new Timelogger(@"fft\fftSplitParallel.txt", "time for fft with splitting loop paralellised");
            for (int ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ++ii)
            //Parallel.For(0, (long)(2 * Math.Floor((double)N / (double)wSamp) - 1), ii =>
            {

                Complex[] temp = new Complex[wSamp];
                Complex[] tempFFT = new Complex[wSamp];

                for (int jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = x[ii * (wSamp / 2) + jj];
                }

                //sw.Start();
                tempFFT = fft(temp);
                //sw.Stop();

                //#TODO parallelisable if you can keep track of highest
                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    if (Y[kk][ii] > fftMax)
                    {
                        fftMax = Y[kk][ii];
                    }
                }


            }
            //sw.log();
            //time.log();

            for (int ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {
                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            }

            totalRuntime.Stop();
            totalRuntime.log();
            return Y;
        }


        //#TODO check for parallelism, nested loops
        public Complex[] fft(Complex[] x)
        {
            int N = x.Length;

            Complex[] Y = new Complex[N];

            // NEED TO MEMSET TO ZERO?

            if (N == 1)
            {
                Y[0] = x[0];
            }
            else{

                Complex[] E = new Complex[N/2];
                Complex[] O = new Complex[N/2];
                Complex[] even = new Complex[N/2];
                Complex[] odd = new Complex[N/2];

                //this for loop and the next fft calls are inherently separable
                //DON"T PARALLEL THIS
                //time.Start();

                //if it's coarse enough, parallelise
                if (N >256)
                {

                    Parallel.Invoke(() =>
                    {
                        for (int ii = 0; ii < N; ii += 2)
                        {
                            even[ii / 2] = x[ii];
                        }

                        E = fft(even);
                    }, () =>
                    {
                        for (int ii = 1; ii < N; ii += 2)
                        {
                            odd[ii / 2] = x[ii];
                        }
                        O = fft(odd);
                    });
                }
                else
                {
                    for (int ii = 0; ii < N / 2; ++ii)
                    {
                        even[ii] = x[2 * ii];
                        odd[ii] = x[2 * ii + 1];
                    }
                    E = fft(even);
                    O = fft(odd);
                }

                //DON"T PARALLEL THIS
                for (int kk = 0; kk < N; kk++){
                    Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * wSamp / N];
                }
            }

           return Y;
        }
        
    }
}
