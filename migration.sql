CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `EventTemplate` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ShowAs` int NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_EventTemplate` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `Users` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Username` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DisplayName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Alias` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AliasMatchingType` int NOT NULL,
        `CalendarName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Enabled` tinyint(1) NOT NULL,
        `CanBeSource` tinyint(1) NOT NULL,
        `AccessToken` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RefreshToken` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `EventTemplateSet` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DifferentiateOnAttendance` tinyint(1) NOT NULL,
        `AttendingId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TentativeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UnavailableId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_EventTemplateSet` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EventTemplateSet_EventTemplate_AttendingId` FOREIGN KEY (`AttendingId`) REFERENCES `EventTemplate` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_EventTemplateSet_EventTemplate_TentativeId` FOREIGN KEY (`TentativeId`) REFERENCES `EventTemplate` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_EventTemplateSet_EventTemplate_UnavailableId` FOREIGN KEY (`UnavailableId`) REFERENCES `EventTemplate` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `Groups` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FilePath` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SourceId` char(36) COLLATE ascii_general_ci NOT NULL,
        `StartTime` time(6) NOT NULL,
        `EndTime` time(6) NOT NULL,
        `TimeZone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EventTemplateSetId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `LastUpdated` datetime(6) NOT NULL,
        CONSTRAINT `PK_Groups` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Groups_EventTemplateSet_EventTemplateSetId` FOREIGN KEY (`EventTemplateSetId`) REFERENCES `EventTemplateSet` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `Events` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `StartTime` datetime(6) NOT NULL,
        `EndTime` datetime(6) NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Organizer` longtext CHARACTER SET utf8mb4 NOT NULL,
        `GroupId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_Events` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Events_Groups_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `Groups` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `GroupUser` (
        `GroupsId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsersId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_GroupUser` PRIMARY KEY (`GroupsId`, `UsersId`),
        CONSTRAINT `FK_GroupUser_Groups_GroupsId` FOREIGN KEY (`GroupsId`) REFERENCES `Groups` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_GroupUser_Users_UsersId` FOREIGN KEY (`UsersId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `PersonalEventTemplateSet` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Enabled` tinyint(1) NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `GroupId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EventTemplateSetId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_PersonalEventTemplateSet` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PersonalEventTemplateSet_EventTemplateSet_EventTemplateSetId` FOREIGN KEY (`EventTemplateSetId`) REFERENCES `EventTemplateSet` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PersonalEventTemplateSet_Groups_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `Groups` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PersonalEventTemplateSet_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE TABLE `EventAttendance` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` int NOT NULL,
        `EventId` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_EventAttendance` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EventAttendance_Events_EventId` FOREIGN KEY (`EventId`) REFERENCES `Events` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_EventAttendance_EventId` ON `EventAttendance` (`EventId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_Events_GroupId` ON `Events` (`GroupId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_EventTemplateSet_AttendingId` ON `EventTemplateSet` (`AttendingId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_EventTemplateSet_TentativeId` ON `EventTemplateSet` (`TentativeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_EventTemplateSet_UnavailableId` ON `EventTemplateSet` (`UnavailableId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_Groups_EventTemplateSetId` ON `Groups` (`EventTemplateSetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_GroupUser_UsersId` ON `GroupUser` (`UsersId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_PersonalEventTemplateSet_EventTemplateSetId` ON `PersonalEventTemplateSet` (`EventTemplateSetId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_PersonalEventTemplateSet_GroupId` ON `PersonalEventTemplateSet` (`GroupId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    CREATE INDEX `IX_PersonalEventTemplateSet_UserId` ON `PersonalEventTemplateSet` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305202833_Initial') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260305202833_Initial', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305205018_Deploy') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260305205018_Deploy', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305205157_Deploy2') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260305205157_Deploy2', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

