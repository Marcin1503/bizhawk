﻿namespace BizHawk.Emulation.Sound 
{
    public static class Waves
    {
        public static readonly short[] SquareWave =
            {
                -32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,
                 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767
            };

        public static readonly short[] ImperfectSquareWave =
            {
                -32768,-30145,-27852,-26213,-24902,-23592,-22282,-20971,-19988,-19005,-18350,-17694,-17366,-17039,-16711,-16711,
                 32767, 30145, 27852, 26213, 24902, 23592, 22282, 20971, 19988, 19005, 18350, 17694, 17366, 17039, 16711, 16711
            };

        public static readonly short[] NoiseWave =
            {
                 32767, 32767, 32767,-32768,-32768,-32768, 32767,-32768, 32767, 32767, 32767, 32767,-32768,-32768,-32768,-32768, 32767, 32767,-32768,-32768, 32767, 32767,-32768,-32768, 32767, 32767,-32768,-32768, 32767, 32767,-32768,-32768, 32767,-32768,-32768,
                 32767, 32767,-32768,-32768, 32767, 32767,-32768,-32768, 32767, 32767,-32768, 32767, 32767,-32768, 32767, 32767, 32767,-32768,-32768, 32767, 32767,-32768,-32768, 32767,-32768,-32768,-32768, 32767,-32768,-32768,-32768,-32768, 32767,-32768, 32767,
                 32767, 32767,-32768,-32768,-32768,-32768,-32768, 32767,-32768,-32768, 32767,-32768, 32767,-32768, 32767,-32768, 32767, 32767,-32768,-32768,-32768, 32767,-32768, 32767,-32768,-32768, 32767, 32767,-32768, 32767,-32768,-32768,-32768, 32767,-32768,
                 32767, 32767, 32767, 32767, 32767, 32767, 32767,-32768, 32767, 32767,-32768, 32767, 32767, 32767,-32768,-32768,-32768, 32767,-32768,-32768, 32767,-32768, 32767,-32768, 32767,-32768,-32768,-32768, 32767,-32768, 32767,-32768,-32768, 32767,-32768,
                -32768, 32767, 32767, 32767,-32768, 32767,-32768,-32768,-32768,-32768,-32768,-32768,-32768, 32767,-32768, 32767,-32768, 32767,-32768,-32768, 32767, 32767, 32767,-32768, 32767, 32767,-32768,-32768, 32767, 32767, 32767,-32768, 32767, 32767, 32767,
                -32768,-32768, 32767, 32767, 32767,-32768,-32768,-32768,-32768,-32768, 32767, 32767,-32768,-32768,-32768, 32767,-32768,-32768, 32767, 32767, 32767,-32768,-32768,-32768,-32768,-32768, 32767, 32767, 32767,-32768,-32768,-32768, 32767,-32768, 32767,
                 32767,-32768, 32767, 32767, 32767, 32767, 32767, 32767, 32767,-32768,-32768, 32767, 32767,-32768, 32767,-32768, 32767, 32767,-32768, 32767,-32768,-32768, 32767, 32767, 32767,-32768, 32767,-32768,-32768, 32767, 32767, 32767, 32767, 32767,-32768,
                 32767, 32767,-32768, 32767,-32768,-32768,-32768, 32767,-32768, 32767, 32767, 32767, 32767,-32768, 32767,-32768, 32767, 32767, 32767,-32768, 32767, 32767, 32767,-32768,-32768, 32767,-32768,-32768, 32767, 32767, 32767,-32768, 32767, 32767, 32767,
                -32768,-32768,-32768, 32767, 32767, 32767,-32768, 32767,-32768,-32768, 32767, 32767,-32768,-32768, 32767,-32768,-32768,-32768, 32767, 32767,-32768,-32768,-32768, 32767, 32767, 32767, 32767,-32768, 32767, 32767, 32767, 32767, 32767,-32768, 32767,
                 32767,-32768,-32768, 32767,-32768,-32768,-32768,-32768, 32767,-32768, 32767, 32767,-32768, 32767, 32767,-32768, 32767,-32768,-32768,-32768,-32768, 32767,-32768, 32767,-32768, 32767, 32767,-32768,-32768, 32767, 32767, 32767, 32767,-32768, 32767,
                -32768, 32767, 32767, 32767,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768,-32768, 32767, 32767, 32767, 32767,-32768, 32767, 32767,-32768,-32768, 32767, 32767, 32767,-32768, 32767, 32767, 32767,-32768,-32768,-32768, 32767, 32767,-32768,
                -32768,-32768,-32768,-32768, 32767, 32767,-32768,-32768, 32767,-32768,-32768, 32767, 32767,-32768,-32768,-32768,-32768,-32768,-32768,-32768, 32767,-32768, 32767, 32767, 32767,-32768,-32768,-32768,-32768,-32768,-32768, 32767, 32767,-32768,-32768,
                 32767,-32768, 32767,-32768, 32767, 32767,-32768,-32768, 32767,-32768,-32768,-32768, 32767,-32768, 32767,-32768, 32767, 32767,-32768,-32768,-32768,-32768, 32767, 32767, 32767, 32767, 32767, 32767, 32767, 32767,-32768,-32768,-32768, 32767, 32767,
                -32768, 32767,-32768, 32767, 32767,-32768,-32768, 32767,-32768,-32768,-32768,-32768, 32767,-32768,-32768, 32767,-32768,-32768,-32768,-32768, 32767,-32768,-32768,-32768,-32768, 32767, 32767,-32768, 32767, 32767,-32768,-32768,-32768, 32767, 32767,
                 32767,-32768,-32768, 32767,-32768,-32768,-32768,-32768, 32767,-32768, 32767,-32768,-32768, 32767, 32767, 32767,-32768,-32768,-32768,-32768,-32768, 32767
            };

        public static readonly short[] TriangleWave = 
            {
                -32768,-32513,-32257,-32001,-31745,-31489,-31233,-30977,-30721,-30465,-30209,-29953,-29697,-29441,-29185,-28929,-28673,-28417,-28161,-27905,-27649,-27393,-27137,-26881,-26625,-26369,-26113,-25857,-25601,-25345,-25089,-24833,-24577,-24321,-24065,
                -23809,-23553,-23297,-23041,-22785,-22529,-22273,-22017,-21761,-21505,-21249,-20993,-20737,-20481,-20225,-19969,-19713,-19457,-19201,-18945,-18689,-18433,-18177,-17921,-17665,-17409,-17153,-16897,-16641,-16385,-16129,-15873,-15617,-15361,-15105,
                -14849,-14593,-14337,-14081,-13825,-13569,-13313,-13057,-12801,-12545,-12289,-12033,-11777,-11521,-11265,-11009,-10753,-10497,-10241,-9985,-9729,-9473,-9217,-8961,-8705,-8449,-8193,-7937,-7681,-7425,-7169,-6913,-6657,-6401,-6145,-5889,-5633,-5377,
                -5121,-4865,-4609,-4353,-4097,-3841,-3585,-3329,-3073,-2817,-2561,-2305,-2049,-1793,-1537,-1281,-1025,-769,-513,-257,-1,255,511,767,1023,1279,1535,1791,2047,2303,2559,2815,3071,3327,3583,3839,4095,4351,4607,4863,5119,5375,5631,5887,6143,6399,
                 6655,6911,7167,7423,7679,7935,8191,8447,8703,8959,9215,9471,9727,9983,10239,10495,10751,11007,11263,11519,11775,12031,12287,12543,12799,13055,13311,13567,13823,14079,14335,14591,14847,15103,15359,15615,15871,16127,16383,16639,16895,
                 17151,17407,17663,17919,18175,18431,18687,18943,19199,19455,19711,19967,20223,20479,20735,20991,21247,21503,21759,22015,22271,22527,22783,23039,23295,23551,23807,24063,24319,24575,24831,25087,25343,25599,25855,26111,26367,26623,
                 26879,27135,27391,27647,27903,28159,28415,28671,28927,29183,29439,29695,29951,30207,30463,30719,30975,31231,31487,31743,31999,32255,32511,32767,32511,32255,31999,31743,31487,31231,30975,30719,30463,30207,29951,29695,29439,29183,
                 28927,28671,28415,28159,27903,27647,27391,27135,26879,26623,26367,26111,25855,25599,25343,25087,24831,24575,24319,24063,23807,23551,23295,23039,22783,22527,22271,22015,21759,21503,21247,20991,20735,20479,20223,19967,19711,19455,
                 19199,18943,18687,18431,18175,17919,17663,17407,17151,16895,16639,16383,16127,15871,15615,15359,15103,14847,14591,14335,14079,13823,13567,13311,13055,12799,12543,12287,12031,11775,11519,11263,11007,10751,10495,10239,9983,9727,9471,
                 9215,8959,8703,8447,8191,7935,7679,7423,7167,6911,6655,6399,6143,5887,5631,5375,5119,4863,4607,4351,4095,3839,3583,3327,3071,2815,2559,2303,2047,1791,1535,1279,1023,767,511,255,-1,-257,-513,-769,-1025,-1281,-1537,-1793,-2049,-2305,-2561,
                -2817,-3073,-3329,-3585,-3841,-4097,-4353,-4609,-4865,-5121,-5377,-5633,-5889,-6145,-6401,-6657,-6913,-7169,-7425,-7681,-7937,-8193,-8449,-8705,-8961,-9217,-9473,-9729,-9985,-10241,-10497,-10753,-11009,-11265,-11521,-11777,-12033,-12289,-12545,
                -12801,-13057,-13313,-13569,-13825,-14081,-14337,-14593,-14849,-15105,-15361,-15617,-15873,-16129,-16385,-16641,-16897,-17153,-17409,-17665,-17921,-18177,-18433,-18689,-18945,-19201,-19457,-19713,-19969,-20225,-20481,-20737,-20993,-21249,-21505,
                -21761,-22017,-22273,-22529,-22785,-23041,-23297,-23553,-23809,-24065,-24321,-24577,-24833,-25089,-25345,-25601,-25857,-26113,-26369,-26625,-26881,-27137,-27393,-27649,-27905,-28161,-28417,-28673,-28929,-29185,-29441,-29697,-29953,-30209,-30465,
                -30721,-30977,-31233,-31489,-31745,-32001,-32257,-32513                                                  
            };

        /*public static short[] SineWave;
        public static short[] SawWave;

        public static void InitWaves()
        {
            TriangleWave = new short[512];
            for (int i = 0; i < 256; i++)
                TriangleWave[i] = (short)((ushort.MaxValue*i/256)-short.MinValue);
            for (int i = 0; i < 256; i++)
                TriangleWave[256+i] = TriangleWave[256-i];
            TriangleWave[256] = short.MaxValue;

            SawWave = new short[512];
            for (int i = 0; i < 512; i++)
                SawWave[i] = (short)((ushort.MaxValue * i / 512) - short.MinValue);

            SineWave = new short[1024];
            for (int i=0; i<1024; i++)
            {
                SineWave[i] = (short) (Math.Sin(i*Math.PI*2/1024d)*32767);
            }
        }*/
    }
}