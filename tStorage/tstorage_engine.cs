using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tStorage
{
    public partial class tStorage
    {
        tStorage.CGlobals _GLOBALS = new CGlobals();
        tstorage_tree.NodeEntryCollection _TREE = new tstorage_tree.NodeEntryCollection();

        public tStorage()
        {
            //init tree
            tstorage_tree.init(_GLOBALS.storage_keys_delim, _GLOBALS.storage_key_max_length, _GLOBALS.storage_max_levels_depth);
        }

        public bool Open(string storage_name, string parameters = "")
        {
            bool bool_ret = true;

            return bool_ret;
        }

        //
        // CREATE
        //

        public bool Create(string key, object data = null, ushort fixed_length = 0)
        {
            bool bool_ret = true;

            if (key.Length == 0) { return false; } //zero-length key
            _GLOBALS.fixed_length = fixed_length;
            _GLOBALS.data_type = tStorage_service.returnTypeAndRawByteArray(ref data, fixed_length, out _GLOBALS.data);

            _TREE.AddEntry(key, 0, ref _GLOBALS.data);

            return bool_ret;
        }

        //
        // READ
        //

        public bool Read(string[] key, string[] parameters = null)
        {
            bool bool_ret = true;

            return bool_ret;
        }

        //
        // UPDATE
        //

        public bool Update(string key, object data, string[] parameters = null)
        {
            bool bool_ret = true;

            return bool_ret;
        }

        //
        // DELETE
        // 

        public bool Delete(string key, string[] parameters = null)
        {
            bool bool_ret = true;

            return bool_ret;
        }


    }
}
