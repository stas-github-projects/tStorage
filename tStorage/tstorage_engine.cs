using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;

namespace tStorage
{
    public partial class tStorage
    {
        tStorage.CGlobals _GLOBALS = new CGlobals();
        tstorage_tree.NodeEntryCollection _TREE = new tstorage_tree.NodeEntryCollection();
        //CLevelsPage _LEVELS;// = new CLevelsPage()

        public tStorage()
        {
            //init tree
            tstorage_tree.init(_GLOBALS, _GLOBALS.storage_keys_delim);//, _GLOBALS.storage_key_max_length, _GLOBALS.storage_max_levels_depth);
            //_LEVELS = new CLevelsPage(_GLOBALS);
        }

        /// <summary>
        /// Open or create storage with certain name
        /// </summary>
        public bool Open(string storage_name, string parameters = "")
        {
            bool bool_ret = true;

            _GLOBALS.storage_name = storage_name + ".tst";
            FileInfo finfo = new FileInfo(_GLOBALS.storage_name);
            if (!finfo.Exists) //create
            {
                bool_ret = create_storage();
                if (bool_ret == false) { _GLOBALS.storage = null; _GLOBALS.storage_open = false; _GLOBALS.storage_length = 0; return false; }
            }
            else //open
            {
                bool_ret = open_storage();
                if (!bool_ret) { _GLOBALS.storage = null; _GLOBALS.storage_open = false; _GLOBALS.storage_length = 0; return false; }
                //load indicies
                bool_ret = load_indicies();
                if (bool_ret == false) { _TREE.Clear(); _GLOBALS.storage = null; _GLOBALS.storage_open = false; _GLOBALS.storage_length = 0;  return false; }
            }

            return bool_ret;
        }

        private bool create_storage()
        {
            bool bool_ret = true;

            _GLOBALS.storage = new FileStream(_GLOBALS.storage_name, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 1024 * 1024, FileOptions.Asynchronous);
            if (_GLOBALS.storage == null) { return false; }

            //defaults
            int ibuflen = 4;// +8 + 4; //version + levels_page_pos + levels_page_length
            int ipos = 0;
            byte[] b_buffer = new byte[ibuflen];

            _GLOBALS.storage_length = ibuflen;

            //_LEVELS.l_levels_page_pos = ibuflen;
            //_LEVELS.i_levels_page_len = 0;

            b_buffer.InsertBytes(_GLOBALS.storage_version, ipos); ipos += 4;
            //b_buffer.InsertBytes(BitConverter.GetBytes(_LEVELS.l_levels_page_pos), ipos); ipos += 8;
            //b_buffer.InsertBytes(BitConverter.GetBytes(_LEVELS.i_levels_page_len), ipos); ipos += 4;

            //write
            try
            {
                _GLOBALS.storage_open = true;
                //header
                _GLOBALS.storage.Write(b_buffer, 0, ibuflen);
                _GLOBALS.storage.Position = ibuflen;
                //settings
                bool_ret = write_params();
            }
            catch (Exception) { }
            if (bool_ret == false) { return false; }

            return bool_ret;
        }

        private bool open_storage()
        {
            bool bool_ret = true;

            //read
            int ibuflen = 4;// +8 + 4; //version + levels_page_pos + levels_page_length
            int ipos = 0, ilen = 0;
            byte[] b_buffer = new byte[ibuflen];

            _GLOBALS.storage = new FileStream(_GLOBALS.storage_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 1024 * 1024, FileOptions.Asynchronous);
            if (_GLOBALS.storage == null) { return false; }

            _GLOBALS.storage_length = _GLOBALS.storage.Length;
            _GLOBALS.storage.Position = 0;
            ilen = _GLOBALS.storage.Read(b_buffer, 0, ibuflen);
            if (ilen < ibuflen) { return false; }

            //parse
            _GLOBALS.storage_version = b_buffer.GetBytes(ipos, 4); ipos += 4;
            //_LEVELS.l_levels_page_pos = BitConverter.ToInt64(b_buffer.GetBytes(ipos, 8), 0); ipos += 8;
            //_LEVELS.i_levels_page_len = BitConverter.ToInt32(b_buffer.GetBytes(ipos, 4), 0); ipos += 4;

            //read settings
            _GLOBALS.storage_open = true;

            bool_ret = load_indicies(); //load indicies
            if (bool_ret == false) { return false; }
            bool_ret = read_params();

            //if (bool_ret==false) { return false; }
            //bool_ret = _LEVELS.LoadLevel();
            //if (bool_ret == false) { return false; }

            return bool_ret;
        }

        private bool load_indicies()
        {
            bool bool_ret = true, bool_exit = false;

            if (_GLOBALS.storage_open == false) { return false; }

            int i = 0, inewpos = 0, ilen = 1, ipos = 0, ikeylen = _GLOBALS.storage_key_length + _GLOBALS.storage_path_max_length, _len = 0;
            long l_pos = 4, _created = 0, _pos = 0, l_keypos = 0;
            byte _active = 0, _data_type = 0, _is_unix = 0;
            ushort _fixed = 0;
            string spath = "";
            byte[] b_buffer = new byte[_GLOBALS.i_read_buffer];

            while (ilen > 0)
            {
                _GLOBALS.storage.Position = l_pos;
                ilen = _GLOBALS.storage.Read(b_buffer, 0, _GLOBALS.i_read_buffer);
                if (ilen == 0) { break; }
                if (ilen < _GLOBALS.i_read_buffer) { bool_exit = true; }

                //parse
                //for (i = 0; i < ilen; i++)
                ipos = 0;
                while (ipos < ilen)
                {
                    //if (b_buffer[i] == 0) //if record is blocked
                    //{ ipos += ikeylen; } //skip
                    //else
                    {
                        //detect completeness
                        if ((ipos + ikeylen) > ilen) //need to load next
                        { inewpos = (ipos + ikeylen) - ilen; l_pos += (ilen - ikeylen + inewpos); break; }
                        //parse next
                        l_keypos = ipos; //key pos
                        _active = b_buffer[ipos]; ipos++; //active
                        _data_type = b_buffer[ipos]; ipos++; //data_type
                        _fixed = BitConverter.ToUInt16(b_buffer.GetBytes(ipos, 2), 0); ipos += 2; //fixed_length
                        _created = BitConverter.ToInt64(b_buffer.GetBytes(ipos, 8), 0); ipos += 8; //created
                        _is_unix = b_buffer[ipos]; ipos++; //is_unix
                        spath = b_buffer.GetBytes(ipos, _GLOBALS.storage_path_max_length).GetClearString_ASCII(); 
                        ipos += _GLOBALS.storage_path_max_length; //path
                        _pos = BitConverter.ToInt64(b_buffer.GetBytes(ipos, 8), 0); ipos += 8; //pos
                        _len = BitConverter.ToInt32(b_buffer.GetBytes(ipos, 4), 0); ipos += 4; //len

                        if(_active==0)
                        { i = i; }

                        if (_fixed > 0)
                        { ipos += _fixed; }
                        else
                        { ipos += _len; }

                        //keyitem
                        if (_active == 1) //(b_buffer[l_keypos] != 0) //record is NOT blocked
                        {
                            tstorage_tree.CKeyItem ckeyitem = new tstorage_tree.CKeyItem();
                            ckeyitem.active = 1;
                            ckeyitem.created = _created;
                            ckeyitem.data_type = _data_type;
                            ckeyitem.fixed_length = _fixed;
                            ckeyitem.is_unix = _is_unix;
                            ckeyitem.s_full_path = spath;
                            ckeyitem.value_length = _len;
                            ckeyitem.value_pos = _pos;
                            ckeyitem.key_pos_in_storage = (l_pos + l_keypos); //pos of key in storage
                            //add to tree
                            tstorage_tree.sEntry_length = spath.Length;
                            _TREE.AddEntry_indicies(spath, 0, ref ckeyitem);
                        }
                    }//else
                }//for

                if (bool_exit) { break; } //exit

            }//while

            return bool_ret;
        }

        private bool write_params()
        {
            bool bool_ret = true;

            _GLOBALS.storage_created = DateTime.Now.Ticks; //current time

            Create("system/created",_GLOBALS.storage_created);
            Create("system/storage_path_max_length", _GLOBALS.storage_path_max_length);
            Create("system/delim", _GLOBALS.storage_keys_delim);
            Create("system/query_delim", _GLOBALS.storage_query_delim);
            Commit(); //save

            return bool_ret;
        }

        private bool read_params()
        {
            bool bool_ret = true;
            
            string[] arr_query = new string[4]
            {
                "system/created",
                "system/storage_path_max_length",
                "system/delim",
                "system/query_delim"
            };
            List<dynamic> lst_ret = Read(arr_query);

            _GLOBALS.storage_created = lst_ret[0];
            _GLOBALS.storage_path_max_length = lst_ret[1];
            _GLOBALS.storage_keys_delim = lst_ret[2];
            _GLOBALS.storage_query_delim = lst_ret[3];

            return bool_ret;
        }


        //
        // CREATE
        //
        
        /// <summary>
        /// Create new pair key path / value
        /// <para>If certain key is already exist engine returns 'False'</para>
        /// </summary>
        public bool Create(string key, object data = null, ushort fixed_length = 0)
        {
            bool bool_ret = true;

            if (_GLOBALS.l_virtual_storage_length == 0) { _GLOBALS.l_virtual_storage_length = _GLOBALS.storage.Length; }
            if (key.Length == 0) { return false; } //zero-length key
            _GLOBALS.fixed_length = fixed_length;
            _GLOBALS.data_type = tStorage_service.returnTypeAndRawByteArray(ref data, fixed_length, out _GLOBALS.data);

            //search & add in tree (in memory)
            bool_ret = _TREE.AddEntry(key, 0, ref _GLOBALS.data_type, ref _GLOBALS.fixed_length, ref _GLOBALS.data);

            return bool_ret;
        }

        //
        // READ
        //

        /// <summary>
        /// Read one or several pairs of values from given keys
        /// <para>If certain key doesn't exist engine returns 'False'</para>
        /// </summary>

        public List<dynamic> Read(string key, string[] parameters = null)
        {
            return Read(new string[] { key }, parameters);
        }
        public List<dynamic> Read(string[] key, string[] parameters = null)
        {
            List<dynamic> lst_out = new List<dynamic>(10);
            if (_GLOBALS.storage_open == false) { return lst_out; }

            int i = 0, ilen = 0;
            long lpos = 0;

            for (i = 0; i < key.Length; i++)
            {
                tstorage_tree.CKeyItem ck = _TREE.SearchForItem(key[i]);

                if (ck.active == 1) //if key is active
                {
                    ilen = ck.value_length;
                    lpos = ck.value_pos;
                    if (ilen > 0 && lpos > 0)
                    {
                        byte[] b_buffer = new byte[ck.value_length];
                        _GLOBALS.storage.Position = lpos;
                        _GLOBALS.storage.Read(b_buffer, 0, ilen);
                        lst_out.Add(tStorage_service.returnObjectFromByteArray(ref b_buffer, ck.data_type));
                    }
                }//if
            }//for

            return lst_out;
        }

        //
        // UPDATE
        //

        /// <summary>
        /// Update certain key with new value
        /// <para>If certain key doesn't exist engine returns 'False'</para>
        /// </summary>
        public bool Update(string key, object data = null, string[] parameters = null)
        {
            bool bool_ret = true;
            if (_GLOBALS.storage_open == false) { return false; }

            bool_ret = _TREE.SearchForKeyUpdate(key, data);//, _keyitem);

            return bool_ret;
        }

        //
        // DELETE
        // 

        /// <summary>
        /// Delete one or several keys from storage
        /// <para>If certain key doesn't exist engine returns 'False'</para>
        /// </summary>
        public bool Delete(string key)
        {
            bool bool_ret = true;
            if (_GLOBALS.storage_open == false) { return false; }

            bool_ret = _TREE.SearchForKeyDelete(key);//, _keyitem);

            return bool_ret;
        }
        

        //
        // Commit
        //

        /// <summary>
        /// Commit changes to storage
        /// <para>If there will occurs any errors engine returns 'False'</para>
        /// </summary>
        public bool Commit()
        {
            bool bool_ret = true;

            if (_GLOBALS.storage_open == false) { return false; }

            //detect forbidden path


            //make pages
            int i = 0, icount = tstorage_tree.CKeysToSave.lst_items.Count, ipos = 0;
            int i_key_length = _GLOBALS.storage_key_length + _GLOBALS.storage_path_max_length, ibuflen = icount * (i_key_length) + tstorage_tree.i_data_length;
            byte[] b_buffer = new byte[ibuflen];
            byte[] b_key;
            //byte[] b
            for (i = 0; i < icount; i++)
            {
                //string skey = tstorage_tree.CKeysToSave.lst_items[i].Key;
                tstorage_tree.CKeyItem ck = tstorage_tree.CKeysToSave.GetKeyItem(tstorage_tree.CKeysToSave.lst_items[i].i_keyitem_index);
                b_key = ck.GetBytes(); //get key bytes
                b_buffer.InsertBytes(b_key, ipos); ipos += i_key_length;
                b_buffer.InsertBytes(tstorage_tree.lst_data[tstorage_tree.CKeysToSave.lst_items[i].i_data_index], ipos);
                ipos += tstorage_tree.lst_data_length[tstorage_tree.CKeysToSave.lst_items[i].i_data_index];
            }//for

            //save data
            _GLOBALS.storage.Position = _GLOBALS.storage_length;
            _GLOBALS.storage.Write(b_buffer, 0, ibuflen);
            _GLOBALS.storage_length += ipos;
            _GLOBALS.storage.Position = _GLOBALS.storage_length;

            //save levels_page
            //bool_ret = _LEVELS.SaveLevel();

            //clear
            tstorage_tree.CKeysToSave.Clear();
            tstorage_tree.Save_Clear();
            _GLOBALS.l_virtual_storage_length = 0;

            return bool_ret;
        }


        //
        // OTHER SERVICES
        //

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public override string ToString()
        {
            return base.ToString();
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

    }
}
