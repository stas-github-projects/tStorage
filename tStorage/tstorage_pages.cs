using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tStorage
{
    public partial class tStorage
    {

        internal class CLevelsPage
        {
            internal long l_levels_page_pos = 0;
            internal int i_levels_page_len = 0;

            internal List<long> lst_levels_pos = new List<long>(10);
            internal List<ushort> lst_filled = new List<ushort>(10);

            internal bool LoadLevel()
            {
                bool bool_ret = true;

                return bool_ret;
            }

            internal bool SaveLevel()
            {
                bool bool_ret = true;

                return bool_ret;
            }

            internal bool AddLevel()
            {
                bool bool_ret = true;

                return bool_ret;
            }
        }

        internal class CKeyPage
        {

        }
    }
}
