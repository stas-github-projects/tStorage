using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tStorage
{
    internal class tstorage_tree
    {
        private static List<byte[]> lst_data = new List<byte[]>();
        private static List<CKeyItem> lst_keyitems = new List<CKeyItem>();
        
        //params
        private static char[] str_delim;
        private static int i_delim_length;
        private static int max_key_length;
        private static int max_levels_depth;

        internal static void init(char[] _delim, int _max_length, int _max_levels_depth)
        { str_delim = _delim; i_delim_length = str_delim.Length; max_key_length = _max_length; max_levels_depth = _max_levels_depth; }

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
            //TO-DO
            byte data_type = 0;
            ushort fixed_length = 0;
            byte is_unix = 0;
            long created = 0;
            long value_pos = 0;
            int value_length = 0;
            //fill
            public void Fill(byte _data_type, ushort _fixed_length, int _value_length = 0)
            {
                data_type = _data_type;
                fixed_length = _fixed_length;
                is_unix = 0;
                created = DateTime.Now.Ticks;
                //value_pos - fill in the end
                value_length = _value_length;
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
        private static int sEntry_length = 0;

        private static bool bool_addentry_result = false;
        private static string sKey = "";
        private static int wEndIndex = 0;
        private static NodeEntry oItem;

        public class NodeEntryCollection : Dictionary<string, NodeEntry>
        {
            public CKeyItem SearchForItem(string sEntry)
            {
                CKeyItem keyitem;
                g_keyitem_index = -1;
                search_recursive(sEntry, 0);
                if (g_keyitem_index > -1)
                { keyitem = lst_keyitems[g_keyitem_index]; }
                else
                { keyitem = new CKeyItem(); }
                return keyitem;
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
            public bool AddEntry(string sEntry, int wBegIndex, ref byte data_type, ref ushort fixed_length, ref byte[] data)
            {
                if (sEntry_length == 0) //length cashing
                { sEntry_length = sEntry.Length; bool_addentry_result = false; }

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
                                keyitem.Fill(data_type, fixed_length,data.Length);
                                lst_keyitems.Add(keyitem);
                                oItem = new NodeEntry(true);
                            }
                            else //else
                            {
                                keyitem.Fill(data_type, fixed_length);
                                lst_keyitems.Add(keyitem);
                                oItem = new NodeEntry();
                            }

                            oItem.Key = sKey;
                            
                            this.Add(sKey, oItem);
                            CKeysToSave.Add(oItem); //add to save list
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
        }

    }

}
