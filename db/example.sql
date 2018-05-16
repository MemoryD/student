CREATE TABLE student(
	Sno CHAR(9) PRIMARY KEY,
	Sname CHAR(20) NOT NULL,
	Ssex CHAR(2) NOT NULL,
	Sage SMALLINT NOT NULL,
	Scomment VARCHAR(100)
);

INSERT INTO Student VALUES
('201215121','李勇','男',20,'傻孩子'),
('201215122','刘晨','女',19,'你是班花'),
('201215123','王敏','女',18,'不知道说什么了'),
('201215124','张立','男',19,'这个功能有点傻');

CREATE TABLE Course(
	Cno CHAR(4) PRIMARY KEY,
	Cname CHAR(40) NOT NULL
);

INSERT INTO Course VALUES
('1','数据库'),
('2','离散数学'),
('3','操作系统'),
('4','数据结构');

CREATE TABLE SC(
	Sno CHAR(9),
	Cno CHAR(4),
	Grade SMALLINT,
	PRIMARY KEY(Sno, Cno),
	FOREIGN KEY (Sno) REFERENCES Student(Sno),
	FOREIGN KEY (Cno) REFERENCES Course(Cno)
);

INSERT INTO SC VALUES
('201215121','1',89),
('201215121','2',85),
('201215121','3',88),
('201215121','4',88),
('201215122','1',91),
('201215122','2',92),
('201215122','3',81),
('201215122','4',88),
('201215123','1',79),
('201215123','2',95),
('201215123','3',78),
('201215123','4',88),
('201215124','1',90),
('201215124','2',90),
('201215124','3',80),
('201215124','4',88);



CREATE VIEW Grade(Sno,Cname,Grade) AS SELECT SC.Sno,Cname,Grade FROM Course,SC WHERE Course.Cno = SC.Cno;
CREATE VIEW AveGrade(Cname,ave) AS SELECT Cname,AVG(Grade) FROM Course,SC WHERE Course.Cno = SC.Cno GROUP BY SC.Cno;
CREATE VIEW GradeOrder(Sno,Sname,total) AS SELECT student.Sno,student.Sname,SUM(Grade) FROM Student,SC WHERE Student.Sno = SC.Sno GROUP BY SC.Sno;