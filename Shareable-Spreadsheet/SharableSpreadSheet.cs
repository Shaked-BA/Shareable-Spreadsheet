using System;
class SharableSpreadSheet
{
    public SharableSpreadSheet(int nRows, int nCols, int nUsers=-1)
    {
        // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
        // construct a nRows*nCols spreadsheet
    }
    public String getCell(int row, int col)
    {
        // return the string at [row,col]
        return "";
    }
    public void setCell(int row, int col, String str)
    {
        // set the string at [row,col]
   
    }
    public Tuple<int,int> searchString(String str)
    {
        int row, col;
        // return first cell indexes that contains the string (search from first row to the last row)
        return <row, col>;
    }
    public void exchangeRows(int row1, int row2)
    {
        // exchange the content of row1 and row2
    }
    public void exchangeCols(int col1, int col2)
    {
        // exchange the content of col1 and col2
    }
    public int searchInRow(int row, String str)
    {
        int col;
        // perform search in specific row
        return col;
    }
    public int searchInCol(int col, String str)
    {
        int row;
        // perform search in specific col
        return row;
    }
    public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)
    {
        int row,col
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2
        return <row,col>;
    }
    public void addRow(int row1)
    {
        //add a row after row1
    }
    public void addCol(int col1)
    {
        //add a column after col1
    }
    public Tuple<int, int>[] findAll(String str,bool caseSensitive)
    {
        // perform search and return all relevant cells according to caseSensitive param
    }
    public void setAll(String oldStr, String newStr bool caseSensitive)
    {
        // replace all oldStr cells with the newStr str according to caseSensitive param
    }
    public Tuple<int, int> getSize()
    {
        int nRows, int nCols;
        // return the size of the spreadsheet in nRows, nCols
        return<nRows,nCols>;
    }
    public void setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.
        // This function is used just in the creation
    }

    public void save(String fileName)
    {
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
    }
    public void load(String fileName)
    {
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
    }
}



