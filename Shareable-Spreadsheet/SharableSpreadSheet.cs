﻿using System;
using System.Threading;
class SharableSpreadSheet
{
    private int rowsNum;
    private int colsNum;
    private int users;
    private long readers = 0;
    private int writers = 0;
    private Semaphore readWriteMutex = new Semaphore(1, 1);
    private Semaphore? searchersSemaphore = null;
    private List<Mutex> rowsMutexes = new List<Mutex>();
    private List<List<String>> sheet;

    // construct a nRows*nCols spreadsheet.
    // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
    public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
    {
        if (nRows <= 0 || nCols <= 0)
            throw new ArgumentOutOfRangeException("Rows and columns must be positive numbers.");
        rowsNum = nRows;
        colsNum = nCols;
        if (nUsers < -1)
            throw new ArgumentOutOfRangeException("Users number must be a positive number, or -1 if not limited.");
        setConcurrentSearchLimit(nUsers);
        for (int i = 0; i < rowsNum; i++)
            rowsMutexes.Add(new Mutex());
        sheet = new List<List<String>>();
        for (int i = 0; i < rowsNum; i++)
        {
            sheet.Add(new List<String>());
            for (int j = 0; j < colsNum; j++)
                sheet[i].Add("");
        }
    }

    // return the string at [row,col]
    // Read action - waits to enter read section.
    public String getCell(int row, int col)
    {
        enterReadSection();
        if (!checkCell(row, col))
        {
            exitReadSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        String cell = sheet[row][col];
        exitReadSection();
        return cell;
    }

    // set the string at [row,col]
    // Write action - waits to enter write section.
    public void setCell(int row, int col, String str)
    {
        enterWriteSection();
        if (!checkCell(row, col))
        {
            exitWriteSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        rowsMutexes[row].WaitOne();
        sheet[row][col] = str;
        rowsMutexes[row].ReleaseMutex();
        exitWriteSection();
    }

    // return first cell indexes that contains the string (search from first row to the last row)
    // Search action - waits to enter read section.
    // Also waits for searcher lock, if searcers number is limited.
    public Tuple<int,int> searchString(String str)
    {
        enterSearchSection();
        for (int i = 0; i < rowsNum; i++)
        {
            for (int j = 0; j < colsNum; j++)
            {
                if (String.Equals(sheet[i][j], str))
                {
                    Tuple<int, int> t = new(i, j);
                    exitSearchSection();
                    return t;
                }
            }
        }
        exitSearchSection();
        throw new Exception(str + " not found.");
    }

    // exchange the content of row1 and row2
    // Write action - waits to enter write section.
    // Also waits for rows locks.
    public void exchangeRows(int row1, int row2)
    {
        enterWriteSection();
        if (!checkRow(row1) || !checkRow(row2))
        {
            exitStructSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        rowsMutexes[row1].WaitOne();
        rowsMutexes[row2].WaitOne();
        List<String> tmp = sheet[row1];
        sheet[row1] = sheet[row2];
        sheet[row2] = tmp;
        rowsMutexes[row1].ReleaseMutex();
        rowsMutexes[row2].ReleaseMutex();
        exitWriteSection();
    }

    // exchange the content of col1 and col2
    // Write action - waits to enter write section.
    // Also waits for struct lock.
    public void exchangeCols(int col1, int col2)
    {
        enterStructSection();
        if (!checkCol(col1) || !checkCol(col2))
        {
            exitStructSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        List<String> tmp = sheet[col1];
        sheet[col1] = sheet[col2];
        sheet[col2] = tmp;
        exitWriteSection();
    }

    // perform search in specific row
    // Search action - waits to enter read section.
    // Also waits for searcher lock, if searcers number is limited.
    public int searchInRow(int row, String str)
    {
        enterSearchSection();
        if (!checkRow(row))
        {
            exitSearchSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        for (int j = 0; j < colsNum; j++)
        {
            if (String.Equals(sheet[row][j], str))
            {
                exitSearchSection();
                return j;
            }
        }
        exitSearchSection();
        throw new Exception(str + " not found.");
    }

    // perform search in specific col
    // Search action - waits to enter read section.
    // Also waits for searcher lock, if searcers number is limited.
    public int searchInCol(int col, String str)
    {
        enterSearchSection();
        if (!checkCol(col))
        {
            exitSearchSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        for (int i = 0; i < rowsNum; i++)
        {
            if (String.Equals(sheet[i][col], str))
            {
                exitSearchSection();
                return i;
            }
        }
        exitSearchSection();
        throw new Exception(str + " not found.");
    }

    // Search action - waits to enter read section.
    // Also waits for searcher lock, if searcers number is limited.
    public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
    {
        enterSearchSection();
        Tuple<int, int>? res =  searchInRangeHelper(col1, col2, row1, row2, str, true);
        exitSearchSection();
        if (res == null)
            throw new Exception(str + " not found.");
        return res;
    }

    // add a row after row1
    // Write action - waits to enter write section.
    // Also waits for struct lock.
    public void addRow(int row1)
    {
        enterStructSection();
        if (!checkRow(row1))
        {
            exitStructSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        rowsNum++;
        List<String> newRow = new List<String>();
        for (int i = 0; i < colsNum; i++)
            newRow.Add("");
        if (row1 + 2 == rowsNum)
            sheet.Add(newRow);
        else
            sheet.Insert(row1 + 1, newRow);
        rowsMutexes.Add(new Mutex());
        exitStructSection();
    }

    // add a column after col1
    // Write action - waits to enter write section.
    // Also waits for struct lock.
    public void addCol(int col1)
    {
        enterStructSection();
        if (!checkCol(col1))
        {
            exitStructSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        colsNum++;
        if (col1 + 2 == colsNum)
        {
            for (int i = 0; i < rowsNum; i++)
                sheet[i].Add("");
        }
        else
        {
            for (int i = 0; i < rowsNum; i++)
                sheet[i].Insert(col1 + 1, "");
        }
        exitStructSection();
    }

    // perform search and return all relevant cells according to caseSensitive param
    // Read action - waits to enter read section.
    public Tuple<int, int>[] findAll(String str, bool caseSensitive)
    {
        List<Tuple<int, int>> res = new List<Tuple<int, int>>();
        int r = 0, c = 0;
        enterSearchSection();
        while (r < rowsNum)
        {
            Tuple<int, int>? currentRes = searchInRangeHelper(c, colsNum - 1, r, rowsNum - 1, str, caseSensitive);
            if (currentRes != null)
            {
                res.Add(currentRes);
                c = currentRes.Item2 + 1 >= colsNum ? 0 : currentRes.Item2 + 1;
                r = c == 0 ? r += 1 : currentRes.Item1;
            }
            else
                break;
        }
        exitSearchSection();
        if (res.Count == 0)
            throw new Exception(str + " not found.");
        return res.ToArray();
    }

    // replace all oldStr cells with the newStr str according to caseSensitive param
    public void setAll(String oldStr, String newStr, bool caseSensitive)
    {
        Tuple<int, int>[] locations = findAll(oldStr, caseSensitive);
        foreach (Tuple<int, int> location in locations)
            setCell(location.Item1, location.Item2, newStr);
    }

    // return the size of the spreadsheet in nRows, nCols
    // Read action - waits to enter read section.
    public Tuple<int, int> getSize()
    {
        enterReadSection();
        Tuple<int, int> res = new Tuple<int, int>(rowsNum, colsNum);
        exitReadSection();
        return res;
    }

    // this function aims to limit the number of users that can perform the search operations concurrently.
    // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
    // In this case additional search operations will wait for existing search to finish.
    // This function is used just in the creation
    public void setConcurrentSearchLimit(int nUsers)
    {
        users = nUsers;
        if (nUsers > 0)
            searchersSemaphore = new Semaphore(users, users);
    }

    // save the spreadsheet to a file fileName.
    // you can decide the format you save the data. There are several options.
    // Read action - waits to enter read section.
    public void save(String fileName)
    {

        using (StreamWriter sw = File.CreateText(fileName))
        {
            enterReadSection();
            sw.WriteLine(rowsNum);
            sw.WriteLine(colsNum);
            sw.WriteLine(users);
            for (int i = 0; i < rowsNum; i++)
            {
                for (int j = 0; j < colsNum; j++)
                    sw.WriteLine(sheet[i][j]);
            }
            exitReadSection();
        }
    }

    // load the spreadsheet from fileName
    // replace the data and size of the current spreadsheet with the loaded data
    // Write action - waits to enter write section.
    // Also waits for struct lock.
    public void load(String fileName)
    {
        enterStructSection();
        try
        {
            using (StreamReader sr = new StreamReader(fileName)) // using statement also closes the StreamReader

            {
                rowsNum = int.Parse(sr.ReadLine());
                colsNum = int.Parse(sr.ReadLine());
                users = int.Parse(sr.ReadLine());
                sheet = new List<List<String>>();
                for (int i = 0; i < rowsNum; i++)
                {
                    List<String> newRow = new List<String>();
                    for (int j = 0; j < colsNum; j++)
                        newRow.Add(sr.ReadLine());
                    sheet.Add(newRow);
                }
            }
        }
        catch (Exception e)
        {
            exitStructSection();
            Console.WriteLine("The file could not be read.");
            throw new Exception(e.StackTrace);
        }
        exitStructSection();
    }

    private bool checkCell(int row, int col)
    {
        return (row >= 0 && row < rowsNum && col >= 0 && col < colsNum);
    }

    public bool checkRow(int row)
    {
        return (row >= 0 && row < rowsNum);
    }

    public bool checkCol(int col)
    {
        return (col >= 0 && col < colsNum);
    }

    // perform search within spesific range: [row1:row2,col1:col2] 
    // includes col1,col2,row1,row2
    private Tuple<int, int>? searchInRangeHelper(int col1, int col2, int row1, int row2, String str, bool _case)
    {
        enterSearchSection();
        if (!checkCell(row1, col1) || !checkCell(row2, col2) || row1 > row2 || (row1 == row2 && col1 > col2))
        {
            exitSearchSection();
            throw new ArgumentOutOfRangeException("Bad parameters");
        }
        int r = row1, c = col1;
        while (r < rowsNum)
        {
            if (caseEquals(sheet[r][c], str, _case))
            {
                exitSearchSection();
                return new(r, c);
            }
            if (r == row2 && c == col2)
                break;
            c = c + 1 == colsNum ? 0 : c + 1;
            r = c == 0 ? r + 1 : r;
        }
        exitSearchSection();
        return null;
    }

    private bool caseEquals(String s1, String s2, bool _case)
    {
        if (_case)
            return String.Equals(s1, s2);
        return String.Equals(s1.ToLower(), s2.ToLower());
    }

    private void enterReadSection()
    {
        //readersMutex.WaitOne();
        if (Interlocked.Increment(ref readers) == 1)
            readWriteMutex.WaitOne();
        //readersMutex.Release();
    }

    private void exitReadSection()
    {
        //readersMutex.WaitOne();
        if (Interlocked.Decrement(ref readers) == 0)
            readWriteMutex.Release();
        //readersMutex.Release();
    }

    private void enterSearchSection()
    {
        if (searchersSemaphore != null)
            searchersSemaphore.WaitOne();
        enterReadSection();
    }

    private void exitSearchSection()
    {
        if (searchersSemaphore != null)
            searchersSemaphore.Release();
        exitReadSection();
    }
    private void enterWriteSection()
    {
        //writersMutex.WaitOne();
        if (Interlocked.Increment(ref writers) == 1)
            readWriteMutex.WaitOne();
        //writersMutex.Release();
    }

    private void exitWriteSection()
    {
        //writersMutex.WaitOne();
        if (Interlocked.Decrement(ref writers) == 0)
            readWriteMutex.Release();
        //writersMutex.Release();
    }

    private void enterStructSection()
    {
        readWriteMutex.WaitOne();
    }

    private void exitStructSection()
    {
        //writersMutex.WaitOne();
        if (Interlocked.Decrement(ref writers) == 0)
            readWriteMutex.Release();
        //writersMutex.Release();
    }

    /// <summary>
    /// /////////////
    /// </summary>
    public void printSheet()
    {
        foreach (List<String> row in sheet)
            Console.WriteLine("[" + String.Join("|", row) + "]");
    }
}