using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
//using tStorage;
using System.Threading.Tasks;

namespace tStorage
{
    internal class tstorage_tree
    {
        private static tStorage.CGlobals _glob;
        internal static int i_data_length = 0;
        internal static List<byte[]> lst_data = new List<byte[]>();
        internal static List<int> lst_data_length = new List<int>();
        internal static List<CKeyItem> lst_keyitems = new List<CKeyItem>();
        
        //params
        private static char[] str_delim;
        private static int i_delim_length;
        private static int i_path_max_length;
        //private static int max_levels_depth;

        internal static void init(tStorage.CGlobals _g, char[] _delim)//, int _max_length, int _max_levels_depth)
        { _glob = _g; str_delim = _delim; i_delim_length = str_delim.Length; i_path_max_length = _g.storage_path_max_length; }//max_key_length = _max_length; max_levels_depth = _max_levels_depth; }

        internal static void Save_Clear()
        {
            i_data_length = 0; lst_data.Clear(); lst_data_length.Clear();
        }

        internal static class CKeysToSave
        {
            internal static List<NodeEntry> lst_items = new List<NodeEntry>(10);
            internal static void Add(NodeEntry _key)
            { lst_items.Add(_key); }
            internal static CKeyItem GetKeyItem(int index)
            {
                return lst_keyitems[index];
            }
            internal static void Clear()
            { lst_items.Clear(); }
        }

        public class CKeyItem
        {
            //pos of key in storage
            public long key_pos_in_storage = 0;
            //private int 
            public byte active = 0;
            public byte data_type = 0;
            public ushort fixed_length = 0;
            public byte is_unix = 0;
            public long created = 0;
            public long value_pos = 0;
            public int value_length = 0;
            public string s_full_path = "";
            //fill
            public void Fill(string s_path, byte _data_type, ushort _fixed_length, int _value_length = 0, long _value_pos = 0)
            {
                int ilen = 0;

                active = 1;
                s_full_path = s_path;
                data_type = _data_type;
                fixed_length = _fixed_length;
                is_unix = 0;
                created = DateTime.Now.Ticks;

                //add keypos in storage
                key_pos_in_storage = _glob.l_virtual_storage_length;

                _glob.l_virtual_storage_length += (_glob.storage_key_length + _glob.storage_path_max_length);//_value_length + _glob.storage_path_max_length + );

                if (_value_pos == 0)
                { value_pos = _glob.l_virtual_storage_length; }
                else
                { value_pos = _value_pos; }

                value_length = _value_length;

                //detect fixed_length or value_length
                if(_fixed_length>0)
                { ilen = _fixed_length; }
                else
                { ilen = _value_length;                }
                i_data_length += ilen; //increase global value length // was '_value_length'

                _glob.l_virtual_storage_length += (ilen); //value_length);
            }

            public byte[] GetBytes()
            {
                int ipos = 2;
                byte[] b_buffer = new byte[_glob.storage_key_length + _glob.storage_path_max_length];

                b_buffer[0] = active; //active
                b_buffer[1] = data_type; //data type
                b_buffer.InsertBytes(BitConverter.GetBytes(fixed_length), ipos); ipos += 2; //fixed length
                b_buffer.InsertBytes(BitConverter.GetBytes(created), ipos); ipos += 8; //created
                b_buffer[ipos] = is_unix; ipos++; //is unix
                b_buffer.InsertBytes(Encoding.ASCII.GetBytes(s_full_path), ipos); ipos += _glob.storage_path_max_length;
                b_buffer.InsertBytes(BitConverter.GetBytes(value_pos), ipos); ipos += 8;
                b_buffer.InsertBytes(BitConverter.GetBytes(value_length), ipos); ipos += 4;

                return b_buffer;
            }
        }

        public class NodeEntry
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NodeEntry(bool bool_add_data_index = false)
            {
                this.Children = new NodeEntryCollection();
                this.i_keyitem_index = getKeyitemIndex();
                if (bool_add_data_index) //add data
                { this.i_data_index = getDataIndex(); }
                else
                { this.i_data_index = -1; }
            }

            public string Key { get; set; }
            public int i_data_index { get; set; }
            public int i_keyitem_index { get; set; }
            //public Dictionary<int, CKeyItem>
            public NodeEntryCollection Children { get; set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int getDataIndex()
            { return lst_data.Count - 1; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int getKeyitemIndex()
            { return lst_keyitems.Count - 1; }
        }


        private static int IndexOfEx(string html, int istart)
        {
            int j = 0, ilen = html.Length, idelim = 0, ipos = -1, idelim_length = str_delim.Length;
            bool bool_found = false;
            if (istart >= ilen) { return -1; }
            for (j = istart; j < ilen; j++)
            {
                if (html[j] == str_delim[idelim])
                {
                    idelim++;
                    if (ipos == -1) { ipos = j; }
                    if (idelim == idelim_length) { return ipos; }
                    bool_found = true;
                }
                else
                { if (bool_found == true) { bool_found = false; idelim = 0; ipos = -1; } }
            }
            return -1;
        }
        //private static bool bool_entry = false;
        private static int g_keyitem_index = 0;
        public static int sEntry_length = 0;
        private static int i_datalength = 0;

        private static bool bool_addentry_result = false;
        private static string sKey = "";
        private static int wEndIndex = 0;
        private static NodeEntry oItem;
        //private static CKeyItem _keyitem;

        public class NodeEntryCollection : Dictionary<string, NodeEntry>
        {
            public CKeyItem SearchForItem(string sEntry)
            {
                CKeyItem keyitem;
                g_keyitem_index = -1;
                sEntry_length = sEntry.Length;

                search_recursive(sEntry, 0); //start search
                if (g_keyitem_index > -1)
                { keyitem = lst_keyitems[g_keyitem_index]; }
                else
                { keyitem = new CKeyItem(); }

                return keyitem;
            }

            public bool SearchForItemQueryDelim(string sEntry,ref Dictionary<string,dynamic> dict_out)
            {
                //CKeyItem keyitem;
                bool bool_ret = false;
                g_keyitem_index = -1;
                sEntry_length = sEntry.Length;

                search_recursive_w_query_delim(sEntry, 0, ref dict_out); //start search
                if (g_keyitem_index > -1)
                { bool_ret = true;}// keyitem = lst_keyitems[g_keyitem_index]; }
                /*else
                { keyitem = new CKeyItem(); }
                */
                return bool_ret;
            }

            public bool SearchForKeyUpdate(string sEntry, object data)
            {
                bool bool_ret = true;

                //_glob.data_type = tStorage_service.returnTypeAndRawByteArray(ref data, 0, out _glob.data);

                sEntry_length = sEntry.Length;
                search_recursive_update(sEntry, 0, ref data);

                if (g_keyitem_index > -1)
                { bool_ret = true; }
                else
                { bool_ret = false; }

                return bool_ret;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void search_recursive_update(string sEntry, int wBegIndex, ref object data)
            {
                if (sEntry_length == 0) //length cashing
                { sEntry_length = sEntry.Length; }

                if (wBegIndex < sEntry_length)
                {
                    string sKey;
                    int wEndIndex;

                    wEndIndex = IndexOfEx(sEntry, wBegIndex); //sEntry.IndexOf("/", wBegIndex); --faster replacement
                    if (wEndIndex == -1)
                    {
                        wEndIndex = sEntry_length; // sEntry.Length;
                    }
                    //sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                    if (wEndIndex > wBegIndex) //--faster replacement
                    //if (!string.IsNullOrEmpty(sKey))
                    //if(sKey.Length>0)
                    {
                        NodeEntry oItem;
                        sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                        if (this.ContainsKey(sKey))
                        {
                            oItem = this[sKey];
                            if (wEndIndex == sEntry_length)
                            {
                                //change KeyItem class instance
                                int iindex = oItem.i_keyitem_index;

                                CKeyItem keyitem = lst_keyitems[iindex];

                                //ref data
                                _glob.data_type = tStorage_service.returnTypeAndRawByteArray(ref data, keyitem.fixed_length, out _glob.data);

                                /* ///test read from certain point - try to get keys' position from storage
                                int itestbuflen2 = _glob.storage_key_length + _glob.storage_path_max_length;
                                byte[] b_test_buffer2= new byte[itestbuflen2];
                                _glob.storage.Position = keyitem.key_pos_in_storage;
                                _glob.storage.Read(b_test_buffer2, 0, itestbuflen2);
                                //*/
                                if (_glob.data_type == keyitem.data_type) //if data types are equals
                                {
                                    long lpos = keyitem.value_pos;
                                    int ilen = keyitem.value_length;
                                    int idatalen = _glob.data.Length; //given
                                    ushort u_fixedlength = keyitem.fixed_length;

                                    //update changes to storage
                                    if (u_fixedlength < idatalen && u_fixedlength > 0) //was 'ilen'
                                    {
                                        g_keyitem_index = -1; return; //exit if fixed_length less than new value_length
                                    }//if
                                    else if (u_fixedlength >= idatalen) //can update //was 'ilen'
                                    {
                                        //in memory
                                        lst_keyitems[iindex].value_length = idatalen;
                                        //save key in storage
                                        int itestbuflen = _glob.storage_key_length + _glob.storage_path_max_length;
                                        byte[] b_test_buffer = keyitem.GetBytes();
                                        _glob.storage.Position = keyitem.key_pos_in_storage;
                                        _glob.storage.Write(b_test_buffer, 0, itestbuflen);
                                        //in storage
                                        _glob.storage.Position = lpos;
                                        _glob.storage.Write(_glob.data, 0, idatalen);
                                        //_glob.storage_length = _glob.storage.Length;
                                        _glob.storage.Position = _glob.storage_length;
                                    }
                                    else if (u_fixedlength==0) //fixed_length == 0 - change pos if need
                                    {
                                        //int idatalen = _glob.data.Length; //given
                                        if (idatalen > ilen) //need to change pos //new data bigger than previous
                                        {
                                            UpdateExistingKeyWithNewBiggerValue(lpos, iindex, idatalen);//, keyitem);

                                            /* WAS
                                            lpos = _glob.storage.Length; //new pos
                                            lst_keyitems[iindex].value_pos = lpos;
                                            lst_keyitems[iindex].value_length = idatalen;
                                            //save key in storage
                                            int itestbuflen = _glob.storage_key_length + _glob.storage_path_max_length;
                                            byte[] b_test_buffer = keyitem.GetBytes();
                                            _glob.storage.Position = keyitem.key_pos_in_storage;
                                            _glob.storage.Write(b_test_buffer, 0, itestbuflen);
                                            //in storage
                                            _glob.storage.Position = lpos;
                                            _glob.storage.Write(_glob.data, 0, idatalen);
                                            _glob.storage_length = lpos + idatalen;
                                            _glob.storage.Position = _glob.storage_length;
                                            */
                                        }
                                        else //just change content
                                        {
                                            //in memory
                                            lst_keyitems[iindex].value_length = idatalen;
                                            //in storage
                                            _glob.storage.Position = lpos;
                                            _glob.storage.Write(_glob.data, 0, idatalen);
                                            _glob.storage.Position = _glob.storage_length;
                                        }
                                    }

                                }
                                return;
                            }
                            else
                            { oItem.Children.search_recursive_update(sEntry, wEndIndex + i_delim_length, ref data); }
                        }
                        //else
                        //{
                        //    wEndIndex = wEndIndex;
                        //}
                        // Now add the rest to the new item's children
                        //oItem.Children.search_recursive(sEntry, wEndIndex + 1);
                        return;
                    }
                }
                else
                { sEntry_length = 0; }
            }

            private void UpdateExistingKeyWithNewBiggerValue(long lpos, int iindex, int idatalen)//, CKeyItem keyitem)
            {
                //block old record in storage
                byte[] b_tempbuffer = new byte[1];
                _glob.storage.Position = lst_keyitems[iindex].key_pos_in_storage;
                _glob.storage.Write(b_tempbuffer, 0, 1);
                //_glob.storage.Position = _glob.storage_length;
                
                //change info in existing record in memory
                int ikeylen = _glob.storage_path_max_length + _glob.storage_key_length;
                lpos = _glob.storage.Length; //new pos
                lst_keyitems[iindex].key_pos_in_storage = lpos;
                lst_keyitems[iindex].value_pos = lpos + ikeylen;
                lst_keyitems[iindex].value_length = idatalen;
                //save key in storage
                int itestbuflen = _glob.storage_key_length + _glob.storage_path_max_length;
                byte[] b_test_buffer = lst_keyitems[iindex].GetBytes();
                _glob.storage.Position = lpos;// keyitem.key_pos_in_storage;
                _glob.storage.Write(b_test_buffer, 0, ikeylen);

                //in storage
                _glob.storage_length = lpos + itestbuflen;
                _glob.storage.Position = _glob.storage_length;
                _glob.storage.Write(_glob.data, 0, idatalen);
                _glob.storage_length += idatalen;
                _glob.storage.Position = _glob.storage_length;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void search_recursive(string sEntry, int wBegIndex)
            {
                if (sEntry_length == 0) //length cashing
                { sEntry_length = sEntry.Length; }

                if (wBegIndex < sEntry_length)
                {
                    string sKey;
                    int wEndIndex;

                    wEndIndex = IndexOfEx(sEntry, wBegIndex); //sEntry.IndexOf("/", wBegIndex); --faster replacement
                    if (wEndIndex == -1)
                    {
                        wEndIndex = sEntry_length; // sEntry.Length;
                    }
                    //sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                    if (wEndIndex > wBegIndex) //--faster replacement
                    //if (!string.IsNullOrEmpty(sKey))
                    //if(sKey.Length>0)
                    {
                        NodeEntry oItem;
                        sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                        if (this.ContainsKey(sKey))
                        {
                            oItem = this[sKey];
                            if (wEndIndex == sEntry_length)
                            { g_keyitem_index = oItem.i_keyitem_index; return; }
                            else
                            { oItem.Children.search_recursive(sEntry, wEndIndex + i_delim_length); }
                        }

                        //else
                        //{
                        //    wEndIndex = wEndIndex;
                        //}
                        // Now add the rest to the new item's children
                        //oItem.Children.search_recursive(sEntry, wEndIndex + 1);
                        return;
                    }
                }
                else
                { sEntry_length = 0; }
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void search_recursive_w_query_delim(string sEntry, int wBegIndex, ref Dictionary<string, dynamic> dict_out)
            {
                //if (sEntry_length == 0) //length cashing
                { sEntry_length = sEntry.Length; }

                if (wBegIndex < sEntry_length)
                {
                    string sKey, sEntry_new2;
                    int wEndIndex;

                    wEndIndex = IndexOfEx(sEntry, wBegIndex); //sEntry.IndexOf("/", wBegIndex); --faster replacement
                    if (wEndIndex == -1)
                    {
                        wEndIndex = sEntry_length; // sEntry.Length;
                    }
                    //sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                    if (wEndIndex > wBegIndex) //--faster replacement
                    //if (!string.IsNullOrEmpty(sKey))
                    //if(sKey.Length>0)
                    {
                        NodeEntry oItem;
                        sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);

                        if (this.ContainsKey(sKey))
                        {
                            oItem = this[sKey];
                            if (wEndIndex == sEntry_length)
                            {
                                string ggg = sEntry.Substring(0, wEndIndex);
                                g_keyitem_index = oItem.i_keyitem_index; GetCKeyInfo(g_keyitem_index, ggg, ref dict_out);
                            }
                            else
                            { oItem.Children.search_recursive_w_query_delim(sEntry, wEndIndex + i_delim_length, ref dict_out); }
                        }
                        else //if found chunk == query delim (like *) -- try to find everything after this chunk
                        {
                            foreach (NodeEntry _kv in this.Values)
                            {

                                oItem = _kv;
                                g_keyitem_index = oItem.i_keyitem_index;
                                sEntry_new2 = ReplaceFirst(sEntry, new string(_glob.storage_query_delim), oItem.Key);

                                //string ggg = sEntry_new2.Substring(0, wEndIndex);
                                g_keyitem_index = oItem.i_keyitem_index; GetCKeyInfo(g_keyitem_index, sEntry_new2, ref dict_out);

                                //sEntry_length
                                oItem.Children.search_recursive_w_query_delim(sEntry_new2 + new string(_glob.storage_keys_delim) + new string(_glob.storage_query_delim),
                                    wEndIndex + oItem.Key.Length, ref dict_out);
                            }//foreach
                        }
                        return;
                    }
                }
                else
                { sEntry_length = 0; }
            }

            //replace only first occurence
            public string ReplaceFirst(string text, string search, string replace)
            {
                int pos = text.IndexOf(search);
                if (pos < 0)
                {
                    return text;
                }
                return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
            }

            private void GetCKeyInfo(int g_keyitem_index, string name, ref Dictionary<string,dynamic> dict_output)
            {
                if (g_keyitem_index > -1)
                {
                    CKeyItem ck = lst_keyitems[g_keyitem_index];

                    int ilen = 0;
                    long lpos = 0;

                    if (ck.active == 1) //if key is active
                    {
                        ilen = ck.value_length;
                        lpos = ck.value_pos;
                        if (ilen > 0 && lpos > 0)
                        {
                            byte[] b_buffer = new byte[ck.value_length];
                            _glob.storage.Position = lpos;
                            _glob.storage.Read(b_buffer, 0, ilen);
                            dict_output.Add(name, tStorage_service.returnObjectFromByteArray(ref b_buffer, ck.data_type));
                        }
                    }//if
                }//if
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AddEntry_indicies(string sEntry, int wBegIndex, ref CKeyItem keyitem)
            {
                if (sEntry_length == 0) //length cashing
                {
                    sEntry_length = sEntry.Length; bool_addentry_result = false;
                    if (sEntry_length > i_path_max_length) { return false; } //path is too long
                }

                if (wBegIndex < sEntry_length)
                {
                    //string sKey;
                    //int wEndIndex;
                    //sKey=new string(sEntry.Ta)
                    wEndIndex = IndexOfEx(sEntry, wBegIndex); //sEntry.IndexOf("/", wBegIndex); --faster replacement
                    if (wEndIndex == -1)
                    {
                        wEndIndex = sEntry_length; // sEntry.Length;
                    }
                    //sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                    if (wEndIndex > wBegIndex) //--faster replacement
                    //if (!string.IsNullOrEmpty(sKey))
                    //if(sKey.Length>0)
                    {
                        //NodeEntry oItem;
                        sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                        if (this.ContainsKey(sKey))
                        {
                            oItem = this[sKey];
                        }
                        else
                        {
                            //add keyitem
                            //CKeyItem keyitem = new CKeyItem();

                            if (wEndIndex == sEntry_length) //if it's finished chain
                            {
                                i_datalength = keyitem.value_length;
                                if (keyitem.fixed_length > i_datalength && i_datalength>0)
                                {
                                    lst_data_length.Add(keyitem.fixed_length);
                                }
                                else
                                {
                                    lst_data_length.Add(i_datalength);
                                }
                                //i_datalength = keyitem.value_length;
                                //lst_data_length.Add(i_datalength); //old
                                //keyitem.Fill(sEntry, data_type, fixed_length, i_datalength);
                                
                                lst_keyitems.Add(keyitem);
                                oItem = new NodeEntry(true);
                                //CKeysToSave.Add(oItem);
                            }
                            else //else
                            {
                                //keyitem.Fill("", data_type, fixed_length);
                                //lst_keyitems.Add(keyitem);
                                oItem = new NodeEntry();
                            }

                            oItem.Key = sKey;

                            this.Add(sKey, oItem);
                            //CKeysToSave.Add(oItem); //add to save list
                            bool_addentry_result = true;
                        }
                        // Now add the rest to the new item's children
                        bool_addentry_result = oItem.Children.AddEntry_indicies(sEntry, wEndIndex + i_delim_length, ref  keyitem);
                        //return;
                    }
                }
                else
                { sEntry_length = 0; return bool_addentry_result; }
                //
                return bool_addentry_result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AddEntry(string sEntry, int wBegIndex, ref byte data_type, ref ushort fixed_length, ref byte[] data)
            {
                if (sEntry_length == 0) //length cashing
                {
                    sEntry_length = sEntry.Length; bool_addentry_result = false;
                    if (sEntry_length > i_path_max_length) { return false; } //path is too long
                }

                if (wBegIndex < sEntry_length)
                {
                    //string sKey;
                    //int wEndIndex;
                    //sKey=new string(sEntry.Ta)
                    wEndIndex = IndexOfEx(sEntry, wBegIndex); //sEntry.IndexOf("/", wBegIndex); --faster replacement
                    if (wEndIndex == -1)
                    {
                        wEndIndex = sEntry_length; // sEntry.Length;
                    }
                    //sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                    if (wEndIndex > wBegIndex) //--faster replacement
                    //if (!string.IsNullOrEmpty(sKey))
                    //if(sKey.Length>0)
                    {
                        //NodeEntry oItem;
                        sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                        if (this.ContainsKey(sKey))
                        {
                            oItem = this[sKey];
                        }
                        else
                        {
                            //add keyitem
                            CKeyItem keyitem = new CKeyItem();

                            if (wEndIndex == sEntry_length) //if it's finished chain
                            {
                                lst_data.Add(data); //data
                                
                                //detect fixed length
                                i_datalength = data.Length;
                                if (fixed_length > 0)
                                {
                                    if (fixed_length > i_data_length)
                                    { 
                                        lst_data_length.Add(fixed_length);
                                    }
                                    else
                                    { 
                                        lst_data_length.Add(i_datalength);
                                    }
                                }
                                else
                                { 
                                    lst_data_length.Add(i_datalength);
                                }

                                //fill up keyitem
                                keyitem.Fill(sEntry, data_type, fixed_length, i_datalength);
                                lst_keyitems.Add(keyitem);
                                oItem = new NodeEntry(true);
                                CKeysToSave.Add(oItem);
                            }
                            else //else
                            {
                                //keyitem.Fill("", data_type, fixed_length);
                                lst_keyitems.Add(keyitem);
                                oItem = new NodeEntry();
                            }

                            oItem.Key = sKey;
                            
                            this.Add(sKey, oItem);
                            //CKeysToSave.Add(oItem); //add to save list
                            bool_addentry_result = true;
                        }
                        // Now add the rest to the new item's children
                        bool_addentry_result = oItem.Children.AddEntry(sEntry, wEndIndex + i_delim_length,ref  data_type, ref  fixed_length, ref data);
                        //return;
                    }
                }
                else
                { sEntry_length = 0; return bool_addentry_result; }
                //
                return bool_addentry_result;
            }


            public bool SearchForKeyDelete(string sEntry)
            {
                bool bool_ret = true;

                //_glob.data_type = tStorage_service.returnTypeAndRawByteArray(ref data, 0, out _glob.data);

                sEntry_length = sEntry.Length;
                search_delete(sEntry, 0);

                if (g_keyitem_index > -1)
                { bool_ret = true; }
                else
                { bool_ret = false; }

                return bool_ret;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void search_delete(string sEntry, int wBegIndex)
            {
                if (sEntry_length == 0) //length cashing
                { sEntry_length = sEntry.Length; }

                if (wBegIndex < sEntry_length)
                {
                    string sKey;
                    int wEndIndex;

                    wEndIndex = IndexOfEx(sEntry, wBegIndex); //sEntry.IndexOf("/", wBegIndex); --faster replacement
                    if (wEndIndex == -1)
                    {
                        wEndIndex = sEntry_length; // sEntry.Length;
                    }
                    //sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                    if (wEndIndex > wBegIndex) //--faster replacement
                    //if (!string.IsNullOrEmpty(sKey))
                    //if(sKey.Length>0)
                    {
                        NodeEntry oItem;
                        sKey = sEntry.Substring(wBegIndex, wEndIndex - wBegIndex);
                        if (this.ContainsKey(sKey))
                        {
                            oItem = this[sKey];
                            if (wEndIndex == sEntry_length)
                            { 
                                byte[] b_tempbuffer = new byte[1];
                                
                                g_keyitem_index = oItem.i_keyitem_index;
                                CKeyItem keyitem = lst_keyitems[g_keyitem_index];
                                
                                //block in memory
                                lst_keyitems[g_keyitem_index].active = 0;
                                //block in storage
                                _glob.storage.Position = keyitem.key_pos_in_storage;
                                _glob.storage.Write(b_tempbuffer, 0, 1);
                                _glob.storage.Position = _glob.storage_length;
                                return;
                            }
                            else
                            { oItem.Children.search_delete(sEntry, wEndIndex + i_delim_length); }
                        }
                        return;
                    }
                }
                else
                { sEntry_length = 0; }
            }

        }

    }

}
