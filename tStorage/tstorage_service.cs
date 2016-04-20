using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace tStorage
{
    
    internal static class tStorage_service
    {
        public static bool CompareArrays(this byte[] _src, byte[] _compare_with)
        {
            return CompareArrays(_src, ref _compare_with);
        }
        public static bool CompareArrays(this byte[] _src, ref byte[] _compare_with)
        {
            int ilen1 = _src.Length, ilen2 = _compare_with.Length;
            if (ilen1 != ilen2) { return false; } //arrays are not equal
            for (int i = 0; i < ilen1; i++)
            {
                if (_src[i] != _compare_with[i])
                { return false; } //not equal
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareHashArrays(this byte[] _src, int src_start_pos, ref byte[] _compare_with, int compare_start_pos)
        {
            if (_src[src_start_pos] != _compare_with[compare_start_pos]) { return false; }
            if (_src[src_start_pos + 1] != _compare_with[compare_start_pos + 1]) { return false; }
            if (_src[src_start_pos + 2] != _compare_with[compare_start_pos + 2]) { return false; }
            if (_src[src_start_pos + 3] != _compare_with[compare_start_pos + 3]) { return false; }
            if (_src[src_start_pos + 4] != _compare_with[compare_start_pos + 4]) { return false; }
            if (_src[src_start_pos + 5] != _compare_with[compare_start_pos + 5]) { return false; }
            if (_src[src_start_pos + 6] != _compare_with[compare_start_pos + 6]) { return false; }
            if (_src[src_start_pos + 7] != _compare_with[compare_start_pos + 7]) { return false; }
            return true;
        }

        public static void InsertBytes(this byte[] _src, byte[] _what, int _pos = 0, int _length = 0)
        {
            InsertBytes(_src, ref _what, _pos, _length);
        }
        public static void InsertBytes(this byte[] _src, byte _what, int _pos = 0, int _length = 0)
        {
            int i = _src.Length;
            if (_pos < 0) { _pos = 0; }
            if (_length == 0) { _length = 1;}// _what.Length; }
            if (_pos + _length > i) { return; }//out of dimensions
            //Buffer.BlockCopy(_what, 0, _src, _pos, _length);
            _src[_pos] = _what;
        }
        public static void InsertBytes(this byte[] _src, ref byte[] _what, int _pos = 0, int _length = 0)
        {
            int i = _src.Length;
            if (_pos < 0) { _pos = 0; }
            if (_length == 0) { _length = _what.Length; }
            if (_pos + _length > i) { return; }//out of dimensions
            Buffer.BlockCopy(_what, 0, _src, _pos, _length);
        }
        public static byte[] GetBytes(this byte[] _src, int _pos = 0, int _length = 0)
        {
            byte[] b_out;
            int ilen = _src.Length;
            if (_length < 1) { _length = ilen; }
            if (_pos < 0) { _pos = 0; }
            if (ilen >= (_pos + _length))
            {
                b_out = new byte[_length];
                Buffer.BlockCopy(_src, _pos, b_out, 0, _length);//copy piece
                return b_out;
            }
            else
            { return _src; }
        }
        public static byte GetByte(this byte[] _src, int _pos = 0)
        {
            byte b_out;
            int ilen = _src.Length, _length = 1;
            if (_length < 1) { _length = ilen; }
            if (_pos < 0) { _pos = 0; }
            if (ilen >= (_pos + _length))
            {
                b_out = _src[_pos];// Buffer.BlockCopy(_src, _pos, b_out, 0, _length);//copy piece
                return b_out;
            }
            else
            { return 0; }
        }

        public static byte returnTypeAndRawByteArray(ref object _data, ushort i_fixed_string, out byte[] out_bytes)
         {
            byte _data_type = new byte();
            _data_type = 0;

            if (_data == null) { out_bytes = new byte[0]; _data_type = 0; return 0; }

            Type _type = _data.GetType();
            if (_type == typeof(bool))// = 3
            {
                out_bytes = BitConverter.GetBytes((bool)_data);
                _data_type = 3;
            }
            else if (_type == typeof(int))// = 4
            {
                out_bytes = BitConverter.GetBytes((int)_data);
                _data_type = 4;
            }
            else if (_type == typeof(long))// = 5
            {
                out_bytes = BitConverter.GetBytes((long)_data);
                _data_type = 5;
            }
            else if (_type == typeof(double))// = 6
            {
                out_bytes = BitConverter.GetBytes((double)_data);
                _data_type = 6;
            }
            else if (_type == typeof(decimal))// = 7
            {
                out_bytes = DecimalToBytes((decimal)_data);
                _data_type = 7;
            }
            else if (_type == typeof(short))// = 8
            {
                out_bytes = BitConverter.GetBytes((short)_data);
                _data_type = 8;
            }
            else if (_type == typeof(float))// = 9
            {
                out_bytes = BitConverter.GetBytes((float)_data);
                _data_type = 9;
            }
            else if (_type == typeof(char))// = 10
            {
                Encoding _enc = Encoding.Unicode;
                out_bytes = BitConverter.GetBytes((char)_data); //Encoding.Default.GetBytes((char)_data);
                _data_type = 10;
            }
            else if (_type == typeof(char[]))// = 11
            {
                //Encoding _enc = Encoding.Default;
                out_bytes = Encoding.Unicode.GetBytes((char[])_data);
                _data_type = 11;
            }
            else if (_type == typeof(string))// = 12
            {
                if (i_fixed_string == 0)//not fixed_string
                {
                    out_bytes = Encoding.Unicode.GetBytes((string)_data);
                    _data_type = 12;
                }
                else
                {
                    out_bytes = Encoding.Unicode.GetBytes((string)_data);
                    if (out_bytes.Length <= i_fixed_string)//if less or equal that it could be
                    { _data_type = 18; }
                    else
                    { _data_type = 0; }//error: length is more than buffer
                }
            }
            else if (_type == typeof(byte))// = 13
            {
                out_bytes = new byte[1];
                out_bytes[0] = (byte)_data;
                _data_type = 13;
            }
            else if (_type == typeof(byte[]))// = 14
            {
                out_bytes = (byte[])_data;
                _data_type = 14;
            }
            else if (_type == typeof(ushort))// = 15
            {
                out_bytes = BitConverter.GetBytes((ushort)_data);
                _data_type = 15;
            }
            else if (_type == typeof(uint))// = 16
            {
                out_bytes = BitConverter.GetBytes((uint)_data);
                _data_type = 16;
            }
            else if (_type == typeof(ulong))// = 17
            {
                out_bytes = BitConverter.GetBytes((ulong)_data);
                _data_type = 17;
            }
            else if(_type == typeof(string[]))// = 20
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 20;
            }
            else if(_type == typeof(bool[]))// = 22
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 22;
            }
            else if(_type == typeof(int[]))// = 23
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 23;
            }
            else if(_type == typeof(long[]))// = 24
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 24;
            }
            else if(_type == typeof(double[]))// = 25
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 25;
            }
            else if(_type == typeof(decimal[]))// = 26
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 26;
            }
            else if(_type == typeof(short[]))// = 27
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 27;
            }
            else if(_type == typeof(float[]))// = 28
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 28;
            }
            else if(_type == typeof(ushort[]))// = 29
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 29;
            }
            else if(_type == typeof(uint[]))// = 30
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 30;
            }
            else if(_type == typeof(ulong[]))// = 31
            {
                MemoryStream mstream_in = new MemoryStream();
                BinaryFormatter bformatter_in = new BinaryFormatter();
                bformatter_in.Serialize(mstream_in, _data);
                out_bytes = mstream_in.ToArray();
                _data_type = 31;
            }
            else
            {
                out_bytes = new byte[0];
                _data_type = 0;
            }
            return _data_type;
        }

        public static object returnObjectFromByteArray(ref byte[] b_output, byte _type)
        {
            switch (_type)
            {
                case 3://bool
                    bool _bool = BitConverter.ToBoolean(b_output, 0);
                    return _bool;
                case 4://int
                    int _int = BitConverter.ToInt32(b_output, 0);
                    return _int;
                case 5://long
                    long _long = BitConverter.ToInt64(b_output, 0);
                    return _long;
                case 6://double
                    double _dbl = BitConverter.ToDouble(b_output, 0);
                    return _dbl;
                case 7://decimal
                    decimal _dec = ByteArrayToDecimal(b_output, 0);//Convert.ToDecimal(BitConverter.ToDouble(b_output, 0));
                    return _dec;
                case 8://short
                    short _short = BitConverter.ToInt16(b_output, 0);
                    return _short;
                case 9://float
                    float _float = BitConverter.ToSingle(b_output, 0);
                    return _float;
                case 10://char
                    char[] _char = Encoding.Unicode.GetChars(b_output);
                    return _char[0];
                case 11://char[]
                    char[] _chararr = Encoding.Unicode.GetChars(b_output);
                    return _chararr;
                case 12://string
                    string _string = Encoding.Unicode.GetString(b_output);
                    return _string;
                case 13://byte
                    return b_output[0];
                case 14://byte[]
                    return b_output;
                case 15://ushort
                    ushort _ushort = (ushort)BitConverter.ToUInt16(b_output, 0);
                    return _ushort;
                case 16://uint
                    uint _uint = (uint)BitConverter.ToUInt32(b_output, 0);
                    return _uint;
                case 17://ulong
                    ulong _ulong = (ulong)BitConverter.ToUInt64(b_output, 0);
                    return _ulong;
                case 18://fixed_string
                    string _fixedstring = Encoding.Unicode.GetString(b_output);
                    return _fixedstring;
                case 19://fixed_byte_array
                    return b_output;
                case 20://string_array
                    MemoryStream mstream_out1 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out1 = new BinaryFormatter();
                    mstream_out1.Seek(0,SeekOrigin.Begin);
                    object _object1 = (string[])bformatter_out1.Deserialize(mstream_out1);
                    return _object1;
                case 22://bool[] array
                    MemoryStream mstream_out2 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out2 = new BinaryFormatter();
                    mstream_out2.Seek(0,SeekOrigin.Begin);
                    object _object2 = (bool[])bformatter_out2.Deserialize(mstream_out2);
                    return _object2;
                case 23://int[] array
                    MemoryStream mstream_out3 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out3 = new BinaryFormatter();
                    mstream_out3.Seek(0,SeekOrigin.Begin);
                    object _object3 = (int[])bformatter_out3.Deserialize(mstream_out3);
                    return _object3;
                case 24://long[] array
                    MemoryStream mstream_out4 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out4 = new BinaryFormatter();
                    mstream_out4.Seek(0,SeekOrigin.Begin);
                    object _object4 = (long[])bformatter_out4.Deserialize(mstream_out4);
                    return _object4;
                case 25://double[] array
                    MemoryStream mstream_out5 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out5 = new BinaryFormatter();
                    mstream_out5.Seek(0,SeekOrigin.Begin);
                    object _object5 = (double[])bformatter_out5.Deserialize(mstream_out5);
                    return _object5;
                case 26://decimal[] array
                    MemoryStream mstream_out6 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out6 = new BinaryFormatter();
                    mstream_out6.Seek(0,SeekOrigin.Begin);
                    object _object6 = (decimal[])bformatter_out6.Deserialize(mstream_out6);
                    return _object6;
                case 27://short[] array
                    MemoryStream mstream_out7 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out7 = new BinaryFormatter();
                    mstream_out7.Seek(0,SeekOrigin.Begin);
                    object _object7 = (short[])bformatter_out7.Deserialize(mstream_out7);
                    return _object7;
                case 28://float[] array
                    MemoryStream mstream_out8 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out8 = new BinaryFormatter();
                    mstream_out8.Seek(0,SeekOrigin.Begin);
                    object _object8 = (float[])bformatter_out8.Deserialize(mstream_out8);
                    return _object8;
                case 29://ushort[] array
                    MemoryStream mstream_out9 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out9 = new BinaryFormatter();
                    mstream_out9.Seek(0,SeekOrigin.Begin);
                    object _object9 = (ushort[])bformatter_out9.Deserialize(mstream_out9);
                    return _object9;
                case 30://uint[] array
                    MemoryStream mstream_out10 = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out10 = new BinaryFormatter();
                    mstream_out10.Seek(0,SeekOrigin.Begin);
                    object _object10 = (uint[])bformatter_out10.Deserialize(mstream_out10);
                    return _object10;					
                case 31://ulong[] array
                    MemoryStream mstream_out = new MemoryStream(b_output);
                    BinaryFormatter bformatter_out = new BinaryFormatter();
                    mstream_out.Seek(0,SeekOrigin.Begin);
                    object _object = (ulong[])bformatter_out.Deserialize(mstream_out);
                    return _object;		
                default: return false;
            }//switch
        }

        public static byte[] DecimalToBytes(decimal dec)
        {
            //Load four 32 bit integers from the Decimal.GetBits function
            Int32[] bits = decimal.GetBits(dec);
            //Create a temporary list to hold the bytes
            List<byte> bytes = new List<byte>();
            //iterate each 32 bit integer
            foreach (Int32 i in bits)
            {
                //add the bytes of the current 32bit integer
                //to the bytes list
                bytes.AddRange(BitConverter.GetBytes(i));
            }
            //return the bytes list as an array
            return bytes.ToArray();
        }
        
        public static decimal ByteArrayToDecimal(byte[] src, int offset)
        {
            int i1 = BitConverter.ToInt32(src, offset);
            int i2 = BitConverter.ToInt32(src, offset + 4);
            int i3 = BitConverter.ToInt32(src, offset + 8);
            int i4 = BitConverter.ToInt32(src, offset + 12);
            
            return new decimal(new int[] { i1, i2, i3, i4 });
        }
        
        public static string GetClearString_ASCII(this byte[] _src)
        {
            string sout="";
            for(int i=0;i<_src.Length;i++)
            {
                if (_src[i] == 0) { sout = Encoding.ASCII.GetString(_src.GetBytes(0, i)); break; }
            }
            return sout;
        }

        public static bool CompareText(this string _src, string what)
        {
            return CompareText(_src, what.ToCharArray());
        }
        public static bool CompareText(this string _src, char[] what)
        {
            bool bool_ret = true;

            int ilen = _src.Length, ilen2 = what.Length, ilast = ilen - 1;
            if (ilen != ilen2) { return false; }
            for (int i = 0; i < ilen;i++ )
            {
                if (_src[i] != what[i]) { return false; }
                if (ilast == i) { break; }
                if (_src[i] != what[ilast]) { return false; }
                ilast--;
            }//for
            return bool_ret;
        }

    }
}
