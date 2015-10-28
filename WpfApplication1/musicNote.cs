using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class musicNote
    {
        public enum notePitch { Ab, A_, Bb, B_, C_, Db, D_, Eb, E_, F_, Gb, G_ }
        public int pitch;//defined in semitones with 1 <-> A2
        public double duration;
        public bool flat;
        public double error;
        public int staffPos;//which position on the staff (0 is bottom line, 1 is bottom gap...)
        public int mult;
        public double frequency;

        public musicNote(double freq, double dur)
        {
            frequency = freq;
            duration = dur;
            //A2 is 110MHz (using A4 = 440MHz), this is base pitch
            double freqPitch = (Math.Log((freq / 110), 2) * 12 + 1);

            //if it's closer to the quantized pitch below (or in the middle)
            if ((Math.Ceiling(freqPitch) - freqPitch) >= (freqPitch - Math.Floor(freqPitch)))
            {
                pitch = (int)Math.Floor(freqPitch);
                error = (freqPitch - Math.Floor(freqPitch));
            }
            //if it's closer to the quantized pitch above
            else
            {
                pitch = (int)Math.Ceiling(freqPitch);
                error = (freqPitch - Math.Ceiling(freqPitch));
            }

            //flats are Ab=0, Bb=2, Db=5, Eb=7, Gb=10 (in every octave, so +12 as well)
            if (pitch%12 == 0 || pitch%12 == 2 || pitch%12 == 5 || pitch%12 == 7 || pitch%12 == 10)
            {
                flat = true;
            }

            //Octaves above A2
            mult = (pitch - pitch % 12) / 12;

            //Bottom line is 0, defined to be A2.  staffPos + 7 is an octave up
            switch (pitch%12)
            {
                //Ab and A are on the same position
                case 0:
                    staffPos = 7*mult;
                    break;
                case 1:
                    staffPos = 7 * mult;
                    break;

                //Bb and B
                case 2:
                    staffPos = 1 + 7 * mult;
                    break;
                case 3:
                    staffPos = 1 + 7 * mult;
                    break;

                //C
                case 4:
                    staffPos = 2 + 7 * mult;
                    break;
                
                //Db and D
                case 5:
                    staffPos = 3 + 7 * mult;
                    break;
                case 6:
                    staffPos = 3 + 7 * mult;
                    break;

                //Eb and E
                case 7:
                    staffPos = 4 + 7 * mult;
                    break;
                case 8:
                    staffPos = 4 + 7 * mult;
                    break;

                //F
                case 9:
                    staffPos = 5 + 7 * mult;
                    break;

                //Gb and G
                case 10:
                    staffPos = 6 + 7 * mult;
                    break;
                case 11:
                    staffPos = 6 + 7 * mult;
                    break;
            }
        }
    }

    

}
