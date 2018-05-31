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
}