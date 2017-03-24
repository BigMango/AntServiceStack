/*
SQLyog Ultimate v12.09 (64 bit)
MySQL - 5.1.49-community : Database - antsoa
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`antsoa` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `antsoa`;

/*Table structure for table `services` */

DROP TABLE IF EXISTS `services`;

CREATE TABLE `services` (
  `Tid` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `DataChange_LastTime` datetime NOT NULL COMMENT '最后更新时间',
  `ServiceName` varchar(50) DEFAULT NULL COMMENT '服务名称',
  `Namespace` varchar(200) DEFAULT NULL COMMENT '命名空间',
  `Domain` varchar(50) DEFAULT NULL COMMENT '部门',
  `Status` int(11) NOT NULL DEFAULT '0' COMMENT '状态',
  `BusinessOwner` varchar(50) DEFAULT NULL COMMENT '维护人员',
  `TechOwner` varchar(50) DEFAULT NULL COMMENT '开发人员',
  `Type` int(11) NOT NULL DEFAULT '0' COMMENT '服务类型 0代表NET 1代表JAVA',
  `FullName` varchar(100) DEFAULT NULL COMMENT '服务发现注册名称',
  `isActive` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否可用',
  `Description` varchar(200) DEFAULT NULL COMMENT '描述',
  `SubDomain` varchar(50) DEFAULT NULL COMMENT '产线',
  PRIMARY KEY (`Tid`),
  UNIQUE KEY `idx_s_n` (`ServiceName`,`Namespace`),
  KEY `idx_DataChange_LastTime` (`DataChange_LastTime`),
  KEY `idx_FullName` (`FullName`),
  KEY `idx_ServiceName` (`ServiceName`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8 COMMENT='服务表';

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
