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

        /// <summary>
        /// Open or create storage with certain name
        /// </summary>
        public bool Open(string storage_name, string parameters = "")
        {
            bool bool_ret = true;

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

            if (key.Length == 0) { return false; } //zero-length key
            _GLOBALS.fixed_length = fixed_length;
            _GLOBALS.data_type = tStorage_service.returnTypeAndRawByteArray(ref data, fixed_length, out _GLOBALS.data);

            //search & add in tree (in memory)
            bool_ret = _TREE.AddEntry(key, 0, ref _GLOBALS.data);

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
