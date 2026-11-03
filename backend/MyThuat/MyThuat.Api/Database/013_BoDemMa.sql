IF OBJECT_ID(N'dbo.BoDemMa',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BoDemMa (
        TienTo varchar(20) NOT NULL CONSTRAINT PK_BoDemMa PRIMARY KEY,
        GiaTriCuoi bigint NOT NULL CONSTRAINT CK_BoDemMa_KhongAm CHECK (GiaTriCuoi >= 0)
    );
END;
