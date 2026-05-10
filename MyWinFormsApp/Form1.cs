using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DuckDB.NET.Data;
using System.Threading;
using System.Drawing.Drawing2D;

namespace MyWinFormsApp
{
    public partial class Form1 : Form
    {
        // کنترل‌های سفارشی برای لیست فایل‌ها
        private FlowLayoutPanel fileListPanel;
        private ProgressBar mainProgressBar;
        private Label lblProgressPercent;
        
        // کلاس برای نگهداری اطلاعات هر آیتم فایل
        private class FileItemControl : Panel
        {
            public PictureBox IconBox { get; private set; }
            public Label FileNameLabel { get; private set; }
            public Label StatusLabel { get; private set; }
            public int ColumnIndex { get; set; }
            public string FilePath { get; set; } = "";
            
            public FileItemControl()
            {
                this.Height = 50;
                this.Dock = DockStyle.Top;
                this.BorderStyle = BorderStyle.FixedSingle;
                this.Margin = new Padding(3);
                
                IconBox = new PictureBox
                {
                    Size = new Size(32, 32),
                    Location = new Point(10, 9),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };
                
                FileNameLabel = new Label
                {
                    Location = new Point(50, 8),
                    AutoSize = false,
                    Size = new Size(400, 20),
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(60, 60, 60)
                };
                
                StatusLabel = new Label
                {
                    Location = new Point(50, 26),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                    ForeColor = Color.Gray
                };
                
                this.Controls.Add(IconBox);
                this.Controls.Add(FileNameLabel);
                this.Controls.Add(StatusLabel);
            }
            
            public void SetWaiting()
            {
                IconBox.Image = CreateWaitIcon();
                StatusLabel.Text = "در انتظار...";
                StatusLabel.ForeColor = Color.Gray;
            }
            
            public void SetLoading()
            {
                IconBox.Image = CreateLoadingIcon();
                StatusLabel.Text = "در حال پردازش...";
                StatusLabel.ForeColor = Color.FromArgb(25, 118, 210);
            }
            
            public void SetSuccess(int count)
            {
                IconBox.Image = CreateSuccessIcon();
                StatusLabel.Text = $"انجام شد ({count:n0} رکورد)";
                StatusLabel.ForeColor = Color.FromArgb(46, 125, 50);
            }
            
            public void SetError(string error)
            {
                IconBox.Image = CreateErrorIcon();
                StatusLabel.Text = $"خطا: {error}";
                StatusLabel.ForeColor = Color.FromArgb(198, 40, 40);
            }
            
            private Bitmap CreateWaitIcon()
            {
                var bmp = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(Color.Gray))
                        g.FillEllipse(brush, 8, 8, 16, 16);
                }
                return bmp;
            }
            
            private Bitmap CreateLoadingIcon()
            {
                var bmp = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.FromArgb(25, 118, 210), 3))
                    {
                        g.DrawArc(pen, 4, 4, 24, 24, 0, 270);
                        g.FillEllipse(new SolidBrush(Color.FromArgb(25, 118, 210)), 20, 4, 8, 8);
                    }
                }
                return bmp;
            }
            
            private Bitmap CreateSuccessIcon()
            {
                var bmp = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(Color.FromArgb(46, 125, 50)))
                    {
                        g.FillEllipse(brush, 0, 0, 32, 32);
                        using (var pen = new Pen(Color.White, 3))
                        {
                            g.DrawLine(pen, 8, 16, 14, 22);
                            g.DrawLine(pen, 14, 22, 24, 10);
                        }
                    }
                }
                return bmp;
            }
            
            private Bitmap CreateErrorIcon()
            {
                var bmp = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(Color.FromArgb(198, 40, 40)))
                    {
                        g.FillEllipse(brush, 0, 0, 32, 32);
                        using (var pen = new Pen(Color.White, 3))
                        {
                            g.DrawLine(pen, 10, 10, 22, 22);
                            g.DrawLine(pen, 22, 10, 10, 22);
                        }
                    }
                }
                return bmp;
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeCustomUI();
        }
        
        private void InitializeCustomUI()
        {
            // تنظیمات فرم اصلی
            this.Text = "پردازشگر حرفه‌ای DuckDB - استخراج ستون‌های CSV";
            this.Font = new Font("Segoe UI", 10F);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            
            // حذف کنترل‌های قدیمی
            this.Controls.Clear();
            
            // پنل بالایی برای ورودی‌ها
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            
            var lblTitle = new Label
            {
                Text = "استخراج هوشمند ستون‌های CSV",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Location = new Point(20, 15),
                AutoSize = true
            };
            
            var lblConnection = new Label
            {
                Text = "Connection String:",
                Location = new Point(20, 50),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            
            txtConnectionString = new TextBox
            {
                Location = new Point(150, 47),
                Size = new Size(550, 25),
                Font = new Font("Segoe UI", 9F),
                Text = "Data Source=:memory:",
                BorderStyle = BorderStyle.FixedSingle
            };
            
            btnProcess = new Button
            {
                Text = "شروع پردازش",
                Location = new Point(20, 80),
                Size = new Size(140, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnProcess.FlatAppearance.BorderSize = 0;
            btnProcess.Click += BtnProcess_Click;
            
            lblStatus = new Label
            {
                Text = "آماده به کار",
                Location = new Point(180, 88),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9F)
            };
            
            topPanel.Controls.AddRange(new Control[] { lblTitle, lblConnection, txtConnectionString, btnProcess, lblStatus });
            
            // پنل میانی برای لیست فایل‌ها
            var listContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(245, 245, 245)
            };
            
            var lblFiles = new Label
            {
                Text = "فایل‌های خروجی ستون‌ها:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                BackColor = Color.FromArgb(245, 245, 245)
            };
            
            fileListPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            
            listContainer.Controls.Add(fileListPanel);
            listContainer.Controls.Add(lblFiles);
            
            // پنل پایینی برای پیشرفت و لاگ
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 180,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            
            mainProgressBar = new ProgressBar
            {
                Location = new Point(20, 15),
                Size = new Size(700, 25),
                Style = ProgressBarStyle.Continuous
            };
            
            lblProgressPercent = new Label
            {
                Text = "0%",
                Location = new Point(730, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 118, 210)
            };
            
            txtLog = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(760, 120),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8F),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(200, 200, 200),
                BorderStyle = BorderStyle.None
            };
            
            bottomPanel.Controls.AddRange(new Control[] { mainProgressBar, lblProgressPercent, txtLog });
            
            this.Controls.Add(bottomPanel);
            this.Controls.Add(listContainer);
            this.Controls.Add(topPanel);
        }

        private async void BtnProcess_Click(object? sender, EventArgs e)
        {
            string connectionString = txtConnectionString.Text.Trim();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("لطفاً Connection String را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnProcess.Enabled = false;
            lblStatus.Text = "در حال آماده‌سازی...";
            txtLog.Clear();
            fileListPanel.Controls.Clear();
            mainProgressBar.Value = 0;
            lblProgressPercent.Text = "0%";
            
            try
            {
                await ProcessRequestsAsync(connectionString);
                lblStatus.Text = "پردازش با موفقیت انجام شد.";
                lblStatus.ForeColor = Color.FromArgb(46, 125, 50);
                MessageBox.Show("پردازش با موفقیت انجام شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "خطا در پردازش";
                lblStatus.ForeColor = Color.FromArgb(198, 40, 40);
                AppendLog($"خطا: {ex.Message}", Color.FromArgb(198, 40, 40));
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
                            
                            AppendLog($"پردازش درخواست ID: {requestId}", Color.FromArgb(25, 118, 210));
                            
                            // استخراج مسیر فایل CSV از ResultData
                            string csvFilePath = resultData.Trim();
                            
                            if (!File.Exists(csvFilePath))
                            {
                                AppendLog($"فایل CSV یافت نشد: {csvFilePath}", Color.FromArgb(198, 40, 40));
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
                                    AppendLog("فایل CSV خالی است.", Color.FromArgb(198, 40, 40));
                                    continue;
                                }
                            }

                            AppendLog($"تعداد ستون‌ها: {headers.Length}", Color.FromArgb(46, 125, 50));

                            // نام پایه فایل خروجی
                            string directory = Path.GetDirectoryName(csvFilePath) ?? "";
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(csvFilePath);
                            string extension = Path.GetExtension(csvFilePath);
                            
                            // ایجاد آیتم‌های لیست برای هر ستون
                            var fileItems = new List<FileItemControl>();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                var item = new FileItemControl
                                {
                                    Width = fileListPanel.ClientSize.Width - 25,
                                    ColumnIndex = i,
                                    FilePath = Path.Combine(directory, $"{fileNameWithoutExt}_{i}{extension}")
                                };
                                
                                item.FileNameLabel.Text = $"ستون {i}: {headers[i].Trim()}";
                                item.SetWaiting();
                                
                                fileListPanel.Controls.Add(item);
                                fileItems.Add(item);
                            }
                            
                            // پردازش موازی ستون‌ها
                            long[] columnDistinctCounts = new long[headers.Length];
                            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                            int completedCount = 0;
                            int totalCount = headers.Length;

                            await Parallel.ForEachAsync(Enumerable.Range(0, headers.Length), parallelOptions, async (i, token) =>
                            {
                                string colName = headers[i].Trim();
                                string colFilePath = Path.Combine(directory, $"{fileNameWithoutExt}_{i}{extension}");
                                string safeColName = $"\"{colName.Replace("\"", "\"\"")}\"";
                                
                                var currentItem = fileItems.FirstOrDefault(x => x.ColumnIndex == i);
                                if (currentItem != null)
                                {
                                    currentItem.Invoke(new Action(() => currentItem.SetLoading()));
                                }

                                try
                                {
                                    // بررسی وجود فایل
                                    if (File.Exists(colFilePath))
                                    {
                                        AppendLog($"فایل ستون {i} از قبل وجود دارد: {colFilePath}", Color.Gray);
                                        // شمارش رکوردهای فایل موجود
                                        var existingLines = File.ReadAllLines(colFilePath);
                                        columnDistinctCounts[i] = Math.Max(0, existingLines.Length - 1); // منهای هدر
                                        
                                        if (currentItem != null)
                                        {
                                            currentItem.Invoke(new Action(() => currentItem.SetSuccess((int)columnDistinctCounts[i])));
                                        }
                                        
                                        Interlocked.Increment(ref completedCount);
                                        UpdateProgress(completedCount, totalCount);
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
                                            
                                            AppendLog($"ستون {i} ({colName}): {distinctCount:n0} رکورد یکتا -> {colFilePath}", Color.FromArgb(46, 125, 50));
                                            
                                            if (currentItem != null)
                                            {
                                                currentItem.Invoke(new Action(() => currentItem.SetSuccess((int)distinctCount)));
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AppendLog($"خطا در ستون {i}: {ex.Message}", Color.FromArgb(198, 40, 40));
                                    if (currentItem != null)
                                    {
                                        currentItem.Invoke(new Action(() => currentItem.SetError(ex.Message)));
                                    }
                                }
                                finally
                                {
                                    Interlocked.Increment(ref completedCount);
                                    UpdateProgress(completedCount, totalCount);
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
                                    // رفع مشکل AddWithValue با استفاده از DuckDBParameter
                                    var countsParam = new DuckDB.NET.Data.DuckDBParameter("@counts", countsString);
                                    var resultIdParam = new DuckDB.NET.Data.DuckDBParameter("@resultId", resultId.Value);
                                    
                                    updateCmd.Parameters.Add(countsParam);
                                    updateCmd.Parameters.Add(resultIdParam);
                                    
                                    int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                                    AppendLog($"نتیجه آپدیت شد: {rowsAffected} ردیف. مقادیر: {countsString}", Color.FromArgb(46, 125, 50));
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void UpdateProgress(int completed, int total)
        {
            if (total == 0) return;
            
            int percent = (int)((completed * 100.0) / total);
            
            if (mainProgressBar.InvokeRequired)
            {
                mainProgressBar.Invoke(new Action(() => {
                    mainProgressBar.Value = Math.Min(percent, 100);
                    lblProgressPercent.Text = $"{percent}%";
                }));
            }
            else
            {
                mainProgressBar.Value = Math.Min(percent, 100);
                lblProgressPercent.Text = $"{percent}%";
            }
        }

        private void AppendLog(string message, Color color)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string, Color>(AppendLog), message, color);
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - ");
                
                int start = txtLog.TextLength;
                txtLog.AppendText(message + "\r\n");
                int end = txtLog.TextLength;
                
                txtLog.Select(start, end - start);
                txtLog.SelectionColor = color;
                txtLog.SelectionFont = new Font("Consolas", 8F);
                txtLog.DeselectAll();
                
                txtLog.ScrollToCaret();
            }
        }
        
        private void AppendLog(string message)
        {
            AppendLog(message, Color.FromArgb(200, 200, 200));
        }
    }
}
