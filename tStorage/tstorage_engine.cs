using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace tStorage
{
    public partial class tStorage
    {
        tStorage.CGlobals _GLOBALS = new CGlobals();
        tstorage_tree.NodeEntryCollection _TREE = new tstorage_tree.NodeEntryCollection();
        CLevelsPage _LEVELS;// = new CLevelsPage()

        public tStorage()
        {
            //init tree
            tstorage_tree.init(_GLOBALS, _GLOBALS.storage_keys_delim);//, _GLOBALS.storage_key_max_length, _GLOBALS.storage_max_levels_depth);
            _LEVELS = new CLevelsPage(_GLOBALS);
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
                if (!bool_ret) { _GLOBALS.storage = null; _GLOBALS.storage_open = false; _GLOBALS.storage_length = 0; }
            }
            else //open
            {
                bool_ret = open_storage();
                if (!bool_ret) { _GLOBALS.storage = null; _GLOBALS.storage_open = false; _GLOBALS.storage_length = 0; }
                //load indicies
                bool_ret =load_indicies();
                if (bool_ret == false) { _TREE.Clear(); return false; }
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

            _LEVELS.l_levels_page_pos = ibuflen;
            _LEVELS.i_levels_page_len = 0;

            b_buffer.InsertBytes(_GLOBALS.storage_version, ipos); ipos += 4;
            //b_buffer.InsertBytes(BitConverter.GetBytes(_LEVELS.l_levels_page_pos), ipos); ipos += 8;
            //b_buffer.InsertBytes(BitConverter.GetBytes(_LEVELS.i_levels_page_len), ipos); ipos += 4;

            //write
            try
            {
                _GLOBALS.storage.Write(b_buffer, 0, ibuflen);
                _GLOBALS.storage.Position = ibuflen;
            }
            catch (Exception) { }
            _GLOBALS.storage_open = true;

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

            //load levels
            _GLOBALS.storage_open = true;
            bool_ret = _LEVELS.LoadLevel();
            if (bool_ret == false) { return false; }

            return bool_ret;
        }

        private bool load_indicies()
        {
            bool bool_ret = true;

            if (_GLOBALS.storage_open == false) { return false; }
            

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
        public bool Read(string[] key, string[] parameters = null)
        {
            bool bool_ret = true;

            return bool_ret;
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

            return bool_ret;
        }

        //
        // DELETE
        // 

        /// <summary>
        /// Delete one or several keys from storage
        /// <para>If certain key doesn't exist engine returns 'False'</para>
        /// </summary>
        public bool Delete(string[] key, string[] parameters = null)
        {
            bool bool_ret = true;

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

            //make pages
            int i = 0, icount = tstorage_tree.CKeysToSave.lst_items.Count, ipos = 0, i_key_length = _GLOBALS.storage_key_length + _GLOBALS.storage_path_max_length, ibuflen = icount * (i_key_length) + tstorage_tree.i_data_length;
            byte[] b_buffer = new byte[ibuflen];
            byte[] b_key;
            //byte[] b
            for (i = 0; i < icount; i++)
            {
                //string skey = tstorage_tree.CKeysToSave.lst_items[i].Key;
                tstorage_tree.CKeyItem ck = tstorage_tree.CKeysToSave.GetKeyItem(tstorage_tree.CKeysToSave.lst_items[i].i_keyitem_index);
                b_key = ck.GetBytes(); //get key bytes
                b_buffer.InsertBytes(b_key, ipos); ipos += i_key_length;
                b_buffer.InsertBytes(tstorage_tree.lst_data[tstorage_tree.CKeysToSave.lst_items[i].i_data_index], ipos); ipos += tstorage_tree.lst_data_length[tstorage_tree.CKeysToSave.lst_items[i].i_data_index];
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
