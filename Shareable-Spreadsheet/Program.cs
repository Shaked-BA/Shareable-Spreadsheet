class Program
{
    //static public void Main(String[] args)
    public void oldTest()
    {
        SharableSpreadSheet sss = new SharableSpreadSheet(5, 5);
        //getCell
        Console.WriteLine(sss.getCell(3, 4));
        //setCell
        sss.setCell(2, 1, "{2,1}");
        sss.setCell(3, 1, "{3,1}");
        sss.setCell(4, 1, "{4,1}");
        sss.setCell(4, 3, "{2,1}");
        //sss.setCell(5, 8, "{5,8}");
        //searchString
        //Console.WriteLine(sss.searchString("{2,2}").ToString());
        Console.WriteLine(sss.searchString("{2,1}").ToString());
        //exchangeRows
        sss.exchangeRows(2, 3);
        Console.WriteLine(sss.getCell(3, 1));
        //exchangeCols
        sss.exchangeCols(1, 4);
        Console.WriteLine(sss.getCell(3, 4));
        //searchInRow
        Console.WriteLine(sss.searchInRow(3, "{2,1}").ToString());
        //searchInCol
        Console.WriteLine(sss.searchInCol(4, "{2,1}").ToString());
        //searchInRange
        Console.WriteLine(sss.searchInRange(3, 4, 3, 4, "{2,1}").ToString());
        //addRow
        sss.addRow(1);
        //addCol
        sss.addCol(4);
        //findAll
        Console.WriteLine(sss.findAll("{2,1}", true).ElementAt(0).ToString());
        Console.WriteLine(sss.findAll("{2,1}", true).ElementAt(1).ToString());
        //setAll
        sss.setAll("{2,1}", "Hello", true);
        Console.WriteLine(sss.findAll("hello", false).ElementAt(0).ToString());
        Console.WriteLine(sss.findAll("hello", false).ElementAt(1).ToString());
        //setConcurrentSearchLimit
        //getSize
        //save
        //load
    }
}

class Test
{
    private static void startGetCell(SharableSpreadSheet sheet, int row, int col)
    {
        sheet.getCell(row, col);
    }
    private static void startSetCell(SharableSpreadSheet sheet, int row, int col, string str)
    {
        sheet.setCell(row, col, str);
    }
    private static void startSearchString(SharableSpreadSheet sheet, string str)
    {
        sheet.searchString(str);
    }
    private static void startExchangeRows(SharableSpreadSheet sheet, int row1, int row2)
    {
        sheet.exchangeRows(row1, row2);
    }
    private static void startExchangeCols(SharableSpreadSheet sheet, int col1, int col2)
    {
        sheet.exchangeCols(col1, col2);
    }
    private static void startSearchInRow(SharableSpreadSheet sheet, int row, string str)
    {
        Console.WriteLine(sheet.searchInRow(row, str).ToString());
    }
    private static void startSearchInCol(SharableSpreadSheet sheet, int col, string str)
    {
        Console.WriteLine(sheet.searchInCol(col, str).ToString());
    }
    private static void startSearchInRange(SharableSpreadSheet sheet, int col1, int col2, int row1, int row2, String str)
    {
        Console.WriteLine(sheet.searchInRange(col1, col2, row1, row2, str).ToString());
    }
    private static void startAddRow(SharableSpreadSheet sheet, int row)
    {
        sheet.addRow(row);
    }
    private static void startAddCol(SharableSpreadSheet sheet, int col)
    {
        sheet.addCol(col);
    }
    private static void startFindAll(SharableSpreadSheet sheet, string str, bool caseSensitive)
    {
        Tuple<int, int>[] res = sheet.findAll(str, caseSensitive);
        String resStr = "";
        foreach (Tuple<int, int> t in res)
            resStr += t.ToString() + " ";
        Console.WriteLine(resStr);
    }
    private static void startSetAll(SharableSpreadSheet sheet, string oldStr, string newStr, bool caseSensitive)
    {
        sheet.setAll(oldStr, newStr, caseSensitive);
    }
    private static void startGetSize(SharableSpreadSheet sheet)
    {
        Console.WriteLine(sheet.getSize().ToString());
    }
    private static void startSetConcurrentSearchLimit(SharableSpreadSheet sheet, int nUsers)
    {
        sheet.setConcurrentSearchLimit(nUsers);
    }
    private static void startSave(SharableSpreadSheet sheet, string fileName)
    {
        sheet.save(fileName);
    }
    private static void startLoad(SharableSpreadSheet sheet, string fileName)
    {
        sheet.load(fileName);
    }
    public void getCell(SharableSpreadSheet sheet, int row, int col)
    {
        Thread thread = new Thread(() => startGetCell(sheet, row, col));
        thread.Start();
    }
    public void setCell(SharableSpreadSheet sheet, int row, int col, string str)
    {
        Thread thread = new Thread(() => startSetCell(sheet, row, col, str));
        thread.Start();
    }
    public void searchString(SharableSpreadSheet sheet, string str)
    {
        Thread thread = new Thread(() => startSearchString(sheet, str));
        thread.Start();
    }
    public void exchangeRows(SharableSpreadSheet sheet, int row1, int row2)
    {
        Thread thread = new Thread(() => startExchangeRows(sheet, row1, row2));
        thread.Start();
    }
    public void exchangeCols(SharableSpreadSheet sheet, int col1, int col2)
    {
        Thread thread = new Thread(() => startExchangeCols(sheet, col1, col2));
        thread.Start();
    }
    public void searchInRow(SharableSpreadSheet sheet, int row, string str)
    {
        Thread thread = new Thread(() => startSearchInRow(sheet, row, str));
        thread.Start();
    }
    public void searchInCol(SharableSpreadSheet sheet, int col, string str)
    {
        Thread thread = new Thread(() => startSearchInCol(sheet, col, str));
        thread.Start();
    }
    public void searchInRange(SharableSpreadSheet sheet, int col1, int col2, int row1, int row2, String str)
    {
        Thread thread = new Thread(() => startSearchInRange(sheet, col1, col2, row1, row2, str));
        thread.Start();
    }
    public void addRow(SharableSpreadSheet sheet, int row)
    {
        Thread thread = new Thread(() => startAddRow(sheet, row));
        thread.Start();
    }
    public void addCol(SharableSpreadSheet sheet, int col)
    {
        Thread thread = new Thread(() => startAddCol(sheet, col));
        thread.Start();
    }
    public void findAll(SharableSpreadSheet sheet, String str, bool caseSensitive)
    {
        Thread thread = new Thread(() => startFindAll(sheet, str, caseSensitive));
        thread.Start();
    }
    public void setAll(SharableSpreadSheet sheet, string oldStr, string newStr, bool caseSensitive)
    {
        Thread thread = new Thread(() => startSetAll(sheet, oldStr, newStr, caseSensitive));
        thread.Start();
    }
    public void getSize(SharableSpreadSheet sheet)
    {
        Thread thread = new Thread(() => startGetSize(sheet));
        thread.Start();
    }
    public void setConcurrentSearchLimit(SharableSpreadSheet sheet, int nUsers)
    {
        Thread thread = new Thread(() => startSetConcurrentSearchLimit(sheet, nUsers));
        thread.Start();
    }
    public void save(SharableSpreadSheet sheet, string fileName)
    {
        Thread thread = new Thread(() => startSave(sheet, fileName));
        thread.Start();
    }
    public void load(SharableSpreadSheet sheet, string fileName)
    {
        Thread thread = new Thread(() => startLoad(sheet, fileName));
        thread.Start();
    }
    private void test()
    {
        int rows = 20, cols = 20;
        SharableSpreadSheet sheet = new SharableSpreadSheet(rows, cols);
        Test test = new Test();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
                test.setCell(sheet, r, c, r.ToString() + "," + c.ToString());
        }
        test.searchInCol(sheet, 5, "5,5");
        test.searchInRow(sheet, 7, "7,7");
        test.searchInRange(sheet, 5, 7, 4, 8, "6,6");
        test.exchangeCols(sheet, 1, 2);
        test.exchangeRows(sheet, 1, 2);
        test.exchangeCols(sheet, 2, 3);
        test.exchangeCols(sheet, 3, 4);
        test.exchangeRows(sheet, 2, 3);
        test.exchangeRows(sheet, 3, 4);
        test.addRow(sheet, 10);
        test.addRow(sheet, 11);
        test.addCol(sheet, 0);
        test.addRow(sheet, 12);
        test.addRow(sheet, 13);
        test.searchInRange(sheet, 5, 7, 4, 8, "7,7");
        test.addRow(sheet, 14);
        test.addRow(sheet, 15);
        test.searchInCol(sheet, 5, "5,5");
        test.searchInRow(sheet, 7, "7,7");
        test.searchInRange(sheet, 5, 7, 4, 8, "6,6");
        test.exchangeCols(sheet, 1, 2);
        test.exchangeRows(sheet, 1, 2);
        test.exchangeCols(sheet, 2, 3);
        test.setAll(sheet, "6,6", "99,99", false);
        test.exchangeCols(sheet, 3, 4);
        test.exchangeRows(sheet, 2, 3);
        test.exchangeRows(sheet, 3, 4);
        test.addRow(sheet, 10);
        test.addRow(sheet, 11);
        test.addCol(sheet, 0);
        test.addRow(sheet, 12);
        test.addRow(sheet, 13);
        test.searchInRange(sheet, 5, 7, 4, 8, "7,7");
        test.addRow(sheet, 14);
        test.addRow(sheet, 15);
        test.getSize(sheet);
        test.findAll(sheet, "1,1", false);
        test.save(sheet, "C:/Users/shake/Desktop/sheet.txt");
        test.addRow(sheet, 14);
        test.addRow(sheet, 15);
        test.getSize(sheet);
        test.findAll(sheet, "1,1", false);
        test.load(sheet, "C:/Users/shake/Desktop/sheet.txt");
        test.getCell(sheet, 5, 7);
        test.searchInCol(sheet, 5, "5,5");
        test.searchInRow(sheet, 7, "7,7");
        test.searchInRange(sheet, 5, 7, 4, 8, "99,99");
        test.exchangeCols(sheet, 1, 2);
        test.exchangeRows(sheet, 1, 2);
        test.exchangeCols(sheet, 2, 3);
        test.exchangeCols(sheet, 3, 4);
        test.exchangeRows(sheet, 2, 3);
        test.exchangeRows(sheet, 3, 4);
        test.addRow(sheet, 10);
        test.addRow(sheet, 11);
        test.addCol(sheet, 0);
        test.addRow(sheet, 12);
        test.addRow(sheet, 13);
        test.searchInRange(sheet, 5, 7, 4, 8, "7,7");
        test.addRow(sheet, 14);
        test.addRow(sheet, 15);
        test.searchInCol(sheet, 5, "5,5");
        test.searchInRow(sheet, 7, "7,7");
        test.searchInRange(sheet, 5, 7, 4, 8, "99,99");
        test.exchangeCols(sheet, 1, 2);
        test.exchangeRows(sheet, 1, 2);
        test.exchangeCols(sheet, 2, 3);
        test.setAll(sheet, "7,7", "99,99", false);
        test.exchangeCols(sheet, 3, 4);
        test.exchangeRows(sheet, 2, 3);
        test.exchangeRows(sheet, 3, 4);
        test.addRow(sheet, 10);
        test.addRow(sheet, 11);
        test.addCol(sheet, 0);
        test.addRow(sheet, 12);
        test.addRow(sheet, 13);
        test.searchInRange(sheet, 5, 7, 4, 8, "99,99");
        test.addRow(sheet, 14);
        test.addRow(sheet, 15);
        test.getSize(sheet);
        test.findAll(sheet, "1,1", false);
        test.save(sheet, "C:/Users/shake/Desktop/sheet.txt");
        test.addRow(sheet, 14);
        test.addRow(sheet, 15);
        test.getSize(sheet);
        test.findAll(sheet, "1,1", false);
        test.load(sheet, "C:/Users/shake/Desktop/sheet.txt");
        test.getCell(sheet, 5, 7);
        sheet.printSheet();
    }
}