using System;
class SharableSpreadSheet
{
    private int rowsNum;
    private int colsNum;
    private int lockDelta;
    private int users;
    private volatile int searches; // Volatile because needs to be updated outside of function scope
    private List<List<String>> sheet;
    //private String[,] sheet;
   
    public SharableSpreadSheet(int nRows, int nCols, int nUsers=-1)
    {
        // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
        // No lock
        rowsNum = nRows;
        colsNum = nCols;
        lockDelta = (int) Math.Sqrt(rowsNum+colsNum);
        users = nUsers;
        searches = 0;
        // construct a nRows*nCols spreadsheet
        //sheet = new String[rowsNum, colsNum];
        sheet = new List<List<String>>();
        for (int i = 0; i < rowsNum; i++)
        {
            sheet.Add(new List<String>());
            for (int j = 0; j < colsNum; j++)
            {
                sheet[i][j] = "";
            }
        }
    }

    public String getCell(int row, int col)
    {
        // return the string at [row,col]
        // Read action
        bool valid = checkCell(row, col);
        if (!valid)
        {
            throw new Exception("Bad parameters");
        }
        //String cell = sheet[row, col];
        String cell = sheet[row][col];
        return cell;
    }

    public void setCell(int row, int col, String str)
    {
        // set the string at [row,col]
        // Write action
        bool valid = checkCell(row, col);
        if (!valid)
        {
            throw new Exception("Bad parameters");
        }
        //sheet[row, col] = str;
        sheet[row][col] = str;
    }

    public Tuple<int,int> searchString(String str)
    {
        // Read action
        int row, col;
        if (users != -1)
        {
            while (searches >= users)
            {
                continue;
            }
        }
        searches++;
        // return first cell indexes that contains the string (search from first row to the last row)
        for (int i = 0; i < rowsNum; i++)
        {
            for (int j = 0; j < colsNum; j++)
            {
                //if (String.Equals(sheet[i, j], str))
                if (String.Equals(sheet[i][j], str))
                {
                    row = i;
                    col = j;
                    Tuple<int, int> t = new(row, col);
                    searches--;
                    return t;
                }
            }
        }
        searches--;
        return null;
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

        String[] tmp = new string[colsNum];
        for (int i = 0; i < colsNum; i++)
        {
            //tmp[i] = sheet[row1, i];
            //sheet[row1, i] = sheet[row2, i];
            //sheet[row2, i] = tmp[i];
            tmp[i] = sheet[row1][i];
            sheet[row1][i] = sheet[row2][i];
            sheet[row2][i] = tmp[i];
        }
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

        String[] tmp = new string[rowsNum];
        for (int i = 0; i < rowsNum; i++)
        {
            //tmp[i] = sheet[i, col1];
            //sheet[i, col1] = sheet[i, col2];
            //sheet[i, col2] = tmp[i];
            tmp[i] = sheet[i][col1];
            sheet[i][col1] = sheet[i][col2];
            sheet[i][col2] = tmp[i];
        }
    }

    public int searchInRow(int row, String str)
    {
        // Read action
        int col;
        // perform search in specific row
        bool valid = checkRow(row);
        if (!valid)
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

        for (int j = 0; j < colsNum; j++)
        {
            //if (String.Equals(sheet[row, j], str))
            if (String.Equals(sheet[row][j], str))
            {
                col = j;
                searches--;
                return col;
            }
        }
        searches--;
        return -1;
    }

    public int searchInCol(int col, String str)
    {
        // Read action
        int row;
        // perform search in specific col
        bool valid = checkCol(col);
        if (!valid)
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

        for (int i = 0; i < rowsNum; i++)
        {
            //if (String.Equals(sheet[i, col], str))
            if (String.Equals(sheet[i][col], str))
            {
                row = i;
                searches--;
                return row;
            }
        }
        searches--;
        return -1;
    }

    public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
    {
        // Read action
        return searchInRangeHelper(col1, col2, row1, row2, str, true);
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

        rowsNum++;
        sheet.Add(new List<string>());
        for (int j = 0; j < colsNum; j++)
        {
            sheet[rowsNum - 1][j] = "";
        }

        for (int i = rowsNum-1; i > row1+1; i--)
        {
            exchangeRows(i, i - 1);
        }
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

        colsNum++;
        for (int i = 0; i < rowsNum; i++)
        {
            sheet[i][colsNum-1] = "";
        }

        for (int j = colsNum - 1; j > col1 + 1; j--)
        {
            exchangeCols(j, j - 1);
        }
    }

    public Tuple<int, int>[] findAll(String str, bool caseSensitive)
    {
        // Read action (unless ToUpper/ToLower needs a lock)
        // perform search and return all relevant cells according to caseSensitive param
        List<Tuple<int, int>> res = new List<Tuple<int, int>>();
        int r = 0;
        int c = 0;
        while (r < rowsNum && c < colsNum)
        {
            if (searchInRangeHelper(r, c, rowsNum, colsNum, str, caseSensitive) != null)
            {
                res.Append(searchInRangeHelper(r, c, rowsNum, colsNum, str, caseSensitive));
                r = res[res.Count - 1].Item1;
                c = res[res.Count - 1].Item2;
            }

            else
            {
                break;
            }
        }
        return res.ToArray();
    }

    public void setAll(String oldStr, String newStr, bool caseSensitive)
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
        int nRows, nCols;
        nRows = rowsNum;
        nCols = colsNum;
        // return the size of the spreadsheet in nRows, nCols
        return new Tuple<int, int>(nRows, nCols);
    }

    public void setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.
        // This function is used just in the creation
        users = nUsers;
    }

    public void save(String fileName)
    {
        // Read action
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
        using (StreamWriter sw = File.CreateText("Saved.txt"))
        {
            sw.WriteLine(rowsNum);
            sw.WriteLine(colsNum);
            sw.WriteLine(lockDelta);
            sw.WriteLine(users);
            sw.WriteLine(searches);
            for (int i = 0; i < rowsNum; i++)
            {
                for (int j = 0; j < colsNum; j++)
                {
                    sw.WriteLine(sheet[i][j]);
                }
            }
        }
    }

    public void load(String fileName)
    {
        // Write action-lock the entire spreadsheet
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
        try
        {
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader("Saved.txt"))
            {
                rowsNum = int.Parse(sr.ReadLine());
                colsNum = int.Parse(sr.ReadLine());
                lockDelta = int.Parse(sr.ReadLine());
                users = int.Parse(sr.ReadLine());
                searches = int.Parse(sr.ReadLine());
                sheet = new List<List<String>>();

                for (int i = 0; i < rowsNum; i++)
                {
                    for (int j = 0; j < colsNum; j++)
                    {
                        sheet[i][j] = sr.ReadLine();
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
    }

    public bool checkCell(int row, int col)
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

    public Tuple<int, int> searchInRangeHelper(int col1, int col2, int row1, int row2, String str, bool _case)
    {
        // Read action
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
        if (row1 >= row2 || col1 >= col2)
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
        for (int i = row1; i < row2; i++)
        {
            for (int j = col1; j < col2; j++)
            {
                //if (String.Equals(sheet[i, j], str))
                //if (String.Equals(sheet[i][j], str))
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
        searches--;
        return null;
    }

    public bool caseEquals(String s1, String s2, bool _case)
    {
        if (_case)
        {
            return String.Equals(s1, s2);
        }

        return String.Equals(s1.ToLower(), s2.ToLower());
    }
}



