namespace CustomUnity
{
    public interface IBit<T> { T Value { get; } }
    public struct Bit2 : IBit<int> { public int Value => 2; }
    public struct Bit3 : IBit<int> { public int Value => 3; }
    public struct Bit4 : IBit<int> { public int Value => 4; }
    public struct Bit5 : IBit<int> { public int Value => 5; }
    public struct Bit6 : IBit<int> { public int Value => 6; }
    public struct Bit7 : IBit<int> { public int Value => 7; }

    public struct NBitArray<N> where N : struct, IBit<int>
    {
        System.Int32[] store;

        public NBitArray(int count)
        {
            store = new System.Int32[(count * default(N).Value + 31) / 32];
        }
        
        public void Resize(int count)
        {
            var newsize = (count * default(N).Value + 31) / 32;
            if(store != null && store.Length != newsize) {
                var newstore = new System.Int32[newsize];
                newstore.CopyTo(store, 0);
                store = newstore;
            }
            else store = new System.Int32[newsize];
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

        public byte this[int index] {
            get {
                return Get(index);
            }
            set {
                Set(index, value);
            }
        }
    }

    public struct BitArray32
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
    }

    public struct BitArray64
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

        public bool Any()
        {
            return store1.Any() || store2.Any();
        }
    }

    public struct BitArray96
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

        public bool Any()
        {
            return store1.Any() || store2.Any() || store3.Any();
        }
    }

    public struct BitArray128
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

        public bool Any()
        {
            return store1.Any() || store2.Any() || store3.Any() || store4.Any();
        }
    }
}
