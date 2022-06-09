class Program
{
    static public void Main(String[] args)
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