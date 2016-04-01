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
            internal CGlobals _g;
            internal long l_levels_page_pos = 0;
            internal int i_levels_page_len = 0;

            internal List<ushort> lst_level = new List<ushort>(10);
            internal List<long> lst_levels_pos = new List<long>(10);
            internal List<ushort> lst_filled = new List<ushort>(10);

            internal CLevelsPage(CGlobals _glob)
            { _g = _glob; }

            internal bool LoadLevel()
            {
                bool bool_ret = true;
                if (_g.storage_open == false) { return false; }

                //read
                int i = 0, ipos = 0;
                long l_pos = 0;
                ushort u_level = 0,u_fil = 0;
                byte[] b_buffer = new byte[i_levels_page_len];
                _g.storage.Position = l_levels_page_pos;
                _g.storage.Read(b_buffer, 0, i_levels_page_len);
                //clear prev levels
                lst_level.Clear(); lst_levels_pos.Clear(); lst_filled.Clear();
                //parse
                for (i = 0; i < i_levels_page_len; i++)
                {
                    u_level = b_buffer.GetByte(ipos); ipos += 2;
                    l_pos = BitConverter.ToInt64(b_buffer.GetBytes(ipos, 8), 0); ipos += 8;
                    u_fil = BitConverter.ToUInt16(b_buffer.GetBytes(ipos, 2), 0); ipos += 2;
                    if (l_pos == 0) { break; }
                    lst_level.Add(u_level);
                    lst_levels_pos.Add(l_pos);
                    lst_filled.Add(u_fil);
                }//for

                return bool_ret;
            }

            internal bool SaveLevel()
            {
                bool bool_ret = true;

                if (_g.storage_open == false) { return false; }
                //gather info
                int i = 0, icount = lst_level.Count, ipos = 0, ibuflen = (icount * (2 + 8 + 2));
                byte[] b_buffer = new byte[ibuflen];
                for (i = 0; i < icount; i++)
                {
                    b_buffer.InsertBytes(BitConverter.GetBytes(lst_level[i]), ipos); ipos += 2;
                    b_buffer.InsertBytes(BitConverter.GetBytes(lst_levels_pos[i]), ipos); ipos += 8;
                    b_buffer.InsertBytes(BitConverter.GetBytes(lst_filled[i]), ipos); ipos += 2;
                }//for
                //write
                try
                {
                    _g.storage.Position = _g.storage_length;
                    _g.storage.Write(b_buffer, 0, ibuflen);
                    _g.storage_length += ibuflen;
                }
                catch(Exception)
                { bool_ret = false; }
                return bool_ret;
            }

            internal bool AddLevel(ushort level, long pos)
            {
                bool bool_ret = true;

                //search for level
                int i = 0, icount = lst_level.Count;
                for (i = icount; i > 0; i--)
                {
                    if (lst_level[i] == level) //found level
                    {
                        if (lst_filled[i] >= _g.storage_max_keys_per_page) //check if it's filled - create new
                        {
                            lst_level.Add(level);
                            lst_filled.Add(1);
                            lst_levels_pos.Add(pos);
                        }
                        else //update existing
                        { lst_filled[i]++; lst_levels_pos[i] = pos; }
                    }
                }//for
                return bool_ret;
            }
        }

        internal class CKeyPage
        {

        }
    }
}
