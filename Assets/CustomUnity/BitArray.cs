using System;
using System.Collections.Generic;

namespace CustomUnity
{
    public interface IBit<T> { T Value { get; } }

    public struct Bit2 : IBit<int>, IEquatable<Bit2>
    {
        public int Value => 2;

        public override bool Equals(object obj) => obj is Bit2 bit && Equals(bit);

        public bool Equals(Bit2 other) => Value == other.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }

    public struct Bit3 : IBit<int>, IEquatable<Bit3>
    {
        public int Value => 3;

        public override bool Equals(object obj) => obj is Bit3 bit && Equals(bit);

        public bool Equals(Bit3 other) => Value == other.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }

    public struct Bit4 : IBit<int>, IEquatable<Bit4>
    {
        public int Value => 4;

        public override bool Equals(object obj) => obj is Bit4 bit && Equals(bit);

        public bool Equals(Bit4 other) => Value == other.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }

    public struct Bit5 : IBit<int>, IEquatable<Bit5>
    {
        public int Value => 5;

        public override bool Equals(object obj) => obj is Bit5 bit && Equals(bit);

        public bool Equals(Bit5 other) => Value == other.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }

    public struct Bit6 : IBit<int>, IEquatable<Bit6>
    {
        public int Value => 6;

        public override bool Equals(object obj) => obj is Bit6 bit && Equals(bit);

        public bool Equals(Bit6 other) => Value == other.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }

    public struct Bit7 : IBit<int>, IEquatable<Bit7>
    {
        public int Value => 7;

        public override bool Equals(object obj) => obj is Bit7 bit && Equals(bit);

        public bool Equals(Bit7 other) => Value == other.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }

    public struct NBitArray<N> : IEquatable<NBitArray<N>> where N : struct, IBit<int>
    {
        int[] store;

        public NBitArray(int count)
        {
            store = new int[(count * default(N).Value + 31) / 32];
        }
        
        public void Resize(int count)
        {
            var newsize = (count * default(N).Value + 31) / 32;
            if(store != null && store.Length != newsize) {
                var newstore = new int[newsize];
                newstore.CopyTo(store, 0);
                store = newstore;
            }
            else store = new int[newsize];
        }

        public byte Get(int index)
        {
            var bitoffset = index * default(N).Value;
            var storeindex = bitoffset / 32;
            var shiftcount = bitoffset - (storeindex * 32);
            return (byte)((store[storeindex] >> shiftcount) & (default(N).Value - 1));
        }
        
        public void Set(int index, int value)
        {
            var bitoffset = index * default(N).Value;
            var storeindex = bitoffset / 32;
            var shiftcount = bitoffset - (storeindex * 32);
            var mask = default(N).Value - 1;
            store[storeindex] = (store[storeindex] & ~(mask << shiftcount)) | ((value & mask) << shiftcount);
        }

        public void Clear()
        {
            for(int i = 0; i < store.Length; ++i) store[i] = 0;
        }

        public override bool Equals(object obj) => obj is NBitArray<N> array && Equals(array);

        public bool Equals(NBitArray<N> other) => EqualityComparer<int[]>.Default.Equals(store, other.store);

        public override int GetHashCode() => 383674718 + EqualityComparer<int[]>.Default.GetHashCode(store);

        public byte this[int index] {
            get {
                return Get(index);
            }
            set {
                Set(index, value);
            }
        }
    }

    public struct BitArray32 : IEquatable<BitArray32>
    {
        public uint store;

        public bool this[int i] {
            get {
                return (store & (1 << i)) > 0;
            }
            set {
                store = (store & (uint)~(1 << i)) | (value ? (uint)(1 << i) : 0);
            }
        }

        public static BitArray32 operator&(BitArray32 lhs, BitArray32 rhs)
        {
            return new BitArray32 { store = lhs.store & rhs.store };
        }

        public static BitArray32 operator|(BitArray32 lhs, BitArray32 rhs)
        {
            return new BitArray32 { store = lhs.store | rhs.store };
        }

        public static BitArray32 operator ^(BitArray32 lhs, BitArray32 rhs)
        {
            return new BitArray32 { store = lhs.store ^ rhs.store };
        }

        public static BitArray32 operator ~(BitArray32 src)
        {
            return new BitArray32 { store = ~src.store };
        }

        public bool Any()
        {
            return store != 0;
        }

        public override bool Equals(object obj) => obj is BitArray32 array && Equals(array);

        public bool Equals(BitArray32 other) => store == other.store;

        public override int GetHashCode() => 383674718 + store.GetHashCode();
    }

    public struct BitArray64 : IEquatable<BitArray64>
    {
        public BitArray32 store1;
        public BitArray32 store2;

        public bool this[int i] {
            get {
                if(i < 32) return store1[i];
                return store2[i - 32];
            }
            set {
                if(i < 32) store1[i] = value;
                else store2[i - 32] = value;
            }
        }

        public static BitArray64 operator &(BitArray64 lhs, BitArray64 rhs)
        {
            return new BitArray64 { store1 = lhs.store1 & rhs.store1, store2 = lhs.store2 & rhs.store2 };
        }

        public static BitArray64 operator |(BitArray64 lhs, BitArray64 rhs)
        {
            return new BitArray64 { store1 = lhs.store1 | rhs.store1, store2 = lhs.store2 | rhs.store2 };
        }

        public static BitArray64 operator ^(BitArray64 lhs, BitArray64 rhs)
        {
            return new BitArray64 { store1 = lhs.store1 ^ rhs.store1, store2 = lhs.store2 ^ rhs.store2 };
        }

        public static BitArray64 operator ~(BitArray64 src)
        {
            return new BitArray64 { store1 = ~src.store1, store2 = ~src.store2 };
        }

        public bool Any() => store1.Any() || store2.Any();

        public override bool Equals(object obj) => obj is BitArray64 array && Equals(array);

        public bool Equals(BitArray64 other) => store1.Equals(other.store1) && store2.Equals(other.store2);

        public override int GetHashCode()
        {
            return HashCode.Combine(store1, store2);
        }
    }

    public struct BitArray96 : IEquatable<BitArray96>
    {
        public BitArray32 store1;
        public BitArray32 store2;
        public BitArray32 store3;

        public bool this[int i] {
            get {
                if(i < 32) return store1[i];
                else if(i < 64) return store2[i - 32];
                return store3[i - 64];
            }
            set {
                if(i < 32) store1[i] = value;
                else if(i < 64) store2[i - 32] = value;
                else store3[i - 64] = value;
            }
        }

        public static BitArray96 operator &(BitArray96 lhs, BitArray96 rhs)
        {
            return new BitArray96 { store1 = lhs.store1 & rhs.store1, store2 = lhs.store2 & rhs.store2, store3 = lhs.store3 & rhs.store3 };
        }

        public static BitArray96 operator |(BitArray96 lhs, BitArray96 rhs)
        {
            return new BitArray96 { store1 = lhs.store1 | rhs.store1, store2 = lhs.store2 | rhs.store2, store3 = lhs.store3 | rhs.store3 };
        }

        public static BitArray96 operator ^(BitArray96 lhs, BitArray96 rhs)
        {
            return new BitArray96 { store1 = lhs.store1 ^ rhs.store1, store2 = lhs.store2 ^ rhs.store2, store3 = lhs.store3 ^ rhs.store3 };
        }

        public static BitArray96 operator ~(BitArray96 src)
        {
            return new BitArray96 { store1 = ~src.store1, store2 = ~src.store2, store3 = ~src.store3 };
        }

        public bool Any() => store1.Any() || store2.Any() || store3.Any();

        public override bool Equals(object obj) => obj is BitArray96 array && Equals(array);

        public bool Equals(BitArray96 other) => store1.Equals(other.store1) && store2.Equals(other.store2) && store3.Equals(other.store3);

        public override int GetHashCode()
        {
            return HashCode.Combine(store1, store2, store3);
        }
    }

    public struct BitArray128 : IEquatable<BitArray128>
    {
        public BitArray32 store1;
        public BitArray32 store2;
        public BitArray32 store3;
        public BitArray32 store4;

        public bool this[int i] {
            get {
                if(i < 32) return store1[i];
                else if(i < 64) return store2[i - 32];
                else if(i < 96) return store3[i - 64];
                return store4[i - 96];
            }
            set {
                if(i < 32) store1[i] = value;
                else if(i < 64) store2[i - 32] = value;
                else if(i < 96) store3[i - 64] = value;
                else store4[i - 96] = value;
            }
        }

        public static BitArray128 operator &(BitArray128 lhs, BitArray128 rhs)
        {
            return new BitArray128 { store1 = lhs.store1 & rhs.store1, store2 = lhs.store2 & rhs.store2, store3 = lhs.store3 & rhs.store3, store4 = lhs.store4 & rhs.store4 };
        }

        public static BitArray128 operator |(BitArray128 lhs, BitArray128 rhs)
        {
            return new BitArray128 { store1 = lhs.store1 | rhs.store1, store2 = lhs.store2 | rhs.store2, store3 = lhs.store3 | rhs.store3, store4 = lhs.store4 | rhs.store4 };
        }

        public static BitArray128 operator ^(BitArray128 lhs, BitArray128 rhs)
        {
            return new BitArray128 { store1 = lhs.store1 ^ rhs.store1, store2 = lhs.store2 ^ rhs.store2, store3 = lhs.store3 ^ rhs.store3, store4 = lhs.store4 ^ rhs.store4 };
        }

        public static BitArray128 operator ~(BitArray128 src)
        {
            return new BitArray128 { store1 = ~src.store1, store2 = ~src.store2, store3 = ~src.store3, store4 = ~src.store4 };
        }

        public bool Any() => store1.Any() || store2.Any() || store3.Any() || store4.Any();

        public override bool Equals(object obj) => obj is BitArray128 array && Equals(array);

        public bool Equals(BitArray128 other) => store1.Equals(other.store1) && store2.Equals(other.store2) && store3.Equals(other.store3) && store4.Equals(other.store4);

        public override int GetHashCode()
        {
            return HashCode.Combine(store1, store2, store3, store4);
        }
    }
}
