using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FooProject
{
    public class PeaceTableItem : EqualityComparer<PeaceTableItem>,IComparable<PeaceTableItem>,IEnumerable<char>
    {
        public int start, length;
        public IStringBuffer buffer;
        public int actual_start;

        public string Text
        {
            get
            {
                return this.buffer.ToString(actual_start, length);
            }
        }

        public PeaceTableItem(int start,IStringBuffer s,int actual_start, int length)
        {
            this.start = start;
            this.buffer = s;
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
            if (other.start >= this.start && other.start < this.start + this.length)
                return 0;
            if (other.start < this.start)
                return -1;
            return 1;
        }
        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < this.length; i++)
                yield return buffer[this.actual_start + i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            PeaceTableItem y = (PeaceTableItem)obj;
            return y.start >= this.start && y.start < this.start + this.length;
        }

        public override bool Equals([AllowNull] PeaceTableItem x, [AllowNull] PeaceTableItem y)
        {
            return y.start >= x.start && y.start < x.start + x.length;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetHashCode([DisallowNull] PeaceTableItem obj)
        {
            return obj.GetHashCode();
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

        public string ToString(int index, int length);
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

        public string ToString(int index, int length)
        {
            return this.text.ToString(index, length);
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
                        int index_node = this.Find(index);
                        _last_obtain = this.list[index_node];
                        node = this.list[index_node];
                    }
                }
                else
                {
                    int index_node = this.Find(index);
                    _last_obtain = this.list[index_node];
                    node = this.list[index_node];
                }

                int index_in_node = index - node.start;
                return node.buffer[node.actual_start + index_in_node];
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
            int index_node = this.Find(index);
            
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
            int start_index_node = this.Find(index);
            int end_index_node = this.Find(index + length - 1);
            int update_start_index = start_index_node + 1;

            PeaceTableItem left_node = null, right_node = null;

            if (start_index_node == end_index_node)
            {
                int remove_index = start_index_node;
                (left_node, right_node) = this.Spilit(index, this.list[remove_index]);
                if (left_node.length != 0)
                {
                    this.list.Insert(start_index_node, left_node);
                    remove_index++;
                }

                (left_node, right_node) = this.Spilit(index + length, this.list[remove_index]);
                if (right_node.length != 0)
                {
                    this.list[remove_index] = right_node;
                    this.list[remove_index].start = index;
                    remove_index = -1;
                }

                if (remove_index == start_index_node)
                {
                    this.list.RemoveAt(remove_index);
                    update_start_index = start_index_node;
                }
            }
            else
            {
                int remove_start_index = start_index_node + 1;

                (left_node, right_node) = this.Spilit(index, this.list[start_index_node]);
                if (left_node.length == 0)
                    update_start_index = remove_start_index = start_index_node;
                else
                    this.list[start_index_node] = left_node;

                (left_node, right_node) = this.Spilit(index + length, this.list[end_index_node]);
                if (right_node.length == 0)
                    this.list[end_index_node] = left_node;
                else
                    this.list.Insert(end_index_node + 1, right_node);

                this.list.RemoveRange(remove_start_index, end_index_node - remove_start_index + 1);
            }


            for (int i = update_start_index; i < this.list.Count; i++)
            {
                this.list[i].start -= length;
            }
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

        int Find(int index)
        {
            var target_node = new PeaceTableItem(index, 0);
            int index_node = this.list.BinarySearch(target_node);
            if(index_node < 0)
                return this.list.IndexOf(target_node);
            return index_node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        (PeaceTableItem,PeaceTableItem) Spilit(int index, PeaceTableItem node)
        {
            int left_node_length = index - node.start;
            var left_node = new PeaceTableItem(node.start, node.buffer, node.actual_start, left_node_length);
            var right_node = new PeaceTableItem(index, node.buffer, node.actual_start + left_node_length, node.length - left_node_length);
            return (left_node, right_node);
        }
    }
}
