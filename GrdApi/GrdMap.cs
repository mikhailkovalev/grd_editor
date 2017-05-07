using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrdApi
{
    public class GrdMap
    {
        public GrdMap(Int16 column_count,
                      Int16 row_count,
                      String text_data = "INFO",
                      Single default_value = 0.0f)
        {
            TextData = text_data;

            _col_count = column_count;
            _row_count = row_count;
            _default_value = default_value;

            _cell_data = new Single[_row_count, _col_count];
            Int16 i = 0, j = 0;
            while (i < _row_count)
            {
                _cell_data[i, j] = default_value;

                if (++j == _col_count) { ++i; j = 0; }
            }

            _z_max = _z_min = Convert.ToDouble(default_value);

            _x_min = 0.0;
            _x_max = Convert.ToDouble(column_count);

            _y_min = 0.0;
            _y_max = Convert.ToDouble(row_count);
        }

        public GrdMap(Int16 column_count,
                      Int16 row_count,
                      String text_data,
                      Single default_value,
                      Double x_min,
                      Double x_max,
                      Double y_min,
                      Double y_max)
        {
            TextData = text_data;

            _col_count = column_count;
            _row_count = row_count;
            _default_value = default_value;

            _cell_data = new Single[_row_count, _col_count];
            Int16 i = 0, j = 0;
            while (i < _row_count)
            {
                _cell_data[i, j] = default_value;

                if (++j == _col_count) { ++i; j = 0; }
            }

            _z_max = _z_min = Convert.ToDouble(default_value);

            _x_min = x_min;
            _x_max = x_max;

            _y_min = y_min;
            _y_max = y_max;
        }

        public GrdMap(String file_name, Single default_value = 0.0f)
        {
            _default_value = default_value;

            FileStream in_stream = new FileStream(file_name, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(in_stream, Encoding.ASCII);

            Int32 i, j;
            StringBuilder sb = new StringBuilder(__text_data_size__);

            for (i = 0; i < __text_data_size__; ++i)
            {
                sb.Append(reader.ReadChar());
            }

            _text_data = sb.ToString();

            _col_count = reader.ReadInt16();
            _row_count = reader.ReadInt16();

            _x_min = reader.ReadDouble();
            _x_max = reader.ReadDouble();
            _y_min = reader.ReadDouble();
            _y_max = reader.ReadDouble();
            _z_min = reader.ReadDouble();
            _z_max = reader.ReadDouble();

            _cell_data = new Single[_row_count, _col_count];

            i = j = 0;
            while (i < _row_count)
            {
                _cell_data[i, j] = reader.ReadSingle();
                if (++j == _col_count) { ++i; j = 0; }
            }

            in_stream.Close();
        }

        public void Save(String file_name)
        {
            FileStream out_stream = new FileStream(file_name, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(out_stream, Encoding.ASCII);

            Int32 i = 0, j = 0;

            foreach (Char c in _text_data)
            {
                writer.Write(c);
            }

            writer.Write(_col_count);
            writer.Write(_row_count);
            writer.Write(_x_min);
            writer.Write(_x_max);
            writer.Write(_y_min);
            writer.Write(_y_max);
            writer.Write(_z_min);
            writer.Write(_z_max);

            i = 0;

            while (i < _row_count)
            {
                writer.Write(_cell_data[i, j]);
                if (++j == _col_count) { ++i; j = 0; }
            }

            out_stream.Close();
        }

        public String TextData
        {
            get
            {
                return _text_data;
            }
            set
            {
                Int32 len = value.Length;
                if (len == __text_data_size__)
                {
                    _text_data = value;
                }
                else if (len > __text_data_size__)
                {
                    _text_data = value.Substring(0, __text_data_size__);
                }
                else
                {
                    StringBuilder sb = new StringBuilder(__text_data_size__);
                    Int32 space_count = __text_data_size__ - len;
                    sb.Append('_', space_count);
                    sb.Append(value);
                    _text_data = sb.ToString();
                }
            }
        }

        public Int32 ColumnCount
        {
            get
            {
                return Convert.ToInt32(_col_count);
            }
            set
            {
                if (value == _col_count) return;
                ResizeMap(_row_count, value);
            }
        }

        public Int32 RowCount
        {
            get
            {
                return Convert.ToInt32(_row_count);
            }
            set
            {
                if (value == _row_count) return;
                ResizeMap(value, _col_count);
            }
        }

        public Single DefaultValue
        {
            get
            {
                return _default_value;
            }
            set
            {
                _default_value = value;
            }
        }

        public Double XMin
        {
            get
            {
                return _x_min;
            }
        }

        public Double XMax
        {
            get
            {
                return _x_max;
            }
        }

        public Double YMin
        {
            get
            {
                return _y_min;
            }
        }

        public Double YMax
        {
            get
            {
                return _y_max;
            }
        }

        public Double ZMin
        {
            get
            {
                return _z_min;
            }
        }

        public Double ZMax
        {
            get
            {
                return _z_max;
            }
        }

        /*public Single this[Int32 row, Int32 column]
        {
            get
            {
                return _cell_data[row, column];
            }
            set
            {
                if (default_value > _z_max) _z_max = Convert.ToDouble(default_value);
                else if (default_value < _z_min) _z_min = Convert.ToDouble(default_value);
                _cell_data[row, column] = default_value;
            }
        }*/

        public Double this[Int32 row, Int32 column]
        {
            get
            {
                return Convert.ToDouble(_cell_data[row, column]);
            }
            set
            {
                if (value > _z_max) _z_max = value;
                else if (value < _z_min) _z_min = value;
                _cell_data[row, column] = Convert.ToSingle(value);
            }
        }

        private void ResizeMap(Int32 row_count, Int32 col_count)
        {
            var new_cell_data = new Single[row_count, col_count];
            Int32 i = 0, j = 0;

            while (i < row_count)
            {
                if (j >= _col_count || i >= _row_count)
                {
                    new_cell_data[i, j] = _default_value;
                }
                else
                {
                    new_cell_data[i, j] = _cell_data[i, j];
                }
                if (++j == col_count)
                {
                    j = 0;
                    ++i;
                }
            }
            _cell_data = new_cell_data;
        }

        private String _text_data;

        private Int16 _col_count;
        private Int16 _row_count;

        private Double _x_min;
        private Double _x_max;
        private Double _y_min;
        private Double _y_max;
        private Double _z_min;
        private Double _z_max;

        private Single[,] _cell_data;

        private Single _default_value;

        private const Int32 __text_data_size__ = 4;
    }
}
