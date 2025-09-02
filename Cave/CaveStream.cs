namespace Cave
{
    /// <summary>
    /// 2D cavern in a given mercator space.
    /// This is made by cellular-automata based on square or hex coordinates.
    /// An iteration eats wall (value of false), and excretes edible rubble.
    /// </summary>
    /// <remarks>
    /// This class by David R Ross 8/24/2025. Started early July.
    /// The square version:
    /// https://www.roguebasin.com/index.php/Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels
    /// Either way this holds a phat array in RAM as doesn't scale over high iterations and area.
    /// It's a forward-only stream over iterations;
    /// also a 2D stream over seed, but inefficient at that.
    /// I added a "bumpseed" method to approximate the next (mostly walls).
    /// </remarks>
    public unsafe class CaveStream //: Stream
    {
        private readonly uint _width; //must be a multiple of 4, for 32-bit PRNG.
        private readonly int _adjacentWalls; //square or hex

        private bool _hasWall;
        private int _iterations;
        private UInt32 _topseed; //presently write-only, not floated to the UI.
        private byte _perbyteAreWalls;
        private byte[] _randommap; //RAM hit, but improves perf for iteration-backtrack and seed-changes.
        private bool[] _map; //false is wall. true is hole in wall. Also I am told 2D arrays have overhead. I dunno about that but I did get Buffer.BulkCopy & pointers from 1D here.

        private bool[] _frame; //lazyload

        //their C# version should be more C-like, to which end... pointers!
        private delegate int CountAdjacentWallsDelegate(bool* map, int x, int y, int h);
        private readonly CountAdjacentWallsDelegate CountAdjacentWalls;
        private delegate int CountNearbyWallsDelegate(bool* map, int x, int y, int h);
        private readonly CountNearbyWallsDelegate CountNearbyWalls;

        private Mersenne64 _randomSeed;

        public CaveStream(uint width, uint height, byte percentAreWalls, bool isHex, uint topseed)
        {
            _width = width;
            _adjacentWalls = isHex ? 4 : 5;

            _iterations = 0;
            _topseed = topseed;

            //this scrolls down the pseudorandom-number generator (PRNG), rooted in zero.

            //Aside on C# Random. The documentation credits Donald E. Knuth (fl. 1981-1997).
            //Knuth over seeds 0-203 (136+ HEIGHT=67) restricts width to 64. Because the next X over, is too-often false.
            //White vertical lines everywhere. (How 1980s.)
            //So I use Mersenne; the classic 32-bit implementation for now. Hence why topseed is marked "UInt32".

            //I get connected spaces in the wall when wall-ratio is 18% or 60%.
            //37% worked well with Knuth past 64-width due to the artifacts; with better randomisers we get islands of rock.
            _randomSeed = new Mersenne64();
            _randommap = FillRandom(height, _topseed);
            _map = new bool[_width * height];
            if (percentAreWalls < 50) //false is light wall, true is dark hole in wall
                _map = GetFrame();
            _perbyteAreWalls = (byte)(percentAreWalls * 2.56);
            _iterations = 0;

            var byteWidth = width - 4;
            var bitPtr = width << 1;
            var bytePtr = 0;
            for (int y = 2; y < height - 2; y++)
            {
                bitPtr += 2;
                for (int x = 0; x < byteWidth; x++)
                    _map[bitPtr++] = _randommap[bytePtr++] > _perbyteAreWalls;
                bitPtr += 2;
            }

            //better this than proliferating subclasses
            if (!isHex) //square, developed/corrected/simplified from RogueBasin's static functions (more C than C#)
            {
                CountAdjacentWalls = (mapAddr, x, y, windowheight) =>
                {
                    var walls = 0;

                    int mapY = y - 1, minmapX = x - 1, maxmapX = x + 1, maxmapY = y + 1;

                    do
                    {
                        var idx = mapAddr + mapY * _width + minmapX;
                        for (var mapX = minmapX; mapX <= maxmapX; mapX++)
                        {
                            if (!*idx && (mapX != x || mapY != y))
                                walls++;
                            idx++;
                        }
                        mapY++;
                    } while (mapY <= maxmapY);

                    return walls;
                };
                CountNearbyWalls = (mapAddr, x, y, windowheight) =>
                {
                    var walls = 0;

                    int minmapY = y - 2, minmapX = x - 2, maxmapX = x + 2, maxmapY = y + 2;

                    //List<Task> ts = new List<Task>() was a thought, but doesn't pan
                    //int wY1 = 0, wY2 = 0, wX1 = 0, wX2 = 0;
                    //look to the four 3-sides of the 9-square.

                    var mapY = y - 1;
                    var idx = mapAddr + mapY * _width + minmapX;
                    do
                    {
                        if (!*idx)
                        {
                            walls++;
                        }
                        mapY++;
                        idx += _width;
                    } while (mapY <= y + 1);

                    mapY = y - 1;
                    idx = mapAddr + mapY * _width + maxmapX;
                    do
                    {
                        if (!*idx)
                        {
                            walls++;
                        }
                        mapY++;
                        idx += _width;
                    } while (mapY <= y + 1);

                    var mapX = x - 1;
                    idx = mapAddr + minmapY * _width + mapX;
                    do
                    {
                        if (!*idx)
                        {
                            walls++;
                        }
                        mapX++;
                        idx++;
                    } while (mapX <= x + 1);

                    mapX = x - 1;
                    idx = mapAddr + maxmapY * _width + mapX;
                    do
                    {
                        if (!*idx)
                        {
                            walls++;
                        }
                        mapX++;
                        idx++;
                    } while (mapX <= x + 1);


                    return walls;
                };
            }
            else //hex. this block is mine.
            {
                CountAdjacentWalls = (mapAddr, x, y, windowheight) =>
                {
                    var walls = 0;

                    //hex pointing vertical. So the iteration follows Y-orientation, top-to-bottom.
                    //also: Gygaxian coordinates. Triangular.
                    for (var mapY = y - 1; mapY <= y + 1; mapY++)
                    {
                        //crosshatch. top 2, middle 3 but skip identity, bottom 2 again
                        int mapX = mapY <= y ? x - 1 : x;
                        int maxX = mapY < y ? x : x + 1;
                        while (mapX <= maxX)
                        {
                            var idx = mapAddr + mapY * _width + mapX;
                            if ((mapX < 0 || mapY < 0 || mapX >= _width || mapY >= windowheight
                                || !*idx) && (mapX != x || mapY != y))
                                walls++;
                            mapX++;
                        }
                    }

                    return walls;
                };
                CountNearbyWalls = (mapAddr, x, y, windowheight) =>
                {
                    var walls = 0;

                    for (var mapY = y - 2; mapY <= y + 2; mapY++)
                    {
                        int mapX = mapY <= y ? x - 2
                            : mapY == y + 1 ? x - 1
                            : x;
                        int maxX = mapY == y - 2 ? x
                            : mapY < y - 1 ? x + 1
                            : x + 2;
                        while (mapX <= maxX)
                        {
                            var idx = mapAddr + mapY * _width + mapX;
                            if ((Math.Abs(mapX - x) > 1 || Math.Abs(mapY - y) > 1) //skip identity+adj
                                && (mapX < 0 || mapY < 0 || mapX >= _width || mapY >= windowheight || !*idx))
                            {
                                walls++;
                            }
                            mapX++;
                        }
                    }

                    return walls;
                };
            }
        }
        private IEnumerable<byte[]> FillLine(Mersenne64 randomSeed)//randomSeed has been fit to topseed and y
        {
            for (int x = 0; x < _width - 4; x += 8) // 64-bit
            {
                var dumdub = randomSeed.genrand();
                yield return randomSeed.ToBytes;
            }
        }

        private byte[] FillRandom(uint height, uint seed)
        {
            byte[] map = new byte[(height - 4) * (_width - 4)];
            uint x = 0;
            for (uint y = seed; y < seed + height - 4; y++)
            {
                _randomSeed.sgenrand(y); //thus making a 2D scale riverworld
                foreach (var horizBytes in FillLine(_randomSeed))
                {
                    Array.Copy(horizBytes, 0, map, x, 8);
                    x += 8;
                }
            }
            return map;
        }
        private bool[] FillSquare(bool[] map, int height)
        {
            bool[] newMap;
            if (!_map[0]) //it's not framed, just use the blank
                newMap = new bool[map.Length];
            else if (_map.Length == map.Length)
                newMap = GetFrame();
            else
                newMap = map.Clone() as bool[]; //inherit what frame there be

            fixed (bool* mapAddr = &map[0], newmapAddr = &newMap[0])
            {
                bool* newmapAddr2 = newmapAddr + (int)(_width << 1) + 2;
                for (int y = 2; y < height - 2; y++)
                {
                    for (int x = 2; x < _width - 2; x++)
                    {
                        *newmapAddr2 = CountAdjacentWalls(mapAddr, x, y, height) < _adjacentWalls
                                && CountNearbyWalls(mapAddr, x, y, height) > 2;
                        newmapAddr2++;
                    }
                    newmapAddr2 += 4;
                }
            }
            return newMap;
        }

        public bool[,] GetMap()
        {
            int height = _map.Length / (int)_width;
            var map = new bool[_width, height];
            fixed (bool* mapAddr = &_map[0])
            {
                var x = mapAddr;
                for (int j = 0; j < height; j++)
                    for (int i = 0; i < _width; i++)
                    {
                        map[i, j] = *x;
                        x++;
                    }
            }
            return map;
        }

        //Good-enough for bump=1. Noticeable improvement 16 iterations / short width.
        //I see some artifacting at iter=4 scrolling down and up again.
        //But it only happens in the openspace mush (dark-mix), not in the solid walls (light).
        public void BumpSeed(short bump)
        {
            uint height = (uint)(_map.Length / _width);

            //possible to put these in separate Task, but the savings weren't really there
            uint seed = _topseed + height - 4; // cf. FillRandom (and FillSquare). -4 b/c x is going to end before taking in the last row of nothing

            var byteWidth = _width - 4;
            uint randomMapCursor = (uint)((height - bump - 4) * byteWidth); //would be -2 and just width if the randommap were including the frame
            //Dequeue the first row(s) in-situ.
            //Array.Copy(), they say, is "safer"
            Array.Copy(_randommap, bump * byteWidth, _randommap, 0, randomMapCursor);
            for (int i = 0; i < bump; i++)
            {
                _randomSeed.sgenrand(seed++);
                foreach (var horizBytes in FillLine(_randomSeed))
                {
                    Array.Copy(horizBytes, 0, _randommap, randomMapCursor, 8);
                    randomMapCursor += 8;
                }
            }
            //the last two rows remain in place.

            //these troubleshooting lines should redo the whole thing perfect. although of course we're not saving much time.
            //var iter = _iterations;
            //SetFirstMap(_percentAreWalls);
            //Iterate(iter);
            //return;
            //now with the randoms we got, look forward...
            ushort spanHeight = (ushort)((_iterations << 1) + bump + 4);
            byte[] newRandom = new byte[byteWidth * spanHeight];
            bool[] newMap = new bool[_width * spanHeight];
            //extract newRandom from the last stages of _randomMap
            Array.Copy(_randommap, (int)((height - spanHeight - 2) * byteWidth), newRandom, 0, (int)((spanHeight - 2) * byteWidth));

            //generate iteration-zero from the newRandom window
            fixed (bool* mapAddr = &newMap[0])
            {
                fixed (byte* randomAddr = &newRandom[0])
                {
                    var bitPtr = mapAddr;
                    var bytePtr = randomAddr;
                    for (int y = 0; y < spanHeight; y++)
                    {
                        if (!_hasWall)
                        {
                            *bitPtr = true;
                            *(bitPtr + 1) = true;
                        }
                        bitPtr += 2;
                        for (int x = 0; x < byteWidth; x++)
                        {
                            *bitPtr = *bytePtr > _perbyteAreWalls;
                            bytePtr++;
                            bitPtr++;
                        }
                        if (!_hasWall)
                        {
                            *bitPtr = true;
                            *(bitPtr + 1) = true;
                        }
                        bitPtr += 2;
                    }
                }
            }
            for (int i = 0; i < _iterations; i++)
            {
                newMap = FillSquare(newMap, spanHeight); //internal square, remember
            }

            //On to the master "map". Dequeue and, for the rest, perserve only up to what we're not patching-over
            uint cursorMap = (uint)(spanHeight >> 1) + 1;
            var edgeToPatch = (int)(height - cursorMap) * (int)_width;
            Buffer.BlockCopy(_map, bump * (int)_width, _map, 0, edgeToPatch);

            //patch in what we just did here
            Buffer.BlockCopy(
                newMap, (int)(_width * (spanHeight - cursorMap)),
                _map, edgeToPatch,
                _map.Length - edgeToPatch);

            _topseed += (uint)bump;
        }

        public void RedoRandom(uint seed)
        {
            _topseed = seed;
            _randommap = FillRandom((uint)(_map.Length / _width), seed);
        }

        //true is the hole in the wall which will look dark green
        private bool[] GetFrame()
        {
            if (_frame != null) return _frame.Clone() as bool[];
            _frame = new bool[_map.Length];
            uint height = (uint)_map.Length / _width;
            uint x;
            uint lastRow = _width * (height - 2);
            for (x = 0; x < _width << 1; x++)
            {
                _frame[x] = true;
                _frame[lastRow++] = true;
            }
            //x = _width << 1 now
            uint lastCol = x + _width - 1;
            for (uint y = 2; y < height - 2; y++)
            {
                _frame[x] = true;
                _frame[x + 1] = true;
                _frame[lastCol - 1] = true;
                _frame[lastCol] = true;
                x += _width;
                lastCol += _width;
            }
            return _frame.Clone() as bool[];
        }

        public void SetFirstMap(byte percentAreWalls)
        {
            //The automaton is dug:
            //from the Elemental Plane Of Earth, or from a rock dropped in the Elemental Plane Of Air.
            //I'm tying Air-or-Earth to the incoming wall-%.
            _hasWall = percentAreWalls >= 50;
            //The picture-frame is so that FillLineWalls doesn't have to test the boundaries
            //false is light wall, true is dark hole in wall
            if (!_hasWall && _perbyteAreWalls >= 128)
                _map = GetFrame();
            else if (_hasWall && _perbyteAreWalls < 128)
                _map = new bool[_map.Length];
            //It was a thought to break Mersenne64's bytes to [0,128) nybbles.
            //But the savings weren't there.
            _perbyteAreWalls = (byte)(percentAreWalls * 2.56);
            _iterations = 0;

            var byteWidth = _width - 4;
            var byteHeight = _randommap.Length / byteWidth;

            fixed (bool* mapAddr = &_map[_width << 1])
            {
                fixed (byte* randomAddr = &_randommap[0])
                {
                    var bitPtr = mapAddr;
                    var bytePtr = randomAddr;
                    for (int y = 0; y < byteHeight; y++)
                    {
                        bitPtr += 2;
                        for (int x = 0; x < byteWidth; x++)
                        {
                            *bitPtr = *bytePtr > _perbyteAreWalls;
                            bitPtr++; bytePtr++;
                        }
                        bitPtr += 2;
                    }
                }
            }
        }

        public void Iterate(int iterations)
        {
            int height = _map.Length / (int)_width;
            _iterations += iterations;
            for (var i = 0; i < iterations; i++)
            {
                _map = FillSquare(_map, height);
            }
        }
        public void Iterate()
        {
            int height = _map.Length / (int)_width;
            _iterations++;
            _map = FillSquare(_map, height);
        }
    }
}

/*troubleshoot!
 * using System.Diagnostics;
using System.Text;


StringBuilder st;
for (int y = _height - spanHeight; y < _height; y++)
{
    st = new StringBuilder();
    for (int x = 0; x < 64; x++)
        st.Append(_map[x, y] ? '#' : ' ');

    Debug.WriteLine(st.ToString());
}
Debug.WriteLine("");

for (int y = 0; y < spanHeight; y++)
{
    st = new StringBuilder();
    for (int x = 0; x < 64; x++)
        st.Append(newMap[x, y] ? '#' : ' ');

    Debug.WriteLine(st.ToString());
}*/
