using System;
using System.Collections.Generic;
using System.Threading;

class Simulator
{
    static private long rows;
    static private long cols;
    static private long wordsRows;
    static private long wordsCols;
    static private int nThreads;
    static private int nOperations;
    static private int mssleep;
    static private SharableSpreadSheet? sheet;
    static private Random random = new Random();
    static private String[,]? words;
    static private Thread[] threads;

    static public void Main(string[] args)
    {
        if (args.Length < 5)
            throw new ArgumentException("Missing arguments.");
        initVars(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]), int.Parse(args[4]));
        threadsStart();
        threadsJoin();
    }

    static private void initVars(int rowsNum, int colsNum, int threadsNum, int operations, int ms)
    {
        rows = rowsNum;
        wordsRows = rowsNum;
        cols = colsNum;
        wordsCols = colsNum;
        nThreads = threadsNum;
        mssleep = ms;
        nOperations = operations;
        sheet = new SharableSpreadSheet((int) rows, (int) cols);
        words = new string[rowsNum, colsNum];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                words[r, c] = "|Cell(" + r.ToString() + "," + c.ToString() + ")|";
                sheet.setCell(r, c, words[r, c]);
            }
        }
        threads = new Thread[nThreads];
        for (int t = 0; t < threadsNum; t++)
        {
            threads[t] = new Thread(() => startProcess());
            threads[t].Name = t.ToString();
        }
    }

    static private void threadsStart()
    {
        foreach (Thread t in threads)
            t.Start();
    }

    static private void threadsJoin()
    {
        foreach (Thread t in threads)
            t.Join();
    }

    static private void startProcess()
    {
        for (int op = 0; op < nOperations; op++)
        {
            work();
            Thread.Sleep(mssleep);
        }
    }

    static private void work()
    {
        switch (random.Next(0, 13))
        {
            case 0: // getCell
                int r0 = random.Next(0, (int) Interlocked.Read(ref rows));
                int c0 = random.Next(0, (int)Interlocked.Read(ref cols));
                try
                {
                    Console.WriteLine("User[{0}]:[{4}] string \"{1}\" found in cell [{2},{3}].", Thread.CurrentThread.Name, sheet.getCell(r0, c0), r0, c0, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{4}] ---Failed--- find string in cell[{1},{2}].\n\tReason: {3}", Thread.CurrentThread.Name, r0, c0, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 1: // setCell
                int r1 = random.Next(0, (int)Interlocked.Read(ref rows));
                int c1 = random.Next(0, (int)Interlocked.Read(ref cols));
                String str1 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    sheet.setCell(r1, c1, str1);
                    Console.WriteLine("User[{0}]:[{4}] string \"{1}\" inserted in cell [{2},{3}].", Thread.CurrentThread.Name, str1, r1, c1);
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{5}] ---Failed--- insert string \"{1}\" in cell[{2},{3}].\n\tReason: {4}", Thread.CurrentThread.Name, str1, r1, c1, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 2: // searchString
                String str2 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    Tuple<int, int> res2 = sheet.searchString(str2);
                    Console.WriteLine("User[{0}]:[{4}] string \"{1}\" was found in cell [{2},{3}].", Thread.CurrentThread.Name, str2, res2.Item1, res2.Item2, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{3}] string \"{1}\" wasn't found.\n\tReason: {2}", Thread.CurrentThread.Name, str2, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 3: // exchangeRows
                int r13 = random.Next(0, (int) Interlocked.Read(ref rows));
                int r23 = random.Next(0, (int) Interlocked.Read(ref rows));
                try
                {
                    sheet.exchangeRows(r13, r23);
                    Console.WriteLine("User[{0}]:[{3}] rows {1} and {2} exchanged successfully.", Thread.CurrentThread.Name, r13, r23, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{4}] ---Failed--- exchange rows {1} and {2}.\n\tReason: {3}", Thread.CurrentThread.Name, r13, r23, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 4: // exchangeCols
                int c14 = random.Next(0, (int) Interlocked.Read(ref cols));
                int c24 = random.Next(0, (int) Interlocked.Read(ref cols));
                try
                {
                    sheet.exchangeCols(c14, c24);
                    Console.WriteLine("User[{0}]:[{3}] columns {1} and {2} exchanged successfully.", Thread.CurrentThread.Name, c14, c24, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{4}] ---Failed--- exchange columns {1} and {2}.\n\tReason: {3}", Thread.CurrentThread.Name, c14, c24, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 5: // searchInRow
                int r5 = random.Next(0, (int) Interlocked.Read(ref rows));
                String str5 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    int c5 = sheet.searchInRow(r5, str5);
                    Console.WriteLine("User[{0}]:[{4}] string \"{1}\" was found in cell [{2},{3}].", Thread.CurrentThread.Name, str5, r5, c5, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{4}] ---Failed--- string \"{1}\" wasn't found in row {2}.\n\tReason: {3}", Thread.CurrentThread.Name, str5, r5, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 6: // searchInCol
                int c6 = random.Next(0, (int) Interlocked.Read(ref cols));
                String str6 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    int r6 = sheet.searchInCol(c6, str6);
                    Console.WriteLine("User[{0}]:[{4}] string \"{1}\" was found in cell [{2},{3}].", Thread.CurrentThread.Name, str6, r6, c6, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{4}] ---Failed--- string \"{1}\" wasn't found in column {2}.\n\tReason: {3}", Thread.CurrentThread.Name, str6, c6, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 7: // searchInRange
                int r17 = random.Next(0, (int) Interlocked.Read(ref rows));
                int c17 = random.Next(0, (int) Interlocked.Read(ref cols));
                int r27 = random.Next(0, (int) Interlocked.Read(ref rows));
                int c27 = random.Next(0, (int) Interlocked.Read(ref cols));
                String str7 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    Tuple<int, int> res7 = sheet.searchInRange(c17, c27, r17, r27, str7);
                    Console.WriteLine("User[{0}]:[{4}] string \"{1}\" was found in cell [{2},{3}].", Thread.CurrentThread.Name, str7, res7.Item1, res7.Item2, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{6}] ---Failed--- string \"{1}\" wasn't found in range [[{2},{3}]:[{4},{5}]].\n\tReason: {6}", Thread.CurrentThread.Name, str7, r17, c17, r27, c27, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 8: // addRow
                int r8 = random.Next(0, (int) Interlocked.Read(ref rows));
                try
                {
                    sheet.addRow(r8);
                    Console.WriteLine("User[{0}]:[{2}] a new row added after row {1}.", Thread.CurrentThread.Name, r8, DateTime.Now.ToString("h:mm:ss"));
                    Interlocked.Increment(ref rows);
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{3}] ---Failed--- a new row wasn't added after row {1}.\n\tReason: {2}", Thread.CurrentThread.Name, r8, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 9: // addCol
                int c9 = random.Next(0, (int) Interlocked.Read(ref cols));
                try
                {
                    sheet.addCol(c9);
                    Console.WriteLine("User[{0}]:[{2}] a new column added after column {1}.", Thread.CurrentThread.Name, c9, DateTime.Now.ToString("h:mm:ss"));
                    Interlocked.Increment(ref cols);
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{3}] ---Failed--- a new column wasn't added after column {1}.\n\tReason: {2}", Thread.CurrentThread.Name, c9, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 10: // findAll
                String str10 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    Tuple<int, int>[] res10 = sheet.findAll(str10, false);
                    String[] out10 = new String[res10.Length];
                    for (int i = 0; i < res10.Length; i++)
                        out10[i] = res10[i].ToString();
                    Console.WriteLine("User[{0}]:[{3}] string \"{1}\" was found in cells [{2}].", Thread.CurrentThread.Name, str10, String.Join(", ", out10), DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{3}] ---Failed--- string \"{1}\" wasn't found.\n\tReason: {2}", Thread.CurrentThread.Name, str10, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 11: // setAll
                String str111 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                String str211 = words[random.Next(0, (int) Interlocked.Read(ref wordsRows)), random.Next(0, (int) Interlocked.Read(ref wordsCols))];
                try
                {
                    sheet.setAll(str111, str211, false);
                    Console.WriteLine("User[{0}]:[{3}] all strings \"{1}\" were replaced with string \"{2}\".", Thread.CurrentThread.Name, str111, str211, DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{4}] ---Failed--- strings \"{1}\" weren't replaced with strings \"{2}\".\n\tReason: {3}", Thread.CurrentThread.Name, str111, str211, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
            case 12: // getSize
                try
                {
                    Console.WriteLine("User[{0}]:[{2}] spreadsheet's size is {1}.", Thread.CurrentThread.Name, sheet.getSize(), DateTime.Now.ToString("h:mm:ss"));
                }
                catch (Exception e)
                {
                    Console.WriteLine("User[{0}]:[{2}] ---Failed--- couldn't get spreadsheet's size.\n\tReason: {1}", Thread.CurrentThread.Name, e.Message, DateTime.Now.ToString("h:mm:ss"));
                }
                break;
        }
    }
}
