USE RRD;

CREATE TABLE Employee (
    Employee_Id VARCHAR(15) PRIMARY KEY NOT NULL,
    First_Name VARCHAR(15) NOT NULL,
    Last_Name VARCHAR(15) NOT NULL,
    NIC_Number VARCHAR(10) NOT NULL UNIQUE,
    DOB DATE,
    Age INT,
    Gender VARCHAR(8),
    Employee_Address VARCHAR(50) NOT NULL,
    Designation VARCHAR(40),
    Email VARCHAR(50) NOT NULL,
    TP VARCHAR(15) NOT NULL,
    Joining_Date DATE
);

-- Table to store employee login information
CREATE TABLE Employee_Login_Info (
    Employee_LoginId VARCHAR(15) PRIMARY KEY NOT NULL,
    User_Password VARCHAR(15) NOT NULL,
    FOREIGN KEY (Employee_LoginId) REFERENCES Employee(Employee_Id)
);

-- Table to store deleted employee details
CREATE TABLE DeletedEmployee (
    Employee_Id VARCHAR(15) PRIMARY KEY NOT NULL,
    First_Name VARCHAR(15) NOT NULL,
    Last_Name VARCHAR(15) NOT NULL,
    NIC_Number VARCHAR(10) NOT NULL,
    Deletion_Date DATETIME
);



-- Table to store employee login information
CREATE TABLE Employee_Login_Info (
    Employee_LoginId VARCHAR(15) PRIMARY KEY NOT NULL,
    User_Password VARCHAR(15) NOT NULL,
    FOREIGN KEY (Employee_LoginId) REFERENCES Employee(Employee_Id)
);

-- Table to store deleted employee details
CREATE TABLE DeletedEmployee (
    Employee_Id VARCHAR(15) PRIMARY KEY NOT NULL,
    First_Name VARCHAR(15) NOT NULL,
    Last_Name VARCHAR(15) NOT NULL,
    NIC_Number VARCHAR(10) NOT NULL,
    Deletion_Date DATETIME
);

CREATE TABLE EmployeeUpdate (
    UpdateId INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId VARCHAR(15) NOT NULL,
    UpdateDetails NVARCHAR(MAX),
    UpdateDateTime DATETIME
);

ALTER TABLE Employee_Login_Info
ALTER COLUMN User_Password VARCHAR(64);


CREATE TABLE LeaveRequests (
    Id varchar(30) PRIMARY KEY,
    EmployeeId varchar(30),
    LeaveType VARCHAR(50),
    FromDate DATE,
    ToDate DATE,
    Reason NVARCHAR(MAX),
    Status VARCHAR(20),
    ManagerComment NVARCHAR(MAX)
);

CREATE TABLE AnnualLeave (
    LeaveId INT PRIMARY KEY IDENTITY(1,1),
    Employee_Id VARCHAR(15) NOT NULL,
    LeaveType VARCHAR(20) NOT NULL,
    LeaveBalance INT NOT NULL,
    LastUpdated DATE NOT NULL,
    CONSTRAINT FK_Employee_Id FOREIGN KEY (Employee_Id) REFERENCES Employee (Employee_Id)
);

CREATE TRIGGER trg_AutoInsertAnnualLeave
ON Employee
AFTER INSERT
AS
BEGIN
    -- Insert the Annual Leave data for the newly inserted employee
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Annual', 20, GETDATE() FROM INSERTED;

    -- Insert the Sick Leave data for the newly inserted employee
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Sick', 14, GETDATE() FROM INSERTED;

    -- Insert the Personal Leave data for the newly inserted employee
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Personal', 5, GETDATE() FROM INSERTED;

    -- Insert the Special Leave data for the newly inserted employee
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Special', 2, GETDATE() FROM INSERTED;
END;


CREATE PROCEDURE dbo.UpdateLeaveBalances
AS
BEGIN
    -- Update Annual Leaves
    UPDATE AnnualLeave
    SET LeaveBalance = 20, -- Set the initial balance for Annual Leave
        LastUpdated = DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()) + 1, -1) + ' 23:59:59'
    WHERE LeaveType = 'Annual';

    -- Update Sick Leaves
    UPDATE AnnualLeave
    SET LeaveBalance = 14, -- Set the initial balance for Sick Leave
        LastUpdated = DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()) + 1, -1) + ' 23:59:59'
    WHERE LeaveType = 'Sick';

    -- Update Personal Leaves
    UPDATE AnnualLeave
    SET LeaveBalance = 5, -- Set the initial balance for Personal Leave
        LastUpdated = DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()) + 1, -1) + ' 23:59:59'
    WHERE LeaveType = 'Personal';

    -- Update Special Leaves
    UPDATE AnnualLeave
    SET LeaveBalance = 2, -- Set the initial balance for Special Leave
        LastUpdated = DATEADD(YEAR, DATEDIFF(YEAR, 0, GETDATE()) + 1, -1) + ' 23:59:59'
    WHERE LeaveType = 'Special';
END;



-- Add an Identity column to auto-generate sequential numbers for Id
ALTER TABLE LeaveRequests
ADD IdCounter INT IDENTITY(1, 1);

-- Add a computed column to generate the final Id in the required format
ALTER TABLE LeaveRequests
ADD GeneratedId AS CONCAT('RRD_', RIGHT('000' + CAST(IdCounter AS VARCHAR(3)), 3), '_Leave');

-- Remove the computed column
ALTER TABLE LeaveRequests
DROP COLUMN GeneratedId;

-- Drop the IdCounter column
ALTER TABLE LeaveRequests
DROP COLUMN IdCounter;


INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
SELECT Employee_Id, 'Annual', 20, GETDATE() FROM Employee;

INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
SELECT Employee_Id, 'Sick', 14, GETDATE() FROM Employee;

INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
SELECT Employee_Id, 'Personal', 5, GETDATE() FROM Employee;

INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
SELECT Employee_Id, 'Special', 2, GETDATE() FROM Employee;



ALTER TRIGGER trg_AutoInsertAnnualLeave
ON Employee
AFTER INSERT
AS
BEGIN
    -- Insert the Annual Leave data for the newly inserted employees
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Annual', 20, GETDATE() FROM INSERTED;

    -- Insert the Sick Leave data for the newly inserted employees
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Sick', 14, GETDATE() FROM INSERTED;

    -- Insert the Personal Leave data for the newly inserted employees
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Personal', 5, GETDATE() FROM INSERTED;

    -- Insert the Special Leave data for the newly inserted employees
    INSERT INTO AnnualLeave (Employee_Id, LeaveType, LeaveBalance, LastUpdated)
    SELECT Employee_Id, 'Special', 2, GETDATE() FROM INSERTED;
END;

DROP TRIGGER trg_AutoInsertAnnualLeave;


DELETE FROM AnnualLeave;
DELETE FROM Employee;
DELETE FROM DeletedEmployee;
DELETE FROM EmployeeUpdate;

CREATE TABLE SystemSettings (
    Id INT PRIMARY KEY,
    LastLeaveId VARCHAR(50)
);


BEGIN TRANSACTION;

DECLARE @IDToDelete VARCHAR(30) = 'RRD001_eLeave'; -- Replace this with the ID you want to delete

-- Delete the record from LeaveRequests table
DELETE FROM LeaveRequests WHERE Id = @IDToDelete;

-- Delete the record from AnnualLeave table
DELETE FROM SystemSettings WHERE Id = LastLeaveId;

COMMIT TRANSACTION;

ALTER TABLE AnnualLeave
ADD CONSTRAINT FK_AnnualLeave_Employee
FOREIGN KEY (Employee_Id)
REFERENCES Employee(Employee_Id)
ON DELETE CASCADE;


drop table Employee_Attendance;


CREATE TABLE Employee_Attendance (
    Attendance_Id NVARCHAR(100) PRIMARY KEY,
    Employee_Id NVARCHAR(100) NOT NULL,
    Login_Time DATETIME NOT NULL,
    Logout_Time DATETIME NULL
);





