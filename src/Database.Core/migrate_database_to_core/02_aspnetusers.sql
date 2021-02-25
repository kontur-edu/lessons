BEGIN TRANSACTION


-- AspNetRoles

ALTER TABLE AspNetRoles
ADD [ConcurrencyStamp] [varchar](max) NULL

ALTER TABLE AspNetRoles 
ADD [NormalizedName] [varchar](256) NULL

-- AspNetUsers

ALTER TABLE AspNetUsers
ADD [ConcurrencyStamp] [varchar](max) NULL

ALTER TABLE AspNetUsers
ADD [LockoutEnd] [datetimeoffset](7) NULL

ALTER TABLE AspNetUsers
ADD [NormalizedEmail] [varchar](256) NULL

ALTER TABLE AspNetUsers
ADD [NormalizedUserName] [varchar](256) NULL



COMMIT TRANSACTION