namespace Cave
{
    /// <summary>
    /// Random Number Generator based on Mersenne-Twister algorithm:
    /// the Mersenne specifically being 2^19937-1.
    /// We're using the Wikipedia 64-bit update.
    /// sgenrand with seed first; then genrand.
    /// </summary>
    /// <remarks>
    /// Algo by Makoto Matsumoto and Takuji Nishimura; 32-bit version coded in C in 1997-8.
    /// http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/VERSIONS/C-LANG/980409/mt19937-2.c
    /// This C# by David Reid Ross 8/25/2025 in that "unsafe" spirit.
    /// My purpose was an array of bytes, rather than some high-resolution rational number. So, no "double" conversion offered.
    /// </remarks>
    public unsafe class Mersenne64
    {
        #region constants

        /// <summary>
        /// Constant vector a
        /// </summary>
        private readonly static ulong[] mag01 = { 0x0, 0xb5026f5aa96619e9 }; //9908b0df aka "matrix A"

        /// <summary>
        /// most significant w-r bits
        /// </summary>
        private readonly static UInt64 UPPER_MASK = 0x8000000000000000;

        /// <summary>
        /// least significant r bits
        /// </summary>
        private readonly static UInt64 LOWER_MASK = 0x7fffffffffffffff;

        /// <summary>
        /// Tempering mask B
        /// </summary>
        private readonly static UInt64 TEMPERING_MASK_B = 0x71d67fffeda60000; //9d2c5680

        /// <summary>
        /// Tempering mask C
        /// </summary>
        private readonly static UInt64 TEMPERING_MASK_C = 0xfff7eee000000000; //efc60000

        private ulong _shiftedValue;

        #endregion

        private readonly int M;

        /// <summary>
        /// the array for the state vector
        /// </summary>
        private readonly ulong[] mt;

        /// <summary>
        /// mti==N+1 means mt[N] is not initialized 
        /// </summary>
        private int mti;

        public Mersenne64()
        {
            var n = 312; //624
            mti = n + 1;
            mt = new ulong[n];
            M = 156; //397
        }

        #region helper methods
        private static ulong TEMPERING_SHIFT_U(ulong y) => y >> 29; //11
        private static ulong TEMPERING_SHIFT_S(ulong y) => y << 17; //7
        private static ulong TEMPERING_SHIFT_T(ulong y) => y << 37; //15
        private static ulong TEMPERING_SHIFT_L(ulong y) => y >> 43; //18

        #endregion


        #region engine
        /// <summary>
        /// setting initial seeds to mt[N] using the generator Line 25 of Table 1 in
        /// [KNUTH 1981, The Art of Computer Programming Vol. 2 (2nd Ed.), pp102] 
        /// </summary>
        /// <param name="seed"></param>
        public void sgenrand(ulong seed)
        {
            fixed (ulong* metAddr = &mt[0])
            {
                *metAddr = seed & 0xffffffffffffffff;
                var metAddr2 = metAddr;
                var N = mt.Length;
                for (mti = 1; mti < N; mti++)
                {
                    var mtOld = *metAddr2;
                    metAddr2++;

                    *metAddr2 = (69069 * mtOld) & 0xffffffffffffffff;
                }
            }
        }

        /// <summary>
        /// Next pseudorandom
        /// </summary>
        /// <returns>The pseudorandom as an unsigned 64-bit / 8-byte integer.</returns>
        public ulong genrand()
        {
            var N = mt.Length;

            if (mti >= N) fixed (ulong* metAddr = &mt[0]) //just came from sgenrand
                { /* generate N words at one time */
                    ulong y;
                    int kk;
                    var metAddrkk = metAddr;
                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (*metAddrkk & UPPER_MASK) | (*(metAddrkk + 1) & LOWER_MASK);
                        *metAddrkk = *(metAddrkk + M) ^ (y >> 1) ^ mag01[y & 0x1];
                        metAddrkk++;
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (*metAddrkk & UPPER_MASK) | (*(metAddrkk + 1) & LOWER_MASK);
                        *metAddrkk = *(metAddrkk + M - N) ^ (y >> 1) ^ mag01[y & 0x1];
                        metAddrkk++;
                    }
                    y = (*(metAddr + N - 1) & UPPER_MASK) | (*metAddr & LOWER_MASK);
                    *(metAddr + N - 1) = *(metAddr + M - 1) ^ (y >> 1) ^ mag01[y & 0x1];

                    mti = 0;
                }

            _shiftedValue = mt[mti++];
            _shiftedValue ^= TEMPERING_SHIFT_U(_shiftedValue);
            _shiftedValue ^= TEMPERING_SHIFT_S(_shiftedValue) & TEMPERING_MASK_B;
            _shiftedValue ^= TEMPERING_SHIFT_T(_shiftedValue) & TEMPERING_MASK_C;
            _shiftedValue ^= TEMPERING_SHIFT_L(_shiftedValue);
            return _shiftedValue;
        }

        #endregion

        /// <summary>
        /// Convert state to array of "[0,256)" bytes.
        /// As a 64-bit algo, the array is 8D.
        /// </summary>
        public byte[] ToBytes => BitConverter.GetBytes(_shiftedValue);
    }
}
