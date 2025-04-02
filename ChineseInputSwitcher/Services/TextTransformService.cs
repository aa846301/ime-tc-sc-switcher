using System;
using System.Linq;
using System.Text;

namespace ChineseInputSwitcher.Services
{
    public class TextTransformService
    {
        public string TransformToSqlFormat(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // 分割输入文本，处理可能的多行输入
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            // 处理第一行作为列名
            var columns = lines[0].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            // 格式化为 'column1','column2','column3' 格式
            return string.Join("','", columns.Select(c => c.Trim()));
        }
        
        public string ClipboardTextToSqlFormat(string clipboardText)
        {
            if (string.IsNullOrWhiteSpace(clipboardText))
                return string.Empty;
                
            var lines = clipboardText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return string.Empty;
                
            // 检测是否已经是 SQL 格式
            if (lines[0].Contains("','"))
                return clipboardText;
                
            var columns = lines[0].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return "'" + string.Join("','", columns.Select(c => c.Trim())) + "'";
        }

        public string ConvertToSqlFormat(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            // 将输入文本转换为SQL安全格式
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();
            
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var escapedLine = line.Replace("'", "''");
                    result.AppendLine($"'{escapedLine}' +");
                }
            }
            
            // 移除最后一个 '+'
            if (result.Length > 0)
            {
                result.Length -= 3; // 移除最后的 " +"
            }
            
            return result.ToString();
        }
    }
} 