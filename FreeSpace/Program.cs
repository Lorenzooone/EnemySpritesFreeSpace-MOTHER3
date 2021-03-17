using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FreeSpace
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("ERROR! WRONG NUMBER OF PARAMETERS!");
                return -1;
            }
            bool verbose = (args.Length == 2 ? (args[1].Equals("-v") ? true : false) : false);
            try
            { 
                byte[] data = Enemy_Graphics.RemoveSame.Import(File.ReadAllBytes(args[0]), verbose);
                File.WriteAllBytes(args[0], data);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR WHILE OPENING THE FILE!");
                return -1;
            }
            return 0;
        }
    }
    public class Pointers
    {
        public static int Base = 0x1C90960;
        public static int BaseSOB = 0x1C91E88;//Start of enemy SOB blocks pointers
        public static int BaseCCG = 0x1C909A8;//Start of enemy CCG blocks pointers
        public static int BasePAL = 0x1C91530;//Start of enemy palettes pointers
        public static void Removal(byte[] memblock)
        {
            int t = 0, g = 0, character = 0;
            int[] PointerSOB = new int[257], SOBLength = new int[257], PointerCCG = new int[257], CCGLength = new int[257];
            for (int i = 0; i <= 256; i++)
            {
                int Enemynum = i;
                int PoiSOB = BaseSOB + (Enemynum * 8);
                int PoiCCG = BaseCCG + (Enemynum * 8);
                int PoiPAL = BasePAL + (Enemynum * 8);
                PointerSOB[i] = memblock[PoiSOB] + (memblock[PoiSOB + 1] << 8) + (memblock[PoiSOB + 2] << 16) + (memblock[PoiSOB + 3] << 24) + Base;//SOB
                PointerCCG[i] = memblock[PoiCCG] + (memblock[PoiCCG + 1] << 8) + (memblock[PoiCCG + 2] << 16) + (memblock[PoiCCG + 3] << 24) + Base;//CCG
                SOBLength[i] = memblock[PoiSOB + 4] + (memblock[PoiSOB + 5] << 8) + (memblock[PoiSOB + 6] << 16) + (memblock[PoiSOB + 7] << 24);//SOBLength
                CCGLength[i] = memblock[PoiCCG + 4] + (memblock[PoiCCG + 5] << 8) + (memblock[PoiCCG + 6] << 16) + (memblock[PoiCCG + 7] << 24);//CCGLength
            }
            g = 0;
            while (t <= 256)
            {
                g = PointerSOB[t];
                character = g;
                for (g = 0; g < (SOBLength[t] + 3) / 4; g++)
                {
                    memblock[(int)character] = 255;
                    memblock[(int)character + 1] = 255;
                    memblock[(int)character + 2] = 255;
                    memblock[(int)character + 3] = 255;
                    character = character + 4;
                }
                t = t + 1;
            }
            character = 0;
            t = 0;
            while (t <= 256)
            {
                g = PointerCCG[t];
                character = g;
                for (g = 0; g < (CCGLength[t] + 3) / 4; g++)
                {
                    memblock[(int)character] = 255;
                    memblock[(int)character + 1] = 255;
                    memblock[(int)character + 2] = 255;
                    memblock[(int)character + 3] = 255;
                    character = character + 4;
                }
                t = t + 1;
            }
        }
    }
}//Import of C++ code. It's commented, but somewhat messy.
namespace GBA
{
    public class OAM
    {
        public int Y, X, Width, Height, Flips, Tile;
        public int Num;
        public int Address;
        public void setSOBEntryTile(byte[] SOB)
        {
            SOB[Address + 4] = (byte)(Tile & 0xFF);
            SOB[Address + 5] = (byte)(((Tile >> 8) & 3) | (SOB[Address + 5] & 0xFC));
        }
        public static int compareTiles(OAM one, OAM two)
        {
            return one.Tile.CompareTo(two.Tile);
        }
        public static void getSizesOAM(int Shape, int Size, out int XSize, out int YSize)
        {
            switch (Shape)
            {
                case 0:
                    switch (Size)
                    {
                        case 0:
                            XSize = 1;
                            break;
                        case 0x40:
                            XSize = 2;
                            break;
                        case 0x80:
                            XSize = 4;
                            break;
                        default:
                            XSize = 8;
                            break;
                    }
                    YSize = XSize;
                    break;
                case 0x40:
                    switch (Size)
                    {
                        case 0:
                            XSize = 2;
                            YSize = 1;
                            break;
                        case 0x40:
                            XSize = 4;
                            YSize = 1;
                            break;
                        case 0x80:
                            XSize = 4;
                            YSize = 2;
                            break;
                        default:
                            XSize = 8;
                            YSize = 4;
                            break;
                    }
                    break;
                default:
                    switch (Size)
                    {
                        case 0:
                            XSize = 1;
                            YSize = 2;
                            break;
                        case 0x40:
                            XSize = 1;
                            YSize = 4;
                            break;
                        case 0x80:
                            XSize = 2;
                            YSize = 4;
                            break;
                        default:
                            XSize = 4;
                            YSize = 8;
                            break;
                    }
                    break;
            }
        }
        public static List<OAM> OAMGet(byte[] data, int address)
        {
            List<OAM> OAM = new List<OAM>();
            int count = data[address + 4] + (data[address + 5] << 8);
            for (int i = 0; i < count; i++)
            {
                int Internal = address + (data[address + 8 + (i * 2)] + (data[address + 9 + (i * 2)] << 8));
                int num = data[Internal + 2] + (data[Internal + 3] << 8);
                OAM.AddRange(OAMGet(data, Internal + 4, num, i));
            }
            return OAM;
        }
        public static List<OAM> OAMGet(byte[] data, int address, int num, int i)
        {
            List<OAM> OAM = new List<OAM>();
            for (int j = 0; j < num; j++)
            {
                OAM Tuk = new OAM();
                Tuk.Address = address + (j * 8);
                Tuk.Num = i;
                Tuk.Y = data[Tuk.Address];
                if (Tuk.Y >= 0x80)
                    Tuk.Y -= 0x80;
                else
                    Tuk.Y += 0x80;
                Tuk.X = data[Tuk.Address + 2] + ((data[Tuk.Address + 3] & 0x1) << 8);
                if (Tuk.X >= 0x100)
                    Tuk.X -= 0x100;
                else
                    Tuk.X += 0x100;
                int Shape = (data[Tuk.Address + 1]) & 0xC0;
                int Size = (data[Tuk.Address + 3]) & 0xC0;
                getSizesOAM(Shape, Size, out Tuk.Width, out Tuk.Height);
                Tuk.Flips = (data[Tuk.Address + 3] >> 4) & 0x3;
                Tuk.Tile = (data[Tuk.Address + 4]) + (((data[Tuk.Address + 5]) & 0x3) << 8);
                OAM.Add(Tuk);
            }
            return OAM;
        }
        public static List<OAM> OAMGet(byte[] data)
        {
            return OAMGet(data, 0, data.Length / 8, 0);
        }
    }
    public class PAL
    {
        public static byte[] getPalette(byte[] data, int address, int bpp)
        {
            byte[] result = new byte[2 * (2 << bpp)];
            for (int i = 0; i < 2 * (2 << bpp); i++)
                result[i] = data[address + i];
            return result;
        }
    }
    public class CCG
    {
        public static byte[] CCGStart = new byte[] { 0x63, 0x63, 0x67, 0x20 };
        public static byte[] CCGEnd = new byte[] { 0x7E, 0x63, 0x63, 0x67 };
        public static byte[] getCCG(byte[] memblock, int position)
        {
            for (int i = 0; i < CCGStart.Length; i++)
                if (CCGStart[i] != memblock[position + i])
                    return null;
            byte[] ReturnCCG = new byte[0xC + LZ77.getLength(memblock, position + 0xC) + 0x4];
            for (int i = 0; i < ReturnCCG.Length - 4; i++)
                ReturnCCG[i] = memblock[position + i];
            for (int i = 0; i < CCGEnd.Length; i++)
                ReturnCCG[ReturnCCG.Length - 4 + i] = CCGEnd[i];
            return ReturnCCG;
        }
    }
    public class SOB
    {
        public static byte[] SOBStart = new byte[] { 0x73, 0x6F, 0x62, 0x20 };
        public static byte[] SOBEnd = new byte[] { 0x7E, 0x73, 0x6F, 0x62 };
        public static byte[] getSOB(byte[] memblock, int position)
        {
            for (int i = 0; i < SOBStart.Length; i++)
                if (SOBStart[i] != memblock[position + i])
                    return null;
            List<byte> CreatedSOB = new List<byte>();
            CreatedSOB.AddRange(SOBStart);
            for (int i = 0; i < 4; i++)
                CreatedSOB.Add(memblock[position + i + 4]);
            int primaryEntries = memblock[position + 4] + (memblock[position + 5] << 8);
            int secondaryEntries = memblock[position + 6] + (memblock[position + 7] << 8);
            int offsetTableLength = 2 * (primaryEntries + secondaryEntries);
            if (offsetTableLength % 4 != 0)
                offsetTableLength += 2;
            byte[] offsetTable = new byte[offsetTableLength];
            int offsetTableBegin = 8;
            List<int> Explored = new List<int>();
            List<int> CorrespondingExplored = new List<int>();
            CreatedSOB.AddRange(offsetTable);
            for (int i = 0; i < primaryEntries + secondaryEntries; i++)
            {
                int foundOffset = memblock[offsetTableBegin + position] + (memblock[offsetTableBegin + position + 1] << 8);
                if (Explored.Contains(foundOffset))
                {
                    CreatedSOB[offsetTableBegin] = (byte)(CorrespondingExplored[Explored.IndexOf(foundOffset)] & 0xFF);
                    CreatedSOB[offsetTableBegin + 1] = (byte)((CorrespondingExplored[Explored.IndexOf(foundOffset)] >> 8) & 0xFF);
                }
                else
                {
                    CreatedSOB[offsetTableBegin] = (byte)(CreatedSOB.Count & 0xFF);
                    CreatedSOB[offsetTableBegin + 1] = (byte)((CreatedSOB.Count >> 8) & 0xFF);
                    Explored.Add(foundOffset);
                    CorrespondingExplored.Add(CreatedSOB.Count);
                    if (i < primaryEntries)
                        CreatedSOB.AddRange(getPrimaryEntry(memblock, position + foundOffset));
                    else
                        CreatedSOB.AddRange(getSecondaryEntry(memblock, position + foundOffset));
                }
                offsetTableBegin += 2;
            }
            CreatedSOB.AddRange(SOBEnd);
            return CreatedSOB.ToArray();
        }
        public static byte[] getPrimaryEntry(byte[] data, int position)
        {
            int preCount = data[position] + (data[position + 1] << 8);
            int count = data[position + (preCount * 8) + 2] + (data[position + (preCount * 8) + 3] << 8);
            byte[] entry = new byte[(preCount * 8) + (count * 8) + 4];
            for (int i = 0; i < entry.Length; i++)
                entry[i] = data[position + i];
            return entry;
        }
        public static byte[] getSecondaryEntry(byte[] data, int position)
        {
            int byteCount = 4 + data[position] + (data[position + 1] << 8);
            byte[] entry = new byte[byteCount];
            for (int i = 0; i < entry.Length; i++)
                entry[i] = data[position + i];
            return entry;
        }
    }
    public class LZ77
    {
        public static int Decompress(byte[] data, int address, out byte[] output)
        {
            output = null;
            int start = address;

            if (data[address++] != 0x10) return -1; // Check for LZ77 signature

            // Read the block length
            int length = data[address++];
            length += (data[address++] << 8);
            length += (data[address++] << 16);
            output = new byte[length];

            int bPos = 0;
            while (bPos < length)
            {
                byte ch = data[address++];
                for (int i = 0; i < 8; i++)
                {
                    switch ((ch >> (7 - i)) & 1)
                    {
                        case 0:

                            // Direct copy
                            if (bPos >= length) break;
                            output[bPos++] = data[address++];
                            break;

                        case 1:

                            // Compression magic
                            int t = (data[address++] << 8);
                            t += data[address++];
                            int n = ((t >> 12) & 0xF) + 3;    // Number of bytes to copy
                            int o = (t & 0xFFF);

                            // Copy n bytes from bPos-o to the output
                            for (int j = 0; j < n; j++)
                            {
                                if (bPos >= length) break;
                                output[bPos] = output[bPos - o - 1];
                                bPos++;
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            return address - start;
        }
        public static int getLength(byte[] data, int address)
        {
            int start = address;

            if (data[address++] != 0x10) return -1; // Check for LZ77 signature

            // Read the block length
            int length = data[address++];
            length += (data[address++] << 8);
            length += (data[address++] << 16);

            int bPos = 0;
            while (bPos < length)
            {
                byte ch = data[address++];
                for (int i = 0; i < 8; i++)
                {
                    switch ((ch >> (7 - i)) & 1)
                    {
                        case 0:
                            // Direct copy
                            if (bPos >= length) break;
                            address++;
                            bPos++;
                            break;
                        default:
                            // Compression magic
                            int t = data[address++];
                            address++;
                            int n = ((t >> 4) & 0xF) + 3;    // Number of bytes to copy
                            bPos += n;
                            break;
                    }
                }
            }
            return address - start;
        }
        public static byte[] Compress(byte[] uncompressed, bool vram)
        {
            LinkedList<int>[] lookup = new LinkedList<int>[256];
            List<byte> Compressed = new List<byte>();
            for (int i = 0; i < 256; i++)
                lookup[i] = new LinkedList<int>();

            int start = 0;
            int current = 0;

            List<byte> temp = new List<byte>();
            int control = 0;

            // Encode the signature and the length
            Compressed.Add(0x10);
            Compressed.Add((byte)(uncompressed.Length & 0xFF));
            Compressed.Add((byte)((uncompressed.Length >> 8) & 0xFF));
            Compressed.Add((byte)((uncompressed.Length >> 16) & 0xFF));

            // VRAM bug: you can't reference the previous byte
            int distanceStart = vram ? 2 : 1;

            while (current < uncompressed.Length)
            {
                temp.Clear();
                control = 0;

                for (int i = 0; i < 8; i++)
                {
                    bool found = false;

                    // First byte should be raw
                    if (current == 0)
                    {
                        byte value = uncompressed[current];
                        lookup[value].AddFirst(current++);
                        temp.Add(value);
                        found = true;
                    }
                    else if (current >= uncompressed.Length)
                    {
                        break;
                    }
                    else
                    {
                        // We're looking for the longest possible string
                        // The farthest possible distance from the current address is 0x1000
                        int max_length = -1;
                        int max_distance = -1;

                        LinkedList<int> possibleAddresses = lookup[uncompressed[current]];

                        foreach (int possible in possibleAddresses)
                        {
                            if (current - possible > 0x1000)
                                break;

                            if (current - possible < distanceStart)
                                continue;

                            int farthest = Math.Min(18, uncompressed.Length - current + start);
                            int l = 0;
                            for (; l < farthest; l++)
                            {
                                if (uncompressed[possible + l] != uncompressed[current + l])
                                {
                                    if (l > max_length)
                                    {
                                        max_length = l;
                                        max_distance = current - possible;
                                    }
                                    break;
                                }
                            }

                            if (l == farthest)
                            {
                                max_length = farthest;
                                max_distance = current - possible;
                                break;
                            }
                        }

                        if (max_length >= 3)
                        {
                            for (int j = 0; j < max_length; j++)
                            {
                                byte value = uncompressed[current + j];
                                lookup[value].AddFirst(current + j);
                            }

                            current += max_length;

                            // We hit a match, so add it to the output
                            int t = (max_distance - 1) & 0xFFF;
                            t |= (((max_length - 3) & 0xF) << 12);
                            temp.Add((byte)((t >> 8) & 0xFF));
                            temp.Add((byte)(t & 0xFF));

                            // Set the control bit
                            control |= (1 << (7 - i));

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        // If we didn't find any strings, copy the byte to the output
                        byte value = uncompressed[current];
                        lookup[value].AddFirst(current++);
                        temp.Add(value);
                    }
                }

                // Flush the temp buffer
                Compressed.Add((byte)(control & 0xFF));

                for (int i = 0; i < temp.Count; i++)
                    Compressed.Add(temp[i]);
            }
            while (Compressed.Count % 4 != 0)
                Compressed.Add(0);
            return Compressed.ToArray();
        }
    }
}
namespace Enemy_Graphics
{
    public class Extraction
    {
        public static int Base = 0x1C90960;
        public static int BaseSOB = 0x1C91E88;//Start of enemy SOB blocks pointers
        public static int BaseCCG = 0x1C909A8;//Start of enemy CCG blocks pointers
        public static int BasePAL = 0x1C91530;//Start of enemy palettes pointers
        public static int[] getPointers(byte[] memblock, int Enemynum)
        {
            int[] returnArray = new int[3];
            int PoiSOB = BaseSOB + (Enemynum * 8);
            int PoiCCG = BaseCCG + (Enemynum * 8);
            int PoiPAL = BasePAL + (Enemynum * 8);
            returnArray[0] = memblock[PoiSOB] + (memblock[PoiSOB + 1] << 8) + (memblock[PoiSOB + 2] << 16) + (memblock[PoiSOB + 3] << 24) + Base;//SOB
            returnArray[1] = memblock[PoiCCG] + (memblock[PoiCCG + 1] << 8) + (memblock[PoiCCG + 2] << 16) + (memblock[PoiCCG + 3] << 24) + Base;//CCG
            returnArray[2] = memblock[PoiPAL] + (memblock[PoiPAL + 1] << 8) + (memblock[PoiPAL + 2] << 16) + (memblock[PoiPAL + 3] << 24) + Base;//Palette
            return returnArray;
        }
    }
    public class RemoveSame
    {
        public const int EndingSOB = 0x1CFF688;
        public const int EndingCCG = 0x1CE5420;
        static int InsertPointer(byte[] memblock, byte[] a, int Pointer, int LastUsed)
        {
            memblock[Pointer] = (byte)(((LastUsed - FreeSpace.Pointers.Base)) & 0xFF);
            memblock[Pointer + 1] = (byte)(((LastUsed - FreeSpace.Pointers.Base) >> 8) & 0xFF);
            memblock[Pointer + 2] = (byte)(((LastUsed - FreeSpace.Pointers.Base) >> 16) & 0xFF);
            memblock[Pointer + 3] = (byte)(((LastUsed - FreeSpace.Pointers.Base) >> 24) & 0xFF);
            memblock[Pointer + 4] = (byte)(((a.Length)) & 0xFF);
            memblock[Pointer + 5] = (byte)(((a.Length) >> 8) & 0xFF);
            memblock[Pointer + 6] = (byte)(((a.Length) >> 16) & 0xFF);
            memblock[Pointer + 7] = (byte)(((a.Length) >> 24) & 0xFF);
            for (int i = 0; i < a.Length; i++)
                memblock[LastUsed + i] = a[i];
            return a.Length + LastUsed;
        }
        static void InsertOldPointer(byte[] memblock, int Pointer, int OldPointsTo, byte[] a)
        {
            memblock[Pointer] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base)) & 0xFF);
            memblock[Pointer + 1] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base) >> 8) & 0xFF);
            memblock[Pointer + 2] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base) >> 16) & 0xFF);
            memblock[Pointer + 3] = (byte)(((OldPointsTo - FreeSpace.Pointers.Base) >> 24) & 0xFF);
            memblock[Pointer + 4] = (byte)(((a.Length)) & 0xFF);
            memblock[Pointer + 5] = (byte)(((a.Length) >> 8) & 0xFF);
            memblock[Pointer + 6] = (byte)(((a.Length) >> 16) & 0xFF);
            memblock[Pointer + 7] = (byte)(((a.Length) >> 24) & 0xFF);
        }
        static bool AreArraySame(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }
        static bool AreArraySame(byte[,] a, int index, byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
                if (a[index, i] != b[i])
                    return false;
            return true;
        }
        public static byte[] Import(byte[] alpha, bool verbose)
        {
            byte[] memblock = alpha;
            int LastCCG = (memblock[FreeSpace.Pointers.BaseCCG]) + (memblock[FreeSpace.Pointers.BaseCCG + 1] << 8) + (memblock[FreeSpace.Pointers.BaseCCG + 2] << 16) + (memblock[FreeSpace.Pointers.BaseCCG + 3] << 24) + FreeSpace.Pointers.Base;
            int LastSOB = (memblock[FreeSpace.Pointers.BaseSOB]) + (memblock[FreeSpace.Pointers.BaseSOB + 1] << 8) + (memblock[FreeSpace.Pointers.BaseSOB + 2] << 16) + (memblock[FreeSpace.Pointers.BaseSOB + 3] << 24) + FreeSpace.Pointers.Base;
            List<FinalProducts> End = new List<FinalProducts>();
            for (int Num = 0; Num <= 256; Num++)
            {
                FinalProducts a = new FinalProducts();
                int[] pointers = Extraction.getPointers(memblock, Num);
                a.SOB = GBA.SOB.getSOB(memblock, pointers[0]);
                a.CCG = GBA.CCG.getCCG(memblock, pointers[1]);
                a.Palette = GBA.PAL.getPalette(memblock, pointers[2], 4);
                End.Add(a);
            }
            FreeSpace.Pointers.Removal(memblock);
            for (int Enemynum = 0; Enemynum <= 256; Enemynum++)
            {
                int BaseSOB = FreeSpace.Pointers.BaseSOB + (Enemynum * 8);
                int BaseCCG = FreeSpace.Pointers.BaseCCG + (Enemynum * 8);
                int BasePAL = FreeSpace.Pointers.BasePAL + (Enemynum * 8);
                End[Enemynum].PointerCCG = BaseCCG;
                End[Enemynum].PointerSOB = BaseSOB;
                End[Enemynum].PointerPAL = BasePAL;
                End[Enemynum].ToCCG = Enemynum;
                End[Enemynum].ToSOB = Enemynum;
                int flag1 = 0, flag2 = 0;
                for (int i = 0; i < Enemynum; i++)
                {
                    if ((AreArraySame(End[Enemynum].CCG, End[i].CCG)) && (flag1 == 0))
                    {
                        End[Enemynum].ToCCG = i;
                        flag1 = 1;
                    }
                    if ((AreArraySame(End[Enemynum].SOB, End[i].SOB)) && (flag2 == 0))
                    {
                        flag2 = 1;
                        End[Enemynum].ToSOB = i;
                    }
                }
            }
            for (int Enemynum = 0; Enemynum <= 256; Enemynum++)
            {
                if (End[Enemynum].ToCCG == Enemynum)
                {
                    LastCCG = (LastCCG & 3) == 0 ? LastCCG : LastCCG + (4 - (LastCCG & 3));
                    End[Enemynum].AddressCCG = LastCCG;
                    LastCCG = InsertPointer(memblock, End[Enemynum].CCG, End[Enemynum].PointerCCG, LastCCG);
                }
                else
                {
                    End[Enemynum].AddressCCG = End[End[Enemynum].ToCCG].AddressCCG;
                    InsertOldPointer(memblock, End[Enemynum].PointerCCG, End[End[Enemynum].ToCCG].AddressCCG, End[Enemynum].CCG);
                }
            }
            for (int Enemynum = 0; Enemynum <= 256; Enemynum++)
            {
                if (End[Enemynum].ToSOB == Enemynum)
                {
                    LastSOB = (LastSOB & 3) == 0 ? LastSOB : LastSOB + (4 - (LastSOB & 3));
                    if (EndingSOB - LastSOB - End[Enemynum].SOB.Count() > 0)
                        End[Enemynum].AddressSOB = LastSOB;
                    else
                    {
                        LastCCG = (LastCCG & 3) == 0 ? LastCCG : LastCCG + (4 - (LastCCG & 3));
                        End[Enemynum].AddressSOB = LastCCG;
                        LastSOB = LastCCG;
                    }
                    LastSOB = InsertPointer(memblock, End[Enemynum].SOB, End[Enemynum].PointerSOB, LastSOB);
                }
                else
                {
                    End[Enemynum].AddressSOB = End[End[Enemynum].ToSOB].AddressSOB;
                    InsertOldPointer(memblock, End[Enemynum].PointerSOB, End[End[Enemynum].ToSOB].AddressSOB, End[Enemynum].SOB);
                }
                int OffsetPAL = memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8)] + (memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8) + 1] << 8) + (memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8) + 2] << 16) + (memblock[FreeSpace.Pointers.BasePAL + (Enemynum * 8) + 3] << 24) + FreeSpace.Pointers.Base;//Palette
                for (int i = 0; i < 32; i++)
                    memblock[OffsetPAL + i] = (byte)(End[Enemynum].Palette[i] & 0xFF);
            }
            if (verbose)
            {
                LastSOB = (LastSOB & 3) == 0 ? LastSOB : LastSOB + (4 - (LastSOB & 3));
                LastCCG = (LastCCG & 3) == 0 ? LastCCG : LastCCG + (4 - (LastCCG & 3));
                if (EndingSOB - LastSOB > 0)
                {
                    Console.WriteLine("Free SOB space from: 0x" + LastSOB.ToString("X7") + " - To: 0x" + EndingSOB.ToString("X7"));
                    Console.WriteLine("Free CCG space from: 0x" + LastCCG.ToString("X7") + " - To: 0x" + EndingCCG.ToString("X7"));
                }
                else
                    Console.WriteLine("Free CCG space from: 0x" + LastSOB.ToString("X7") + " - To: 0x" + EndingCCG.ToString("X7"));
            }
            return memblock;
        }
    }
    class FinalProducts
    {
        public byte[] CCG, SOB, Palette;
        public int PointerCCG, PointerSOB, PointerPAL, ToCCG, ToSOB, AddressCCG, AddressSOB;
    }
}