using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FooProject
{
    public class PeaceTableItem : IComparable<PeaceTableItem>,IEnumerable<char>
    {
        public int start, length;
        public IStringBuffer text;
        public int actual_start;

        public PeaceTableItem(int start,IStringBuffer s,int actual_start, int length)
        {
            this.start = start;
            this.text = s;
            this.length = length;
            this.actual_start = actual_start;
        }

        public PeaceTableItem(int start, int length)
        {
            this.start = start;
            this.length = length;
        }

        public int CompareTo(PeaceTableItem other)
        {
            if (this.start > other.start)
                return 1;
            if (other.start >= this.start && other.start + other.length < this.start + this.length)
                return 0;
            return -1;
        }
        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < this.length; i++)
                yield return text[this.actual_start + i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public interface IStringBuffer
    {
        public char this[int i]
        {
            get;
        }

        public int Length
        {
            get;
        }

        public int LastAddedIndex
        {
            get;
        }

        public void Add(string s);

        public void Remove(int index, int length);

        public void Clear();
    }


    public class StringBuffer : IStringBuffer
    {
        int last_add_index;
        StringBuilder text = new StringBuilder();

        public char this[int i] => text[i];

        public int Length
        {
            get
            {
                return text.Length;
            }
        }

        public int LastAddedIndex => last_add_index;

        public void Add(string s)
        {
            text.Append(s);
            this.last_add_index += s.Length;
        }

        public void Clear()
        {
            text.Clear();
            last_add_index = 0;
        }

        public void Remove(int index, int length)
        {
            text.Remove(index, length);
            this.last_add_index -= length;
        }
    }

    public enum PeaceTableType
    {
        Origin = 0,
        Add,
    }

    public class PeaceTable : IEnumerable<char>
    {
        List<PeaceTableItem> list = new List<PeaceTableItem>();
        List<IStringBuffer> table_list = new List<IStringBuffer>() { new StringBuffer(), new StringBuffer() };
        PeaceTableItem _last_obtain = null;

        public char this[int index]
        {
            get
            {
                PeaceTableItem node = null;
                
                if(_last_obtain != null)
                {
                    node = _last_obtain;
                    if (index < node.start || index >= node.start + node.length)
                    {
                        int index_node = this.list.BinarySearch(new PeaceTableItem(index, 0));
                        _last_obtain = this.list[index_node];
                        node = this.list[index_node];
                    }
                }
                else
                {
                    int index_node = this.list.BinarySearch(new PeaceTableItem(index, 0));
                    _last_obtain = this.list[index_node];
                    node = this.list[index_node];
                }

                int index_in_node = index - node.start;
                return node.text[node.actual_start + index_in_node];
            }
        }

        public string ToString(int start,int length)
        {
            StringBuilder temp = new StringBuilder();
            for(int i = 0; i < length; i++)
            {
                temp.Append(this[i + start]);
            }
            return temp.ToString();
        }

        public void Add(string s, PeaceTableType type = PeaceTableType.Add)
        {
            var target_table = this.table_list[(int)type];
            if (this.list.Count == 0)
            {
                this.list.Add(new PeaceTableItem(0, target_table, target_table.LastAddedIndex, s.Length));
            }
            else
            {
                var last_node = this.list.Last();
                if(last_node.actual_start + last_node.length == target_table.LastAddedIndex)
                {
                    last_node.length += s.Length;
                }
                else
                {
                    this.list.Add(new PeaceTableItem(last_node.start + last_node.length, target_table, target_table.LastAddedIndex, s.Length));
                }
            }

            target_table.Add(s);
        }

        public void Insert(int index,string s, PeaceTableType type = PeaceTableType.Add)
        {
            int index_node = this.list.BinarySearch(new PeaceTableItem(index, 0));
            
            PeaceTableItem left_node, right_node;
            (left_node, right_node) = this.Spilit(index,this.list[index_node]);
            var target_table = this.table_list[(int)type];

            int update_begin_index;
            //分割すべきノードがない
            if (left_node.length == 0)
            {
                this.list.Insert(index_node, new PeaceTableItem(index, target_table, target_table.LastAddedIndex, s.Length));
                update_begin_index = index_node + 1;
            }else if(right_node.length == 0)
            {
                if(left_node.actual_start + left_node.length == target_table.LastAddedIndex)
                {
                    left_node.length += s.Length;
                    this.list[index_node] = left_node;
                    update_begin_index = index_node + 1;
                }
                else
                {
                    this.list.Insert(index_node + 1, new PeaceTableItem(index, target_table, target_table.LastAddedIndex, s.Length));
                    update_begin_index = index_node + 2;
                }
            }
            else
            {
                this.list[index_node] = left_node;
                this.list.InsertRange(index_node + 1, new PeaceTableItem[] {
                    new PeaceTableItem(index, target_table,target_table.LastAddedIndex, s.Length),
                    right_node
                });
                update_begin_index = index_node + 2;
            }

            for (int i = update_begin_index; i < this.list.Count; i++)
                this.list[i].start += s.Length;

            target_table.Add(s);
        }

        public void Remove(int index,int length)
        {
            int start_index_node = this.list.BinarySearch(new PeaceTableItem(index, 0));
            int end_index_node = this.list.BinarySearch(new PeaceTableItem(index + length - 1, 0));

            PeaceTableItem left_node, right_node;

            if(start_index_node == end_index_node)
            {
                var node = this.list[start_index_node];
                node.text.Remove(node.actual_start + index - node.start, length);
                node.length = node.length - length;
                this.list[start_index_node] = node;
            }
            else
            {
                int remove_start_index = start_index_node + 1;
                (left_node, right_node) = this.Spilit(index, this.list[start_index_node]);
                if(left_node.length == 0)
                    remove_start_index = start_index_node;
                else
                    this.list[start_index_node] = left_node;

                (left_node, right_node) = this.Spilit(index + length - 1, this.list[end_index_node]);
                if(right_node.length == 0)
                    this.list[end_index_node] = left_node;
                else
                    this.list[end_index_node] = right_node;

                this.list.RemoveRange(remove_start_index, end_index_node - remove_start_index + 1);
            }

            for (int i = start_index_node + 1; i < this.list.Count; i++)
                this.list[i].start -= length;
        }

        public void Clear()
        {
            this.list.Clear();
            foreach (var table in this.table_list)
                table.Clear();
        }

        public IEnumerator<char> GetEnumerator()
        {
            foreach(var item in this.list)
            {
                foreach (var c in item)
                    yield return c;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        (PeaceTableItem,PeaceTableItem) Spilit(int index, PeaceTableItem node)
        {
            int left_node_length = index - node.start;
            var left_node = new PeaceTableItem(node.start, node.text, node.actual_start, left_node_length);
            var right_node = new PeaceTableItem(index, node.text, node.actual_start + left_node_length, node.length - left_node_length);
            return (left_node, right_node);
        }
    }
}
