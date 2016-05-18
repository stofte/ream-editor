use testdb;

create table Foo (
    Id int PRIMARY KEY,
    Description Text
);

-- sqlserver types: https://msdn.microsoft.com/en-us/library/ms187752.aspx
create table TypeTestTable (
    -- Exact Numerics
    bigintcol bigint,
    bitcol bit,
    decimalcol decimal(19, 1),
    intcol int,
    moneycol money,
    numericcol numeric(19, 1),
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
    charcol char(6),
    varcharcol varchar(6),
    varcharmaxcol varchar(max),
    textcol text,
    -- Unicode Character Strings
    ncharcol nchar(3),
    nvarcharcol nvarchar(3),
    ntextcol ntext,
    
    -- skipping image/xml types for now
    binarycol binary(8),
    varbinarycol varbinary(8),
    varbinarymaxcol varbinary(max),
    --imagecol image,
    --xmlcol xml

    -- Other Data Types
    rowversioncol rowversion,
    uniqueidentifiercol uniqueidentifier
);
