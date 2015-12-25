using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public class CustomReader
{
	private BinaryReader BinaryReader;

	public byte[] GetBytes(int OffsetInFile)
	{
		BinaryReader.BaseStream.Position = BinaryReader.BaseStream.Length - OffsetInFile;
		return BinaryReader.ReadBytes((int)(BinaryReader.BaseStream.Length - BinaryReader.BaseStream.Position));
	}

	public byte[] GetBytes(int OffsetInFile, int Count)
	{
		BinaryReader.BaseStream.Position = OffsetInFile;
		return BinaryReader.ReadBytes(Count);
	}

    public T ReadType<T>()
    {
        byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
        GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
        return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
    }

    public T[] ReadType<T>(uint Count)
    {
        T[] TypeArray = new T[Count];
        for (int i = 0; i < Count; i++)
        {
            byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            TypeArray[i] = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }

        return TypeArray;
    }

    public T ReadType<T>(int OffsetInFile)
	{
		BinaryReader.BaseStream.Position = OffsetInFile;
		byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
		GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
		return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
	}

	public T[] ReadType<T>(int OffsetInFile, int Count)
	{
		BinaryReader.BaseStream.Position = OffsetInFile;

		T[] TypeArray = new T[Count];
		for (int i = 0; i < Count; i++)
		{
			byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
			GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			TypeArray[i] = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
		}

		return TypeArray;
	}

	public string ReadNullTerminatedString(int OffsetInFile)
	{
		BinaryReader.BaseStream.Position = OffsetInFile;

		List<byte> StrBytes = new List<byte> (); byte b;
		while ((b = BinaryReader.ReadByte()) != 0x00)
			StrBytes.Add(b);

		return Encoding.ASCII.GetString (StrBytes.ToArray());
	}

	public CustomReader(BinaryReader FileReader)
	{
		BinaryReader = FileReader;
	}
}
