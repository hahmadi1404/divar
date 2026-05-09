using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DuckDB.NET.Data;
using System.Threading;

namespace MyWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnProcess_Click(object? sender, EventArgs e)
        {
            string connectionString = txtConnectionString.Text.Trim();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("لطفاً Connection String را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnProcess.Enabled = false;
            lblStatus.Text = "در حال پردازش...";
            txtLog.Clear();

            try
            {
                await ProcessRequestsAsync(connectionString);
                lblStatus.Text = "پردازش با موفقیت انجام شد.";
                MessageBox.Show("پردازش با موفقیت انجام شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "خطا در پردازش";
                txtLog.AppendText($"خطا: {ex.Message}\r\n");
                MessageBox.Show($"خطا: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnProcess.Enabled = true;
            }
        }

        private async Task ProcessRequestsAsync(string connectionString)
        {
            using (var conn = new DuckDBConnection(connectionString))
            {
                await conn.OpenAsync();
                
                // دریافت تمام درخواست‌ها و نتایج مرتبط
                string query = @"
                    SELECT 
                        r.Id, r.RequestData, r.Headers, r.Source,
                        res.ID as ResultId, res.ResultData, res.ColsResultCount
                    FROM Request_t r
                    LEFT JOIN Result_t res ON r.ResultID = res.ID
                    WHERE r.IsDeleted = 0
                ";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            long requestId = reader.GetInt64(0);
                            string requestData = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            string headersStr = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            string source = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            
                            long? resultId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4);
                            string resultData = reader.IsDBNull(5) ? "" : reader.GetString(5);
                            
                            AppendLog($"پردازش درخواست ID: {requestId}");
                            
                            // استخراج مسیر فایل CSV از ResultData
                            string csvFilePath = resultData.Trim();
                            
                            if (!File.Exists(csvFilePath))
                            {
                                AppendLog($"فایل CSV یافت نشد: {csvFilePath}");
                                continue;
                            }

                            // استخراج هدرها
                            string[] headers;
                            if (!string.IsNullOrEmpty(headersStr))
                            {
                                headers = headersStr.Split(',');
                            }
                            else
                            {
                                // خواندن سطر اول به عنوان هدر
                                var lines = File.ReadAllLines(csvFilePath);
                                if (lines.Length > 0)
                                {
                                    headers = lines[0].Split(',');
                                }
                                else
                                {
                                    AppendLog("فایل CSV خالی است.");
                                    continue;
                                }
                            }

                            AppendLog($"تعداد ستون‌ها: {headers.Length}");

                            // نام پایه فایل خروجی
                            string directory = Path.GetDirectoryName(csvFilePath) ?? "";
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(csvFilePath);
                            string extension = Path.GetExtension(csvFilePath);
                            
                            // پردازش موازی ستون‌ها
                            long[] columnDistinctCounts = new long[headers.Length];
                            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

                            await Parallel.ForEachAsync(Enumerable.Range(0, headers.Length), parallelOptions, async (i, token) =>
                            {
                                string colName = headers[i].Trim();
                                string colFilePath = Path.Combine(directory, $"{fileNameWithoutExt}_{i}{extension}");
                                string safeColName = $"\"{colName.Replace("\"", "\"\"")}\"";

                                // بررسی وجود فایل
                                if (File.Exists(colFilePath))
                                {
                                    AppendLog($"فایل ستون {i} از قبل وجود دارد: {colFilePath}");
                                    // شمارش رکوردهای فایل موجود
                                    var existingLines = File.ReadAllLines(colFilePath);
                                    columnDistinctCounts[i] = existingLines.Length - 1; // منهای هدر
                                    return;
                                }

                                // ساخت جدول موقت و لود داده‌ها
                                using (var tempConn = new DuckDBConnection(connectionString))
                                {
                                    await tempConn.OpenAsync(token);
                                    using (var tempCmd = tempConn.CreateCommand())
                                    {
                                        // لود فایل CSV به جدول موقت
                                        string createTableQuery = $@"
                                            CREATE TEMP TABLE IF NOT EXISTS TempCsv_{requestId}_{i} (
                                                {string.Join(", ", headers.Select((h, idx) => $"\"{h.Trim().Replace("\"", "\"\"")}\" VARCHAR"))}
                                            );
                                            COPY TempCsv_{requestId}_{i} FROM '{csvFilePath}' (HEADER, DELIMITER ',');
                                        ";
                                        
                                        tempCmd.CommandText = createTableQuery;
                                        await tempCmd.ExecuteNonQueryAsync(token);

                                        // استخراج داده‌های یکتا برای این ستون
                                        string extractQuery = $@"
                                            COPY (
                                                SELECT DISTINCT {safeColName} 
                                                FROM TempCsv_{requestId}_{i}
                                                WHERE {safeColName} IS NOT NULL AND {safeColName} != ''
                                            ) TO '{colFilePath}' (HEADER, DELIMITER ',')
                                        ";

                                        tempCmd.CommandText = extractQuery;
                                        long distinctCount = await tempCmd.ExecuteNonQueryAsync(token);
                                        columnDistinctCounts[i] = distinctCount;
                                        
                                        AppendLog($"ستون {i} ({colName}): {distinctCount} رکورد یکتا -> {colFilePath}");
                                    }
                                }
                            });

                            // آپدیت جدول Result_t با تعداد رکوردها
                            if (resultId.HasValue)
                            {
                                string countsString = string.Join(",", columnDistinctCounts);
                                
                                using (var updateCmd = conn.CreateCommand())
                                {
                                    updateCmd.CommandText = @"
                                        UPDATE Result_t 
                                        SET ColsResultCount = @counts
                                        WHERE ID = @resultId
                                    ";
                                    updateCmd.Parameters.AddWithValue("@counts", countsString);
                                    updateCmd.Parameters.AddWithValue("@resultId", resultId.Value);
                                    
                                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                                    AppendLog($"نتیجه آپدیت شد: {rowsAffected} ردیف. مقادیر: {countsString}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(AppendLog), message);
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\r\n");
            }
        }
    }
}
