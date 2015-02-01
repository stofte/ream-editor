create table Foo (
    Id int PRIMARY KEY,
    Description Text
);

-- sqlserver types: https://msdn.microsoft.com/en-us/library/ms187752.aspx
create table TypeTestTable (
    -- Exact Numerics
    bigintcol bigint,
    bitcol bit,
    decimalcol decimal,
    intcol int,
    moneycol money,
    numericcol numeric,
    smallintcol smallint,
    smallmoneycol smallmoney,
    tinyintcol tinyint,
    -- Approximate Numerics
    floatcol float,
    realcol real,
    -- Date and Time
    datecol date,
    datetime2col datetime2,
    datetimecol datetime,
    datetimeoffsetcol datetimeoffset,
    smalldatetimecol smalldatetime,
    timecol time,
    -- Character Strings
    charcol char,
    textcol text,
    varcharcol varchar,
    -- Unicode Character Strings
    ncharcol nchar,
    ntextcol ntext,
    nvarcharcol nvarchar,
    -- Binary Strings
    binarycol binary,
    imagecol image,
    varbinarycol varbinary,
    -- Other Data Types
    timestampcol timestamp,
    uniqueidentifiercol uniqueidentifier
);