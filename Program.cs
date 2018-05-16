using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;

namespace Student
{
    class Program
    {
        private static SQL connect;                                             // 数据库连接,用来检查学生用户是否存在
        static void Main(string[] args)
        {
            string path = "..\\..\\db\\student.db";                             // 数据库存放的位置
            initConsole();                                                      // 设置控制台的几个参数
            connect = new SQL(path);
            login(path);                                                        // 登录界面
            Console.Clear();                                                    // 清屏
        }
        public static void initConsole()
        {
            Console.Title = "Student System";
            Console.ForegroundColor = ConsoleColor.DarkYellow;                  // 字体颜色
            Console.SetWindowSize(90, 30);                                      // 控制台窗口大小
        }
        public static int login(string path)
        {
            while (true)
            {
                Console.WriteLine("------>登录<------");
                Console.WriteLine();
                Console.WriteLine("Tip:学生登录名为学号");
                Console.WriteLine();

                Console.Write("Username:");
                string username = Console.ReadLine();

                Console.Write("Password:");
                string password = UI.readByChar(true);
                Console.WriteLine();
                
                if (username != password)                                       // 用户名密码不匹配
                {
                    UI.waitReturn("用户名或密码错误！按任意键重新登录...");
                    Console.Clear();
                    continue;
                }
                    
                if (username == "admin" && username == password)                // 教师用户
                {
                    Teacher teacher = new Teacher(path);
                    teacher.input();                                            // 处理输入
                    return 1;
                }

                string sql = "SELECT Sno FROM student WHERE Sno= " + username;  // 查看是否是学生用户

                SQLiteDataReader reader = connect.selectSQL(sql);
                if (reader == null)                                             // 若出错
                {
                    UI.waitReturn("用户名或密码错误！按任意键重新登录...");
                    Console.Clear();
                    continue;
                }
                while (reader.Read())                                           
                {
                    if (reader["Sno"].ToString() == username)                   // 若匹配数据库的学号
                    {
                        Student stu = new Student(username, path);
                        stu.input();
                        return 2;
                    }
                }
                
                UI.waitReturn("用户名或密码错误！按任意键重新登录...");
                Console.Clear();
            }
        }
    }
    class UI
    {
        public static void showTip(string tip)                                  // 在屏幕左下方显示一条提示信息
        {
            clearTip();
            int x = Console.CursorLeft;                                         // 保存当前位置信息
            int y = Console.CursorTop;
            Console.SetCursorPosition(0, Console.WindowHeight - 2);             // 将光标移到左下角
            Console.Write(tip);
            Console.SetCursorPosition(x, y);                                    // 显示完提示后将光标移回原位置
        }
        public static void clearTip()                                           // 清除提示
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            for (int i = 0; i < Console.WindowWidth; i++)                       // 用空格覆盖原来的提示来达到覆盖效果
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(x, y);
        }
        public static void waitReturn(string tip = "按任意键返回....")          // 使程序停止,按任意键继续执行
        {
            UI.showTip(tip);
            Console.ReadKey(true);
        }

        public static void clearCurrentRow()                                    // 清除当前行
        {
            clearRow(Console.CursorTop);
        }
        public static void clearRow(int row)                                    // 清除指定的行
        {
            Console.SetCursorPosition(0, row);
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, row);
        }
        public static string readByChar(bool isPassword = false)                // 逐字符读取，按下回车结束输入，参数isPassword为true时，输入的内容显示为‘*’
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(isPassword);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key == ConsoleKey.Backspace)                           // 实现回删功能
                {
                    if (password.Length <= 0)                                   // 没有输入时，不能回删
                    {
                        info = Console.ReadKey(isPassword);
                        continue;
                    }
                    if(isPassword)                                              // 若是密码，需要清除掉 '*'
                    {
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    
                    password = password.Remove(password.Length - 1, 1);         // 将最后一个输入的字符移除
                }
                else
                {   
                    if(isPassword)                                              // 若是密码，显示 '*'
                    {
                        Console.Write("*");
                    }
                    password += info.KeyChar;
                }

                info = Console.ReadKey(isPassword);
            }
            return password;
        }
        public static string readNumberStr(int min, int max, string head, string tip) // 读取一串数字，长度在 min 和 max 之间，不满足条件时显示 tip，并重新输入
        {
            string numstr;
            while (true)
            {
                Console.Write("{0}:",head); 
                numstr = readByChar(false);
                if( numstr.ToUpper() == "QUIT")                                 // 输入quit 时，中断输入
                {
                    return numstr;
                }
                if (numstr.Length >= min && numstr.Length <= max && isInt(numstr))
                    break;
                else
                    showTip(tip);
                clearCurrentRow();                                              // 清除当前行以重新输入
            }
            clearTip();
            Console.WriteLine();
            return numstr;
        }
        public static void centerWrite(string str, int width)                   // 将字符串 str 居中显示在 width 宽度的地方
        {
            int len = width - Encoding.Default.GetByteCount(str);
            if (len < 0)                                                        // 如果width太小，则优先显示所有内容而不是对齐
                len = 0;

            string tmpstr = new string(' ', len / 2) + str + new string(' ', len - (len / 2));

            Console.Write(tmpstr);
        }
        public static void printRow(string[] str, int[] width)                  // 显示一行，str 和 width 对应显示的字符串和宽度
        {                                                                       // 最后的效果是 "| xxx | XX |...|" 的形式，相当于表格的一行
            Console.Write("|");
            for (int i = 0; i < str.Length; i++)
            {
                centerWrite(str[i], width[i]);
                Console.Write("|");
            }
            Console.WriteLine();
        }
        public static void showMenu(string[] menu, int row = 0, int start = 0)  // 显示功能菜单，menu 存储要显示的菜单内容
        {                                                                       // row 为当前选中第几行， start 为菜单从第几行开始显示
            Console.SetCursorPosition(0, start);
            Console.ForegroundColor = ConsoleColor.DarkYellow;                  
            foreach (string s in menu)
            {
                Console.WriteLine(s);
            }
            Console.SetCursorPosition(0, start);
            moveChooseHelper(menu, 0, row, start);
        }
        public static void moveChooseHelper(string[] menu, int now, int next, int start = 0)    // 辅助移动选择，改变字体颜色
        {                                                                                       // now 为前为第几个功能， next 为下一个功能
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.SetCursorPosition(0, now + start);
            Console.Write(menu[now]);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(0, next + start);
            Console.Write(menu[next]);
            Console.SetCursorPosition(0, next + start);
        }
        public static void moveChoose(string[] menu, ConsoleKeyInfo info, ref int row, int start = 0)   // 移动到下一个功能
        {
            if (info.Key == ConsoleKey.UpArrow)                                 // 向上方向键
            {
                int t = row;
                row = (row + menu.Length - 1) % menu.Length;
                moveChooseHelper(menu, t, row, start);
            }
            else if (info.Key == ConsoleKey.DownArrow)                          // 向下方向键
            {
                int t = row;
                row = (row + 1) % menu.Length;
                moveChooseHelper(menu, t, row, start);
            }
        }
        public static bool isInt(string value)                                  // 检查 value 是否是只由数字组成的字符串
        {
            return Regex.IsMatch(value, @"^\d*$");
        }
    }
    class SQL                                                                  
    {
        private SQLiteConnection connect;                                       // 存储一个数据库的连接，在此基础上的各种操作
        public SQL(string path)                                                 // 传入数据库的路径
        {
            connectDatabase(path);
        }
        public string openFile(string path)                                     // 读取一个文件存在字符串中                                
        {
            string str = "";

            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                str = String.Concat(str, line.ToString());
            }
            return str;
        }
        public void connectDatabase(string path)                                // 连接数据库，如果数据库不存在，则创建一个，并写入预先写好的 SQL 文件
        {
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
                connect = new SQLiteConnection("Data Source=" + path);          // 连接
                connect.Open();                                                 // 打开
                string sql = openFile("..\\..\\db\\example.sql");
                excuteSQL(sql);
            }
            else
            {
                connect = new SQLiteConnection("Data Source=" + path);
                connect.Open();
            }
        }
        public bool excuteSQL(string sql)                                       // 执行一条不需要返回值的 SQL 语句
        {
            SQLiteCommand cmd = new SQLiteCommand(sql, connect);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)                                                 // 执行失败显示错误信息
            {
                UI.showTip(e.Message);
                return false;
            }
        }
        public SQLiteDataReader selectSQL(string sql)                           // 执行一条需要返回结果的 SQL 语句
        {
            SQLiteCommand cmd = new SQLiteCommand(sql, connect);
            try
            {
                SQLiteDataReader reader = cmd.ExecuteReader();                  
                return reader;                                                  // 执行成功，返回一个 reader
            }
            catch (Exception e)
            {
                UI.showTip(e.Message);
                return null;                                                    // 执行失败，返回 null
            }
        } 
        public bool isExist(string sql)                                         // 执行一条语句，查看是否有返回值
        {
            SQLiteDataReader reader = selectSQL(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
    }
    class Teacher                                                               // 教师的界面
    {
        private SQL connect;
        private string[] menu_all;                                              // 一级菜单
        private string[] menu_one;                                              // 二级第一个菜单
        private string[] menu_two;                                              // 二级第二个菜单
        private string[] menu_three;                                            // 二级第三个菜单
        private string[] menu_now;                                              // 当前菜单
        private string[] cno;                                                   // 所有课程号
        private string[] cname;                                                 // 所有课程名称
        private int row;                                                        // 当前菜单的第几个功能
        private int choose;                                                     // 选择了哪个功能
        
        public Teacher(string path)
        {
            connect = new SQL(path);
            row = 0;
            choose = 0;
            
            initMenu();
            getCourse();
        }
        private void initMenu()                                                 // 初始化菜单
        {
            menu_all = new string[]{
                "1.学生信息管理",
                "2.学生成绩管理",
                "3.学生成绩统计",
                "4.退出"};
            menu_now = menu_all;                                                // 初始当前菜单为一级菜单

            menu_one = new string[]{
                "1.添加学生",
                "2.删除学生",
                "3.评价学生",
                "4.查看所有学生信息",
                "5.返回"};

            menu_two = new string[]{
                "1.录入/修改成绩",
                "2.查看所有学生成绩",
                "3.返回"};
           
            menu_three = new string[]{
                "1.各科平均成绩",
                "2.成绩排名",
                "3.返回"};
        }
        private void getCourse()                                                // 查询当前数据库有哪些课程
        {
            List<string> no = new List<string>();
            List<string> name = new List<string>();
            string sql = "SELECT * FROM Course";
            SQLiteDataReader r = connect.selectSQL(sql);
            if (r != null)
            {
                while (r.Read())
                {
                    no.Add(r["Cno"].ToString());
                    name.Add(r["Cname"].ToString());
                }
            }
            cno = no.ToArray();
            cname = name.ToArray();
        }
        public void input()                                                     // 接受用户的输入，也是主循环
        {
            bool flag = true;
            Console.CursorVisible = false;                                      // 为了显示效果，光标设为不可见
            while (true)
            {
                if(flag)                                                        // flag 表示是否要显示新的菜单
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;            // 问候语
                    Console.WriteLine("--->学生管理系统<---");
                    Console.WriteLine("----->老师你好<-----");
                    UI.showMenu(menu_now, row, 3);
                    flag = false;
                }
                ConsoleKeyInfo info = Console.ReadKey(true);                    // 获取一个按键的输入

                UI.moveChoose(menu_now, info, ref row, 3);                      // 移动选择

                if (info.Key == ConsoleKey.Enter)                               // 若是回车，必然要显示新的菜单
                {
                    flag = true;
                    
                    if ( !Enter() )                                             // 若是退出，则询问是否结束程序
                    {
                        UI.showTip("再按一下退出。。。");
                        info = Console.ReadKey(true);
                        if (info.Key == ConsoleKey.Enter)
                            break;
                    }
                }        
            }
        }
        public bool Enter()                                                     // 按下回车键做出的一系列反应，返回值表示程序是否继续
        {
            if (menu_now == menu_all)                                           // 先判断当前菜单是一级还是二级
            {
                choose = row + 1;
                if( row == menu_now.Length - 1 )                                // 一级菜单最后一行为退出程序
                {
                    return false;
                }
                else
                {
                    if (row == 0)
                    {
                        menu_now = menu_one;
                    }
                    else if (row == 1)
                    {
                        menu_now = menu_two;
                    }
                    else if (row == 2)
                    {
                        menu_now = menu_three;
                    }
                    row = 0;
                    return true;
                }
            }
            else
            {
                if (row == menu_now.Length - 1)                                 // 二级菜单最后一行表示是否返回上一级菜单
                {
                    menu_now = menu_all;
                    row = 0;
                }
                else
                {
                    choose = choose * 10 + row;
                    callFuction(choose);
                    choose = (choose - row) / 10;
                }
                return true;
            }
        }
        public void callFuction(int choose)                                     // 根据 choose 选择进入哪个功能
        {
            Console.Clear();
            switch(choose)
            {
                case 10:
                    addStudent();                                               // 添加学生
                    break;
                case 11:
                    deleteStudent();                                            // 删除学生
                    break;
                case 12:
                    commentStudent();                                           // 评价学生
                    break;
                case 13:
                    viewStudent();                                              // 查看所有学生信息
                    break;
                case 20:
                    inputScore();                                               // 录入/修改 成绩
                    break;
                case 21:
                    viewScore();                                                // 查看所有学生成绩
                    break;
                case 30:
                    showAveGrade();                                             // 查看各科平均成绩
                    break;
                case 31:
                    showGradeOrder();                                           // 查看成绩排名
                    break;
            } 
        }
        public void fucHeader( bool quit = true, bool cvisile = true)           // 通用的显示标题
        {
            
            Console.CursorVisible = cvisile;
            string str = new string('-', 20);
            Console.WriteLine("------>{0}<------",menu_now[row].Remove(0, 2));
            Console.WriteLine();
            if ( quit )
                Console.WriteLine("Tip:输入quit退出");
            Console.WriteLine();
        }
        public string readSno(bool flag)                                        // 读取一个学号，由于多次使用，故写成函数
        {
            string sno;
            while (true)
            {
                sno = UI.readNumberStr(9, 9, "学号", "学号为9位数字");          // 读取学号
                if (sno.ToUpper() == "QUIT")                                    // 若为 QUIT 则马上返回
                {
                    break;
                }
                if ( connect.isExist(String.Format("SELECT Sno FROM student WHERE Sno = '{0}'", sno)) == flag)
                {
                    break;
                }
                else
                {
                    if (flag)
                        UI.showTip("学生不存在！");
                    else
                        UI.showTip("学生已存在！");
                }
                
                UI.clearRow(Console.CursorTop - 1);
            }
            UI.clearTip();
            return sno;
        }
        public string nameOfSno(string sno)                                     // 给定学号，返回姓名
        {
            string sql = String.Format("SELECT Sname FROM student WHERE Sno = '{0}'",sno);
            SQLiteDataReader reader = connect.selectSQL(sql);
            reader.Read();

            return reader["Sname"].ToString();
        }
        public void insertToSC(string sno)                                      // 添加学生时，为学生分配课程
        {
            foreach(string no in cno)
            {
                string sql = String.Format("INSERT INTO SC(Sno,Cno) VALUES ('{0}','{1}')", sno, no);
                connect.excuteSQL(sql);
            }
        }
        public void addStudent()                                                // 添加学生
        {
            fucHeader();

            string sno = readSno(false);
            if( sno.ToUpper() == "QUIT")                                        // 若 QUIT 则马上返回上一级
            {
                Console.CursorVisible = false;
                return;
            }
            Console.WriteLine();

            string sname;
            while (true)                                                        // 输入姓名，不允许为空
            {
                Console.Write("姓名:");
                sname = Console.ReadLine();
                if (sname.ToUpper() == "QUIT")
                {
                    Console.CursorVisible = false;
                    return;
                }
                if ( sname.Trim().Length > 0 )
                {
                    break;
                }
                UI.showTip("名字不能为空");
                UI.clearRow(Console.CursorTop - 1);
            }
            UI.clearTip();

            Console.WriteLine();

            string ssex;
            while ( true )                                                      // 输入性别，只允许输入 男 或 女
            {
                Console.Write("性别:");
                ssex = Console.ReadLine();
                if (ssex.ToUpper() == "QUIT")
                {
                    Console.CursorVisible = false;
                    return;
                }
                if ( ssex == "男" || ssex == "女")
                {

                    break;
                }
                UI.showTip("性别只能为男或女");
                UI.clearRow(Console.CursorTop - 1);
            }
            UI.clearTip();

            Console.WriteLine();
            string sage = UI.readNumberStr(2, 2, "年龄", "请输入正确的年龄");   // 年龄只允许为 2 位数的数字
            if (sage.ToUpper() == "QUIT")
            {
                Console.CursorVisible = false;
                return;
            }
            Console.CursorVisible = false;                                      // 输入结束，将光标隐藏

            string sql = String.Format("INSERT INTO Student(Sno,Sname,Ssex,Sage) VALUES('{0}','{1}','{2}',{3})", sno, sname, ssex, sage);
            if (!connect.excuteSQL(sql))
            {
                UI.waitReturn("添加失败! 请检查信息后重新添加...");
            }
            else
            {
                UI.waitReturn("添加成功！按任意键返回...");
            }
            insertToSC(sno);                                                    // 为此学生分配课程
        }
        public void deleteStudent()                                             // 删除学生
        {
            fucHeader();

            string sno = readSno(true);                                         // 读取学号
            if( sno.ToUpper() == "QUIT" )
            {
                Console.CursorVisible = false;
                return;
            }
            
            string sql = String.Format("SELECT Sno FROM student WHERE Sno = '{0}'", sno);
            
            if( !connect.isExist(sql) )                                         // 查看学生是否存在
            {
                UI.waitReturn("此学生不存在!按任意键返回...");
                Console.CursorVisible = false;
                return;
            }

            string sname = nameOfSno(sno);                                      // 获取学生姓名
            Console.Write("确定删除学号为 {0} 的 {1} 同学？(Y/N): ", sno, sname);               
            string ok = Console.ReadLine();
            if( ok.ToUpper() != "Y" )
            {
                Console.CursorVisible = false;
                return;
            }

            sql = String.Format("DELETE FROM student WHERE Sno = '{0}'", sno);

            Console.CursorVisible = false;
            if (!connect.excuteSQL(sql))
            {
                UI.waitReturn("删除失败! 请检查学号后重新删除...");
            }
            else
            {
                UI.waitReturn("删除成功！按任意键返回...");
            }
        }
        public void commentStudent()                                            // 评价学生
        {
            fucHeader(true);
            string sno = readSno(true);

            if(sno.ToUpper() == "QUIT")
            {
                Console.CursorVisible = false;
                return;
            }
            string sname = nameOfSno(sno);

            Console.WriteLine();
            Console.WriteLine("请输入对 {0} 的评价(50字以内）:",sname);
            Console.WriteLine();

            string scomment = Console.ReadLine();
            if(scomment.ToUpper() == "QUIT")
            {
                Console.CursorVisible = false;
                return;
            }
            if( scomment.Length > 50 )                                          // 评价的长度最大为 50，超过的部分则丢弃
            {
                scomment = scomment.Remove(50);
            }

            Console.CursorVisible = false;

            string sql = String.Format("UPDATE student SET Scomment = '{0}' WHERE Sno = '{1}'", scomment, sno);
            if( connect.excuteSQL(sql) )
            {
                UI.waitReturn("评价成功！按任意键返回...");
            }
            else
            {
                UI.waitReturn("评价失败！请检查后重试...");
            }
            
        }
        public void viewStudent()                                               // 查看所有学生的信息
        {
            fucHeader(false,false);

            string sql = "SELECT * FROM student";
            if( !connect.isExist(sql) )
            {
                UI.waitReturn("暂无学生！");
                return;
            }

            SQLiteDataReader reader = connect.selectSQL(sql);
            if (reader != null)
            {
                int[] width = { 12, 11, 7, 7, 0 };
                int five = Console.WindowWidth - width.Sum() - 6;
                width[4] = five;

                string str = new string('-', width.Sum() + width.Length + 1);
                Console.WriteLine(str);
                string[] header = { "学号", "姓名", "年龄", "性别", "评价" };
                UI.printRow(header, width);
                Console.WriteLine(str);

                while (reader.Read())
                {
                    string[] body = { reader["Sno"].ToString(), reader["Sname"].ToString(),
                        reader["Sage"].ToString(), reader["Ssex"].ToString(),
                        reader["Scomment"].ToString() };

                    UI.printRow(body, width);
                }
                Console.WriteLine(str);
            }
            UI.waitReturn();
        }
        public void inputScore()                                                // 录入成绩
        {
            fucHeader();
            
            string sno = readSno(true);

            if (sno.ToUpper() == "QUIT")
            {
                Console.CursorVisible = false;
                return;
            }

            string sname = nameOfSno(sno);
            
            Console.WriteLine();
            Console.WriteLine("请输入 {0} 的各科成绩(括号中为原来分数，回车跳过输入):", sname);
            Console.WriteLine();

            for (int i = 0; i < cno.Length; i++)                                // 录入或修改所有科目的成绩
            {
                string sql = String.Format("SELECT * FROM SC WHERE Sno = '{0}' AND Cno = '{1}'",sno, cno[i]);
                string tcname = cname[i];
                SQLiteDataReader r = connect.selectSQL(sql);
                bool exists = false;
                if( r != null )
                {
                    if(r.Read())
                    {
                        tcname += String.Format("({0})",r["Grade"]);
                        exists = true;
                    }
                }
                string score = UI.readNumberStr(0, 3, tcname, "请输入正确的分数");

                if( score.ToUpper() == "QUIT" )
                {
                    Console.CursorVisible = false;
                    return;
                }

                if (score.Length == 0)
                {
                    Console.WriteLine();
                    continue;
                }
                
                if( score.Length == 3 && score != "100" )
                {
                    i--;
                    UI.clearRow(Console.CursorTop - 1);
                    UI.showTip("分数不得大于100分！");
                    continue;
                }

                if(exists)
                {
                    sql = String.Format("UPDATE SC SET Grade = {0} WHERE Sno = '{1}' AND Cno = '{2}'", score,sno,cno[i]);
                }
                else
                {
                    sql = String.Format("INSERT INTO SC(Sno,Cno,Grade) VALUES('{0}','{1}',{2})", sno, cno[i], score);
                }

                if( connect.excuteSQL(sql) )
                {
                    UI.showTip("录入成功!");
                }
                else
                {
                    UI.showTip("录入失败!");
                }
                Console.WriteLine();
            }
            Console.CursorVisible = false;
            UI.waitReturn();
        }
        public void viewScore()                                                 // 查看所有学生成绩
        {
            fucHeader(false, false);

            List<string> h = new List<string>();
            h.Add("学号");
            h.Add("姓名");
            foreach(string s in cname)
            {
                h.Add(s);
            }
            string[] header = h.ToArray();
            int[] width = { 12, 11, 12, 12, 12, 12 };

            string str = new string('-', width.Sum() + width.Length + 1);
            Console.WriteLine(str);
            UI.printRow(header, width);
            Console.WriteLine(str);

            string sql = "SELECT * FROM student";
            SQLiteDataReader r = connect.selectSQL(sql);
            if (r == null)
            {
                UI.waitReturn("暂无学生成绩信息！按任意键返回....");
                return;
            }

            while(r.Read())
            {
                List<string> list = new List<string>();
                list.Add(r["Sno"].ToString());
                list.Add(r["Sname"].ToString());

                string se = string.Format("SELECT * FROM SC WHERE Sno = '{0}'",r["Sno"]);
                SQLiteDataReader rr = connect.selectSQL(se);
                if (rr == null)
                    break;
                
                while(rr.Read())
                {
                    list.Add(rr["Grade"].ToString());
                }
                string[] body = list.ToArray();

                UI.printRow(body, width);
            }
            Console.WriteLine(str);
            UI.waitReturn();
        }
        public void showAveGrade()                                              // 显示各科平均分
        {
            fucHeader(false, false);

            string[] header = { "课程", "平均分" };
            int[] width = { 16, 18 };
            string str = new string('-', width.Sum() + width.Length + 1);
            Console.WriteLine(str);
            UI.printRow(header, width);
            Console.WriteLine(str);

            string sql = "SELECT * FROM AveGrade";
            SQLiteDataReader r = connect.selectSQL(sql);
            if( r != null )
            {
                while( r.Read() )
                {
                    string[] body = { r["Cname"].ToString(), r["ave"].ToString() };
                    UI.printRow(body, width);
                }
            }
            Console.WriteLine(str);
            UI.waitReturn();
        }
        public void showGradeOrder()                                            // 显示成绩排名，用总分排名
        {
            fucHeader(false, false);

            string[] header = { "排名", "学号","姓名","总分" };
            int[] width = { 6, 12, 11, 6 };
            string str = new string('-', width.Sum() + width.Length + 1);
            Console.WriteLine(str);
            UI.printRow(header, width);
            Console.WriteLine(str);

            string sql = "SELECT * FROM GradeOrder ORDER BY total DESC";
            SQLiteDataReader r = connect.selectSQL(sql);
            if (r != null)
            {
                int order = 1;
                while (r.Read())
                {
                    string[] body = { order.ToString(),r["Sno"].ToString(), r["Sname"].ToString(), r["total"].ToString() };
                    UI.printRow(body, width);
                    order++;
                }
            }
            Console.WriteLine(str);
            UI.waitReturn();
        }
    }
    class Student                                                               // 学生用户的界面
    {
        private SQL connect;                                                    // 数据库连接
        private string username;                                                // 用户名，也就是学号
        private string sname;                                                   // 姓名
        private string[] menu;                                                  // 只有一级菜单
        private int row;                                                        // 当前在第几个功能
        public Student(string user,string path)                                 // 传入用户名和数据库路径
        {
            connect = new SQL(path);
            username = user;

            string sql = string.Format("SELECT * FROM Student WHERE Sno = '{0}'",username);
            SQLiteDataReader r = connect.selectSQL(sql);
            if( r != null )
            {
                while(r.Read())
                    sname = r["Sname"].ToString();
            }
            
            menu = new string[] { "1.查看成绩", "2.查看寄语", "3.退出" };
            row = 0;
        }
        public void input()                                                     // 获取用户输入，主循环
        {
            bool flag = true;
            Console.CursorVisible = false;
            while (true)
            {
                if (flag)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("--->学生管理系统<---");
                    Console.WriteLine("--->{0}同学你好<---",sname);
                    UI.showMenu(menu, row, 3);
                    flag = false;
                }
                ConsoleKeyInfo info = Console.ReadKey(true);

                UI.moveChoose(menu, info, ref row, 3);

                if (info.Key == ConsoleKey.Enter)
                {
                    flag = true;

                    if (row == menu.Length - 1)
                    {
                        UI.showTip("再按一下退出。。。");
                        ConsoleKeyInfo quit = Console.ReadKey(true);
                        if (quit.Key == ConsoleKey.Enter)
                            break;
                    }
                    else if(row == 0)
                    {
                        Console.Clear();
                        viewScore();
                    }
                    else if(row == 1)
                    {
                        Console.Clear();
                        viewComment();
                    }
                
                }
            }
        }
        public void viewScore()                                                 // 查看各科成绩
        {
            Console.WriteLine("------->{0}<------", menu[row].Remove(0,2));
            Console.WriteLine();

            string[] header = { "课程", "成绩" };
            int[] width = { 12, 6 };
            string str = new string('-', width.Sum() + width.Length + 1);

            Console.WriteLine(str);
            UI.printRow(header, width);
            Console.WriteLine(str);

            string sql = String.Format("SELECT * FROM Grade WHERE Sno = '{0}'",username);
            SQLiteDataReader r = connect.selectSQL(sql);
            if (r == null)
                return;
            while( r.Read() )
            {
                string[] body = { r["Cname"].ToString(), r["Grade"].ToString() };
                UI.printRow(body, width);
            }
            Console.WriteLine(str);
            UI.waitReturn();
        }
        public void viewComment()                                               // 查看老师对自己的评价
        {
            Console.WriteLine("------>{0}<------", menu[row].Remove(0, 2));
            Console.WriteLine();

            Console.WriteLine("你的老师对你的评价是:");
            Console.WriteLine();

            string sql = String.Format("SELECT Scomment FROM student WHERE Sno = '{0}'", username);
            SQLiteDataReader r = connect.selectSQL(sql);
            if (r == null)
                return;
            while (r.Read())
            {
                Console.WriteLine(r["Scomment"]);
            }
            UI.waitReturn();
        }
    }
}

