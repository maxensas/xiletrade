using LibDat2;
using LibDat2.Types;
using System.Text;
using System.Text.RegularExpressions;

namespace XiletradeJson
{
    // NOT USED FOR NOW - Output directly to json and bypass csv.
    internal class Dat : DatContainer
    {
        public Dat(string filePath, bool SchemaMin = false) : base(filePath, SchemaMin)
        {
        }

        public Dat(byte[] fileData, string fileName, bool SchemaMin = false) : base(fileData, fileName, SchemaMin)
        {
        }

        public Dat(Stream stream, string fileName, bool SchemaMin = false) : base(stream, fileName, SchemaMin)
        {
        }

        public Dat(string fileName, List<IFieldData[]> fieldDatas, bool SchemaMin = false) : base(fileName, fieldDatas, SchemaMin)
        {
        }

        /// <summary>
		/// Convert <see cref="FieldDatas"/> to csv format
		/// </summary>
		/// <returns>Content of the csv file</returns>
		public virtual string ToCsvNew()
        {
            var f = new StringBuilder();
            var reg = new Regex("\"|\n|\r|,", RegexOptions.Compiled);

            // Field Names
            foreach (var field in FieldDefinitions.Select(t => t.Key))
                if (reg.IsMatch(field))
                    f.Append("\"" + field.Replace("\"", "\"\"") + "\",");
                else
                    f.Append(field + ",");

            if (f.Length == 0)
            {
                for (var i = 0; i < FieldDatas.Count; ++i)
                    f.AppendLine();
                return f.ToString();
            }
            else
                f.Length -= 1; // Remove ,
            f.AppendLine();

            foreach (var row in FieldDatas)
            {
                foreach (var col in row!)
                {
                    var s = col!.ToString();
                    if (reg.IsMatch(s))
                        f.Append("\"" + s.Replace("\"", "\"\"") + "\",");
                    else
                        f.Append(s + ",");
                }
                f.Length -= 1; // Remove ,
                f.AppendLine();
            }
            f.Length -= 1; // Remove ,

            return f.ToString();
        }

        public virtual object? ToJson()
        {
            // TODO
            return null;
        }
    }
}
