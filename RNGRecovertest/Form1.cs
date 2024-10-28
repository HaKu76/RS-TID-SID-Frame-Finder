using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RNGRecovertest
{
    public partial class Form1 : Form
    {
        // 构造函数
        public Form1()
        {
            InitializeComponent();
        }

        // 恢复事件处理程序
        private void Recover(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear(); // 清空数据表格的行
            uint add;
            uint k;
            uint mult;
            byte[] low = new byte[0x10000]; // 用于存储低字节值
            bool[] flags = new bool[0x10000]; // 用于标记有效性

            k = 0xC64E6D00; // Mult << 8
            mult = 0x41c64e6d; // pokerng 常量
            add = 0x6073; // pokerng 常量
            uint count = 0;

            // 初始化 low 数组
            foreach (byte element in low)
            {
                low[count] = 0; // 将每个元素初始化为 0
                count++;
            }
            count = 0;
            // 初始化 flags 数组
            foreach (bool element in flags)
            {
                flags[count] = true; // 将每个标记初始化为 true
                count++;
            }

            // 计算可能的 SID
            for (short i = 0; i < 256; i++)
            {
                uint right = (uint)(mult * i + add);
                ushort val = (ushort)(right >> 16);
                flags[val] = true; // 标记有效性
                low[val--] = (byte)i; // 存储低字节
                flags[val] = true; // 标记有效性
                low[val] = (byte)i; // 存储低字节
            }
            uint tid, pid, PIDhigh, PIDlow, sid;
            // 处理 TID 输入
            try
            {
                tid = uint.Parse(textBox1.Text);
            }
            catch
            {
                MessageBox.Show("Error: TID entered incorrectly."); // 错误提示
                return;
            }


            if (radioButton1.Checked)
            {
                // 处理 PID 输入
                try
                {
                    pid = uint.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);
                    PIDhigh = pid >> 16;
                    PIDlow = pid & 0xFFFF;
                }
                catch
                {
                    MessageBox.Show("Error: PID entered incorrectly."); // 错误提示
                    return;
                }
                //显示异色列
                dataGridView1.Columns["Column5"].Visible = true;
                uint xor = PIDlow ^ PIDhigh; // 计算 XOR
                uint psv = xor / 8; // 计算 PSV
                List<uint> shinySids = new List<uint>(); // 存储闪光 SID
                List<uint> possibileSids = new List<uint>(); // 存储可能的 SID

                // 生成闪光 SID
                for (uint testsid = (xor ^ tid) & 0xFFF8; testsid < ((xor ^ tid) & 0xFFF8) + 8; testsid++)
                {
                    shinySids.Add(testsid);
                }
                List<uint> origin = new List<uint>(); // 存储起始种子
                foreach (uint sids in shinySids)
                {
                    string TSIDlow = sids.ToString("X4"); // 将 SID 转换为字符串
                    string TSIDhigh = tid.ToString("X4");
                    string TSID = TSIDhigh + TSIDlow; // 拼接 SID

                    uint tspid = uint.Parse(TSID, System.Globalization.NumberStyles.HexNumber); // 转换为整数
                    uint first = tspid << 16;
                    uint second = tspid & 0xFFFF0000;
                    uint search = second - first * mult;

                    // 遍历可能的值
                    for (short i = 0; i < 256; i++, search -= k)
                    {
                        if (flags[search >> 16]) // 检查有效性
                        {
                            uint test = first | (uint)(i << 8) | low[search >> 16];
                            if (((test * mult + add) & 0xffff0000) == second)
                            {
                                PokeRNGR rng = new PokeRNGR(test); // 创建随机数生成器
                                uint seed = rng.nextUInt(); // 获取下一个种子
                                origin.Add(seed); // 存储种子
                                possibileSids.Add(sids); // 存储可能的 SID
                            }
                        }
                    }
                }
                // 计算框架和输出结果
                uint frame;
                int index = 0;
                foreach (uint s in origin)
                {
                    frame = 0;
                    PokeRNGR rng = new PokeRNGR(s);

                    // 计算帧数
                    while (rng.seed > 0xFFFF)
                    {
                        rng.nextUInt();
                        frame++;
                    }

                    uint tempxor = possibileSids[index] ^ tid;
                    uint temptsv = tempxor / 8;
                    string shiny = "Star"; // 默认闪光状态为 Star
                    if (tempxor == xor)
                    {
                        shiny = "Square"; // 如果满足条件，则为 Square
                    }

                    // 计算时间
                    var start = new DateTime(2000, 1, 1, 1, 0, 0);
                    start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                    var lastdate = new DateTime(start.Year, 12, 31);
                    lastdate = DateTime.SpecifyKind(lastdate, DateTimeKind.Utc);
                    int minDay = 0;
                    int maxDay = 0;
                    List<DateTime> times = new List<DateTime>(); // 存储时间

                    // 计算每年的天数
                    for (short x = 2001; x < 2000; x++)
                    {
                        var temp = new DateTime(x, 1, 1);
                        temp = DateTime.SpecifyKind(temp, DateTimeKind.Utc);
                        minDay += (int)(lastdate - start).TotalDays;
                        maxDay += (int)(lastdate - start).TotalDays;
                    }

                    // 计算每个月的时间
                    for (byte month = 1; month < 13; month++)
                    {
                        var temp = new DateTime(2000, month, 1);
                        maxDay += DateTime.DaysInMonth(2000, month);

                        for (int day = minDay; day < maxDay; day++)
                        {
                            for (int hour = 0; hour < 24; hour++)
                            {
                                for (int minute = 0; minute < 60; minute++)
                                {
                                    int v = 1440 * day + 960 * (hour / 10) + 60 * (hour % 10) + 16 * (minute / 10) + (minute % 10) + 0x5A0;
                                    v = (v >> 16) ^ (v & 0xFFFF);
                                    if (v == rng.seed) // 检查种子
                                    {
                                        var finalTime = start.AddDays(day).AddSeconds(((hour - 1) * 60 * 60) + (minute * 60));
                                        finalTime = DateTime.SpecifyKind(finalTime, DateTimeKind.Utc);
                                        times.Add(finalTime); // 存储计算出的时间
                                    }
                                }
                            }
                        }
                        minDay += DateTime.DaysInMonth(2000, month);
                    }

                    times.Sort((a, b) => a.Month.CompareTo(b.Month)); // 按月份排序
                    var dt = DateTime.SpecifyKind(times[0], DateTimeKind.Utc);
                    string timestring = dt.ToString("ddd MMM d HH:mm:ss yyyy"); // 格式化时间

                    // 将结果添加到数据表格中
                    dataGridView1.Rows.Add(rng.seed.ToString("X4"), frame.ToString(), tid, xor,possibileSids[index++], shiny, timestring);
                }
            }
            else if (radioButton2.Checked)
            {
                try
                {
                    sid = uint.Parse(textBox3.Text);
                }
                catch
                {
                    MessageBox.Show("Error: SID entered incorrectly."); // 错误提示
                    return;
                }
                //不显示异色列
                dataGridView1.Columns["Column5"].Visible = false;
                //计算tsv
                uint tsv = sid ^ tid;
                string TSIDlow = sid.ToString("X4"); // 将 SID 转换为字符串
                string TSIDhigh = tid.ToString("X4");
                string TSID = TSIDhigh + TSIDlow; // 拼接 SID

                uint tspid = uint.Parse(TSID, System.Globalization.NumberStyles.HexNumber); // 转换为整数
                uint first = tspid << 16;
                uint second = tspid & 0xFFFF0000;
                uint search = second - first * mult;
                List<uint> origin = new List<uint>(); // 存储起始种子
                // 遍历可能的值
                for (short i = 0; i < 256; i++, search -= k)
                {
                    if (flags[search >> 16]) // 检查有效性
                    {
                        uint test = first | (uint)(i << 8) | low[search >> 16];
                        if (((test * mult + add) & 0xffff0000) == second)
                        {
                            PokeRNGR rng = new PokeRNGR(test); // 创建随机数生成器
                            uint seed = rng.nextUInt(); // 获取下一个种子
                            origin.Add(seed); // 存储种子
                        }
                    }
                }
                // 计算框架和输出结果
                uint frame;
                int index = 0;
                foreach (uint s in origin)
                {
                    frame = 0;
                    PokeRNGR rng = new PokeRNGR(s);

                    // 计算帧数
                    while (rng.seed > 0xFFFF)
                    {
                        rng.nextUInt();
                        frame++;
                    }

                    // 计算时间
                    var start = new DateTime(2000, 1, 1, 1, 0, 0);
                    start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                    var lastdate = new DateTime(start.Year, 12, 31);
                    lastdate = DateTime.SpecifyKind(lastdate, DateTimeKind.Utc);
                    int minDay = 0;
                    int maxDay = 0;
                    List<DateTime> times = new List<DateTime>(); // 存储时间

                    // 计算每年的天数
                    for (short x = 2001; x < 2000; x++)
                    {
                        var temp = new DateTime(x, 1, 1);
                        temp = DateTime.SpecifyKind(temp, DateTimeKind.Utc);
                        minDay += (int)(lastdate - start).TotalDays;
                        maxDay += (int)(lastdate - start).TotalDays;
                    }

                    // 计算每个月的时间
                    for (byte month = 1; month < 13; month++)
                    {
                        var temp = new DateTime(2000, month, 1);
                        maxDay += DateTime.DaysInMonth(2000, month);

                        for (int day = minDay; day < maxDay; day++)
                        {
                            for (int hour = 0; hour < 24; hour++)
                            {
                                for (int minute = 0; minute < 60; minute++)
                                {
                                    int v = 1440 * day + 960 * (hour / 10) + 60 * (hour % 10) + 16 * (minute / 10) + (minute % 10) + 0x5A0;
                                    v = (v >> 16) ^ (v & 0xFFFF);
                                    if (v == rng.seed) // 检查种子
                                    {
                                        var finalTime = start.AddDays(day).AddSeconds(((hour - 1) * 60 * 60) + (minute * 60));
                                        finalTime = DateTime.SpecifyKind(finalTime, DateTimeKind.Utc);
                                        times.Add(finalTime); // 存储计算出的时间
                                    }
                                }
                            }
                        }
                        minDay += DateTime.DaysInMonth(2000, month);
                    }

                    times.Sort((a, b) => a.Month.CompareTo(b.Month)); // 按月份排序
                    var dt = DateTime.SpecifyKind(times[0], DateTimeKind.Utc);
                    string timestring = dt.ToString("ddd MMM d HH:mm:ss yyyy"); // 格式化时间

                    // 将结果添加到数据表格中
                    dataGridView1.Rows.Add(rng.seed.ToString("X4"), frame.ToString(), tid, sid,tsv, null, timestring);

                }
            }
        }

        //选择PID计算
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                textBox2.ReadOnly = false;
            }
            else
            {
                textBox2.ReadOnly = true;
            }
        }
        //选择SID计算
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                textBox3.ReadOnly = false;
            }
            else
            {
                textBox3.ReadOnly = true;
            }
        }
    }
}
