-- Nullable for existing accounts: never infer a birth date from another student.
SET XACT_ABORT ON;
IF COL_LENGTH('dbo.ThanhVien','NgaySinh') IS NULL
    ALTER TABLE dbo.ThanhVien ADD NgaySinh date NULL;
