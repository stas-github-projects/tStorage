using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace tStorage
{
    public partial class tStorage
    {

        internal class CGlobals
        {
            internal string storage_name;
            internal Stream storage;
            internal long storage_length;
            internal bool storage_open;

            internal long l_virtual_storage_length = 0;

            internal byte[] storage_version = Encoding.ASCII.GetBytes(new char[] { 'T', 'S', 'T', '1' });
            internal long storage_created = 0;

            internal int i_read_buffer = 1024;
            internal char[] storage_keys_delim = new char[] { '/'};
            internal char[] storage_query_delim = new char[] { '*' };
            internal int storage_key_length = 25;
            internal int storage_path_max_length = 50; //max chars in path
            //internal int storage_max_levels_depth = 10; //max_levels
            internal ushort storage_max_keys_per_page = 1;

            //interchange
            internal ushort fixed_length;
            internal byte[] data;
            internal byte data_type;

            //forbidden path
            internal List<string> lst_forbidden_path = new List<string>()
            {
                "system/created",
                "system/storage_path_max_length",
                "system/delim",
                "system/query_delim"
            };

        }

    }
}
