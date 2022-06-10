using System;
using System.Threading;
class SharableSpreadSheet
{
    private int rowsNum;
    private int colsNum;
    private long users;
    private long searches = 0;

    private long readers = 0; //
    private int writers = 0;
    private Mutex queue = new Mutex(); // TODO: needed??
    private Mutex readersMutex = new Mutex();
    private Mutex writersMutex = new Mutex();
    private Mutex readWriteMutex = new Mutex(); //
    private Mutex tableMutex = new Mutex();
    private Semaphore? searchersSemaphore = null; //
    private List<Mutex> rowsMutexes = new List<Mutex>(); //
    private List<Mutex> colsMutexes = new List<Mutex>(); //

    private List<List<string>> sheet;

    // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
    // No locks
    public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
    {
        if (nRows <= 0 || nCols <= 0)
            throw new ArgumentOutOfRangeException("Rows and columns must be positive numbers.");
        rowsNum = nRows;
        colsNum = nCols;
        if (nUsers < -1)
            throw new ArgumentOutOfRangeException("Users number must be a positive number, or -1 if not limited.");
        users = nUsers;
        if (users > 0)
            searchersSemaphore = new Semaphore(0, (int) users);
        for (int i = 0; i < rowsNum; i++)
            rowsMutexes.Add(new Mutex());
        for (int i = 0; i < colsNum; i++)
            colsMutexes.Add(new Mutex());
        sheet = new List<List<string>>(); // construct a nRows*nCols spreadsheet
        for (int i = 0; i < rowsNum; i++)
        {
            sheet.Add(new List<string>());
            for (int j = 0; j < colsNum; j++)
                sheet[i].Add("");
        }
    }

        // return the string at [row,col]
        // Read action
        public string getCell(int row, int col)
    {
        bool valid = checkCell(row, col);
        if (!valid)
            throw new ArgumentOutOfRangeException("Bad parameters");
        enterReadSection();
        string cell = sheet[row][col];
        exitReadSection();
        return cell;
    }

    // set the string at [row,col]
    // Write action
    public void setCell(int row, int col, string str)
    {
        bool valid = checkCell(row, col);
        if (!valid)
            throw new ArgumentOutOfRangeException("Bad parameters");
        enterWriteSection();
        rowsMutexes[row].WaitOne();
        sheet[row][col] = str;
        exitSearchSection();
    }

    public Tuple<int,int> searchString(string str)
    {
        // Read action
        int row, col;
        enterSearchSection();
        // return first cell indexes that contains the string (search from first row to the last row)
        for (int i = 0; i < rowsNum; i++)
        {
            for (int j = 0; j < colsNum; j++)
            {
                //if (string.Equals(sheet[i, j], str))
                if (string.Equals(sheet[i][j], str))
                {
                    row = i;
                    col = j;
                    Tuple<int, int> t = new(row, col);
                    exitSearchSection();
                    return t;
                }
            }
        }
        exitSearchSection();
        throw new Exception(str + " not found.");
    }

    public void exchangeRows(int row1, int row2)
    {
        // Write action, lock the rows
        // exchange the content of row1 and row2
        bool valid1 = checkRow(row1);
        bool valid2 = checkRow(row2);
        if (!valid1 || !valid2)
        {
            throw new Exception("Bad parameters");
        }
        enterWriteSection();
        rowsMutexes[row1].WaitOne();
        rowsMutexes[row2].WaitOne();
        string[] tmp = new string[colsNum];
        for (int i = 0; i < colsNum; i++)
        {
            tmp[i] = sheet[row1][i];
            sheet[row1][i] = sheet[row2][i];
            sheet[row2][i] = tmp[i];
        }
        rowsMutexes[row2].ReleaseMutex();
        rowsMutexes[row1].ReleaseMutex();
        exitSearchSection();
    }

    public void exchangeCols(int col1, int col2)
    {
        // Write action, lock the cols
        // exchange the content of col1 and col2
        bool valid1 = checkCol(col1);
        bool valid2 = checkCol(col2);
        if (!valid1 || !valid2)
        {
            throw new Exception("Bad parameters");
        }
        enterWriteSection();
        colsMutexes[col1].WaitOne();
        colsMutexes[col2].WaitOne();
        string[] tmp = new string[rowsNum];
        for (int i = 0; i < rowsNum; i++)
        {
            tmp[i] = sheet[i][col1];
            sheet[i][col1] = sheet[i][col2];
            sheet[i][col2] = tmp[i];
        }
        colsMutexes[col2].ReleaseMutex();
        colsMutexes[col1].ReleaseMutex();
        exitWriteSection();
    }

    public int searchInRow(int row, string str)
    {
        // Read action
        int col;
        // perform search in specific row
        bool valid = checkRow(row);
        if (!valid)
        {
            throw new Exception("Bad parameters");
        }
        enterSearchSection();
        for (int j = 0; j < colsNum; j++)
        {
            if (string.Equals(sheet[row][j], str))
            {
                col = j;
                exitSearchSection();
                return col;
            }
        }
        exitSearchSection();
        throw new Exception(str + " not found.");
    }

    public int searchInCol(int col, string str)
    {
        // Read action
        int row;
        // perform search in specific col
        bool valid = checkCol(col);
        if (!valid)
        {
            throw new Exception("Bad parameters");
        }
        enterSearchSection();
        for (int i = 0; i < rowsNum; i++)
        {
            //if (string.Equals(sheet[i, col], str))
            if (string.Equals(sheet[i][col], str))
            {
                row = i;
                exitSearchSection();
                return row;
            }
        }
        exitSearchSection();
        throw new Exception(str + " not found.");
    }

    public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, string str)
    {
        // Read action
        enterSearchSection();
        Tuple<int, int> t =  searchInRangeHelper(col1, col2, row1, row2, str, true);
        exitSearchSection();
        if (t == null)
        {
            throw new Exception(str + " not found.");
        }
        return t;
    }

    public void addRow(int row1)
    {
        // Write action, lock rows from row1 (check collision with exchangeRows)
        //add a row after row1
        bool valid = checkRow(row1);
        if (!valid)
        {
            throw new Exception("Bad parameters");
        }
        enterWriteSection();
        for (int i = row1 + 1; i < rowsNum; i++)
        {
            rowsMutexes[i].WaitOne();
        }
        rowsNum++;
        if (row1 + 2 == rowsNum)
            rowsMutexes.Add(new Mutex());
        else
            rowsMutexes.Insert(row1 + 1, new Mutex());
        rowsMutexes[row1 + 1].WaitOne();
        List<string> newRow = new List<string>();
        for (int i = 0; i < colsNum; i++)
        {
            newRow.Add("");
        }
        if (row1 + 2 == rowsNum)
            sheet.Add(newRow);
        else
            sheet.Insert(row1 + 1, newRow);
        for (int i = row1 + 1; i < rowsNum; i++)
        {
            rowsMutexes[i].ReleaseMutex();
        }
        exitWriteSection();
    }

    public void addCol(int col1)
    {
        // Write action, lock cols from col1 (check collision with exchangeCols)
        //add a column after col1
        bool valid = checkCol(col1);
        if (!valid)
        {
            throw new Exception("Bad parameters");
        }
        enterWriteSection();
        for (int i = col1 + 1; i < colsNum; i++)
        {
            colsMutexes[i].WaitOne();
        }
        colsNum++;
        if (col1 + 2 == colsNum)
            colsMutexes.Add(new Mutex());
        else
            colsMutexes.Insert(col1 + 1, new Mutex());
        colsMutexes[col1+1].WaitOne();
        if (col1 + 2 == colsNum)
        {
            for (int i = 0; i < rowsNum; i++)
            {    
                sheet[i].Add("");
            }
        }
        else
        {
            for (int i = 0; i < rowsNum; i++)
            {    
                sheet[i].Insert(col1 + 1, "");
            }
        }
        for (int i = col1 + 1; i < colsNum; i++)
        {
            colsMutexes[i].ReleaseMutex();
        }
        exitWriteSection();
    }

    public Tuple<int, int>[] findAll(string str, bool caseSensitive)
    {
        // Read action (unless ToUpper/ToLower needs a lock)
        // perform search and return all relevant cells according to caseSensitive param
        List<Tuple<int, int>> res = new List<Tuple<int, int>>();
        int r = 0;
        int c = 0;
        enterSearchSection();
        while (r < rowsNum)
        {
            if (searchInRangeHelper(c, colsNum - 1, r, rowsNum - 1, str, caseSensitive) != null)
            {
                res.Add(searchInRangeHelper(c, colsNum-1, r, rowsNum-1, str, caseSensitive));
                r = res[res.Count - 1].Item1;
                c = res[res.Count - 1].Item2 + 1;
                if (c >= colsNum)
                {
                    r++;
                    c = 0;
                }
            }

            else
            {
                break;
            }
        }
        if (res.Count == 0)
        {
            throw new Exception(str + " not found.");
        }
        exitSearchSection();
        return res.ToArray();
    }

    public void setAll(string oldStr, string newStr, bool caseSensitive)
    {
        // Write action, lock relevant cells/their area
        // replace all oldStr cells with the newStr str according to caseSensitive param
        Tuple<int, int>[] tup = findAll(oldStr, caseSensitive);
        int r;
        int c;
        for (int i = 0; i < tup.Length; i++)
        {
            r = tup[i].Item1;
            c = tup[i].Item2;
            setCell(r, c, newStr);
        }
    }

    public Tuple<int, int> getSize()
    {
        // Read action
        enterReadSection();
        int nRows, nCols;
        nRows = rowsNum;
        nCols = colsNum;
        exitReadSection();
        // return the size of the spreadsheet in nRows, nCols
        return new Tuple<int, int>(nRows, nCols);
    }

    public void setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.
        // This function is used just in the creation
        Interlocked.CompareExchange(ref users, users, nUsers);
    }

    public void save(string fileName)
    {
        // Read action
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
        using (StreamWriter sw = File.CreateText("Saved.txt"))
        {
            enterReadSection();
            sw.WriteLine(rowsNum);
            sw.WriteLine(colsNum);
            sw.WriteLine(users);
            sw.WriteLine(searches);
            for (int i = 0; i < rowsNum; i++)
            {
                for (int j = 0; j < colsNum; j++)
                {
                    sw.WriteLine(sheet[i][j]);
                }
            }
            exitReadSection();
        }
    }

    public void load(string fileName)
    {
        // Write action-lock the entire spreadsheet
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
        readWriteMutex.WaitOne();
        try
        {
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader("Saved.txt"))
            {
                rowsNum = int.Parse(sr.ReadLine());
                colsNum = int.Parse(sr.ReadLine());
                users = int.Parse(sr.ReadLine());
                searches = int.Parse(sr.ReadLine());
                sheet = new List<List<string>>();

                for (int i = 0; i < rowsNum; i++)
                {
                    sheet.Add(new List<string>());
                    for (int j = 0; j < colsNum; j++)
                    {
                        sheet[i].Add(sr.ReadLine());
                    }
                }
            }
        }

        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
        readWriteMutex.ReleaseMutex();
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

    private Tuple<int, int> searchInRangeHelper(int col1, int col2, int row1, int row2, string str, bool _case)
    {
        // Read action
        // Check bounds
        // Needs fixing on loops
        bool valid1 = checkCell(row1, col1);
        if (!valid1)
        {
            throw new Exception("Bad parameters");
        }
        bool valid2 = checkCell(row2, col2);
        if (!valid2)
        {
            throw new Exception("Bad parameters");
        }
        if (row1 > row2 || (row1 == row2 && col1 > col2))
        {
            throw new Exception("Bad parameters");
        }

        if (users != -1)
        {
            while (searches >= users)
            {
                continue;
            }
        }
        searches++;

        int row, col;
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2
        if (row2 > row1)
        {
            for (int i = row1; i < row1 + 1; i++)
            {
                for (int j = col1; j < colsNum; j++)
                {
                    //if (string.Equals(sheet[i, j], str))
                    //if (string.Equals(sheet[i][j], str))
                    if (caseEquals(sheet[i][j], str, _case))
                    {
                        row = i;
                        col = j;
                        Tuple<int, int> t = new(row, col);
                        searches--;
                        return t;
                    }
                }
            }

            for (int i = row1 + 1; i < row2; i++)
            {
                for (int j = 0; j < colsNum; j++)
                {
                    //if (string.Equals(sheet[i, j], str))
                    //if (string.Equals(sheet[i][j], str))
                    if (caseEquals(sheet[i][j], str, _case))
                    {
                        row = i;
                        col = j;
                        Tuple<int, int> t = new(row, col);
                        searches--;
                        return t;
                    }
                }
            }

            for (int i = row2; i < row2 + 1; i++)
            {
                for (int j = 0; j < col2 + 1; j++)
                {
                    //if (string.Equals(sheet[i, j], str))
                    //if (string.Equals(sheet[i][j], str))
                    if (caseEquals(sheet[i][j], str, _case))
                    {
                        row = i;
                        col = j;
                        Tuple<int, int> t = new(row, col);
                        searches--;
                        return t;
                    }
                }
            }
        }
        else
        {
            for (int i = row1; i < row2+1; i++)
            {
                for (int j = col1; j < col2+1; j++)
                {
                    if (caseEquals(sheet[i][j], str, _case))
                    {
                        row = i;
                        col = j;
                        Tuple<int, int> t = new(row, col);
                        searches--;
                        return t;
                    }
                }
            }
        }
        searches--;
        return null;
    }

    private bool caseEquals(string s1, string s2, bool _case)
    {
        if (_case)
        {
            return string.Equals(s1, s2);
        }

        return string.Equals(s1.ToLower(), s2.ToLower());
    }

    private void enterReadSection()
    {
        if (Interlocked.Increment(ref readers) == 1)
            readWriteMutex.WaitOne();
    }

    private void exitReadSection()
    {
        if (Interlocked.Decrement(ref readers) == 0)
            readWriteMutex.ReleaseMutex();
    }

    private void enterSearchSection()
    {
        if (users != -1)
            searchersSemaphore.WaitOne();
        enterReadSection();
    }

    private void exitSearchSection()
    {
        if (users != -1)
            searchersSemaphore.Release();
        exitReadSection();
    }
    private void enterWriteSection()
    {
        if (Interlocked.Increment(ref writers) == 1)
            readWriteMutex.WaitOne();
    }

    private void exitWriteSection()
    {
        if (Interlocked.Decrement(ref writers) == 0)
            writersMutex.ReleaseMutex();
    }
}